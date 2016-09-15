namespace Midif {
	public class MidiHeader {
		public MidiFormat MidiFormat;
		public MidiTimeFormat TimeFormat;
		public int PulsesPerQuaterNote;

		public void SetMidiFormat (int format) {
			MidiFormat = (MidiFormat)format;
		}

		public void SetTimeFormat (int division) {
			TimeFormat = ((division & 0x8000) > 0) ? MidiTimeFormat.FramesPerSecond : MidiTimeFormat.PulsesPerQuarterNote;
			PulsesPerQuaterNote = (division & 0x7FFF);
		}
	}
}
