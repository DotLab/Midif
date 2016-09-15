using System;

namespace Midif.Synth {
	public class TriangleGenerator : BaseSignalProvider {
		public int Transpose;
		public int Tune;

		double phaseStep;
		double phase;


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			phaseStep = 4 * SynthTable.Note2Freq[note + Transpose] * SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * sampleRateRecip;
			phase = 3;
		}

		public override double Render () {
			if ((phase += phaseStep) > 4) phase %= 4;
			return Math.Abs(phase - 2) - 1;
		}
	}
}