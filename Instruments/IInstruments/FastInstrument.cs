namespace Midif {
	public class FastInstrument : IInstrument {
		public double GetRawSample (int note, double time) {
			return 0;
		}

		public double GetSample (int note, double onTime) {
			return WaveHelper.GetSquare(onTime * 440.0 * WaveHelper.GetFrequencyFactor(note));
		}
		public double GetSample (int note, double onTime, double offTime) {
			return WaveHelper.GetSquare(onTime * 440.0 * WaveHelper.GetFrequencyFactor(note));
		}

		public bool IsEnded (double offTime) {
			return offTime > 0.1;
		}
	}
}