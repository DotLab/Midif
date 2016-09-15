namespace Midif.Synth {
	public interface ISynthesizer {
		void Init (double sampleRate);


		void NoteOn (byte note, byte velocity);

		void NoteOff (byte note, byte velocity);


		double Render ();
	}
}

