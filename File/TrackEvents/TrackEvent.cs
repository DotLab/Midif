using System;

namespace Midif.File {
	public abstract class TrackEvent : IComparable {
		public int Track;
		public int Time;


		protected TrackEvent (int track, int time) {
			Track = track;
			Time = time;
		}

		public int CompareTo (object other) {
			return Time.CompareTo(((TrackEvent)other).Time);
		}
	}
}

