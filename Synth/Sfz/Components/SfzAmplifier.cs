using System;

namespace Midif.Synth.Sfz {
	public class SfzAmplifier : SfzComponent {
		public static readonly double[] Vel2Decibel;

		static SfzAmplifier () {
			Vel2Decibel = new double[128];
			for (int i = 0; i < Vel2Decibel.Length; i++)
				Vel2Decibel[i] = -20 * Math.Log10(Math.Pow(127, 2) / Math.Pow(i, 2));
		}

		public ISignalProvider Source;

		public double Volume;

		double initGain, baseGain, gain;

		public SfzAmplifier () {
			EgDepth = 1;
		}

		public override void Init (double sampleRate) {
			base.Init(sampleRate);
			Source.Init(sampleRate);

			gain = baseGain = SynthConstants.Decibel2Gain(Volume);
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
			Source.NoteOn(note, velocity);

			baseGain = SynthConstants.Decibel2Gain(Volume + Vel2Decibel[velocity]);
			if (keyTrackEnabled || velTrackEnabled) {
				if (keyTrackEnabled)
					baseGain *= SynthConstants.Decibel2Gain(keyTrackDepth);
				if (velTrackEnabled)
					baseGain *= velTrackDepth;
			}
			gain = baseGain;
		}

		public override void NoteOff (byte velocity) {
			base.NoteOff(velocity);
			Source.NoteOff(velocity);
		}

		public override double Render () {
			if (egEnabled && !Eg.IsActive())
				return 0;

			if (egEnabled || lfoEnabled) {
				gain = baseGain;
				if (egEnabled)
					gain *= Eg.Render(flag) * egTotalDepth;
				if (lfoEnabled)
					gain *= SynthConstants.Decibel2Gain(Lfo.Render(flag) * lfoTotalDepth);
			}
				
			return Source.Render(flag) * gain;
		}
	}
}

