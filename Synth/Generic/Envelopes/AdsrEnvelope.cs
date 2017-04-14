namespace Midif.Synth {
	public class AdsrEnvelope : MidiComponent {
		public MidiComponent Source;

		public double Attack;
		public double Decay;
		public double Sustain = 1;
		public double Release;

		protected int attackSample, decaySample, releaseSample;
		protected double decayDrop, releaseDrop;

		protected int onLength;
		int onCounter, offCounter = int.MaxValue;


		public sealed override void Init (double sampleRate) {
			SampleRate = sampleRate;
			SampleRateRecip = 1 / sampleRate;

			attackSample = (int)(Attack * SampleRate);
			decaySample = (int)(Decay * SampleRate);
			onLength = attackSample + decaySample;

			releaseSample = (int)(Release * SampleRate);

			decayDrop = 1 - Sustain;

			Source.Init(sampleRate);
		}

		public sealed override void NoteOn (byte note, byte velocity) {
			IsOn = true;

			onCounter = offCounter = 0;
		
			Source.NoteOn(note, velocity);
		}

		public sealed override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			if (onCounter >= onLength)
				releaseDrop = Sustain;
			else if (onCounter <= attackSample)
				releaseDrop = (double)onCounter / attackSample;
			else
				releaseDrop = 1 - decayDrop * (onCounter - attackSample) / decaySample;

			Source.NoteOff(note, velocity);
		}

		public sealed override bool IsFinished () {
			return offCounter >= releaseSample;
		}

		public sealed override double Render (bool flag) {
			return RenderCache;
		}

		public override void Process (float[] buffer) {
			if (!IsOn && offCounter >= releaseSample) {
				// Off and offed
				System.Array.Clear(buffer, 0, buffer.Length);
				return;
			}

			Source.Process(buffer);

			// On and oned
			if (IsOn && onCounter >= onLength) {
				for (int i = 0; i < buffer.Length; i++)
					buffer[i] *= (float)Sustain;
				return;
			}

			// On but not oned
			if (IsOn)
				for (int i = 0; i < buffer.Length; i++) {
					if (onCounter <= attackSample)
						buffer[i] *= (float)onCounter / attackSample;
					else
						buffer[i] *= (float)(1 - decayDrop * (onCounter - attackSample) / decaySample);

					++onCounter;

					if (onCounter >= onLength) {
						// Fast forward
						for (i = i + 1; i < buffer.Length; i++)
							buffer[i] *= (float)Sustain;
						return;
					}
				}
			else
				for (int i = 0; i < buffer.Length; i++) {
					if (offCounter >= releaseSample) {
						System.Array.Clear(buffer, i, buffer.Length - i);
						return;
					}

					buffer[i] *= (float)releaseDrop * (1 - (float)offCounter / releaseSample);

					++offCounter;
				}
		}
	}
}
