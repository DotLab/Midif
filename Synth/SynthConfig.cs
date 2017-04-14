namespace Midif.Synth {
	static class SynthConfig {
		public static double SampleRate;
		public static double SampleRateRecip;

		// 16 KB L1 2MB L2 Cache
		// 2048 * 4 Bytes = 8 KB / Buffer
		// Must use float for buffers.
		public static int BufferSize;

		public static void Init (double sampleRate, int bufferSize) {
			SampleRate = sampleRate;
			SampleRateRecip = 1.0 / sampleRate;

			BufferSize = bufferSize;
		}
	}
}
