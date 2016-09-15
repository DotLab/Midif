using System;

namespace Midif.Synth.Sfz {
	public class SfzLfo : CachedSignalProvider {
		public static readonly double Period = Math.PI * 2;

		public double Delay;
		public double Frequency;

		int delaySample;
		int delay;

		double phaseStep;
		double phase;

		public override void Init (double sampleRate) {
			delaySample = (int)(Delay * sampleRate);

			phaseStep = Period * Frequency / sampleRate;
		}

		public override void NoteOn (byte note, byte velocity) {
			delay = delaySample;
		}

		public override double Render () {
			if (delay > 0) {
				delay--;
				return 0;
			}

			var sample = Math.Sin(phase);

			phase += phaseStep;
			if (phase > Period)
				phase -= Period;

			return sample;
		}
	}
}

