namespace Midif {
	public class MidiNoteSythesizer {
		public const int MaxPolyphony = 16;
		public const double EndDelay = 1;

		public readonly int SampleRate;
		public readonly MidiNoteSequence Sequence;
		public IInstrumentBank Bank;

		uint currentSample;
		uint endDelaySample;

		double[] buffer = new double[0];

		Voice[] voicePool;

		double gain = 0.3;

		public int CurrentSample {
			get { return (int)currentSample; }
		}

		public double[] Buffer {
			get { return buffer; }
		}

		public double Gain {
			get { return gain; }
		}

		public MidiNoteSythesizer (IInstrumentBank bank, MidiNoteSequence sequence, int sampleRate) {
			this.Bank = bank;
			this.Sequence = sequence;
			this.SampleRate = sampleRate;

			this.Sequence.SampleRate = sampleRate;

			endDelaySample = (uint)((double)sampleRate * EndDelay);

			voicePool = new Voice[MaxPolyphony];
			for (int i = 0; i < MaxPolyphony; i++) {
				voicePool[i] = new Voice(sampleRate);
			}
		}

		public void UpdateBuffer (int bufferSize) {
			if (currentSample > Sequence.SampleLength + endDelaySample) {
				return;
			}

			if (buffer.Length != bufferSize) {
				buffer = new double[bufferSize];
			}
			
			for (int i = 0; i < bufferSize; i++) {
				GetBuffer(ref buffer[i]);

				currentSample ++;
			}
		}

		void GetBuffer (ref double buffer) {
			if (Sequence.HasMidiNoteAtSample(currentSample)) {
				MidiNote[] midiNotes = Sequence.GetMidiNotesAtSample(currentSample);
				foreach (var midiNote in midiNotes) {
					foreach (var voice in voicePool) {
						if (!voice.Active) {
							voice.SetInstrument(Bank.GetInstrument(midiNote.Channel));
							voice.Start(midiNote);
							break;
						}
					}
				}
			}

			buffer = 0;
			foreach (var voice in voicePool) {
				if (voice.Active) {
					buffer += voice.Update(currentSample);
				}
			}

			buffer *= gain;
		}
	}
}