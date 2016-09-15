namespace Midif.Wave {
	public class FmWave : IWave {
		public IWave Carrier;
		public IWave Modulator;
		public IWave ModulationIndex;

		public FmWave (IWave carrier, IWave modulator, IWave modulationIndex) {
			Carrier = carrier;
			Modulator = modulator;
			ModulationIndex = modulationIndex;
		}

		public double GetWave (double onTime, double offTime, double frequency) {
			return Carrier.GetWave(
				onTime,
				offTime,
				frequency + Modulator.GetWave(onTime, offTime, frequency) * ModulationIndex.GetWave(onTime, offTime, frequency));
		}

		public bool IsEnded (double offTime) {
			return Carrier.IsEnded(offTime) && Modulator.IsEnded(offTime) && ModulationIndex.IsEnded(offTime);
		}
	}
}