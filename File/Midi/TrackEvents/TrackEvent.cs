using System;

namespace Midif {
	[Flags]
	public enum MidiTrack {
		None = 0,

		Trk0 = 1 << 0,
		Trk1 = 1 << 1,
		Trk2 = 1 << 2,
		Trk3 = 1 << 3,
		Trk4 = 1 << 4,
		Trk5 = 1 << 5,
		Trk6 = 1 << 6,
		Trk7 = 1 << 7,
		Trk8 = 1 << 8,
		Trk9 = 1 << 9,
		Trk10 = 1 << 10,
		Trk11 = 1 << 11,
		Trk12 = 1 << 12,
		Trk13 = 1 << 13,
		Trk14 = 1 << 14,
		Trk15 = 1 << 15,
		Trk16 = 1 << 16,
		Trk17 = 1 << 17,
		Trk18 = 1 << 18,
		Trk19 = 1 << 19,
		Trk20 = 1 << 20,
		Trk21 = 1 << 21,
		Trk22 = 1 << 22,
		Trk23 = 1 << 23,
		Trk24 = 1 << 24,
		Trk25 = 1 << 25,
		Trk26 = 1 << 26,
		Trk27 = 1 << 27,
		Trk28 = 1 << 28,
		Trk29 = 1 << 29,
		Trk30 = 1 << 30,

		All = Trk0 | Trk1 | Trk2 | Trk3 | Trk4 | Trk5 | Trk6 | Trk7 | Trk8 | Trk9 | Trk10 | Trk11 | Trk12 | Trk13 | Trk14 | Trk15 | Trk16 | Trk17 | Trk18 | Trk19 | Trk20 | Trk21 | Trk22 | Trk23 | Trk24 | Trk25 | Trk26 | Trk27 | Trk28 | Trk29 | Trk30
	}

	public delegate void TrackEventHandler (TrackEvent trackEvent);

	public interface ITrackEventHandler {
		void TrackEventHandler (TrackEvent trackEvent);
	}

	[Serializable]
	public abstract class TrackEvent : IComparable {
		public int Track;
		public int Tick;

		public MidiTrack MidiTrack {
			get { return (MidiTrack)(1 << Track); }
		}


		protected TrackEvent (int track, int tick) {
			Track = track;
			Tick = tick;
		}

		public virtual int CompareTo (object other) {
			return Tick.CompareTo(((TrackEvent)other).Tick);
		}
	}
}

