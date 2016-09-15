namespace Midif {
	public class MidiDynamicNoteSythesizer {
		public const int MaxPolyphony = 16;
		
		public delegate void OnBufferPrepare (uint curSample);
		public event OnBufferPrepare OnBufferPrepareEvent;
		public delegate void OnSamplePrepare (uint curSample);
		public event OnSamplePrepare OnSamplePrepareEvent;

		public readonly int SampleRate;
		public IInstrumentBank Bank;
		
		uint curSample;
		
		double[] buffer = new double[0];
		
		Voice[] voicePool;
		
		double gain = 0.3;
		
		public uint CurrentSample {
			get { return curSample; }
		}
		
		public double[] Buffer {
			get { return buffer; }
		}
		
		public double Gain {
			get { return gain; }
		}
		
		public MidiDynamicNoteSythesizer (IInstrumentBank bank, int sampleRate) {
			this.Bank = bank;
			this.SampleRate = sampleRate;
			
			voicePool = new Voice[MaxPolyphony];
			for (int i = 0; i < MaxPolyphony; i++) {
				voicePool[i] = new Voice(sampleRate);
			}
		}
		
		public void UpdateBuffer (int bufferSize) {
			if (buffer.Length != bufferSize) {
				buffer = new double[bufferSize];
			}
			
			OnBufferPrepareEvent(curSample);
			for (int i = 0; i < bufferSize; i++) {
				GetSample(ref buffer[i]);
				
				curSample ++;
			}
		}
		
		void GetSample (ref double sample) {
			OnSamplePrepareEvent(curSample);
			
			sample = 0;
			foreach (var voice in voicePool) {
				if (voice.Active) {
					sample += voice.Update(curSample);
				}
			}
			
			sample *= gain;
		}
		
		public void StartMidiNote (MidiNote midiNote, bool fromCurSample) {
			foreach (var voice in voicePool) {
				if (!voice.Active) {
					voice.SetInstrument(Bank.GetInstrument(midiNote.Channel));
					if (fromCurSample) {
						voice.Start(midiNote, curSample);
					} else {
						voice.Start(midiNote);
					}
					break;
				}
			}
		}
	}
}