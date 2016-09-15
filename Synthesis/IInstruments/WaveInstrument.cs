namespace Midif {
	public class WaveInstrument : IInstrument {
		public IWave Wave;

		public WaveInstrument (IWave wave) {
			Wave = wave;
		}

		public double GetRawSample (int note, double time) {
			return Wave.GetWave(time, 0, 440.0 * WaveHelper.GetFrequencyFactor(note));
		}

		public double GetSample (int note, double onTime) {
			return Wave.GetWave(onTime, 0, 440.0 * WaveHelper.GetFrequencyFactor(note));
		}

		public double GetSample (int note, double onTime, double offTime) {
			return Wave.GetWave(onTime, offTime, 440.0 * WaveHelper.GetFrequencyFactor(note));
		}

		public bool IsEnded (double offTime) {
			return Wave.IsEnded(offTime);
		}
	}
}