namespace Midif {
    public class MidiHeader {
        //--Variables
        public MidiFormat MidiFormat;
        public MidiTimeFormat TimeFormat;
		public int PulsesPerQuaterNote;

        //--Public Methods
        public void SetMidiFormat(int format) {
            if (format == 0)
                MidiFormat = MidiFormat.SingleTrack;
            else if (format == 1)
                MidiFormat = MidiFormat.MultiTrack;
            else if (format == 2)
                MidiFormat = MidiFormat.MultiSong;
        }

		public void SetMidiTime (int division) {
			TimeFormat = ((division & 0x8000) > 0) ? MidiTimeFormat.FramesPerSecond : MidiTimeFormat.PulsesPerQuarterNote;
			PulsesPerQuaterNote = (division & 0x7FFF);
		}
    }
}
