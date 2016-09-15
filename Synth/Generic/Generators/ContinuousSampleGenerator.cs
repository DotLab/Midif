using System;

namespace Midif.Synth.Generic {
	/// <summary>
	/// Play from Start and loop continuously.
	/// </summary>
	public class ContinuousSampleGenerator : CachedSignalProvider {
		enum LoopState {
			BeforeLoop,
			Looping
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


		public override double Render () {
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

