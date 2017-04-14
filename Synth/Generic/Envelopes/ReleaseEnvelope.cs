namespace Midif.Synth {
	public sealed class ReleaseEnvelope : MidiComponent {
		public MidiComponent Source;

		public double Release;

		int releaseSample, releaseCounter = -1;


		public override void Init (double sampleRate) {
			releaseSample = (int)(Release * sampleRate);

			Source.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;

			releaseCounter = releaseSample;

			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			releaseCounter = releaseSample;

			Source.NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			return releaseCounter <= 0;
		}


		public override double Render (bool flag) {

			return RenderCache;
		}

		public override void Process (float[] buffer) {
			if (!IsOn && releaseCounter <= 0) {
				// Off and released
				System.Array.Clear(buffer, 0, buffer.Length);
				return;
			}

			Source.Process(buffer);

			// On
			if (IsOn) return;

			// Off but not released
			for (int i = 0; i < buffer.Length; i++) {
				if (releaseCounter <= 0) {
					System.Array.Clear(buffer, i, buffer.Length - i);
					return;
				}
					
				buffer[i] *= (float)releaseCounter / releaseSample;

				--releaseCounter;
			}
		}
	}
}
