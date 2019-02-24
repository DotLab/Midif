using Unsaf;

namespace Midif.V2 {
	public unsafe struct MidiFile {
		public unsafe struct Event {
			public int delta;
			public int status;
			public int length;
			// followed by vaialbe-length data

			public static void Init(Event *self, int delta, byte status, int length) {
				self->delta = delta;
				self->status = status;
				self->length = length;
			}
		}

		static int GetMidiEventLength(byte statusByte) {
			switch (statusByte & 0xf0) {
			case 0x80:  // note off
			case 0x90:  // note on
			case 0xa0:  // aftertouch
			case 0xb0:  // controller
			case 0xe0:  // pitch bend
				return 2;
			case 0xc0:  // program change
			case 0xd0:  // channel pressure
				return 1;
			}
			return 0;
		}

		public short format;
		public short trackCount;
		public short ticksPerBeat;

		public byte **tracks;

		public static void Init(MidiFile *self, byte *bytes) {
			int EventSize = sizeof(Event);

			int i = 0;

			// "MThd" 4 bytes
			i += 4;
			// <header_length> 4 bytes
			int length = BitBe.ReadInt(bytes, &i);
			int ii = i;
			// <format> 2 bytes
			self->format = BitBe.ReadShort(bytes, &i);
			// <n> 2 bytes
			self->trackCount = BitBe.ReadShort(bytes, &i);
			self->tracks = (byte **)Mem.Malloc(self->trackCount * sizeof(byte *));
			// <division> 2 bytes
			self->ticksPerBeat = BitBe.ReadShort(bytes, &i);
			// end chunk
			i = ii + length;

			byte runingStatus = 0;
			for (short j = 0, count = self->trackCount; j < count; j += 1) {
				Buf *buf = stackalloc Buf[1];
				Buf.Init(buf);
				// "MTrk" 4 bytes
				i += 4;
				// <length> 4 bytes
				length = BitBe.ReadInt(bytes, &i);
				ii = i;
				// <track_event>
//				UnityEngine.Debug.LogFormat("New track {0}", j);
				while (i < ii + length) {
					int delta = BitBe.ReadVlv(bytes, &i);
					byte statusByte = Bit.ReadByte(bytes, &i);
					int dataLength;
					byte *data;
					if (statusByte < 0x80) {  // running status
						dataLength = GetMidiEventLength(runingStatus);
						Event *e = (Event *)Buf.Alloc(buf, EventSize + dataLength);
						Event.Init(e, delta, runingStatus, dataLength);
						data = (byte *)(e + 1);
						data[0] = statusByte;
						data += 1;
						dataLength -= 1;
//						UnityEngine.Debug.LogFormat("{1}: midi {0:X} {2:X} R", runingStatus, delta, statusByte);
					} else if (statusByte < 0xf0) {  // midi events
						runingStatus = statusByte;
						dataLength = GetMidiEventLength(statusByte);
						Event *e = (Event *)Buf.Alloc(buf, EventSize + dataLength);
						Event.Init(e, delta, statusByte, dataLength);
						data = (byte *)(e + 1);
//						UnityEngine.Debug.LogFormat("{1}: midi {0:X}", statusByte, delta);
					} else if (statusByte == 0xf0 || statusByte == 0xf7) {  // sysex events | escape sequences
						dataLength = BitBe.ReadVlv(bytes, &i);
						Event *e = (Event *)Buf.Alloc(buf, EventSize + dataLength);
						Event.Init(e, delta, statusByte, dataLength);
						data = (byte *)(e + 1);
//						UnityEngine.Debug.LogFormat("{1}: sysex {0:X}", statusByte, delta);
					} else if (statusByte == 0xff) {  // meta events
						byte type = Bit.ReadByte(bytes, &i);
						dataLength = BitBe.ReadVlv(bytes, &i);
						Event *e = (Event *)Buf.Alloc(buf, EventSize + 1 + dataLength);
						Event.Init(e, delta, statusByte, 1 + dataLength);
						data = (byte *)(e + 1);
						data[0] = type;
						data += 1;
//						UnityEngine.Debug.LogFormat("{2}: meta {0:X} {1:X}", statusByte, type, delta);
					} else {
						return;
					}
					Mem.Memcpy(data, bytes + i, dataLength);
					i += dataLength;
				}
				self->tracks[j] = (byte *)Buf.Trim(buf);
				// end chunk
				i = ii + length;
			}
		}

		public static void Free(MidiFile *self) {
			for (int i = 0; i < self->trackCount; i++) {
				Mem.Free(self->tracks[i]);
			}
			Mem.Free(self->tracks);
		}
	}
}