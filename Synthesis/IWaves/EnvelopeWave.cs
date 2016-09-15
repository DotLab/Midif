namespace Midif.Wave {
	public class EnvelopeWave : IWave {
		public double Delay;
		public double Attack;
		public double Hold;
		public double Decay;
		public double Sustain;
		public double Release;

		public EnvelopeWave (double delay, double attack, double hold, double decay, double sustain, double release) {
			Delay = delay;
			Attack = attack;
			Hold = hold;
			Decay = decay;
			Sustain = sustain;
			Release = release;
		}

		public double GetWave (double onTime, double offTime, double frequency) {
			if (onTime < Delay || offTime > Release) return 0;

			if (offTime > 0) {
				onTime -= Delay;
				if (onTime < Attack) return onTime / Attack * (1 - offTime / Release);
				onTime -= Attack;
				if (onTime < Hold) return 1 * (1 - offTime / Release);
				onTime -= Hold;
				if (onTime < Decay) return (1 - onTime / Decay * (1 - Sustain)) * (1 - offTime / Release);
				return Sustain * (1 - offTime / Release);
			} else {
				onTime -= Delay;
				if (onTime < Attack) return onTime / Attack;
				onTime -= Attack;
				if (onTime < Hold) return 1;
				onTime -= Hold;
				if (onTime < Decay) return 1 - onTime / Decay * (1 - Sustain);
				return Sustain;
			}
		}

		public bool IsEnded (double offTime) {
			return offTime > Release;
		}
	}
}