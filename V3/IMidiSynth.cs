namespace Midif.V3 {
	public interface IMidiSynth {
		void NoteOff(int track, byte channel, byte note, byte velocity);
		void NoteOn(int track, byte channel, byte note, byte velocity);
		void Controller(int track, byte channel, byte controller, byte value);
		void PitchBend(int track, byte channel, byte lsb, byte msb);

		void Reset();
		void SetVolume(float volume);
		void Process(float[] buffer);
	}
}

