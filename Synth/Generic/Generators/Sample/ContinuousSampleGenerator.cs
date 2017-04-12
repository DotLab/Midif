namespace Midif.Synth {
	public sealed class ContinuousSampleGenerator : SampleGenerator {
		/// <summary>
		/// The start of the loop.
		/// </summary>
		public int LoopStart;
		/// <summary>
		/// The end of the loop, will be played.
		/// </summary>
		public int LoopEnd;
		int loopDuration;

		/// <summary>
		/// Loop until Release and then play through to the end point or until amplitude EG reaching zero.
		/// </summary>
		public bool UseSustain;


		public override void Init (double sampleRate) {
			SampleRateRecip = 1 / sampleRate;

			gain = SynthTable.Deci2Gain(Level);
			duration = End - Start;
			loopDuration = LoopEnd - LoopStart;
		}


		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;

			phaseStep = SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * Rate * SampleRateRecip;
			if (KeyTrack) phaseStep *= SynthTable.Semi2Pitc[note - KeyCenter + Transpose + SynthTable.Semi2PitcShif];
			phase = Start;
		}

		public override bool IsFinished () {
			return !UseSustain || phase > End;
		}


		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				phase += phaseStep;

				// Sustain over;
				if (UseSustain && phase > End)
					return RenderCache = 0;

				// If IsOn or UseSustain, loop again;
				if ((IsOn || !UseSustain) && phase > LoopEnd)
					phase = LoopStart + ((phase - LoopStart) % loopDuration);

				return RenderCache = Samples[(int)(phase) * Channels] * gain;
			}

			return RenderCache;
		}
	}
}