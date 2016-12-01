namespace Midif.Synth {
	public class ContinuousSampleGenerator : MidiComponent {
		public override bool IsActive {
			get {
				return isOn || (useSustain && phase < End);
			}
		}

		public int KeyCenter = 60;
		public int Transpose;
		public int Tune;

		public double Level;
		double gain;

		public int Start;
		public int End;

		/// <summary>
		/// The start of the loop.
		/// </summary>
		public int LoopStart;
		/// <summary>
		/// The end of the loop, will be played.
		/// </summary>
		public int LoopEnd;
		int loopDuration;

		// If LoopEnd < End, useSustain.
		bool useSustain;

		public double[] Samples;
		public double SampleRate;

		double phaseStep;
		double phase;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			if (LoopEnd <= LoopStart)
				LoopEnd = Samples.Length - 1;
			
			if (End <= Start)
				End = LoopEnd;

			loopDuration = LoopEnd - LoopStart;
			useSustain = LoopEnd < End;
			gain = SynthTable.Deci2Gain(Level);
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			phaseStep = SynthTable.Semi2Pitc[note - KeyCenter + Transpose + SynthTable.Semi2PitcShif] *
			SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * SampleRate * sampleRateRecip;
			phase = Start;
		}

		public override double Render () {
			phase += phaseStep;

			if (useSustain && phase > End)
				return 0;

			if (!useSustain && phase > LoopEnd) {
				phase = LoopStart + ((phase - LoopStart) % loopDuration);
			}

			return Samples[(int)(phase)] * gain;

//			var whole = (int)phase;
//			var shift = phase - whole;
//	
//			var delta = (whole < End ? Samples[whole + 1] : Samples[whole]) - Samples[whole];
//
//			return (Samples[whole] + shift * delta) * gain;
		}

		public override string ToString () {
			return string.Format("[ContinuousSampleGenerator: KeyCenter={0}, Transpose={1}, Tune={2}, Level={3}, Start={4}, End={5}, LoopStart={6}, LoopEnd={7}, SampleRate={8}]", KeyCenter, Transpose, Tune, Level, Start, End, LoopStart, LoopEnd, SampleRate);
		}
	}
}