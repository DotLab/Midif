namespace Midif.Wave {
	public abstract class InfiniteWave : IWave {
		public abstract double GetWave (double onTime, double offTime, double frequency);

		public bool IsEnded (double offTime) {
			return offTime > 0;
		}
	}
}