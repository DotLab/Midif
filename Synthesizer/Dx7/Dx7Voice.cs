using System;

namespace Midif.Synthesizer.Dx7 {
	public class Dx7Voice {
		static double[] OUTPUT_LEVEL_TABLE = {
			0.000000, 0.000337, 0.000476, 0.000674, 0.000952, 0.001235, 0.001602, 0.001905, 0.002265, 0.002694,
			0.003204, 0.003810, 0.004531, 0.005388, 0.006408, 0.007620, 0.008310, 0.009062, 0.010776, 0.011752,
			0.013975, 0.015240, 0.016619, 0.018123, 0.019764, 0.021552, 0.023503, 0.025630, 0.027950, 0.030480,
			0.033238, 0.036247, 0.039527, 0.043105, 0.047006, 0.051261, 0.055900, 0.060960, 0.066477, 0.072494,
			0.079055, 0.086210, 0.094012, 0.102521, 0.111800, 0.121919, 0.132954, 0.144987, 0.158110, 0.172420,
			0.188025, 0.205043, 0.223601, 0.243838, 0.265907, 0.289974, 0.316219, 0.344839, 0.376050, 0.410085,
			0.447201, 0.487676, 0.531815, 0.579948, 0.632438, 0.689679, 0.752100, 0.820171, 0.894403, 0.975353,
			1.063630, 1.159897, 1.264876, 1.379357, 1.504200, 1.640341, 1.788805, 1.950706, 2.127260, 2.319793,
			2.529752, 2.758714, 3.008399, 3.280683, 3.577610, 3.901411, 4.254519, 4.639586, 5.059505, 5.517429,
			6.016799, 6.561366, 7.155220, 7.802823, 8.509039, 9.279172, 10.11901, 11.03486, 12.03360, 13.12273
		};

		static Dx7Patch patch;
		static double aftertouch;
		static double mod;
		static double bend;

		static double FrequencyFromNoteNumber (double note) {
			return 440 * Math.Pow(2, (note - 69) / 12);
		}

		public static void SetPatch (Dx7Patch patch) {
			Dx7Lfo.SetPatch(patch);
			Dx7Voice.patch = patch;
		}

		public static void SetFeedback (double value) {
			patch.fbRatio = Math.Pow(2, (value - 7));
		}

		public static void SetOutputLevel (int opIndex, double value) {
			patch.operators[opIndex].outputLevel = MapOutputLevel(value);
		}

		public static void UpdateFrequency (int opIndex) {
			var op = patch.operators[opIndex];
			if (op.oscMode == 0) {
				var freqCoarse = op.freqCoarse == 0 ? 0.5 : op.freqCoarse; // freqCoarse of 0 is used for ratio of 0.5
				op.freqRatio = freqCoarse * (1 + op.freqFine / 100);
			} else {
				op.freqFixed = Math.Pow(10, op.freqCoarse % 4) * (1 + (op.freqFine / 99) * 8.772);
			}
		}

		public static void UpdateLfo () {
			Dx7Lfo.Update();
		}

		public static void SetPan (int opIndex, double value) {
			var op = patch.operators[opIndex];
			op.ampL = Math.Cos(Math.PI / 2 * (value + 50) / 100);
			op.ampR = Math.Sin(Math.PI / 2 * (value + 50) / 100);
		}

		static double MapOutputLevel (double input) {
			var idx = Math.Min(99, Math.Max(0, Math.Floor(input)));
			return OUTPUT_LEVEL_TABLE[(int)idx] * 1.27;
		}

		public static void ChannelAftertouch (double value) {
			aftertouch = value;
			UpdateMod();
		}

		public static void ModulationWheel (double value) {
			mod = value;
			UpdateMod();
		}

		static void UpdateMod () {
			var pressure = patch.aftertouchEnabled ? aftertouch : 0;
			patch.controllerModVal = Math.Min(1.27, pressure + mod);
		}

		public static void PitchBend (double value) {
			bend = value;
		}

		public bool down;
		public int note;
		double frequency;
		double velocity;
		Dx7Operator[] operators;


		public Dx7Voice (int note, double velocity) {
			down = true;

			this.note = note;
			this.velocity = velocity;

			frequency = FrequencyFromNoteNumber(note);

			operators = new Dx7Operator[6];
			for (var i = 0; i < 6; i++) {
				// Not sure about detune.
				// see https://github.com/smbolton/hexter/blob/621202b4f6ac45ee068a5d6586d3abe91db63eaf/src/dx7_voice.c#L789
				// https://github.com/asb2m10/dexed/blob/1eda313316411c873f8388f971157664827d1ac9/Source/msfa/dx7note.cc#L55
				// https://groups.yahoo.com/neo/groups/YamahaDX/conversations/messages/15919
				var opParams = patch.operators[i];
				var op = 
					new Dx7Operator(
						opParams,
						frequency,
						new Dx7Envelope(opParams.levels, opParams.rates),
						new Dx7Lfo(opParams)
					);
				// TODO: DX7 accurate velocity sensitivity map
				op.outputLevel = (1 + (this.velocity - 1) * ((double)opParams.velocitySens / 7)) * opParams.outputLevel;
				operators[i] = op;
			}

			UpdatePitchBend();
		}

		public double[] Render () {
			var algorithmIdx = patch.algorithm - 1;
			var modulationMatrix = Dx7Algorithm.Algorithms[algorithmIdx].modulationMatrix;
			var outputMix = Dx7Algorithm.Algorithms[algorithmIdx].outputMix;
			var outputScaling = 1.0 / outputMix.Length;
			var outputL = 0.0;
			var outputR = 0.0;

			for (var i = 5; i >= 0; i--) {
				var mod = 0.0;

				if (patch.operators[i].enabled) {
					for (int j = 0, length = modulationMatrix[i].Length; j < length; j++) {
						var modulator = modulationMatrix[i][j];
						if (patch.operators[modulator].enabled) {
							var modOp = operators[modulator];
							if (modulator == i) {
								// Operator modulates itself; use feedback ratio
								// TODO: implement 2-sample feedback averaging (anti-hunting filter)
								// http://d.pr/i/1kuZ7/3h7jQN7w
								// https://code.google.com/p/music-synthesizer-for-android/wiki/Dx7Hardware
								// http://music.columbia.edu/pipermail/music-dsp/2006-June/065486.html
								mod += modOp.val * patch.fbRatio;
							} else {
								mod += modOp.val * modOp.outputLevel;
							}
						}
					}
				}

				operators[i].Render(mod);
			}

			for (int k = 0, length = outputMix.Length; k < length; k++) {
				if (patch.operators[outputMix[k]].enabled) {
					var carrier = operators[outputMix[k]];
					var carrierParams = patch.operators[outputMix[k]];
					var carrierLevel = carrier.val * carrier.outputLevel;
					outputL += carrierLevel * carrierParams.ampL;
					outputR += carrierLevel * carrierParams.ampR;
				}
			}

			return new [] { outputL * outputScaling, outputR * outputScaling };
		}

		public void NoteOff () {
			down = false;
			for (int i = 0; i < 6; i++)
				operators[i].NoteOff();
		}

		public void UpdatePitchBend () {
			var frequency = FrequencyFromNoteNumber(note + bend);
			for (int i = 0; i < 6; i++)
				operators[i].UpdateFrequency(frequency);
		}

		public bool IsFinished () {
			var outputMix = Dx7Algorithm.Algorithms[patch.algorithm - 1].outputMix;
			for (var i = 0; i < outputMix.Length; i++)
				if (!operators[outputMix[i]].IsFinished()) return false;
			
			return true;
		}
	}
}