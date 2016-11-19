using System;

namespace Midif.Synth {
	public class BiquadFilter : MidiComponent {
		public enum FilterType {
			LowPass,
			HighPass,
			BandPass,
			Notch,
			Peak,
			LowShelf,
			HighShelf,
		}

		public IComponent Source;

		public FilterType Type = FilterType.LowPass;
		public double Fc, Q = 0.707, PeakGain;

		public override bool IsActive {	get { return Source.IsActive; } }

		double a0, a1, a2, b1, b2;
		double z1, z2;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			UpdateCoeffs();

			Source.Init(sampleRate);
		}

		public void UpdateCoeffs () {
			// http://www.earlevel.com/main/2012/11/26/biquad-c-source-code/
			double norm;
			double V = Math.Pow(10, Math.Abs(PeakGain) / 20.0);
			double K = Math.Tan(Math.PI * Fc * sampleRateRecip);

			switch (Type) {
			case FilterType.LowPass:
				norm = 1 / (1 + K / Q + K * K);
				a0 = K * K * norm;
				a1 = 2 * a0;
				a2 = a0;
				b1 = 2 * (K * K - 1) * norm;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			case FilterType.HighPass:
				norm = 1 / (1 + K / Q + K * K);
				a0 = 1 * norm;
				a1 = -2 * a0;
				a2 = a0;
				b1 = 2 * (K * K - 1) * norm;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			case FilterType.BandPass:
				norm = 1 / (1 + K / Q + K * K);
				a0 = K / Q * norm;
				a1 = 0;
				a2 = -a0;
				b1 = 2 * (K * K - 1) * norm;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			case FilterType.Notch:
				norm = 1 / (1 + K / Q + K * K);
				a0 = (1 + K * K) * norm;
				a1 = 2 * (K * K - 1) * norm;
				a2 = a0;
				b1 = a1;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			case FilterType.Peak:
				if (PeakGain >= 0) {    // boost
					norm = 1 / (1 + 1 / Q * K + K * K);
					a0 = (1 + V / Q * K + K * K) * norm;
					a1 = 2 * (K * K - 1) * norm;
					a2 = (1 - V / Q * K + K * K) * norm;
					b1 = a1;
					b2 = (1 - 1 / Q * K + K * K) * norm;
				} else {    // cut
					norm = 1 / (1 + V / Q * K + K * K);
					a0 = (1 + 1 / Q * K + K * K) * norm;
					a1 = 2 * (K * K - 1) * norm;
					a2 = (1 - 1 / Q * K + K * K) * norm;
					b1 = a1;
					b2 = (1 - V / Q * K + K * K) * norm;
				}
				break;
			case FilterType.LowShelf:
				if (PeakGain >= 0) {    // boost
					norm = 1 / (1 + Math.Sqrt(2) * K + K * K);
					a0 = (1 + Math.Sqrt(2 * V) * K + V * K * K) * norm;
					a1 = 2 * (V * K * K - 1) * norm;
					a2 = (1 - Math.Sqrt(2 * V) * K + V * K * K) * norm;
					b1 = 2 * (K * K - 1) * norm;
					b2 = (1 - Math.Sqrt(2) * K + K * K) * norm;
				} else {    // cut
					norm = 1 / (1 + Math.Sqrt(2 * V) * K + V * K * K);
					a0 = (1 + Math.Sqrt(2) * K + K * K) * norm;
					a1 = 2 * (K * K - 1) * norm;
					a2 = (1 - Math.Sqrt(2) * K + K * K) * norm;
					b1 = 2 * (V * K * K - 1) * norm;
					b2 = (1 - Math.Sqrt(2 * V) * K + V * K * K) * norm;
				}
				break;
			case FilterType.HighShelf:
				if (PeakGain >= 0) {    // boost
					norm = 1 / (1 + Math.Sqrt(2) * K + K * K);
					a0 = (V + Math.Sqrt(2 * V) * K + K * K) * norm;
					a1 = 2 * (K * K - V) * norm;
					a2 = (V - Math.Sqrt(2 * V) * K + K * K) * norm;
					b1 = 2 * (K * K - 1) * norm;
					b2 = (1 - Math.Sqrt(2) * K + K * K) * norm;
				} else {    // cut
					norm = 1 / (V + Math.Sqrt(2 * V) * K + K * K);
					a0 = (1 + Math.Sqrt(2) * K + K * K) * norm;
					a1 = 2 * (K * K - 1) * norm;
					a2 = (1 - Math.Sqrt(2) * K + K * K) * norm;
					b1 = 2 * (K * K - V) * norm;
					b2 = (V - Math.Sqrt(2 * V) * K + K * K) * norm;
				}
				break;
			}
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			z1 = z2 = 0;
	
			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);

			Source.NoteOff(note, velocity);
		}


		public override double Render () {
			var input = Source.Render(renderFlag);
			var output = input * a0 + z1;

			z1 = input * a1 + z2 - b1 * output;
			z2 = input * a2 - b2 * output;

			return output;
		}
	}
}

