using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Midif.File;

namespace Midif {
	public enum MidiFileFormat {
		SingleTrack = 0x00,
		MultiTrack = 0x01,
		MultiSong = 0x02
	}

	[System.Serializable]
	public class MidiFile {
		static int Gcd (int a, int b) {
			while (b > 0) {
				int temp = b;
				b = a % b; // % is remainder
				a = temp;
			}

			return a;
		}

		public MidiFileFormat Format;
		public int NumberOfTracks;
		public int TicksPerBeat;

		public int Length;

		public List<MidiEvent> MidiEvents = new List<MidiEvent>();
		public List<SysExEvent> SysExEvents = new List<SysExEvent>();
		public List<MetaEvent> MetaEvents = new List<MetaEvent>();

		#region Constructor

		// Chunk is not Midif.File.RiffChunk, Chunk.Length is Big Endian;
		class Chunk {
			public string Id;
			public int Length;
			public byte[] Bytes;

			// chunk = <chunk_id> + <chunk_length> + <chunk_bytes>
			public Chunk (Stream stream) {
				Id = StreamHelperBe.ReadString(stream, 4);

				Length = (int)StreamHelperBe.ReadUInt32(stream);
//				DebugConsole.WriteLine("Padding " + (Length & 1));
				Bytes = new byte[Length];
				stream.Read(Bytes, 0, Length);
			}

			public Stream GetStream () {
				return new MemoryStream(Bytes);
			}
		}

		public MidiFile (Stream stream) {
			// header_chunk = "MThd" + <header_length> + <format> + <n> + <division>
			var chunk = new Chunk(stream);
			if (chunk.Id != "MThd")
				throw new FileFormatException("MidiFile.chunk.Id", chunk.Id, "MThd");

			using (var header = chunk.GetStream()) {
				Format = (MidiFileFormat)StreamHelperBe.ReadInt16(header);
				NumberOfTracks = StreamHelperBe.ReadUInt16(header);

				var division = StreamHelperBe.ReadInt16(header);
				if (division > 0)
					TicksPerBeat = division;
				else
					throw new Exception("STMPE based time is not supported");
			}

			// track_chunk = "MTrk" + <track_length> [+ <delta_time> + <event> ...]
			for (int track = 0; track < NumberOfTracks; track++) {
				chunk = new Chunk(stream);
				if (chunk.Id != "MTrk")
					throw new FileFormatException("MidiFile.chunk.Id", chunk.Id, "MTrk");

				using (var trackStream = chunk.GetStream()) {
					BuildTrack(trackStream, track);
				}
			}

			Sort();
			Rebase();
			Trim();
		}

