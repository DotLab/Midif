namespace Midif.Synth {
	public class OneshotSampleGenerator : MidiComponent {
		public override bool IsActive {
			get {
				return isOn || (ignoreNoteOff && count < Count);
			}
		}

		public bool KeyTrack;
		public int KeyCenter = 60;
		public int Transpose;
		public int Tune;

		public double Level;
		double gain;

		public int Start;
		public int End;
		int duration;

		// If Count > 0, ignore NoteOff.
		public int Count;
		bool ignoreNoteOff;

		public double[] Samples;
		public double SampleRate;

		double phaseStep;
		double phase;

		int count;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			if (End <= Start)
				End = Samples.Length;

			duration = End - Start;
			ignoreNoteOff = Count > 0;
			count = Count;
			gain = SynthTable.Deci2Gain(Level);
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			phaseStep = (KeyTrack ? SynthTable.Semi2Pitc[note - KeyCenter + Transpose + SynthTable.Semi2PitcShif] : 1) *
			SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * SampleRate * sampleRateRecip;
			phase = 0;

			count = 0;
		}

		public override double Render () {
			if (ignoreNoteOff && count >= Count)
				return 0;

			if ((phase += phaseStep) > duration) {
				phase %= duration;
				count++;
			}
				
			return Samples[Start + (int)(phase)] * gain;
		}
	}
}