namespace Midif.Wave {
	public class PulseWave : InfiniteWave {
		public IWave DutyCycle;

		public PulseWave (IWave dutyCycle) {
			DutyCycle = dutyCycle;
		}

		public override double GetWave (double onTime, double offTime, double frequency) {
			var time = (onTime + offTime) * frequency;
			time = time - (int)time;
			return time < DutyCycle.GetWave(onTime, offTime, frequency) ? 1 : -1;
		}
	}
}