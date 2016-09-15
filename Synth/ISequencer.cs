namespace Midif.Synth {
	public interface ISequencer {
		event MidiEventHandler OnProcessMidiEvent;

		event MetaEventHandler OnProcessMetaEvent;


		/// <summary>
		/// Init the SignalProvider with a sampleRate.
		/// Set fields before calling this.
		/// </summary>
		void Init (double sampleRate);

		void Reset ();


		void AdvanceSamples (double samples);

		void AdvanceSeconds (double seconds);

		void AdvanceTicks (double ticks);


		bool IsFinished ();
	}
}

