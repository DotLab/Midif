using System;

namespace Midif.Synthesizer.Dx7 {
	public enum LfoWaveform {
		Triangle,
		SawDown,
		SawUp,
		Square,
		Sine,
		SampleHold
	}

	public enum LfoDelayState {
		Onset,
		Ramp,
		Complete
	}

	public class Dx7Lfo {
		static Random rand = new Random();

		static double phaseStep;
		static double pitchModDepth;
		static double ampModDepth;
		static double sampleHoldRandom;
		static readonly double[] delayTimes = { 0, 0, 0 };
		static readonly double[] delayIncrements = { 0, 0, 0 };
		static readonly double[] delayVals = { 0, 0, 0 };
		static Dx7Patch patch;

		public static void SetPatch (Dx7Patch newSettings) {
			patch = newSettings;
		}

		public static void Update () {
			var frequency = Dx7Config.LfoFrequencyTable[patch.lfoSpeed];
			phaseStep = Dx7Config.Period * frequency / Dx7Config.LfoRate;  // Radians per sample
			ampModDepth = patch.lfoAmpModDepth * 0.01;
			// ignoring amp mod table for now. it seems shallow LFO_AMP_MOD_TABLE[params.lfoAmpModDepth];
			delayTimes[(int)LfoDelayState.Onset] = (Dx7Config.LfoRate * 0.001753 * Math.Pow(patch.lfoDelay, 3.10454) + 169.344 - 168) / 1000;
			delayTimes[(int)LfoDelayState.Ramp] = (Dx7Config.LfoRate * 0.321877 * Math.Pow(patch.lfoDelay, 2.01163) + 494.201 - 168) / 1000;
			delayIncrements[(int)LfoDelayState.Ramp] = 1 / (delayTimes[(int)LfoDelayState.Ramp] - delayTimes[(int)LfoDelayState.Onset]);
		}


		Dx7Patch.Operator opConfig;
		double phase;
		double pitchVal;
		int counter;
		double ampVal;
		double ampValTarget;
		double ampIncrement;
		double delayVal;
		LfoDelayState delayState;

		public Dx7Lfo (Dx7Patch.Operator opSettings) {
			this.opConfig = opSettings;

			phase = 0;
			pitchVal = 0;
			counter = 0;
			ampVal = 1;
			ampValTarget = 1;
			ampIncrement = 0;
			delayVal = 0;
			delayState = LfoDelayState.Onset;

			Update();
		}

		public double Render () {
			double amp = 0;

			if (counter % Dx7Config.LfoSamplePeriod == 0) {
				switch (patch.lfoWaveform) {
				case (int)LfoWaveform.Triangle:
					if (phase < Dx7Config.PeriodHalf)
						amp = 4 * phase * Dx7Config.PeriodRecip - 1;
					else
						amp = 3 - 4 * phase * Dx7Config.PeriodRecip;
					break;
				case (int)LfoWaveform.SawDown:
					amp = 1 - 2 * phase * Dx7Config.PeriodRecip;
					break;
				case (int)LfoWaveform.SawUp:
					amp = 2 * phase * Dx7Config.PeriodRecip - 1;
					break;
				case (int)LfoWaveform.Square:
					amp = (phase < Dx7Config.PeriodHalf) ? -1 : 1;
					break;
				case (int)LfoWaveform.Sine:
					amp = Math.Sin(phase);
					break;
				case (int)LfoWaveform.SampleHold:
					amp = sampleHoldRandom;
					break;
				}

				switch (delayState) {
				case LfoDelayState.Onset:
				case LfoDelayState.Ramp:
					delayVal += delayIncrements[(int)delayState];
					if ((double)counter / Dx7Config.LfoSamplePeriod > delayTimes[(int)delayState]) {
						delayState++;
						delayVal = delayVals[(int)delayState];
					}
					break;
				case LfoDelayState.Complete:
					break;
				}

				// if (this.counter % 10000 == 0 && this.operatorIndex === 0) console.log("lfo amp value", this.ampVal);
				amp *= delayVal;
				pitchModDepth = 1 + Dx7Config.LfoPitchModTable[patch.lfoPitchModSens] * (patch.controllerModVal + patch.lfoPitchModDepth / 99.0);
				pitchVal = Math.Pow(pitchModDepth, amp);

				// TODO: Simplify ampValTarget calculation.
				// ampValTarget range = 0 to 1. lfoAmpModSens range = -3 to 3. ampModDepth range =  0 to 1. amp range = -1 to 1.
				var ampSensDepth = Math.Abs(opConfig.lfoAmpModSens) * 0.333333;
				var phase2 = (opConfig.lfoAmpModSens > 0) ? 1 : -1;
				ampValTarget = 1 - ((ampModDepth + patch.controllerModVal) * ampSensDepth * (amp * phase2 + 1) * 0.5);
				ampIncrement = (ampValTarget - ampVal) / Dx7Config.LfoSamplePeriod;
				phase += phaseStep;
				if (phase >= Dx7Config.Period) {
					sampleHoldRandom = 1 - rand.NextDouble() * 2;
					phase -= Dx7Config.Period;
				}
			}

			counter++;
			return pitchVal;
		}

		public double RenderAmp () {
			ampVal += ampIncrement;
			return ampVal;
		}
	}
}

