namespace Midif.V3 {
	public interface IMidiSynth {
		void NoteOff(int channel, byte note, byte velocity);
		void NoteOn(int channel, byte note, byte velocity);
		void Controller(int channel, byte controller, byte value);
		void ProgramChange(int channel, byte program);
		void PitchBend(int channel, byte lsb, byte msb);

		void Reset();
		void SetVolume(float volume);
		void Process(float[] buffer);
	}
}

