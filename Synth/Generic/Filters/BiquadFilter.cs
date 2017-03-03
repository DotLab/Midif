using System;

namespace Midif.Synth {
	public sealed class BiquadFilter : MidiComponent {
		public enum FilterType {
			LowPass,
			HighPass,
			BandPass,
			Notch,
			Peak,
			LowShelf,
			HighShelf,
		}

		public MidiComponent Source;

		public FilterType Type = FilterType.LowPass;
		public double Fc, Q = 0.707, PeakGain;

		double a0, a1, a2, b1, b2;
		double z1, z2;

		public override void Init (double sampleRate) {
			SampleRate = sampleRate;

			CalcCoeffs(sampleRate, Type, Fc, Q, PeakGain, out a0, out a1, out a2, out b1, out b2);

			Source.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
//			if (!IsOn) z1 = z2 = 0;

			IsOn = true;

			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			Source.NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			return Source.IsOn || Source.IsFinished();
		}


		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				var input = Source.Render(flag);
				var output = input * a0 + z1;

				z1 = input * a1 + z2 - b1 * output;
				z2 = input * a2 - b2 * output;

				return RenderCache = output;
			}

			return RenderCache;
		}


		public static void Process (
			double[] samples, double sampleRate, 
			FilterType type, double fc, double q = 0.7071, double peakGain = 0) {

			double a0, a1, a2, b1, b2;
			double z1 = 0, z2 = 0;

			CalcCoeffs(sampleRate, type, fc, q, peakGain, out a0, out a1, out a2, out b1, out b2);

			int length = samples.Length;
			for (int i = 0; i < length + length / 2; i++) {
				var input = samples[i % length];
				var output = input * a0 + z1;
				z1 = input * a1 + z2 - b1 * output;
				z2 = input * a2 - b2 * output;

				samples[i % length] = output;
			}
		}

		/// <summary>
		/// Calculates the coefficencies used by the filter.
		/// See http://www.earlevel.com/main/2012/11/26/biquad-c-source-code/
		/// </summary>
		public static void CalcCoeffs (
			double sampleRate, FilterType type, double fc, double q, double peakGain, 
			out double a0, out double a1, out double a2, out double b1, out double b2) {

			double norm;
			double v = Math.Pow(10, Math.Abs(peakGain) / 20.0);
			double k = Math.Tan(Math.PI * fc / sampleRate);

			switch (type) {
			case FilterType.LowPass:
				norm = 1 / (1 + k / q + k * k);
				a0 = k * k * norm;
				a1 = 2 * a0;
				a2 = a0;
				b1 = 2 * (k * k - 1) * norm;
				b2 = (1 - k / q + k * k) * norm;
				return;
			case FilterType.HighPass:
				norm = 1 / (1 + k / q + k * k);
				a0 = 1 * norm;
				a1 = -2 * a0;
				a2 = a0;
				b1 = 2 * (k * k - 1) * norm;
				b2 = (1 - k / q + k * k) * norm;
				return;
			case FilterType.BandPass:
				norm = 1 / (1 + k / q + k * k);
				a0 = k / q * norm;
				a1 = 0;
				a2 = -a0;
				b1 = 2 * (k * k - 1) * norm;
				b2 = (1 - k / q + k * k) * norm;
				return;
			case FilterType.Notch:
				norm = 1 / (1 + k / q + k * k);
				a0 = (1 + k * k) * norm;
				a1 = 2 * (k * k - 1) * norm;
				a2 = a0;
				b1 = a1;
				b2 = (1 - k / q + k * k) * norm;
				return;
			case FilterType.Peak:
				if (peakGain >= 0) {  // boost
					norm = 1 / (1 + 1 / q * k + k * k);
					a0 = (1 + v / q * k + k * k) * norm;
					a1 = 2 * (k * k - 1) * norm;
					a2 = (1 - v / q * k + k * k) * norm;
					b1 = a1;
					b2 = (1 - 1 / q * k + k * k) * norm;
				} else {  // cut
					norm = 1 / (1 + v / q * k + k * k);
					a0 = (1 + 1 / q * k + k * k) * norm;
					a1 = 2 * (k * k - 1) * norm;
					a2 = (1 - 1 / q * k + k * k) * norm;
					b1 = a1;
					b2 = (1 - v / q * k + k * k) * norm;
				}
				return;
			case FilterType.LowShelf:
				if (peakGain >= 0) {  // boost
					norm = 1 / (1 + Math.Sqrt(2) * k + k * k);
					a0 = (1 + Math.Sqrt(2 * v) * k + v * k * k) * norm;
					a1 = 2 * (v * k * k - 1) * norm;
					a2 = (1 - Math.Sqrt(2 * v) * k + v * k * k) * norm;
					b1 = 2 * (k * k - 1) * norm;
					b2 = (1 - Math.Sqrt(2) * k + k * k) * norm;
				} else {  // cut
					norm = 1 / (1 + Math.Sqrt(2 * v) * k + v * k * k);
					a0 = (1 + Math.Sqrt(2) * k + k * k) * norm;
					a1 = 2 * (k * k - 1) * norm;
					a2 = (1 - Math.Sqrt(2) * k + k * k) * norm;
					b1 = 2 * (v * k * k - 1) * norm;
					b2 = (1 - Math.Sqrt(2 * v) * k + v * k * k) * norm;
				}
				return;
			case FilterType.HighShelf:
				if (peakGain >= 0) {  // boost
					norm = 1 / (1 + Math.Sqrt(2) * k + k * k);
					a0 = (v + Math.Sqrt(2 * v) * k + k * k) * norm;
					a1 = 2 * (k * k - v) * norm;
					a2 = (v - Math.Sqrt(2 * v) * k + k * k) * norm;
					b1 = 2 * (k * k - 1) * norm;
					b2 = (1 - Math.Sqrt(2) * k + k * k) * norm;
				} else {  // cut
					norm = 1 / (v + Math.Sqrt(2 * v) * k + k * k);
					a0 = (1 + Math.Sqrt(2) * k + k * k) * norm;
					a1 = 2 * (k * k - 1) * norm;
					a2 = (1 - Math.Sqrt(2) * k + k * k) * norm;
					b1 = 2 * (k * k - v) * norm;
					b2 = (v - Math.Sqrt(2 * v) * k + k * k) * norm;
				}
				return;
			default:
				a0 = 0;
				a1 = 0;
				a2 = 0;
				b1 = 0;
				b2 = 0;
				return;
			}
		}
	}
}

