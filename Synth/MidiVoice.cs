namespace Midif.Synth {
	public delegate MidiVoice MidiVoiceBuilder ();

	public class MidiVoice : IVoice {
		public double LeftGain = 0.5, RightGain = 0.5;

		public readonly bool IsStereo;
		public readonly IComponent LeftComponent, RightComponent;


		public MidiVoice (IComponent component) {
			IsStereo = false;

			LeftComponent = component;
			RightComponent = component;
		}

		public MidiVoice (IComponent leftComponent, IComponent rightComponent) {
			IsStereo = true;

			LeftComponent = leftComponent;
			RightComponent = rightComponent;
		}


		public void Init (double sampleRate) {
			LeftComponent.Init(sampleRate);

			if (IsStereo)
				RightComponent.Init(sampleRate);
		}

		public void Reset () {
			LeftComponent.Reset();

			if (IsStereo)
				RightComponent.Reset();
		}

		public void NoteOn (byte note, byte velocity) {
			LeftComponent.NoteOn(note, velocity);

			if (IsStereo)
				RightComponent.NoteOn(note, velocity);
		}

		public void NoteOff (byte note, byte velocity) {
			LeftComponent.NoteOff(note, velocity);

			if (IsStereo)
				RightComponent.NoteOff(note, velocity);
		}


		public double RenderLeft (bool flag) {
			return LeftComponent.Render(flag) * LeftGain;
		}

		public double RenderRight (bool flag) {
			return RightComponent.Render(flag) * RightGain;
		}
	}
}