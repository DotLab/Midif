using Midif.File;

namespace Midif.Synthesizer {
	public interface ISynthesizer {
		void MidiEventHandler (MidiEvent midiEvent);
	}

	public abstract class Synthesizer : ISynthesizer {
		public readonly double SampleRate;

		protected Synthesizer (double sampleRate) {
			SampleRate = sampleRate;
		}

		public abstract void MidiEventHandler (MidiEvent midiEvent);
	}
}