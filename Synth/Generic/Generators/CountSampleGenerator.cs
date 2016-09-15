using System;

namespace Midif.Synth.Generic {
	/// <summary>
	/// Play from Start to End and loop for Count times.
	/// </summary>
	public class CountSampleGenerator : CachedSignalProvider {
		public byte RootKey;

		public int Count = 1;
		public bool IgnoreNoteOff = true;

		public int Start;
		public int End = -1;
		int loopLength;

		double[] samples;
		int sampleRate;

		double sampleRateFactor;

		int count;
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

			loopLength = End - Start;
		}


		public override void NoteOn (byte note, byte velocity) {
			phaseStep = SynthConstants.Semitone2Pitch[note - RootKey + 127] * sampleRateFactor;

			phase = Start;
			count = 0;
		}

		public override bool IsActive () {
			if (IgnoreNoteOff)
				return count < Count;
			
			return false;
		}


		public override double Render () {
			if (count > Count - 1)
				return 0;

			var sample = samples[(int)phase];
			phase += phaseStep;

			if (phase > End) {
				count++;
				while (phase > End)
					phase -= loopLength;
			}

			return sample;
		}
	}
}

