namespace Midif {
	public interface ISynthesizer {
		uint CurrentSample { get; }
		double[] Buffer { get; }

		void SetSampleRate (uint sampleRate);
		void UpdateBuffer (int sampleCount);
	}
}