using System;

namespace Midif.Synth.Generic {
	public class SquareWaveGenerator : CachedSignalProvider {
		public static readonly double[] SquareTable = { 1, -1 };

		double timeSqure;
		double phaseStep;
		double phase;

		public override void Init (double sampleRate) {
			timeSqure = 2 / sampleRate;
		}


		public override void NoteOn (byte note, byte velocity) {
			phaseStep = SynthConstants.Note2Freq[note] * timeSqure;
			phase = 0;
		}


		public override double Render () {
			var sample = SquareTable[(int)(phase += phaseStep) & 1];

			return sample;
		}
	}
}

