namespace Midif {
	public class MidiNoteSynthesizer : ISynthesizer {
		public const int MaxPolyphony = 8;
		public const double Gain = 0.1;

		public MidiNoteSequence Sequence;
		public IInstrumentBank Bank;

		uint currentSample;
		double[] buffer = new double[0];

		MidiNoteVoice[] voicePool;

		public uint CurrentSample {
			get { return currentSample; }
		}
		public double[] Buffer {
			get { return buffer; }
		}

		public MidiNoteSynthesizer (IInstrumentBank bank, MidiNoteSequence sequence) {
			Bank = bank;
			Sequence = sequence;
		}

		public void SetSampleRate (uint sampleRate) {
			Sequence.SetSampleRate(sampleRate);

			voicePool = new MidiNoteVoice[MaxPolyphony];
			for (int i = 0; i < MaxPolyphony; i++) {
				voicePool[i] = new MidiNoteVoice(sampleRate);
			}
		}

		public void UpdateBuffer (int sampleCount) {
			if (currentSample > Sequence.SampleLength) {
				return;
			}

			if (buffer.Length != sampleCount) {
				UiConsole.Log(sampleCount);
				UiConsole.Log();
				buffer = new double[sampleCount];
			}

			for (int i = 0; i < sampleCount; i++) {
				buffer[i] = GetSample();

				currentSample ++;
			}

			foreach (var voice in voicePool) {
				voice.TryEnd();
			}
		}

		double GetSample () {
			if (Sequence.HasMidiNoteAtSample(currentSample)) {
				MidiNote[] midiNotes = Sequence.GetMidiNotesAtSample(currentSample);
				foreach (var midiNote in midiNotes) {
					StartVoice(midiNote);
				}
			}

			double sample = 0;
			foreach (var voice in voicePool) {
				if (voice.Active) {
					sample += voice.Update(currentSample);
				}
			}
			return sample * Gain;
		}

		void StartVoice (MidiNote midiNote) {
			foreach (var voice in voicePool) {
				if (voice.Active) {
					continue;
				}

				voice.SetInstrument(Bank.GetInstrument(midiNote.Channel));
				voice.Start(midiNote);
				return;
			}

		}
	}
}