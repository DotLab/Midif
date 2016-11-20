namespace Midif.Synth {
	public class KeymapMixer : MidiComponent {
		public override bool IsActive {
			get {
				return isOn || (!disabled && Sources[note].IsActive);
			}
		}

		public readonly IComponent[] Sources = new IComponent[0x80];

		bool disabled = true;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			foreach (var source in Sources)
				if (source != null)
					source.Init(sampleRate);
		}


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
		
			disabled = Sources[note] == null;

			if (!disabled)
				Sources[note].NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);
		
			if (!disabled)
				Sources[note].NoteOff(note, velocity);
		}


		public override double Render () {
			return disabled ? 0 : Sources[note].Render(renderFlag);
		}
	}
}