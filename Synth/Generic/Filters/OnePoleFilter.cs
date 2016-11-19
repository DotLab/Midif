using System;

namespace Midif.Synth {
	public class OnePoleFilter : MidiComponent {
		public enum FilterType {
			LowPass,
			HighPass,
		}

		public IComponent Source;

		public FilterType Type = FilterType.LowPass;
		public double Fc;

		public override bool IsActive {	get { return Source.IsActive; } }

		double a0, b1;
		double z1;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			UpdateCoeffs();
		
			Source.Init(sampleRate);
		}

		public void UpdateCoeffs () {
			// http://www.earlevel.com/main/2012/12/15/a-one-pole-filter/
			switch (Type) {
			case FilterType.LowPass:
				b1 = Math.Exp(-2 * Math.PI * Fc * sampleRateRecip);
				a0 = 1.0 - b1;
				break;
			case FilterType.HighPass:
				b1 = -Math.Exp(-2 * Math.PI * (0.5 - Fc * sampleRateRecip));
				a0 = 1.0 + b1;
				break;
			}
		}


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

//			z1 = 0;
		
			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);

			Source.NoteOff(note, velocity);
		}


		public override double Render () {
			return z1 = Source.Render(renderFlag) * a0 + z1 * b1;
		}
	}
}
	