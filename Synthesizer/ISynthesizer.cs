namespace Midif.Synthesizer {
	public interface ISynthesizer {
		// Synthesizer should be fully functional after this function.
		void Init (int polyphony, double sampleRate);

		void SetChannelMask (MidiChannel channelMask);


		void NoteOn (byte channel, byte note, byte velocity);

		void NoteOff (byte channel, byte note, byte velocity);

		void Aftertouch (byte channel, byte note, byte velocity);

		void ChannelAftertouch (byte channel, byte pressure);

		void PitchBend (byte channel, int pitchBend);

		void Controller (byte channel, MidiControllerType controller, byte value);


		double RenderMono ();

		void RenderStereo (ref double sampleL, ref double sampleR);
	}
}