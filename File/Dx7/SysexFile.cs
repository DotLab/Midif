using System;
using System.IO;

namespace Midif.File.Dx7 {
	[Serializable]
	public class SysexFile {
		public const int PatchCount = 32;

		public byte StatusByte;
		public byte Id;
		public byte SubStatus;
		public byte FormatNumber;
		public int ByteCount;

		public Patch[] Patches;

		public byte Checksum;
		public byte EndFlag;


		public SysexFile (Stream stream) {
			StatusByte = (byte)stream.ReadByte();
			Id = (byte)stream.ReadByte();
			SubStatus = (byte)stream.ReadByte();
			FormatNumber = (byte)stream.ReadByte();
			ByteCount = stream.ReadByte() << 7 + stream.ReadByte();

			Patches = new Patch[PatchCount];
			for (int i = 0; i < PatchCount; i++)
				Patches[i] = new Patch(stream);

			Checksum = (byte)stream.ReadByte();
			EndFlag = (byte)stream.ReadByte();
		}

		[Serializable]
		public class Patch {
			public Operator[] operators;

			public int[] pitchRates;
			public int[] pitchLevels;
			public int algorithm;
			public int feedback;

			public int lfoSpeed;
			public int lfoDelay;
			public int lfoPitchModDepth;
			public int lfoAmpModDepth;
			public int lfoPitchModSens;
			public int lfoWaveform;
			public int lfoSync;
			public int transpose;

			public string name;

			public double feedbackRatio;

			public Patch (Stream stream) {
				operators = new Operator[6];

				for (int i = 5; i >= 0; i--)
					operators[i] = new Operator(i, stream);

				pitchRates = new [] { stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte() };
				pitchLevels = new [] { stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte() };
				algorithm = stream.ReadByte();
				feedback = stream.ReadByte() & 7;

				lfoSpeed = stream.ReadByte();
				lfoDelay = stream.ReadByte();
				lfoPitchModDepth = stream.ReadByte();
				lfoAmpModDepth = stream.ReadByte();
				var tmp = stream.ReadByte();
				lfoPitchModSens = tmp >> 4;
				lfoWaveform = tmp >> 1 & 7;
				lfoSync = tmp & 1;

				transpose = stream.ReadByte();

				var nameBytes = new byte[10];
				stream.Read(nameBytes, 0, 10);
				name = System.Text.Encoding.ASCII.GetString(nameBytes).Trim();

				feedbackRatio = Math.Pow(2, (feedback - 7));
			}

			public override string ToString () {
				return string.Format("[Dx7Patch: algorithm={0}, transpose={1}, name={2}]", algorithm, transpose, name);
			}
		}

		[Serializable]
		public class Operator {
			static readonly double[] OUTPUT_LEVEL_TABLE =
				{
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

			public int idx;
			public int pan;
			public bool enabled;

			public int[] rates;
			public int[] levels;
			public int keyScaleBreakpoint;
			public int keyScaleDepthL;
			public int keyScaleDepthR;
			public int keyScaleCurveL;
			public int keyScaleCurveR;
			public int keyScaleRate;
			public int detune;
			public int lfoAmpModSens;
			public int velocitySens;
			public int volume;
			public int oscMode;
			public int freqCoarse;
			public int freqFine;

			public double freqRatio;
			public double freqFixed;

			public double outputLevel;
			public double ampLeft;
			public double ampRight;


			public Operator (int i, Stream stream) {
				idx = i;
				pan = ((idx + 1) % 3 - 1) * 25; // Alternate panning: -25, 0, 25, -25, 0, 25
				enabled = true;

				int tmp;
				rates = new [] { stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte() };
				levels = new [] { stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte() };
				keyScaleBreakpoint = stream.ReadByte();
				keyScaleDepthL = stream.ReadByte();
				keyScaleDepthR = stream.ReadByte();
				tmp = stream.ReadByte();
				keyScaleCurveL = tmp & 3;
				keyScaleCurveR = tmp >> 2;
				tmp = stream.ReadByte();
				keyScaleRate = tmp & 7;
				detune = tmp >> 3 - 7; // OSC DETUNE     0-14
				tmp = stream.ReadByte();
				lfoAmpModSens = tmp & 3; // Lo 2 bits
				velocitySens = tmp >> 2; // Hi 6 bits
				volume = stream.ReadByte();
				tmp = stream.ReadByte();
				oscMode = tmp & 1;
				freqCoarse = tmp >> 1;
				freqFine = stream.ReadByte();

				if (oscMode == 0) {
					var coarse = freqCoarse == 0 ? 0.5 : freqCoarse; // freqCoarse of 0 is used for ratio of 0.5
					freqRatio = coarse * (1 + freqFine / 100);
				} else {
					freqFixed = Math.Pow(10, freqCoarse % 4) * (1 + (freqFine / 99) * 8.772);
				}

				outputLevel = OUTPUT_LEVEL_TABLE[volume] * 1.27;
				ampLeft = Math.Cos(Math.PI / 2 * (pan + 50) / 100);
				ampRight = Math.Sin(Math.PI / 2 * (pan + 50) / 100);
			}
		}
	}
}