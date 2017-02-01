namespace Midif.Synth {
	public sealed class ReleaseEnvelope : MidiComponent {
		public MidiComponent Source;

		public double Release;

		int releaseSample, releaseCounter = -1;
		double releaseSampleRecip;


		public override void Init (double sampleRate) {
			releaseSample = (int)(Release * sampleRate);
			releaseSampleRecip = 1.0 / releaseSample;

			Source.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;

			releaseCounter = releaseSample;

			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			Source.NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			return releaseCounter <= 0;
		}


		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				if (IsOn)
					return RenderCache = Source.Render(flag);

				if (releaseCounter <= 0)
					return RenderCache = 0;
				
				return RenderCache = Source.Render(flag) * (releaseCounter-- * releaseSampleRecip);
			}

			return RenderCache;
		}
	}
}
