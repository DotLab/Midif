namespace Midif {
	public class FastInstrument : IInstrument {
		public double GetRawSample (int note, double time) {
			return WaveHelper.GetSquare(time * 440.0 * WaveHelper.GetFrequencyFactor(note));
		}

		public double GetSample (int note, double onTime) {
			return GetRawSample(note, onTime);
		}

		public double GetSample (int note, double onTime, double offTime) {
			return GetRawSample(note, onTime);
		}

		public bool IsEnded (double offTime) {
			return offTime > 0;
		}
	}
}