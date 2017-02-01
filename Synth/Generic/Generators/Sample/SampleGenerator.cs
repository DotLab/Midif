namespace Midif.Synth {
	public abstract class SampleGenerator : MidiGenerator {
		public bool KeyTrack = true;
		public int KeyCenter = 60;

		public double Level;
		protected double gain;

		public int Start;
		public int End;
		protected int duration;

		public double[] Samples;
		public double Rate;

		/// <summary>
		/// Resample the specified samples, from oldRate to newRate.
		/// See https://github.com/tng2903/CSUnitySynth/blob/master/Assets/Extensions/CSSynth/Wave/WaveHelper.cs
		/// </summary>
		/// <param name="samples">Samples.</param>
		/// <param name="oldRate">Old sample rate.</param>
		/// <param name="newRate">New sample rate.</param>
		public static double[] Resample (double[] samples, int oldRate, int newRate) {
			if (newRate == oldRate) return samples;

			int a = newRate, b = oldRate, r;
			// Find the biggest factor between the rates
			while (b != 0) {
				r = a % b;
				a = b;
				b = r;
			}
			oldRate = oldRate / a;
			newRate = newRate / a;

			if (newRate < oldRate) { 
				// Downsample
				if (oldRate % newRate == 0) { 
					// Simple Downsample
					BiquadFilter.Process(
						samples, newRate * a, 
						BiquadFilter.FilterType.LowPass, (newRate * a) / 2.0, 1);
					samples = Downsample(samples, oldRate / newRate);
				} else { 
					// Upsample then Downsample
					samples = Upsample(samples, newRate);
					// Filter
					BiquadFilter.Process(
						samples, newRate * a, 
						BiquadFilter.FilterType.LowPass, (newRate * a) / 2.0, 1);
					// Downsample
					samples = Downsample(samples, oldRate);
				}
			} else if (newRate > oldRate) { 
				// Upsample
				if (newRate % oldRate == 0) { 
					// Simple Upsample
					samples = Upsample(samples, newRate / oldRate);
					BiquadFilter.Process(
						samples, newRate * a, 
						BiquadFilter.FilterType.LowPass, (oldRate * a) / 2.0, 1);
				} else { 
					// Upsample then Downsample
					samples = Upsample(samples, newRate);
					// Filter
					BiquadFilter.Process(
						samples, newRate * a, 
						BiquadFilter.FilterType.LowPass, (oldRate * a) / 2.0, 1);
					// Downsample
					samples = Downsample(samples, oldRate);
				}
			}

			return samples;
		}

		/// <summary>
		/// Downsample by skipping samples.
		/// </summary>
		/// <param name="samples">Samples.</param>
		/// <param name="factor">Factor.</param>
		static double[] Downsample (double[] samples, int factor) {
			if (factor == 1) return samples;

			int newLength = (int)(samples.Length * (1.0 / factor));
			var newSamples = new double[newLength];

			for (int i = 0; i < newLength; i++)
				newSamples[i] = samples[i * factor];

			return newSamples;
		}

		/// <summary>
		/// Upsample by padding zeros.
		/// Need low pass filtering.
		/// </summary>
		/// <param name="samples">Samples.</param>
		/// <param name="factor">Factor.</param>
		static double[] Upsample (double[] samples, int factor) {
			if (factor == 1) return samples;

			int oldLength = samples.Length;
			var newSamples = new double[oldLength * factor];

			// Since the array is already all zero, just need to inseart the sample.
			for (int i = 0; i < oldLength; i++)
				newSamples[i * factor] = samples[i];

			return newSamples;
		}
	}
}