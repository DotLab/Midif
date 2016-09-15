namespace Midif.Wave {
	public class ConstantWave : InfiniteWave {
		public double Value;

		public ConstantWave (double value) {
			Value = value;
		}

		public override double GetWave (double onTime, double offTime, double frequency) {
			return Value;
		}
	}
}