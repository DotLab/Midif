namespace Midif {
	public class MidiNoteVoice {
		public readonly double SampleRate;

		IInstrument instrument;
		
		int note;
		double gain;
		
		uint startSample;
		uint endSample;

		bool active;

		public bool Active {
			get { return active; }
		}


		public MidiNoteVoice (double sampleRate) {
			SampleRate = sampleRate;
		}
		
		public void SetInstrument (IInstrument newInstrument) {
			instrument = newInstrument;
		}
		
		public void Start (MidiNote midiNote) {
			Start(midiNote, midiNote.StartSample);
		}
		
		public void Start (MidiNote midiNote, uint curSample) {
			note = midiNote.Note;
			gain = midiNote.Gain;
			
			startSample = curSample;
			endSample = startSample + midiNote.Duration;

			active = true;
		}
		
		public double Update (uint curSample) {
			if (curSample < startSample) {
				return 0;
			}

			double onTime = ((double)curSample - (double)startSample) / SampleRate;
			if (curSample < endSample) {
				return instrument.GetSample(note, onTime) * gain;
			} else {
				double offTime = ((double)curSample - (double)endSample) / SampleRate;
				double sample = instrument.GetSample(note, onTime, offTime);
				if (sample == 0) {
					active = false;
					return 0;
				} else {
					return sample * gain;
				}
			}			
		}
	}
}