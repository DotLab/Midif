using System;

namespace Midif.Synth.Dx7 {
	public sealed class Dx7Lfo : Dx7Component {
		#region Static

		static readonly Random Rand = new Random();

		public const double LfoSampleRate = 441;
		public static readonly double[] LfoFrequencyTable =
			{ // see https://github.com/smbolton/hexter/tree/master/src/dx7_voice.c#L1002
				0.062506,  0.124815,  0.311474,  0.435381,  0.619784,
				0.744396,  0.930495,  1.116390,  1.284220,  1.496880,
				1.567830,  1.738994,  1.910158,  2.081322,  2.252486,
				2.423650,  2.580668,  2.737686,  2.894704,  3.051722,
				3.208740,  3.366820,  3.524900,  3.682980,  3.841060,
				3.999140,  4.159420,  4.319700,  4.479980,  4.640260,
				4.800540,  4.953584,  5.106628,  5.259672,  5.412716,
				5.565760,  5.724918,  5.884076,  6.043234,  6.202392,
				6.361550,  6.520044,  6.678538,  6.837032,  6.995526,
				7.154020,  7.300500,  7.446980,  7.593460,  7.739940,
				7.886420,  8.020588,  8.154756,  8.288924,  8.423092,
				8.557260,  8.712624,  8.867988,  9.023352,  9.178716,
				9.334080,  9.669644, 10.005208, 10.340772, 10.676336,
				11.011900, 11.963680, 12.915460, 13.867240, 14.819020,
				15.770800, 16.640240, 17.509680, 18.379120, 19.248560,
				20.118000, 21.040700, 21.963400, 22.886100, 23.808800,
				24.731500, 25.759740, 26.787980, 27.816220, 28.844460,
				29.872700, 31.228200, 32.583700, 33.939200, 35.294700,
				36.650200, 37.812480, 38.974760, 40.137040, 41.299320,
				42.461600, 43.639800, 44.818000, 45.996200, 47.174400,
				47.174400, 47.174400, 47.174400, 47.174400, 47.174400,
				47.174400, 47.174400, 47.174400, 47.174400, 47.174400,
				47.174400, 47.174400, 47.174400, 47.174400, 47.174400,
				47.174400, 47.174400, 47.174400, 47.174400, 47.174400,
				47.174400, 47.174400, 47.174400, 47.174400, 47.174400,
				47.174400, 47.174400, 47.174400
			};

		// 0..3 -> 0..1
		public static readonly double[] LfoAmpModSensTable =
			{
				0, 0.238058, 0.460510, 1
			};

		// 0..7 -> 0..1
		public static readonly double[] LfoPitchModSensTable =
			{
				0, 0.0264, 0.0534, 0.0889, 0.1612, 0.2769, 0.4967, 1
			};

		public enum Waveform {
			Triangle = 0,
			SawDown,
			SawUp,
			Square,
			Sine,
			SampleHold
		}

		#endregion

		public int LfoSpeed;
		public Waveform LfoWaveform;

		/// <summary>
		/// The Lfo phase.
		/// phase += phaseStep once when calculating the output.
		/// </summary>
		double phase;
		double phaseStep;

		double sampleHoldRandom;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			phaseStep = Pi2 * LfoFrequencyTable[LfoSpeed] / sampleRate;
		}

		public override void NoteOn (byte note, byte velocity) {
			phase = 0;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				if (LfoWaveform == Waveform.Triangle) {
					if (phase < Pi) RenderCache = 4 * phase * Pi2Recip - 1;
					else RenderCache = 3 - 4 * phase * Pi2Recip;
				} else if (LfoWaveform == Waveform.SawDown)
					RenderCache = 1 - 2 * phase * Pi2Recip;
				else if (LfoWaveform == Waveform.SawUp)
					RenderCache = 2 * phase * Pi2Recip - 1;
				else if (LfoWaveform == Waveform.Square)
					RenderCache = (phase < Pi) ? -1 : 1;
				else if (LfoWaveform == Waveform.Sine)
					RenderCache = Math.Sin(phase);
				else if (LfoWaveform == Waveform.SampleHold)
					RenderCache = sampleHoldRandom;
				else throw new Exception("Incorrect Waveform");

				phase += phaseStep;
				if (phase > Pi2) {
					phase -= Pi2;

					if (LfoWaveform == Waveform.SampleHold)
						sampleHoldRandom = 1 - Rand.NextDouble() * 2;
				}

				return RenderCache;
			}

			return RenderCache;
		}

		public override void Process (float[] buffer) {
			switch (LfoWaveform) {
			case Waveform.Triangle:
				for (int i = 0; i < buffer.Length; i++) {
					if (phase < Pi) buffer[i] = (float)(4 * phase * Pi2Recip - 1);
					else buffer[i] = (float)(3 - 4 * phase * Pi2Recip);

					phase += phaseStep;
					if (phase > Pi2) phase -= Pi2;
				}
				break;
			case Waveform.SawDown:
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] = (float)(1 - 2 * phase * Pi2Recip);

					phase += phaseStep;
					if (phase > Pi2) phase -= Pi2;
				}
				break;
			case Waveform.SawUp:
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] = (float)(2 * phase * Pi2Recip - 1);

					phase += phaseStep;
					if (phase > Pi2) phase -= Pi2;
				}
				break;
			case Waveform.Square:
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] = (phase < Pi) ? 1 : -1;

					phase += phaseStep;
					if (phase > Pi2) phase -= Pi2;
				}
				break;
			case Waveform.Sine:
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] = (float)Math.Sin(phase);

					phase += phaseStep;
					if (phase > Pi2) phase -= Pi2;
				}
				break;
			case Waveform.SampleHold:
				for (int i = 0; i < buffer.Length; i++) {
					buffer[i] = (float)sampleHoldRandom;
					phase += phaseStep;
					if (phase > Pi2) {
						phase -= Pi2;

						sampleHoldRandom = 1 - Rand.NextDouble() * 2;
					}
				}
				break;
			default:
				throw new Exception("Incorrect Waveform");
			}
		}
	}
}