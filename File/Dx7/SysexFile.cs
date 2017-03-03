﻿using System;
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

			public double controllerModVal;
			public bool aftertouchEnabled;
			public double fbRatio;

			public Patch (Stream stream) {
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

			public override string ToString () {
				return string.Format("[Dx7Patch: algorithm={0}, transpose={1}, name={2}]", algorithm, transpose, name);
			}
		}

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
	}
}