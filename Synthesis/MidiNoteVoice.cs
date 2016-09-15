namespace Midif {
	public class MidiNoteVoice {
		public readonly double SampleRate;

		IInstrument instrument;
		
		int note;
		double gain;
		
		uint startSample;
		uint endSample;

		double duration;
		double onTime;
		double offTime;

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

			duration = (double)midiNote.Duration / SampleRate;

			active = true;
		}
		
		public double Update (uint curSample) {
			if (curSample < startSample) {
				return 0;
			}

			if (curSample < endSample) {
				onTime = ((double)curSample - (double)startSample) / SampleRate;
				return instrument.GetSample(note, onTime) * gain;
			} else {
				offTime = ((double)curSample - (double)endSample) / SampleRate;
				return instrument.GetSample(note, duration, offTime) * gain;
			}
		}

		public void TryEnd () {
			if (active && instrument.IsEnded(offTime)) {
				offTime = 0;
				active = false;
			}
		}
	}
}