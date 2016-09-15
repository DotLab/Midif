using System;
using System.IO;
using System.Collections.Generic;

namespace Midif {
	public enum MidiFileFormat {
		SingleTrack = 0x00,
		MultiTrack = 0x01,
		MultiSong = 0x02
	}

	public class MidiFile {
		class Chunk {
			public string Id;
			public int Length;
			public byte[] Bytes;

			// chunk = <chunk_id> + <chunk_length> + <chunk_bytes>
			public Chunk (Stream stream) {
				Id = MidiStreamHelper.ReadString(stream, 4);

				Length = MidiStreamHelper.ReadUInt32(stream);

				Bytes = new byte[Length];
				stream.Read(Bytes, 0, Length);
			}

			public Stream GetStream () {
				return new MemoryStream(Bytes);
			}
		}

		public static bool AutoRebaseTick = true;

		public MidiFileFormat Format;
		public int NumberOfTracks;
		public int TicksPerBeat;

		public int Length;

		public List<MidiEvent> MidiEvents;
		public List<SysExEvent> SysExEvents;
		public List<MetaEvent> MetaEvents;

		// midi = <header_chunk> + <track_chunk> [+ <track_chunk> ...]
		public MidiFile (Stream stream) {
			// header_chunk = "MThd" + <header_length> + <format> + <n> + <division>
			var chunk = new Chunk(stream);
			if (chunk.Id != "MThd")
				throw new Exception(string.Format("Unexpected chunk.Id : {0}, 'MThd' expected.", chunk.Id));
			
			using (var header = chunk.GetStream()) {
				Format = (MidiFileFormat)MidiStreamHelper.ReadInt16(header);

				NumberOfTracks = MidiStreamHelper.ReadUInt16(header);

				var division = MidiStreamHelper.ReadInt16(header);
				if (division > 0) TicksPerBeat = division;
				else throw new Exception("STMPE based time is not supported");
			}

			MidiEvents = new List<MidiEvent>();
			SysExEvents = new List<SysExEvent>();
			MetaEvents = new List<MetaEvent>();

			// track_chunk = "MTrk" + <track_length> [+ <delta_time> + <event> ...]
			for (int track = 0; track < NumberOfTracks; track++) {
				chunk = new Chunk(stream);
				if (chunk.Id != "MTrk")
					throw new Exception(string.Format("Unexpected chunk.Id : {0}, 'MTrk' expected.", chunk.Id));

				using (var trackStream = chunk.GetStream()) {
					int tick = 0;
					byte runningStatus = 0x00;
					SysExEvent pendingSysExEvent = null;

					while (trackStream.Position < chunk.Length) {
						tick += MidiStreamHelper.ReadVlv(trackStream);
						var statusByte = (byte)trackStream.ReadByte();

						if (statusByte < 0x80) { // If the first (status) byte is less than 128 (hex 80), this implies that running status is in effect, and that this byte is actually the first data byte.
							var midiEvent = new MidiEvent(track, tick, runningStatus);

							switch (runningStatus & 0xF0) {
							case 0x80: // Note Off
							case 0x90: // Note On
							case 0xA0: // Aftertouch
							case 0xB0: // Controller Change
							case 0xE0: // Pitch Bend
								midiEvent.DataByte1 = statusByte;
								midiEvent.DataByte2 = (byte)trackStream.ReadByte();
								break;
							case 0xC0: // Program Change
							case 0xD0: // Channel Aftertouch
								midiEvent.DataByte1 = statusByte;
								break;
							default:
								throw new Exception("Unexpected runningStatus : " + runningStatus.ToString("X"));
							}

							if (midiEvent.Type == MidiEventType.NoteOn && midiEvent.Velocity == 0)
								midiEvent.StatusByte = (byte)(0x80 | midiEvent.Channel);
						
							MidiEvents.Add(midiEvent);
						} else if (statusByte < 0xF0) { // Midi events (status bytes 0x8n - 0xEn)
							var midiEvent = new MidiEvent(track, tick, statusByte);

							switch (statusByte & 0xF0) {
							case 0x80: // Note Off
							case 0x90: // Note On
							case 0xA0: // Aftertouch
							case 0xB0: // Controller Change
							case 0xE0: // Pitch Bend
								midiEvent.DataByte1 = (byte)trackStream.ReadByte();
								midiEvent.DataByte2 = (byte)trackStream.ReadByte();
								break;
							case 0xC0: // Program Change
							case 0xD0: // Channel Aftertouch
								midiEvent.DataByte1 = (byte)trackStream.ReadByte();
								break;
							}

							if (midiEvent.Type == MidiEventType.NoteOn && midiEvent.Velocity == 0)
								midiEvent.StatusByte = (byte)(0x80 | midiEvent.Channel);

							MidiEvents.Add(midiEvent);
						} else if (statusByte == 0xF0) { // When an event with 0xF0 status but lacking a terminal 0xF7 is encountered, then this is the first of a Casio-style multi-packet message.
							runningStatus = 0x00;

							var sysExEvent = new SysExEvent(track, tick, trackStream);
							if (!sysExEvent.IsTerminated) pendingSysExEvent = sysExEvent;

							SysExEvents.Add(sysExEvent);
						} else if (statusByte == 0xF7) { // SysEx events (status bytes 0xF0 and 0xF7)
							runningStatus = 0x00;

							if (pendingSysExEvent == null) { // If an event with 0xF7 status is encountered whilst flag is clear, then this event is an escape sequence.
								var sysExEvent = new SysExEvent(track, tick, trackStream);

								SysExEvents.Add(sysExEvent);
							} else { // If an event with 0xF7 status is encountered whilst this flag is set, then this is a continuation event.
								pendingSysExEvent.Append(stream);
								if (pendingSysExEvent.IsTerminated) pendingSysExEvent = null;
							}
						} else if (statusByte == 0xFF) { // Meta events (status byte 0xFF)
							runningStatus = 0x00;

							var metaEvent = new MetaEvent(track, tick, trackStream);

							MetaEvents.Add(metaEvent);
						} else throw new Exception("Unexpected statusByte : " + statusByte.ToString("X"));
					}

					var lastMetaEvent = MetaEvents[MetaEvents.Count - 1];
					if (lastMetaEvent.Type != MetaEventType.EndOfTrack)
						throw new Exception(string.Format("Unexpected lastMetaEvent.Type : {0}({1}), 'MetaEventType.EndOfTrack' expected.", lastMetaEvent.Type, lastMetaEvent.TypeByte.ToString("X")));
					Length = Math.Max(Length, tick);
				}
			}

			MidiEvents.Sort();
			SysExEvents.Sort();
			MetaEvents.Sort();

			if (AutoRebaseTick) RebaseTick();
		}

		// Calculate minimum tick
		public void RebaseTick () {
			int cdTick = 0;
			foreach (var trackEvent in MidiEvents) cdTick = gcd(cdTick, trackEvent.Tick);
//			foreach (var trackEvent in SysExEvents) cdTick = gcd(cdTick, trackEvent.Tick);
//			foreach (var trackEvent in MetaEvents) cdTick = gcd(cdTick, trackEvent.Tick);

			if (cdTick == 1) return;
			DebugConsole.Log(string.Format("Rebase TicksPerBeat from {0} to {1}.", TicksPerBeat, TicksPerBeat / cdTick));

			TicksPerBeat /= cdTick;
			Length /= cdTick;
			foreach (var trackEvent in MidiEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in SysExEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in MetaEvents) trackEvent.Tick /= cdTick;
		}

		static int gcd (int a, int b) {
			while (b > 0) {
				int temp = b;
				b = a % b; // % is remainder
				a = temp;
			}

			return a;
		}

		public override string ToString () {
			return string.Format("(MidiFile: Format={0}, TicksPerBeat={1}, NumberOfTracks={2}, Length={3})",
				Format, TicksPerBeat, NumberOfTracks, Length);
		}
	}
}