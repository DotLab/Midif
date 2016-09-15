namespace Midif.Wave {
	public class AsWave : IWave {
		public IWave Wave;

		public double Scale;
		public double Shift;

		public AsWave (IWave wave, double scale, double shift) {
			Wave = wave;
			Scale = scale;
			Shift = shift;
		}

		public double GetWave (double onTime, double offTime, double frequency) {
			return Wave.GetWave(onTime, offTime, frequency) * Scale + Shift;
		}

		public bool IsEnded (double offTime) {
			return Wave.IsEnded(offTime);
		}
	}
}