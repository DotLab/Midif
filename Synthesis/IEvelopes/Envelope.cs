namespace Midif {
	public class Envelope : IEnvelope {
		public double Delay;
		public double Attack;
		public double Hold;
		public double Decay;
		public double Sustain;
		public double Release;

		public Envelope (double delay, double attack, double hold, double decay, double sustain, double release) {
			Delay = delay;
			Attack = attack;
			Hold = hold;
			Decay = decay;
			Sustain = sustain;
			Release = release;
		}

		public double GetEnvelope (double onTime) {
			if (onTime < Delay) {
				return 0;
			}

			onTime -= Delay;
			if (onTime < Attack) {
				return onTime / Attack;
			}

			onTime -= Attack;
			if (onTime < Hold) {
				return 1;
			}

			onTime -= Hold;
			if (onTime < Decay) {
				return (1 - onTime / Decay) * (1 - Sustain) + Sustain;
			}

			return Sustain;
		}

		public double GetEnvelope (double onTime, double offTime) {
			return offTime < Release ? (1 - offTime / Release) * GetEnvelope(onTime) : 0;
		}
	}
}