using System;

namespace Midif.Synth {
	public sealed class OnePoleFilter : MidiComponent {
		public enum FilterType {
			LowPass,
			HighPass,
		}

		public MidiComponent Source;

		public FilterType Type = FilterType.LowPass;
		public double Fc;

		double a0, b1;
		double z1;

		public override void Init (double sampleRate) {
			SampleRate = sampleRate;

			CalcCoeffs(sampleRate, Type, Fc, out a0, out b1);
		
			Source.Init(sampleRate);
		}

		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) z1 = 0;

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
				return RenderCache = z1 = Source.Render(flag) * a0 + z1 * b1;
			}

			return RenderCache;
		}


		public static void Process (
			double[] samples, double sampleRate, 
			FilterType type, double fc) {

			double a0, b1;
			double z1 = 0;

			CalcCoeffs(sampleRate, type, fc, out a0, out b1);

			int length = samples.Length;
			for (int i = 0; i < length; i++)
				samples[i] = z1 = samples[i] * a0 + z1 * b1;
		}

		/// <summary>
		/// Calculates the coefficencies used by the filter.
		/// See http://www.earlevel.com/main/2012/12/15/a-one-pole-filter/
		/// </summary>
		public static void CalcCoeffs (double sampleRate, FilterType type, double fc, out double a0, out double b1) {
			switch (type) {
			case FilterType.LowPass:
				b1 = Math.Exp(-2 * Math.PI * fc / sampleRate);
				a0 = 1.0 - b1;
				return;
			case FilterType.HighPass:
				b1 = -Math.Exp(-2 * Math.PI * (0.5 - fc / sampleRate));
				a0 = 1.0 + b1;
				return;
			default:
				a0 = 0;
				b1 = 0;
				return;
			}
		}
	}
}
	