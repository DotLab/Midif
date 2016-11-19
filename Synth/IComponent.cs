namespace Midif.Synth {
	public interface IComponent {
		byte Note { get; }

		bool IsOn { get; }

		bool IsActive { get; }


		void Init (double sampleRate);

		void Reset ();


		void NoteOn (byte note, byte velocity);

		void NoteOff (byte note, byte velocity);


		double Render (bool flag);
	}
}