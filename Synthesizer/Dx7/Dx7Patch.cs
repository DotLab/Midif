using System;
using System.IO;

namespace Midif.Synthesizer.Dx7 {
	[Serializable]
	public class Dx7Patch {
		[Serializable]
		public class Operator {
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
			public double ampL;
			public double ampR;


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
				lfoAmpModSens = tmp & 3;
				velocitySens = tmp >> 2;
				volume = stream.ReadByte();
				tmp = stream.ReadByte();
				oscMode = tmp & 1;
				freqCoarse = tmp >> 1;
				freqFine = stream.ReadByte();
			}
		}

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

		public double controllerModVal;
		public bool aftertouchEnabled;
		public double fbRatio;

		public Dx7Patch (Stream stream) {
			operators = new Operator[6];

			for (int i = 5; i >= 0; i--)
				operators[i] = new Operator(i, stream);

			pitchRates = new [] { stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte() };
			pitchLevels = new [] { stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte() };
			algorithm = stream.ReadByte() + 1; // start at 1 for readability
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

			controllerModVal = 0;
			aftertouchEnabled = false;
		}
	}
}