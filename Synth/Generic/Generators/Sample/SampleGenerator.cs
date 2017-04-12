namespace Midif.Synth {
	public abstract class SampleGenerator : MidiGenerator {
		public bool KeyTrack = true;
		public int KeyCenter = 60;

		public double Level;
		protected double gain;

		/// <summary>
		/// Define the start and end of the Samples, inclusive.
		/// </summary>
		public int Start, End;
		protected int duration;

		/// <summary>
		/// Input Samples' sample rate.
		/// </summary>
		public double Rate;
		public int Channels = 1;
		public double[] Samples;


		/// <summary>
		/// Resample the specified samples, from oldRate to newRate.
		/// See https://github.com/tng2903/CSUnitySynth/blob/master/Assets/Extensions/CSSynth/Wave/WaveHelper.cs
		/// </summary>
		/// <param name="samples">Samples.</param>
		/// <param name="sampleRate">Sample rate.</param>
		/// <param name="targetSampleRate">Target sample rate.</param>
		public static double[] Resample (double[] samples, int sampleRate, int targetSampleRate) {
			if (targetSampleRate == sampleRate)
				return samples;

			double max = 0;
			for (int i = 0; i < samples.Length; i++)
				if (samples[i] > max)
					System.Math.Abs(max = samples[i]);

			// Simple Decimation
			if (targetSampleRate < sampleRate && sampleRate % targetSampleRate == 0)
				samples = Decimation(samples, sampleRate, targetSampleRate);
			else if (sampleRate < targetSampleRate && targetSampleRate % sampleRate == 0)
				samples = Interpolation(samples, sampleRate, targetSampleRate);
			else {			// Interpolation then Decimation
				var gcd = Mathf.Gcd(sampleRate, targetSampleRate);
				var tempSampleRate = sampleRate * (targetSampleRate / gcd);
				samples = Interpolation(samples, sampleRate, tempSampleRate);
				samples = Decimation(samples, tempSampleRate, targetSampleRate);
			}

			double newMax = 0;
			for (int i = 0; i < samples.Length; i++)
				if (samples[i] > newMax)
					System.Math.Abs(newMax = samples[i]);

			double ratio = max / newMax;
			for (int i = 0; i < samples.Length; i++)
				samples[i] *= ratio;

			return samples;
		}

		/// <summary>
		/// Decimation.
		/// </summary>
		/// <param name="samples">Samples.</param>
		/// <param name="sampleRate">Sample rate.</param>
		/// <param name="targetSampleRate">Target sample rate.</param>
		static double[] Decimation (double[] samples, int sampleRate, int targetSampleRate) {
			if (sampleRate == targetSampleRate)
				return samples;

			// Filter
			BiquadFilter.Process(
				samples, sampleRate, 
				BiquadFilter.FilterType.LowPass, targetSampleRate / 4, 1);

			// Downsample
			var ratio = (double)targetSampleRate / sampleRate;
			int newLength = (int)(samples.Length * ratio + 0.5);
			var newSamples = new double[newLength];

			ratio = (double)sampleRate / targetSampleRate;
			for (int i = 0; i < newLength; i++)
				newSamples[i] = samples[(int)(i * ratio + 0.5)];

			return newSamples;
		}

		/// <summary>
		/// Interpolation.
		/// </summary>
		/// <param name="samples">Samples.</param>
		/// <param name="sampleRate">Sample rate.</param>
		/// <param name="targetSampleRate">Target sample rate.</param>
		static double[] Interpolation (double[] samples, int sampleRate, int targetSampleRate) {
			if (sampleRate == targetSampleRate)
				return samples;

			// Upsample
			var ratio = targetSampleRate / sampleRate;
			int oldLength = samples.Length;
			var newSamples = new double[oldLength * ratio];

			// Since the array is already all zero, just need to inseart the sample.
			for (int i = 0; i < oldLength; i++)
				newSamples[i * ratio] = samples[i];

			// Filter
			BiquadFilter.Process(
				newSamples, targetSampleRate, 
				BiquadFilter.FilterType.LowPass, sampleRate / 4, 1);

			return newSamples;
		}
	}
}