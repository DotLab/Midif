using System;

namespace Midif.Synth.Dx7 {
	/// <summary>
	/// Dx7 envelope generator.
	/// https://github.com/google/music-synthesizer-for-android/blob/master/wiki/Dx7Envelope.wiki
	/// </summary>
	public sealed class Dx7Envelope : Dx7Component {
		const int EnvelopeOff = 4;

		// 0..99 -> 0..127
		public static readonly int[] Level2ScaledLevel =
			{
				0, 5, 9, 13, 17, 20, 23, 25, 27, 29, 31, 33, 35, 37, 39,
				41, 42, 43, 45, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
				62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
				81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
				100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114,
				115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127
			};

		public static readonly float[] ScaledLevel2Gain;

		static Dx7Envelope () {
			ScaledLevel2Gain = new float[3841];

			for (var j = 0; j < 3841; j++) {
				// The minimum level seems to be clipped at 3824 counts from full scale.
				// 0..3824 -> -89.864dB..0dB
				var dB = 0.0235 * (j - 3824);
				ScaledLevel2Gain[j] = (float)SynthTable.Deci2Gain(dB);
			}
		}

		/// <summary>
		/// DX7 levels.
		/// 0..99
		/// </summary>
		public int[] Levels;
		/// <summary>
		/// DX7 rates.
		/// 0..99
		/// </summary>
		public int[] Rates;

		/// <summary>
		/// State of the Envelope Generator.
		/// 0 - To Level 1; 
		/// 1 - To Level 2;
		/// 2 - To Level 3 / Stay Level 3;
		/// 3 - To Level 4;
		/// 4 - Finished.
		/// </summary>
		int state = EnvelopeOff;

		// Scaled levels.
		double level;
		double targetLevel;
		double levelStep;

		bool rising;


		public override void NoteOn (byte note, byte velocity) {
			level = 0;
			levelStep = 0;

			IsOn = true;

			SetState(0);
		}

		public override void NoteOff (byte note, byte velocity) {
			IsOn = false;

			SetState(3);
		}

		void SetState (int newState) {
			state = newState;

			if (state < EnvelopeOff) {
				var nextLevel = Levels[state];
				targetLevel = Math.Max(0, (Level2ScaledLevel[nextLevel] << 5) - 224); // 1 -> -192; 99 -> 127 -> 3840
//				targetLevel = Math.Max(0, (Level2ScaledLevel[nextLevel] << 5) - 240); // 1 -> 5 -> 160 -> -80 ; 99 -> 127 -> 4064 -> 3824 -> 0dB
				rising = (targetLevel - level) > 0;

				// An exponential decay corresponds to a linear change in dB units. 
				// First, the R parameter in the patch (range 0..99) is converted to a 6 bit value (0..63), by the formula qrate = (rate * 41) / 64.
				double qrate = ((double)Rates[state] * 41) / 64; // 5 -> 3; 49 -> 31; 99 -> 63

				// The rate of decay is then 0.2819 * 2 ^ (qrate / 4) * (1 + 0.35 * (qrate mod 4)) dB/s. 
				// This is a reasaonably good approximation to 0.28 * 2 ^ (qrate / 4).

				// At a qrate of 0, the amplitude decreases by one step every 4096 samples, in other words halves every 2^20 samples.
				// qrate = 0, decay rate: 0.2819 dB/second, 11.99574 step/second, 1.0007856 step/4096 samples
//				levelStep = Math.Pow(2, qrate / 4) / 2048;
				levelStep = Math.Pow(2, qrate / 4) / 2048 * sampleRateDiff;
			}
		}

		public override bool IsFinished () {
			return state >= EnvelopeOff;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				if (state < 3 || (!IsOn && state < EnvelopeOff)) {
					UpdateLevel();
				}
				
				// DX7 level -> dB -> amplitude
				return RenderCache = ScaledLevel2Gain[(int)(level)];
			}

			return RenderCache;
		}

		public override void Process (float[] buffer) {
//			if (state == EnvelopeOff - 1 && ScaledLevel2Gain[(int)(level)] < 0.01)
//				state = EnvelopeOff;

			for (int i = 0; i < buffer.Length; i++) {
				if (state < 3 || (!IsOn && state < EnvelopeOff))
					UpdateLevel();

				buffer[i] = ScaledLevel2Gain[(int)(level)];
			}
		}

		void UpdateLevel () {
			var curLevel = level;

			if (rising) { // Attack
				// Attack is based on decay, multiplying it by a factor dependent on the current level.
				// In .0235 dB units, this factor is 2 + floor((full scale - current level) / 256). 
				curLevel += levelStep * (2 + (targetLevel - curLevel) / 256);
				if (curLevel >= targetLevel) {
					curLevel = targetLevel;
					SetState(state + 1);
				}
			} else { // Decay
				curLevel -= levelStep;
				if (curLevel <= targetLevel) {
					curLevel = targetLevel;
					SetState(state + 1);
				}
			}

			level = curLevel;
		}
	}
}