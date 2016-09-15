namespace Midif {
	[System.Serializable]
	public class AnalogInstrument : EnvelopedInstrument {
		public WaveType waveType;
		public double frequency;

		public AnalogInstrument (IEnvelope envelope, WaveType waveType, double frequency) : base(envelope) {
			this.waveType = waveType;
			this.frequency = frequency;
		}

		public override double GetRawSample (int note, double time) {
			double frequencyFactor = WaveHelper.GetFrequencyFactor(note);
			return WaveHelper.GetWave(waveType, time * frequency * frequencyFactor);
		}
	}
}