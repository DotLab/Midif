namespace Midif {
	public class MidiNote {
		public int Track;
		public int Channel;
		public int Note;

		public double Gain;

		public uint StartSample;
		public uint EndSample;
		public uint Duration;


		public MidiNote (int track, int channel, int note, int velocity) {
			Track = track;
			Channel = channel;
			Note = note;

			Gain = (double)velocity / 127.0;
		}

		public void SetStartSample (uint startSample) {
			StartSample = startSample;
		}

		public void SetEndSample (uint endSample) {
			EndSample = endSample;
			Duration = endSample - StartSample;
		}
	}
}