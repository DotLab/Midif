namespace Midif.Synth {
	public class DelayEnvelope : BaseSignalProvider {
		public ISignalProvider Source;

		public double Delay;

		public override bool IsActive { get { return isOn || Source.IsActive; } }

		protected int delaySample;
		int delayCounter;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);
		
			delaySample = (int)(Delay * sampleRate);

			Source.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
		
			delayCounter = delaySample;
		
			Source.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);

			Source.NoteOff(note, velocity);
		}


		public override double Render () {
			// if is faster by 2 ticks
			if (delayCounter < 0)
				return Source.Render();
			delayCounter--;
			return 0;
//			return delayCounter-- > 0 ? 0 : Source.Render();
		}
	}
}
