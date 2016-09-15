namespace Midif.Synth {
	public interface ISequencer {
		event MidiEventHandler OnProcessMidiEvent;

		event MetaEventHandler OnProcessMetaEvent;


		void Init (double sampleRate);

		void Reset ();


		void Advance (double sampleCount);

		bool IsFinished ();
	}
}

