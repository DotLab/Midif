namespace Midif {
	[System.Serializable]
	public class PmInstrument : EnvelopedInstrument {
		public WaveType cWaveType;
		public WaveType mWaveType;
		public double cFrequency;
		public double mFrequency;
		public double mIndex;
		
		public PmInstrument (IEnvelope envelope, WaveType cWaveType, double cFrequency, WaveType mWaveType, double mFrequency, double mIndex) : base(envelope) {
			this.cWaveType = cWaveType;
			this.mWaveType = mWaveType;
			this.cFrequency = cFrequency;
			this.mFrequency = mFrequency;
			this.mIndex = mIndex;
		}
		
		public override double GetRawSample (int note, double time) {
			double frequencyFactor = WaveHelper.GetFrequencyFactor(note);
			return WaveHelper.GetWave(
				cWaveType, 
				time * cFrequency * frequencyFactor + mIndex * WaveHelper.GetWave(mWaveType, time * mFrequency * frequencyFactor));
		}
	}
}