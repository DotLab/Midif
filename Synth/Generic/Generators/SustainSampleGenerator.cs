using System;

namespace Midif.Synth.Generic {
	/// <summary>
	/// Play from Start and loop until NoteOff and play to the End.
	/// </summary>
	public class SustainSampleGenerator : CachedSignalProvider {
		enum LoopState {
			BeforeLoop,
			Looping,
			AfterLoop,
		}

		public byte RootKey;

		public int Start;
		public int End = -1;

		public int LoopStart;
		public int LoopEnd = -1;

		int loopLength;

		double[] samples;
		int sampleRate;

		double sampleRateFactor;

		LoopState state;
		double phaseStep;
		double phase;


		public void SetSamples (double[] samples, int sampleRate) {
			this.samples = samples;
			this.sampleRate = sampleRate;
		}

		public override void Init (double sampleRate) {
			sampleRateFactor = this.sampleRate / sampleRate;

			if (End < 0)
				End = samples.Length;

			if (LoopEnd < 0 || LoopEnd > End)
				LoopEnd = End;

			loopLength = LoopEnd - LoopStart;
		}


		public override void NoteOn (byte note, byte velocity) {
			phaseStep = SynthConstants.Semitone2Pitch[note - RootKey + 127] * sampleRateFactor;

			state = LoopState.BeforeLoop;
			phase = Start;
		}

		public override void NoteOff (byte velocity) {
			state = LoopState.AfterLoop;
		}

		public override bool IsActive () {
			return phase < End;
		}


		public override double Render () {
			if (phase > End)
				return 0;

			var sample = samples[(int)phase];
			phase += phaseStep;

			switch (state) {
			case LoopState.BeforeLoop:
				if (phase > LoopStart) {
					state = LoopState.Looping;
					while (phase > LoopEnd)
						phase -= loopLength;
				}
				break;
			case LoopState.Looping:
				while (phase > LoopEnd)
					phase -= loopLength;
				break;
			}

			return sample;
		}
	}
}

