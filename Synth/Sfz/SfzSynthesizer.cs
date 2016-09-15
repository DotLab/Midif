using System;

using Midif.File.Sfz;

namespace Midif.Synth.Sfz {
	[Serializable]
	public class SfzSynthesizer : MidiSynthesizer {
		const double PerVoiceLevel = 0.125 / 4;

		public SfzFile file;

		public SfzVoice[] voices;

		public SfzSynthesizer (SfzFile file) {
			this.file = file;
		}

		public override void Init (double sampleRate, int polyphony) {
			base.Init(sampleRate, polyphony);

			voices = new SfzVoice[polyphony];
			for (int i = 0; i < polyphony; i++) {
				voices[i] = new SfzVoice(file);
				voices[i].Init(sampleRate);
			}
		}

		public override void Reset () {
			foreach (var voice in voices)
				voice.NoteOff(0);
		}

		public override void NoteOn (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (!voice.IsActive()) {
					voice.NoteOn(note, velocity);
					break;
				}

			foreach (var voice in voices)
				if (!voice.IsOn) {
					voice.NoteOn(note, velocity);
					break;
				}
		}

		public override void NoteOff (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.IsActive() && voice.Note == note) {
					voice.NoteOff(velocity);
//					break;
				}
		}

		public override void Render (ref double sample) {
			foreach (var voice in voices)
				if (voice.IsActive())
					sample += voice.Render();
		}
	}
}