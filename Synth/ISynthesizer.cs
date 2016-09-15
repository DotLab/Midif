namespace Midif.Synth {
	public interface ISynthesizer {
		void Init (double sampleRate, int polyphony);

		void Reset ();


		void NoteOn (byte channel, byte note, byte velocity);

		void NoteOff (byte channel, byte note, byte velocity);

		void Aftertouch (byte channel, byte note, byte velocity);

		void ChannelAftertouch (byte channel, byte pressure);

		void PitchBend (byte channel, int pitchBend);


		void Controller (byte channel, MidiControllerType controller, byte value);


		void Render (ref double sample);

		void Render (ref double sampleL, ref double sampleR);
	}
}

