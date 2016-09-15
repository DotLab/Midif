using System;

namespace Midif {
	public abstract class TrackEvent : IComparable {
		public int Track;
		public int Tick;


		protected TrackEvent (int track, int tick) {
			Track = track;
			Tick = tick;
		}

		public int CompareTo (object other) {
			return Tick.CompareTo(((TrackEvent)other).Tick);
		}
	}
}

