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
			return GetRawSample(note, onTime) * Envelope.GetEnvelope(onTime, offTime);
		}

		public abstract double GetRawSample (int note, double time);
	}
}