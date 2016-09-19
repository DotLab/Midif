using System;
using System.IO;
using System.Text;

using Midif.File;

namespace Midif {
	public static class MidiFileBuilder {
		// MidiFile uses Big Endian;
		static class StreamHelper {
			const int Int16Length = 2;
			const int Int32Length = 4;

			public static string ReadString (Stream stream, int length) {
				var bytes = new byte[length];
				stream.Read(bytes, 0, length);
				return Encoding.UTF8.GetString(bytes);
			}

			public static int ReadInt16 (Stream stream) {
				var bytes = new byte[Int16Length];
				stream.Read(bytes, 0, Int16Length);
				Array.Reverse(bytes);
				return (int)BitConverter.ToInt16(bytes, 0);
			}

			public static int ReadInt32 (Stream stream) {
				var bytes = new byte[Int32Length];
				stream.Read(bytes, 0, Int32Length);
				Array.Reverse(bytes);
				return BitConverter.ToInt32(bytes, 0);
			}

			public static int ReadUInt16 (Stream stream) {
				var bytes = new byte[Int16Length];
				stream.Read(bytes, 0, Int16Length);
				Array.Reverse(bytes);
				return (int)BitConverter.ToUInt16(bytes, 0);
			}

			public static int ReadUInt32 (Stream stream) {
				var bytes = new byte[Int32Length];
				stream.Read(bytes, 0, Int32Length);
				Array.Reverse(bytes);
				return (int)BitConverter.ToUInt32(bytes, 0);
			}

			public static int ReadVlv (Stream stream) {
				int b, value = 0;

				do {
					b = stream.ReadByte();

					value = value << 7;
					value += b & 0x7F;
				} while ((b & 0x80) != 0) ;

				return value;
			}
		}

		// Chunk is not Midif.File.RiffChunk, Chunk.Length is Big Endian;
		class Chunk {
			public string Id;
			public int Length;
			public byte[] Bytes;

			// chunk = <chunk_id> + <chunk_length> + <chunk_bytes>
			public Chunk (Stream stream) {
				Id = StreamHelper.ReadString(stream, 4);

				Length = StreamHelper.ReadUInt32(stream);

				Bytes = new byte[Length];
				stream.Read(Bytes, 0, Length);
			}

			public Stream GetStream () {
				return new MemoryStream(Bytes);
			}
		}

		public static MidiFile Build (byte[] bytes) {
			using (var stream = new MemoryStream(bytes)) {
				return Build(stream);
			}
		}

		// midi = <header_chunk> + <track_chunk> [+ <track_chunk> ...]
		public static MidiFile Build (Stream stream) {
			var file = new MidiFile();

			// header_chunk = "MThd" + <header_length> + <format> + <n> + <division>
			var chunk = new Chunk(stream);
			if (chunk.Id != "MThd")
				throw new FileFormatException("MidiFile.chunk.Id", chunk.Id, "MThd");

			using (var header = chunk.GetStream()) {
				file.Format = (MidiFileFormat)StreamHelper.ReadInt16(header);
				file.NumberOfTracks = StreamHelper.ReadUInt16(header);

				var division = StreamHelper.ReadInt16(header);
				if (division > 0)
					file.TicksPerBeat = division;
				else
					throw new Exception("STMPE based time is not supported");
			}

			// track_chunk = "MTrk" + <track_length> [+ <delta_time> + <event> ...]
			for (int track = 0; track < file.NumberOfTracks; track++) {
				chunk = new Chunk(stream);
				if (chunk.Id != "MTrk")
					throw new FileFormatException("MidiFile.chunk.Id", chunk.Id, "MTrk");
				
				using (var trackStream = chunk.GetStream()) {
					BuildTrack(file, track, trackStream);
				}
			}

			file.Sort();
			file.Rebase();
			file.Trim();

			return file;
		}

		static void BuildTrack (MidiFile file, int track, Stream stream) {
			int tick = 0;
			byte runningStatus = 0x00;

			while (stream.Position < stream.Length) {
				tick += StreamHelper.ReadVlv(stream);
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

					file.MidiEvents.Add(midiEvent);
				} else if (statusByte == 0xF0 || statusByte == 0xF7) { // SysEx events (status bytes 0xF0 and 0xF7)
					runningStatus = 0x00;

					var sysExEvent = 
						new SysExEvent(track, tick, StreamHelper.ReadVlv(stream));
					stream.Read(sysExEvent.Bytes, 0, sysExEvent.Length);

					file.SysExEvents.Add(sysExEvent);
				} else if (statusByte == 0xFF) { // Meta events (status byte 0xFF)
					runningStatus = 0x00;

					var metaEvent = 
						new MetaEvent(track, tick, (byte)stream.ReadByte(), StreamHelper.ReadVlv(stream));
					stream.Read(metaEvent.Data, 0, metaEvent.Length);

					file.MetaEvents.Add(metaEvent);
				} else
					throw new FileFormatException("MidiFile.statusByte", statusByte.ToString("X"), "[midi statusByte]");
			}

			file.Length = Math.Max(file.Length, tick);
		}
	}
}

