using System;

namespace Midif.Synth.Generic {
	public class SineWaveGenerator : CachedSignalProvider {
		public static readonly double Period = Math.PI * 2;

		double timeSqure;
		double phaseStep;
		double phase;

		public override void Init (double sampleRate) {
			timeSqure = Period / sampleRate;
		}


		public override void NoteOn (byte note, byte velocity) {
			phaseStep = SynthConstants.Note2Freq[note] * timeSqure;
			phase = 0;
		}


		public override double Render () {
			var sample = Math.Sin(phase += phaseStep);

			if (phase > Period)
				phase -= Period;

			return sample;
		}
	}
}

