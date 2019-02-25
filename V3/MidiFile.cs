using Unsaf;
using System.Collections.Generic;

namespace Midif.V3 {
	public struct MidiEvent {
		public int delta;
		public byte status;
		public byte type;
		public byte b1;
		public byte b2;
		public int dataLoc;
		public int dataLen;

		public MidiEvent(int delta, byte status, byte type, int dataLoc, int dataLen, byte[] bytes) {
			this.delta = delta;
			this.status = status;
			this.type = type;
			this.dataLoc = dataLoc;
			this.dataLen = dataLen;

			if (dataLen > 0) b1 = bytes[dataLoc]; else b1 = 0;
			if (dataLen > 1) b2 = bytes[dataLoc + 1]; else b2 = 0;
		}
	}

	public struct MidiFile {
		public short format;
		public short trackCount;
		public short ticksPerBeat;

		public byte[] bytes;
		public MidiEvent[][] tracks;
		public int[] trackLengths;

		public MidiFile(byte[] bytes) {
			this.bytes = bytes;

			int i = 0;
			// "MThd" 4 bytes
			i += 4;
			// <header_length> 4 bytes
			int length = BitBe.ReadInt32(bytes, ref i);
			int ii = i;
			// <format> 2 bytes
			format = BitBe.ReadInt16(bytes, ref i);
			// <n> 2 bytes
			trackCount = BitBe.ReadInt16(bytes, ref i);
			tracks = new MidiEvent[trackCount][];
			trackLengths = new int[trackCount];
			// <division> 2 bytes
			ticksPerBeat = BitBe.ReadInt16(bytes, ref i);
			// end chunk
			i = ii + length;

			byte runingStatus = 0;
			for (short j = 0, count = trackCount; j < count; j += 1) {
				var track = new List<MidiEvent>();

				// "MTrk" 4 bytes
				i += 4;
				// <length> 4 bytes
				length = BitBe.ReadInt32(bytes, ref i);
				ii = i;
				// <track_event>
				while (i < ii + length) {
					int delta = BitBe.ReadVlv(bytes, ref i);
					byte statusByte = Bit.ReadByte(bytes, ref i);
					int dataLength;
					if (statusByte < 0x80) {  // running status
						dataLength = GetMidiEventLength(runingStatus);
						track.Add(new MidiEvent(delta, runingStatus, 0, i - 1, dataLength, bytes));
						dataLength -= 1;
					} else if (statusByte < 0xf0) {  // midi events
						runingStatus = statusByte;
						dataLength = GetMidiEventLength(statusByte);
						track.Add(new MidiEvent(delta, statusByte, 0, i, dataLength, bytes));
					} else if (statusByte == 0xf0 || statusByte == 0xf7) {  // sysex events | escape sequences
						dataLength = BitBe.ReadVlv(bytes, ref i);
						track.Add(new MidiEvent(delta, statusByte, 0, i, dataLength, bytes));
					} else if (statusByte == 0xff) {  // meta events
						byte type = Bit.ReadByte(bytes, ref i);
						dataLength = BitBe.ReadVlv(bytes, ref i);
						track.Add(new MidiEvent(delta, statusByte, type, i, dataLength, bytes));
					} else {
						return;
					}
					i += dataLength;
				}
				tracks[j] = track.ToArray();
				trackLengths[j] = track.Count;
				// end chunk
				i = ii + length;
			}
		}
	
		static int GetMidiEventLength(byte statusByte) {
			switch (statusByte >> 4) {
			case 0x8:  // note off
			case 0x9:  // note on
			case 0xa:  // aftertouch
			case 0xb:  // controller
			case 0xe:  // pitch bend
				return 2;
			case 0xc:  // program change
			case 0xd:  // channel pressure
				return 1;
			}
			return 0;
		}
	}
}