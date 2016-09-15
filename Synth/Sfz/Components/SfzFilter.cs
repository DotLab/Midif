using System;

using Midif.File.Sfz;

namespace Midif.Synth.Sfz {
	public class SfzFilter : SfzComponent {
		public ISignalProvider Source;

		public SfzFilterType FilterType = SfzFilterType.BiquadLowPass;
		public double CutOff;
		public double Resonance = 0.7;

		double sampleRateRecip;
		double baseFc, fc;

		double a0, a1, a2, b1, b2;
		double z1, z2;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);
			Source.Init(sampleRate);

			sampleRateRecip = 1 / sampleRate;
			fc = baseFc = CutOff;

			UpdateCoefficients();
		}

		void UpdateCoefficients () {
			double norm, K, Q = Resonance;

			switch (FilterType) {
			case SfzFilterType.OnePoleLowPass:
				b1 = Math.Exp(-2 * Math.PI * fc * sampleRateRecip);
				a0 = 1.0 - b1;
				break;
			case SfzFilterType.OnePoleHighPass:
				b1 = -Math.Exp(-2 * Math.PI * (0.5 - fc * sampleRateRecip));
				a0 = 1.0 + b1;
				break;
			case SfzFilterType.BiquadLowPass:
				K = Math.Tan(Math.PI * fc * sampleRateRecip);
				norm = 1 / (1 + K / Q + K * K);
				a0 = K * K * norm;
				a1 = 2 * a0;
				a2 = a0;
				b1 = 2 * (K * K - 1) * norm;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			case SfzFilterType.BiquadHighPass:
				K = Math.Tan(Math.PI * fc * sampleRateRecip);
				norm = 1 / (1 + K / Q + K * K);
				a0 = 1 * norm;
				a1 = -2 * a0;
				a2 = a0;
				b1 = 2 * (K * K - 1) * norm;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			case SfzFilterType.BiquadBandPass:
				K = Math.Tan(Math.PI * fc * sampleRateRecip);
				norm = 1 / (1 + K / Q + K * K);
				a0 = K / Q * norm;
				a1 = 0;
				a2 = -a0;
				b1 = 2 * (K * K - 1) * norm;
				b2 = (1 - K / Q + K * K) * norm;
				break;
			}
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
			Source.NoteOn(note, velocity);
			
			if (keyTrackEnabled || velTrackEnabled) {
				baseFc = CutOff;
				if (keyTrackEnabled)
					baseFc *= SynthConstants.Cents2Pitch((int)(keyTrackDepth));
				if (velTrackEnabled)
					baseFc *= SynthConstants.Cents2Pitch((int)(velTrackDepth));
				fc = baseFc;
				UpdateCoefficients();
			}

			z1 = z2 = 0;
		}

		public override void NoteOff (byte velocity) {
			base.NoteOff(velocity);
			Source.NoteOff(velocity);
		}

		public override bool IsActive () {
			return Source.IsActive();
		}

		public override double Render () {
			var input = Source.Render(flag);

			if (egEnabled || lfoEnabled) {
				fc = baseFc;
				if (egEnabled)
					fc *= SynthConstants.Cents2Pitch((int)(Eg.Render(flag) * egTotalDepth));
				if (lfoEnabled)
					fc *= SynthConstants.Cents2Pitch((int)(Lfo.Render(flag) * lfoTotalDepth));
				UpdateCoefficients();
			}

			switch (FilterType) {
			case SfzFilterType.OnePoleLowPass:
			case SfzFilterType.OnePoleHighPass:
				return z1 = input * a0 + z1 * b1;
			case SfzFilterType.BiquadLowPass:
			case SfzFilterType.BiquadHighPass:
			case SfzFilterType.BiquadBandPass:
				var output = input * a0 + z1;
				z1 = input * a1 + z2 - b1 * output;
				z2 = input * a2 - b2 * output;
				return output;
			default:
				return input;
			}
		}
	}
}

