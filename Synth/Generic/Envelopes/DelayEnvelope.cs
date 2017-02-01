namespace Midif.Synth {
	public sealed class DelayEnvelope : MidiComponent {
		public MidiComponent Source;

		public double Delay;

		int delaySample;
		int delayCounter;


		public override void Init (double sampleRate) {
			delaySample = (int)(Delay * sampleRate);

			Source.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;
		
			delayCounter = delaySample;
		
			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			Source.NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			return Source.IsOn || Source.IsFinished();
		}


		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;
		
				if (delayCounter > 0) {
					delayCounter--;
					return RenderCache = 0;
				}

				return RenderCache = Source.Render(flag);
			}

			return RenderCache;
		}
	}
}
