namespace Midif {
	public class Envelope {
		public readonly double delay;
		public readonly double attack;
		public readonly double hold;
		public readonly double decay;
		public readonly double sustain;
		public readonly double release;

		public Envelope (double delay, double attack, double hold, double decay, double sustain, double release) {
			this.delay = delay;
			this.attack = attack;
			this.hold = hold;
			this.decay = decay;
			this.sustain = sustain;
			this.release = release;
		}

		public double GetOnEnvelope (double time) {
			if (time < delay) {
				return 0;
			}

			time -= delay;
			if (time < attack) {
				return time / attack;
			}

			time -= attack;
			if (time < hold) {
				return 1;
			}

			time -= hold;
			if (time < decay) {
				return (1 - time / decay) * (1 - sustain) + sustain;
			}

			return sustain;
		}

		public double GetOffEnvelope (double onTime, double offTime) {
			if (offTime < release) {
				return (1 - offTime / release) * GetOnEnvelope(onTime);
			}

			return 0;
		}
	}
}