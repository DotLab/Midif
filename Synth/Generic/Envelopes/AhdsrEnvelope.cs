namespace Midif.Synth {
	public sealed class AhdsrEnvelope : AdsrEnvelope {
		public double Hold;

		int holdSample;


		public override void BuildLevels () {
			attackSample = (int)(Attack * SampleRate);
			holdSample = (int)(Hold * SampleRate);
			decaySample = (int)(Decay * SampleRate);
			releaseSample = (int)(Release * SampleRate);

			decayDrop = 1 - Sustain;
			releaseDrop = Sustain;

			onLevels = new double[attackSample + holdSample + decaySample];
			offLevels = new double[releaseSample];

			for (int i = 0; i < onLevels.Length; i++)
				onLevels[i] = 
					i < attackSample ? (double)i / attackSample :
					i < (attackSample + holdSample) ? 1 :
					1 - decayDrop * (i - holdSample - attackSample) / decaySample;
			for (int i = 0; i < offLevels.Length; i++)
				offLevels[i] = 1 - (double)i / releaseSample;

			onLength = onLevels.Length - 1;
			offLength = offLevels.Length - 1;
		}
	}
}
