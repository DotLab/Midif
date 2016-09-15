namespace Midif {
	public abstract class EnvelopedInstrument : IInstrument {
		public IEnvelope Envelope;

		public EnvelopedInstrument (IEnvelope envelope) {
			Envelope = envelope;
		}

		public double GetSample (int note, double onTime) {
			return GetRawSample(note, onTime) * Envelope.GetEnvelope(onTime);
		}

		public double GetSample (int note, double onTime, double offTime) {
			return GetRawSample(note, onTime + offTime) * Envelope.GetEnvelope(onTime, offTime);
		}

		public bool IsEnded (double offTime) {
			return Envelope.IsEnded(offTime);
		}

		public abstract double GetRawSample (int note, double time);
	}
}