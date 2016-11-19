namespace Midif.Synth {
	public interface IVoice {
		void Init (double sampleRate);

		void Reset ();


		void NoteOn (byte note, byte velocity);

		void NoteOff (byte note, byte velocity);


		double RenderLeft (bool flag);

		double RenderRight (bool flag);
	}
}
