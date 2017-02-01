namespace Midif.Synth {
	public sealed class OneshotSampleGenerator : SampleGenerator {
		/// <summary>
		/// Whether to play the sample a fixed number of times, ignoring NoteOff and any loop information.
		/// </summary>
		public bool IgnoreNoteOff = true;

		public int Count;
		int count = int.MaxValue;

		public override void Init (double sampleRate) {
			SampleRateRecip = 1 / sampleRate;

			gain = SynthTable.Deci2Gain(Level);
			duration = End - Start;
		}

		public override void NoteOn (byte note, byte velocity) {
			IsOn = true;

			phaseStep = SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * Rate * SampleRateRecip;
			if (KeyTrack) phaseStep *= SynthTable.Semi2Pitc[note - KeyCenter + Transpose + SynthTable.Semi2PitcShif];
			if (!IgnoreNoteOff) phase = Start;

			count = 0;
		}


		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			// If not IgnoreNoteOff, truncate the repetition;
			if (!IgnoreNoteOff && count < Count - 1)
				count = Count - 1;
		}

		public override bool IsFinished () {
			return !IgnoreNoteOff || count < Count;
		}


		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				if (count >= Count)
					return RenderCache = 0;

				if ((phase += phaseStep) > duration) {
					phase = Start + ((phase - Start) % duration);
					count++;
				}

				return RenderCache = Samples[(int)(phase)] * gain;
			}

			return RenderCache;
		}
	}
}