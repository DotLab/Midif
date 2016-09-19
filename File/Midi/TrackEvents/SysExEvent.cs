using System;

namespace Midif {
	[Serializable]
	public class SysExEvent : TrackEvent {
		public int Length;
		public byte[] Bytes;

		public bool IsTerminated {
			get { return Bytes[Bytes.Length - 1] == 0xF7; }
		}

		public SysExEvent (int track, int tick, int length) : base(track, tick) {
			Length = length;

			Bytes = new byte[Length];
		}

		public override string ToString () {
			return string.Format("[SysExEvent: Track={0}, Time={1}, Bytes={2}]", Track, Tick, BitConverter.ToString(Bytes));
		}
	}
}

