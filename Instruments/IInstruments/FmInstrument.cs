namespace Midif {
	public class FmInstrument : EnvelopedInstrument {
		public readonly WaveType cWaveType;
		public readonly WaveType mWaveType;
		public readonly double cFrequency;
		public readonly double mFrequency;
		public readonly double mIndex;
		
		public FmInstrument (IEnvelope envelope, WaveType cWaveType, double cFrequency, WaveType mWaveType, double mFrequency, double mIndex) : base(envelope) {
			this.cWaveType = cWaveType;
			this.mWaveType = mWaveType;
			this.cFrequency = cFrequency;
			this.mFrequency = mFrequency;
			this.mIndex = mIndex / (2.0 * System.Math.PI);
		}
		
		public override double GetRawSample (int note, double time) {
			double frequencyFactor = WaveHelper.GetFrequencyFactor(note);
			return WaveHelper.GetWave(
				cWaveType, 
				cFrequency * frequencyFactor,
				time + mIndex / (cFrequency * frequencyFactor) * WaveHelper.GetWave(mWaveType, mFrequency * frequencyFactor, time));
		}
	}
}