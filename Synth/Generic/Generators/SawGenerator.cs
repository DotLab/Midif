namespace Midif.Synth {
	public class SawGenerator : BaseSignalProvider {
		public int Transpose;
		public int Tune;

		double phaseStep;
		double phase;


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			phaseStep = SynthTable.Note2Freq[note + Transpose] * SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * sampleRateRecip;
			phase = 0;
		}

		public override double Render () {
			if ((phase += phaseStep) > 1) phase %= 1;
			return 2 * (phase - (int)(phase + 0.5));
		}
	}
}