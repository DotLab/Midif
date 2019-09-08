using System;

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
}