		void BuildTrack (Stream stream, int track) {
			int tick = 0;
			byte runningStatus = 0x00;

			while (stream.Position < stream.Length) {
				tick += (int)StreamHelperBe.ReadVlv(stream);
				var statusByte = (byte)stream.ReadByte();

				if (statusByte < 0xF0) { // Midi events (status bytes 0x8n - 0xEn)
					byte dataByte1;
					if (statusByte < 0x80) { // If the first byte < 0x80, running status is in effect, and this byte is actually the first data byte.
						dataByte1 = statusByte;
						statusByte = runningStatus;
					} else {
						dataByte1 = (byte)stream.ReadByte();
						runningStatus = statusByte;
					}

					var midiEvent = new MidiEvent(track, tick, statusByte);

					switch (statusByte & 0xF0) {
					case 0x80: // Note Off
					case 0x90: // Note On
					case 0xA0: // Aftertouch
					case 0xB0: // Controller Change
					case 0xE0: // Pitch Bend
						midiEvent.DataByte1 = dataByte1;
						midiEvent.DataByte2 = (byte)stream.ReadByte();
						break;
					case 0xC0: // Program Change
					case 0xD0: // Channel Aftertouch
						midiEvent.DataByte1 = dataByte1;
						break;
					default:
						throw new FileFormatException("MidiFile.statusByte", statusByte.ToString("X"), "[midi statusByte]");
					}

					if (midiEvent.Type == MidiEventType.NoteOn && midiEvent.Velocity == 0)
						midiEvent.StatusByte = (byte)(0x80 | midiEvent.Channel);

					MidiEvents.Add(midiEvent);
				} else if (statusByte == 0xF0 || statusByte == 0xF7) { // SysEx events (status bytes 0xF0 and 0xF7)
					runningStatus = 0x00;

					var sysExEvent = 
						new SysExEvent(track, tick, (int)StreamHelperBe.ReadVlv(stream));
					stream.Read(sysExEvent.Bytes, 0, sysExEvent.Length);

					SysExEvents.Add(sysExEvent);
				} else if (statusByte == 0xFF) { // Meta events (status byte 0xFF)
					runningStatus = 0x00;

					var metaEvent = 
						new MetaEvent(track, tick, (byte)stream.ReadByte(), (int)StreamHelperBe.ReadVlv(stream));
					stream.Read(metaEvent.Data, 0, metaEvent.Length);

					MetaEvents.Add(metaEvent);
				} else
					throw new FileFormatException("MidiFile.statusByte", statusByte.ToString("X"), "[midi statusByte]");
			}

			Length = Math.Max(Length, tick);
		}

		#endregion

		public void Sort () {
			MidiEvents.Sort();
			SysExEvents.Sort();
			MetaEvents.Sort();
		}

		public void Rebase () {
			int cdTick = 0;
			foreach (var trackEvent in MidiEvents) cdTick = Gcd(cdTick, trackEvent.Tick);
			// foreach (var trackEvent in SysExEvents) cdTick = gcd(cdTick, trackEvent.Tick);
			foreach (var trackEvent in MetaEvents) cdTick = Gcd(cdTick, trackEvent.Tick);

			if (cdTick == 1) return;
//			DebugConsole.Log(string.Format("Rebase TicksPerBeat from {0} to {1}.", TicksPerBeat, TicksPerBeat / cdTick));

			TicksPerBeat /= cdTick;
			Length /= cdTick;
			foreach (var trackEvent in MidiEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in SysExEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in MetaEvents) trackEvent.Tick /= cdTick;
		}

		public void Trim () {
			Length = MidiEvents[MidiEvents.Count - 1].Tick + TicksPerBeat;
		}

		#region Info

		public double LengthInSeconds;
		public List<int> Tracks = new List<int>();
		public List<int> Channels = new List<int>();

		public void Analyze () {
			CalcLengthInSeconds();

			foreach (var midiEvent in MidiEvents) {
				if (!Tracks.Contains(midiEvent.Track))
					Tracks.Add(midiEvent.Track);

				if (!Channels.Contains(midiEvent.Channel))
					Channels.Add(midiEvent.Channel);
			}

			Tracks.Sort();
			Channels.Sort();
		}

		double CalcLengthInSeconds () {
			const double microsecondPerSecond = 1000000;
			var ticksPerSecond = (double)TicksPerBeat / 500000 * microsecondPerSecond;
			double time = 0;
			int lastTick = 0;

			foreach (var metaEvent in MetaEvents)
				if (metaEvent.Type == MetaEventType.Tempo) {
					time += (metaEvent.Tick - lastTick) / ticksPerSecond;
					lastTick = metaEvent.Tick;
					ticksPerSecond = (double)TicksPerBeat / metaEvent.Tempo * microsecondPerSecond;
				}

			time += (Length - lastTick) / ticksPerSecond;

			return LengthInSeconds = time;
		}

		#endregion

		public override string ToString () {
			return string.Format("[MidiFile: Format={0}, NumberOfTracks={1}, TicksPerBeat={2}, Length={3}]", Format, NumberOfTracks, TicksPerBeat, Length);
		}
	}
}

