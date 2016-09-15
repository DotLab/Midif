namespace Midif {
	public class Voice {
		double sampleRate;
		bool active;
		
		IInstrument instrument;
		
		int note;
		double gain;
		
		uint startSample;
		uint endSample;
		uint relaseDuration;
		uint relaseSample;

		public bool Active {
			get { return active; }
		}
		
		public Voice (double sampleRate) {
			this.sampleRate = sampleRate;
		}
		
		public void SetInstrument (IInstrument newInstrument) {
			instrument = newInstrument;
			relaseDuration = (uint)(instrument.GetReleaseTime() * sampleRate);
		}
		
		public void Start (MidiNote midiNote) {
			Start(midiNote, midiNote.StartSample);
		}
		
		public void Start (MidiNote midiNote, uint curSample) {
			note = midiNote.Note;
			gain = midiNote.Gain;
			
			startSample = curSample;
			endSample = startSample + midiNote.Duration;
			relaseSample = endSample + relaseDuration;
			
			active = true;
		}
		
		public double Update (uint curSample) {
			if (curSample < startSample) {
				return 0;
			}

			double onTime = ((double)curSample - (double)startSample) / sampleRate;
			double offTime = -1;

			if (curSample > endSample) {
				if (curSample >= relaseSample) {
					active = false;

					return 0;
				}
				
				offTime = ((double)curSample - (double)endSample) / sampleRate;
			}
			
			return instrument.GetEnvelopedSample(note, onTime, offTime) * gain;
		}
	}
}