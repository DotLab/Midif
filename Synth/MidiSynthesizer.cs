namespace Midif.Synth {
	public abstract class MidiSynthesizer : ISynthesizer, IMidiEventHandler {
		protected static double cache;

		protected double sampleRate;


		public virtual void Init (double sampleRate) {
			this.sampleRate = sampleRate;
		}

		public virtual void Reset () {
			Init(sampleRate);
		}


		public virtual void NoteOn (byte note, byte velocity) {
		}

		public virtual void NoteOff (byte note, byte velocity) {
		}


		public abstract double Render ();


		public MidiChannel Channel = MidiChannel.All;


		public virtual void MidiEventHandler (MidiEvent midiEvent) {
			if (((int)Channel >> midiEvent.Channel & 0x01) == 0x01)
				switch (midiEvent.Type) {
				case MidiEventType.NoteOn:
					NoteOn(midiEvent.Note, midiEvent.Velocity);
					break;
				case MidiEventType.NoteOff:
					NoteOff(midiEvent.Note, midiEvent.Velocity);
					break;
				}
		}
	}
}

