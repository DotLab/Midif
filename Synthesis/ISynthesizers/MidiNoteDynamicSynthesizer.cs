namespace Midif {
	public class MidiNoteDynamicSynthesizer : ISynthesizer {
		public const int MaxPolyphony = 16;
		public const double Gain = 0.3;

		public delegate void OnBufferPrepare (uint curSample);
		public event OnBufferPrepare OnBufferPrepareEvent;
		public delegate void OnSamplePrepare (uint curSample);
		public event OnSamplePrepare OnSamplePrepareEvent;

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
		
		public MidiNoteDynamicSynthesizer (IInstrumentBank bank, int sampleRate) {
			Bank = bank;
		}

		public void SetSampleRate (uint sampleRate) {
			voicePool = new MidiNoteVoice[MaxPolyphony];
			for (int i = 0; i < MaxPolyphony; i++) {
				voicePool[i] = new MidiNoteVoice(sampleRate);
			}
		}
		
		public void UpdateBuffer (int bufferSize) {
			if (buffer.Length != bufferSize) {
				buffer = new double[bufferSize];
			}
			
			OnBufferPrepareEvent(currentSample);

			for (int i = 0; i < bufferSize; i++) {
				buffer[i] = GetSample();
				
				currentSample ++;
			}
		}
		
		double GetSample () {
			OnSamplePrepareEvent(currentSample);
			
			double sample = 0;
			foreach (var voice in voicePool) {
				if (voice.Active) {
					sample += voice.Update(currentSample);
				}
			}
			return sample * Gain;
		}
		
		public void StartVoice (MidiNote midiNote, bool immediately) {
			foreach (var voice in voicePool) {
				if (!voice.Active) {
					voice.SetInstrument(Bank.GetInstrument(midiNote.Channel));
					if (immediately) {
						voice.Start(midiNote, currentSample);
					} else {
						voice.Start(midiNote);
					}
					break;
				}
			}
		}
	}
}