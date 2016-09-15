using System;

using Midif.File.Sfz;

namespace Midif.Synth.Sfz {
	[Serializable]
	public class SfzVoice {
		ISignalProvider[] signalProviders;
		double[] pans;
		public bool[] flags;

		public int[] noteMap = new int[128];

		public int Note;
		public int index;
		public bool IsOn;

		public SfzVoice (SfzFile file) {
			signalProviders = new ISignalProvider[file.Regions.Count];
			pans = new double[signalProviders.Length];
			flags = new bool[signalProviders.Length];

			for (int i = 0; i < signalProviders.Length; i++) {
				signalProviders[i] = BuildSignalProvider(file.Regions[i], i);

				for (int j = file.Regions[i].loKey; j <= file.Regions[i].hiKey; j++)
					noteMap[j] = i;
			}
		}

		ISignalProvider BuildSignalProvider (SfzRegion region, int index) {
			ISignalProvider root;

			var generator = new SfzGenerator();
			root = generator;

			var waveFile = SfzWaveBank.GetWaveFile(region.sample);
			generator.SetSamples(waveFile.Samples[0], waveFile.SamplePerSec);
//			region.filterEnabled = true;
//			region.cutOff = waveFile.SamplePerSec / 2;
//			region.resonance = 0.7f;

			generator.Delay = region.delay;
			generator.Offset = region.offset;
			generator.End = region.end;
			generator.Count = region.count;
			generator.LoopMode = region.loopMode;
			generator.LoopStart = region.loopStart;
			generator.LoopEnd = region.loopEnd;

			generator.Transpose = region.transpose;
			generator.Tune = region.tune;
			generator.KeyCenter = region.pitchKeyCenter;
			generator.KeyTrack = region.pitchKeyTrack;
			generator.VelTrack = region.pitchVelTrack;

			if (region.pitchEgEnabled) {
				var eg = new SfzEnvelope();
				generator.Eg = eg;

				eg.Delay = region.pitchEgDelay;
				eg.Start = region.pitchEgStart / 100;
				eg.Attack = region.pitchEgAttack;
				eg.Hold = region.pitchEgHold;
				eg.Decay = region.pitchEgDecay;
				eg.Sustain = region.pitchEgSustain / 100;
				eg.Release = region.pitchEgRelease;
				generator.EgDepth = region.pitchEgDepth;

				eg.Vel2Delay = region.pitchEgVel2Delay;
				eg.Vel2Attack = region.pitchEgVel2Attack;
				eg.Vel2Hold = region.pitchEgVel2Hold;
				eg.Vel2Decay = region.pitchEgVel2Decay;
				eg.Vel2Sustain = region.pitchEgVel2Sustain / 100;
				eg.Vel2Release = region.pitchEgVel2Release;
				generator.EgVel2Depth = region.pitchEgVel2Depth;
			}

			if (region.pitchLfoEnabled) {
				var lfo = new SfzLfo();
				generator.Lfo = lfo;

				lfo.Delay = region.pitchLfoDelay;
				lfo.Frequency = region.pitchLfoFrequency;
				generator.LfoDepth = region.pitchLfoDepth;
			}

			if (region.filterEnabled) {
				var filter = new SfzFilter();
				filter.Source = root;
				root = filter;

				filter.FilterType = region.filterType;
				filter.CutOff = region.cutOff;
				filter.Resonance = region.resonance;

				filter.KeyCenter = region.filterKeyCenter;
				filter.KeyTrack = region.filterKeyTrack;
				filter.VelTrack = region.filterVelTrack;

				if (region.filterEgEnabled) {
					var eg = new SfzEnvelope();
					filter.Eg = eg;

					eg.Delay = region.filterEgDelay;
					eg.Start = region.filterEgStart / 100;
					eg.Attack = region.filterEgAttack;
					eg.Hold = region.filterEgHold;
					eg.Decay = region.filterEgDecay;
					eg.Sustain = region.filterEgSustain / 100;
					eg.Release = region.filterEgRelease;
					filter.EgDepth = region.filterEgDepth;

					eg.Vel2Delay = region.filterEgVel2Delay;
					eg.Vel2Attack = region.filterEgVel2Attack;
					eg.Vel2Hold = region.filterEgVel2Hold;
					eg.Vel2Decay = region.filterEgVel2Decay;
					eg.Vel2Sustain = region.filterEgVel2Sustain / 100;
					eg.Vel2Release = region.filterEgVel2Release;
					filter.EgVel2Depth = region.filterEgVel2Depth;
				}

				if (region.filterLfoEnabled) {
					var lfo = new SfzLfo();
					filter.Lfo = lfo;

					lfo.Delay = region.filterLfoDelay;
					lfo.Frequency = region.filterLfoFrequency;
					filter.LfoDepth = region.filterLfoDepth;
				}
			}

			if (region.ampEnabled) {
				var amp = new SfzAmplifier();
				amp.Source = root;
				root = amp;

				amp.Volume = region.volume;
				pans[index] = region.pan;

				amp.KeyCenter = region.ampKeyCenter;
				amp.KeyTrack = region.ampKeyTrack;
				amp.VelTrack = region.ampVelTrack;

				if (region.ampEgEnabled) {
					var eg = new SfzEnvelope();
					amp.Eg = eg;

					eg.Delay = region.ampEgDelay;
					eg.Start = region.ampEgStart / 100;
					eg.Attack = region.ampEgAttack;
					eg.Hold = region.ampEgHold;
					eg.Decay = region.ampEgDecay;
					eg.Sustain = region.ampEgSustain / 100;
					eg.Release = region.ampEgRelease;

					eg.Vel2Delay = region.ampEgVel2Delay;
					eg.Vel2Attack = region.ampEgVel2Attack;
					eg.Vel2Hold = region.ampEgVel2Hold;
					eg.Vel2Decay = region.ampEgVel2Decay;
					eg.Vel2Sustain = region.ampEgVel2Sustain / 100;
					eg.Vel2Release = region.ampEgVel2Release;
				}

				if (region.ampLfoEnabled) {
					var lfo = new SfzLfo();
					amp.Lfo = lfo;

					lfo.Delay = region.ampLfoDelay;
					lfo.Frequency = region.ampLfoFrequency;
					amp.LfoDepth = region.ampLfoDepth;
				}
			}

			var f = new Midif.Synth.Generic.OnePoleFilter();
			f.Fc = waveFile.SamplePerSec / 2;
			f.Source = root;
			root = f;
			return root;
		}

		public void Init (double sampleRate) {
			foreach (var signalProvider in signalProviders)
				signalProvider.Init(sampleRate);
		}

		public void NoteOn (byte note, byte velocity) {
			Note = note;
			index = noteMap[note];

			signalProviders[index].NoteOn(note, velocity);

			IsOn = true;
		}

		public void NoteOff (byte velocity) {
			signalProviders[index].NoteOff(velocity);

			IsOn = false;
		}

		public bool IsActive () {
			return IsOn || signalProviders[index].IsActive();
		}

		public double Render () {
			return signalProviders[index].Render(flags[index] ^= true);
		}
	}
}

