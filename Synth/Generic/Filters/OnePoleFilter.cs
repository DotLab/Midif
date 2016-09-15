using System;

namespace Midif.Synth.Generic {
	public class OnePoleFilter : CachedSignalProvider {
		public enum FilterType {
			LowPass,
			HighPass,
		}

		public ISignalProvider Source;

		public FilterType Type = FilterType.LowPass;
		public double Fc;

		double a0, b1;
		double z1;

		public override void Init (double sampleRate) {
			Source.Init(sampleRate);

			// http://www.earlevel.com/main/2012/12/15/a-one-pole-filter/
			switch (Type) {
			case FilterType.LowPass:
				b1 = Math.Exp(-2 * Math.PI * Fc / sampleRate);
				a0 = 1.0 - b1;
				break;
			case FilterType.HighPass:
				b1 = -Math.Exp(-2 * Math.PI * (0.5 - Fc / sampleRate));
				a0 = 1.0 + b1;
				break;
			}
		}

		public override void NoteOn (byte note, byte velocity) {
			Source.NoteOn(note, velocity);

			z1 = 0;
		}

		public override void NoteOff (byte velocity) {
			Source.NoteOff(velocity);
		}

		public override bool IsActive () {
			return Source.IsActive();
		}

		public override double Render () {
			return z1 = Source.Render(flag) * a0 + z1 * b1;
		}
	}
}

