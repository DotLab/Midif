namespace Midif.Synth {
	public interface ISignalProvider {
		void Init (double sampleRate);


		void NoteOn (byte note, byte velocity);

		void NoteOff (byte velocity);

		// Will be called after NoteOff.
		bool IsActive ();


		// Will be called after !IsActive().
		// Flag is an alternative way of telling the components whether or not them are rending the last sample.
		// Flag will be fliped every sample.
		double Render (bool flag);
	}
}
