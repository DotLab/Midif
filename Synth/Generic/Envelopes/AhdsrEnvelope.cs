namespace Midif.Synth {
	public class AhdsrEnvelope : AdsrEnvelope {
		public double Hold;

		protected int holdSample;


		public override void BuildLevels () {
			attackSample = (int)(Attack * sampleRate);
			holdSample = (int)(Hold * sampleRate);
			decaySample = (int)(Decay * sampleRate);
			releaseSample = (int)(Release * sampleRate);

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
