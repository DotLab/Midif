namespace Midif {
	public class AnalogInstrument : InstrumentBase {
		public WaveType waveType;
		public double frequency;

		public AnalogInstrument (Envelope envelope, WaveType waveType, double frequency) : base(envelope) {
			this.waveType = waveType;
			this.frequency = frequency;
		}

		public override double GetSample (int note, double time) {
			double frequencyFactor = WaveHelper.GetFrequencyFactor(note);
			return WaveHelper.GetWave(waveType, frequency * frequencyFactor, time);
		}
	}
}