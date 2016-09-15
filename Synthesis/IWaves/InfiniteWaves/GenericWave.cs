namespace Midif.Wave {
	public abstract class GenericWave : InfiniteWave {
		public override double GetWave (double onTime, double offTime, double frequency) {
			return GetWave((onTime + offTime) * frequency);
		}

		public abstract double GetWave (double time);
	}
}