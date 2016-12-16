namespace Midif.Note {
	public delegate void MixNoteHandler (MixNote note);

	public class MixNote : TrackEvent {
		public int TickEnd;
		public int TickLength;

		public byte Channel, Note, Velocity;

		public MixNote (
			int track, byte channel, byte note, byte velocity, int tickStart, int tickEnd) : base(track, tickStart) {

			Track = track;
			Channel = channel;

			Note = note;
			Velocity = velocity;
		
			TickEnd = tickEnd;
			TickLength = tickEnd - tickStart;
		}

		public override string ToString () {
			return string.Format("[MixNote: Channel={0}, Note={1}, Velocity={2}]", Channel, Note, Velocity);
		}
	}
}
