using System;

namespace Midif.Synthesizer.Dx7 {
	public class Dx7Operator {
		// http://www.chipple.net/dx7/fig09-4.gif
		//const double OCTAVE_1024 = 1.0006771307;
		static double OCTAVE_1024 = Math.Exp(Math.Log(2) / 1024);

		double phase;
		double phaseStep;
		public double val;
		Dx7Patch.Operator opConfig;
		Dx7Envelope envelope;
		Dx7Lfo lfo;

		public double outputLevel;

		public Dx7Operator (Dx7Patch.Operator opConfig, double baseFrequency, Dx7Envelope envelope, Dx7Lfo lfo) {
			phase = 0;
			val = 0;

			this.opConfig = opConfig;
			this.envelope = envelope;
			// TODO: Pitch envelope
			// this.pitchEnvelope = pitchEnvelope;
			this.lfo = lfo;

			UpdateFrequency(baseFrequency);
		}

		public void UpdateFrequency (double baseFrequency) {
			var frequency = opConfig.oscMode == 1 ?
				opConfig.freqFixed :
				baseFrequency * opConfig.freqRatio * Math.Pow(OCTAVE_1024, opConfig.detune);
			
			phaseStep = Dx7Config.Period * frequency / Dx7Config.SampleRate; // Radians per sample
		}

		public double Render (double mod) {
			val = Math.Sin(phase + mod) * envelope.Render() * lfo.RenderAmp();
			//	this.phase += this.phaseStep * this.pitchEnvelope.render() * this.lfo.render();
			phase += phaseStep * lfo.Render();
			if (phase >= Dx7Config.Period)
				phase -= Dx7Config.Period;
			
			return val;
		}

		public void NoteOff () {
			envelope.NoteOff();
		}

		public bool IsFinished () {
			return envelope.IsFinished();
		}
	}
}