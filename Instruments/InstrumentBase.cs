namespace Midif {
	public abstract class InstrumentBase : IInstrument {
		public readonly Envelope envelope;

		public InstrumentBase (Envelope envelope) {
			this.envelope = envelope;
		}

		public double GetEnvelopedSample (int note, double onTime, double offTime) {
			if (offTime <= 0) {
				return GetSample(note, onTime) * envelope.GetOnEnvelope(onTime);
			}

			return GetSample(note, onTime) * envelope.GetOffEnvelope(onTime, offTime);
		}

		public abstract double GetSample (int note, double time);
	
		public double GetReleaseTime () {
			return envelope.release;
		}
	}
}