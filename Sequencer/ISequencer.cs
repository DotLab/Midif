namespace Midif.Sequencer {
	public interface ISequencer {
		void Init (double sampleRate);

		void Reset ();

		void Advance (double sampleCount);

		bool IsFinished ();
	}
}

