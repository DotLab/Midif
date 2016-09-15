namespace Midif.Synth {
	public abstract class MidiSynthesizer : ISynthesizer, IMidiEventHandler {
		protected double sampleRate;
		protected int polyphony;

		protected MidiChannel channelMask;


		public virtual void Init (double sampleRate, int polyphony) {
			this.sampleRate = sampleRate;
			this.polyphony = polyphony;
		}

		public virtual void SetChannelMask (MidiChannel channelMask) {
			this.channelMask = channelMask;
		}

		public virtual void Reset () {
		}


		public virtual void MidiEventHandler (MidiEvent midiEvent) {
			if ((channelMask & midiEvent.MidiChannel) == 0)
				return;

			switch (midiEvent.Type) {
			case MidiEventType.NoteOn:
				NoteOn(midiEvent.Channel, midiEvent.Note, midiEvent.Velocity);
				break;
			case MidiEventType.NoteOff:
				NoteOff(midiEvent.Channel, midiEvent.Note, midiEvent.Velocity);
				break;
			case MidiEventType.Aftertouch:
				Aftertouch(midiEvent.Channel, midiEvent.Note, midiEvent.Velocity);
				break;
			case MidiEventType.Controller:
				Controller(midiEvent.Channel, midiEvent.Controller, midiEvent.Value);
				break;
			case MidiEventType.ChannelAftertouch:
				ChannelAftertouch(midiEvent.Channel, midiEvent.Pressure);
				break;
			case MidiEventType.PitchBend:
				PitchBend(midiEvent.Channel, midiEvent.PitchBend);
				break;
			}
		}


		public abstract void NoteOn (byte channel, byte note, byte velocity);

		public abstract void NoteOff (byte channel, byte note, byte velocity);

		public virtual void Aftertouch (byte channel, byte note, byte velocity) {
		}

		public virtual void ChannelAftertouch (byte channel, byte pressure) {
		}

		public virtual void PitchBend (byte channel, int pitchBend) {
		}

		public virtual void Controller (byte channel, MidiControllerType controller, byte value) {
		}


		public abstract void Render (ref double sample);

		public virtual void Render (ref double sampleL, ref double sampleR) {
			double sample = 0;
			Render(ref sample);

			sampleL += sample;
			sampleR += sample;
		}
	}
}

