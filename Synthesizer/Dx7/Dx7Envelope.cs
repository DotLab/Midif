using System;

namespace Midif.Synthesizer.Dx7 {
	public class Dx7Envelope {
		const int EnvOff = 4;

		static int[] outputlevel = { 0, 5, 9, 13, 17, 20, 23, 25, 27, 29, 31, 33, 35, 37, 39,
			41, 42, 43, 45, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
			62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
			81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
			100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114,
			115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127
		};

		static double[] outputLUT;

		int state;
		double targetlevel;
		bool rising;
		double qr;

		int[] levels;
		int[] rates;
		double level;
		double i;
		bool down;
		double decayIncrement;

		public Dx7Envelope (int[] levels, int[] rates) {
			if (outputLUT == null) {
				outputLUT = new double[4096];

				for (var j = 0; j < 4096; j++) {
					var dB = (j - 3824) * 0.0235;
					outputLUT[j] = Math.Pow(20, (dB / 20));
				}
			}

			this.levels = levels;
			this.rates = rates;

			level = 0; // should start here
			i = 0;
			down = true;
			decayIncrement = 0;

			Advance(0);
		}

		public void Advance (int newState) {
			state = newState;
			if (state < 4) {
				var newlevel = levels[state];
				targetlevel = Math.Max(0, (outputlevel[newlevel] << 5) - 224); // 1 -> -192; 99 -> 127 -> 3840
				rising = (targetlevel - level) > 0;
				var rateScaling = 0;
				qr = Math.Min(63, rateScaling + ((rates[state] * 41) >> 6)); // 5 -> 3; 49 -> 31; 99 -> 63
				decayIncrement = Math.Pow(2, qr / 4) / 2048;
			}
		}

		public double Render () {
			if (state < 3 || (state < 4 && !down)) {
				var lev = level;
				if (rising) {
					lev += decayIncrement * (2 + (targetlevel - lev) / 256);
					if (lev >= targetlevel) {
						lev = targetlevel;
						Advance(state + 1);
					}
				} else {
					lev -= decayIncrement;
					if (lev <= targetlevel) {
						lev = targetlevel;
						Advance(state + 1);
					}
				}
				level = lev;
			}
			i++;

			// Convert DX7 level -> dB -> amplitude
			return outputLUT[(int)Math.Floor(level)];
		}

		public void NoteOff () {
			down = false;
			Advance(3);
		}

		public bool IsFinished () {
			return state == EnvOff;
		}
	}
}