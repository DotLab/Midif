using System.Collections.Generic;

namespace Midif.Synth {
	public class MidiMix : ISynth {
		public readonly List<MidiSynth> Synths = new List<MidiSynth>();

		readonly List<MidiVoice> voices = new List<MidiVoice>();


		public void Init (double sampleRate) {
			voices.Clear();

			foreach (var synth in Synths)
				voices.AddRange(synth.Voices);

			foreach (var voice in voices)
				voice.Init(sampleRate);
		}

		public void Reset () {
			foreach (var voice in voices)
				voice.Reset();
		}


		public double RenderLeft (bool flag) {
			var sample = 0.0;

			foreach (var voice in voices)
				if (voice.LeftComponent.IsActive)
					sample += voice.RenderLeft(flag);

			return sample;
		}

		public double RenderRight (bool flag) {
			var sample = 0.0;

			foreach (var voice in voices)
				if (voice.LeftComponent.IsActive)
					sample += voice.RenderRight(flag);

			return sample;
		}


		public void MidiEventHandler (MidiEvent midiEvent) {
			foreach (var synth in Synths)
				synth.MidiEventHandler(midiEvent);
		}
	}
}