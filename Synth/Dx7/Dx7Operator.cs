using System;

namespace Midif.Synth.Dx7 {
	public sealed class Dx7Operator : Dx7Component {
		// http://www.chipple.net/dx7/fig09-4.gif
		const double OCTAVE_1024 = 1.0006771307;
		//		static readonly double OCTAVE_1024 = Math.Exp(Math.Log(2) / 1024);

		public MidiComponent Envelope;
		public MidiComponent Lfo;

		public int LfoAmpModSens;
		public int LfoAmpModDepth;

		public int LfoPitchModSens;
		public int LfoPitchModDepth;


		public int Detune;
		/// <summary>
		/// Fix frequency when > 0.
		/// </summary>
		public double FixedFrequency = -1;
		public double FrequencyRatio;
		public double ControllerModVal;

		public double Modulation;

		double phase;
		double phaseStep;

		static double lfoAmpGain;
		static double lfoPitchGain;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			Envelope.Init(sampleRate);
			Lfo.Init(sampleRate);

			lfoAmpGain = 
				1 + Dx7Lfo.LfoAmpModSensTable[LfoAmpModSens] * (ControllerModVal + LfoAmpModDepth / 99.0);
			lfoPitchGain = 
				1 + Dx7Lfo.LfoPitchModSensTable[LfoPitchModSens] * (ControllerModVal + LfoPitchModDepth / 99.0);
		}

		public override void NoteOn (byte note, byte velocity) {
			phase = 0;

			double freq;

			if (FixedFrequency > 0)
				freq = FixedFrequency;
			else
				freq = SynthTable.Note2Freq[note] * FrequencyRatio * Math.Pow(OCTAVE_1024, Detune);
			DebugConsole.WriteLine(FrequencyRatio);

			phaseStep = Pi2 * freq * SampleRateRecip;

			Envelope.NoteOn(note, velocity);
		}

		public override void NoteOff (byte note, byte velocity) {
			Envelope.NoteOff(note, velocity);
		}

		public override bool IsFinished () {
			return Envelope.IsFinished();
		}

		public override double Render (bool flag) {
			if (RenderFlag ^ flag) {
				RenderFlag = flag;

				var lfo = Lfo.Render(flag);

				RenderCache = Math.Sin(phase + Modulation) * Envelope.Render(flag) * (lfo * lfoAmpGain);

				phase += phaseStep * Math.Pow(lfoPitchGain, lfo);
				if (phase >= Pi2) phase -= Pi2;

				return RenderCache;
			}

			return RenderCache;
		}
	}
}