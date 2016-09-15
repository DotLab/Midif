using System;

using Midif.File.Sfz;

namespace Midif.Synth.Sfz {
	public class SfzGenerator : SfzComponent {
		enum State {
			PreLoop,
			MidLoop,
			AftLoop,
		}

		public double Delay;
		public int Offset;
		public int End = -1;
		public int Count = -1;

		public SfzLoopMode LoopMode = SfzLoopMode.NoLoop;
		public int LoopStart = -1;
		public int LoopEnd = -1;

		public int Transpose;
		public int Tune;

		double[] samples;
		double originalSampleRate;

		double frequencyFactor;
		double basePhaseStep, phaseStep;
		double phase;

		int delaySample;
		int delay;

		// If countLoop, loop count times, ignoring NoteOff; If not, loop untill NoteOff.
		bool countLoop;
		int count;

		// If countLoop, loopLength = End - Offset; If not, loopLength = LoopEnd - LoopStart.
		int loopLength;

		// If NoLoop (countLoop or real NoLoop), state will not go beyound PreLoop.
		State state;

		public SfzGenerator () {
			keyTrackEnabled = true;
			KeyTrack = 100;
		}

		public void SetSamples (double[] samples, int sampleRate) {
			this.samples = samples;
			originalSampleRate = sampleRate;
		}

		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			frequencyFactor = originalSampleRate / sampleRate;

			delaySample = (int)(Delay * sampleRate);

			// If oneShot, set count to 1
			if (LoopMode == SfzLoopMode.OneShot)
				Count = 1;

			// If count is set, countLoop; If not, countLoop with NoteOff event.
			countLoop = Count > 0;
			if (countLoop)
				LoopMode = SfzLoopMode.NoLoop;
			else
				Count = 1;

			// If end is set, cancel looping.
			if (End > 0)
				LoopMode = SfzLoopMode.NoLoop;
			else
				End = samples.Length;
			
			if (LoopMode != SfzLoopMode.NoLoop) {
				if (LoopStart < 0) LoopStart = 0;
				if (LoopEnd < 0) LoopEnd = samples.Length;
				loopLength = LoopEnd - LoopStart;
			} else
				loopLength = End - Offset;
		}


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			basePhaseStep = frequencyFactor * SynthConstants.Cents2Pitch((int)(keyTrackDepth) + Transpose * 100 + Tune);
			if (velTrackEnabled) {
				basePhaseStep *= SynthConstants.Cents2Pitch((int)(velTrackDepth));
			}
			phaseStep = basePhaseStep;
			phase = 0;

			delay = delaySample;

			count = 0;
		
			state = State.PreLoop;
		}

		public override void NoteOff (byte velocity) {
			base.NoteOff(velocity);

			if (!countLoop)
				state = State.AftLoop;
		}

		public override bool IsActive () {
			if (countLoop)
				return phase < samples.Length;
			
			return base.IsActive();
		}


		public override double Render () {
			if (phase > samples.Length)
				return 0;

			if (delay > 0) {
				delay--;
				if (egEnabled) Eg.Render(flag);
				if (lfoEnabled) Lfo.Render(flag);
				return 0;
			}

			var sample = samples[(int)phase];
			phase += phaseStep;

			if (egEnabled || lfoEnabled) {
				phaseStep = basePhaseStep;
				if (egEnabled)
					phaseStep *= SynthConstants.Cents2Pitch((int)(Eg.Render(flag) * egTotalDepth));
				if (lfoEnabled)
					phaseStep *= SynthConstants.Cents2Pitch((int)(Lfo.Render(flag) * lfoTotalDepth));
			}

			switch (state) {
			case State.PreLoop:
				// If not looped, do nothing.
				if (LoopMode != SfzLoopMode.NoLoop && phase > LoopStart) {
					// If looping and phase pass the loopStart.
					state = State.MidLoop;
					while (phase > LoopEnd)
						phase -= loopLength;
				} else if (Count > 0 && phase > End) {
					// If loop by count and phase pass the end.
					count++;
					if (count < Count)
						while (phase > End)
							phase -= loopLength;
				}
				break;
			case State.MidLoop:
				// Loop.
				if (phase > LoopEnd)
					while (phase > LoopEnd)
						phase -= loopLength;
				break;
			case State.AftLoop:
				// If coutinuous loop, loop; If sustain loop, do nothing.
				if (LoopMode == SfzLoopMode.Continuous && phase > LoopEnd)
					while (phase > LoopEnd)
						phase -= loopLength;
				break;
			}

			return sample;
		}
	}
}

