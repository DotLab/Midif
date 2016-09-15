namespace Midif {
	public delegate void TrackEventHandler (TrackEvent trackEvent);

	public interface ITrackEventHandler {
		void TrackEventHandler (TrackEvent trackEvent);
	}

	[System.Serializable]
	public abstract class TrackEvent : System.IComparable {
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

