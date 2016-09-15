using System;

namespace Midif {
	public enum WaveType {
		Sine,
		Square,
		Sawtooth,
		Triangle,
	}

	static class WaveHelper {
		static double[] frequencyFactorMap = new double[128];
		public static double GetFrequencyFactor (int note) {
			if (frequencyFactorMap[note] == 0) {
				frequencyFactorMap[note] = Math.Pow(2.0, (double)(note - 69) / 12.0);
			}
			return frequencyFactorMap[note];
		}

		public static double GetWave (WaveType waveType, double time) {
			switch (waveType) {
			case WaveType.Sine:
				return GetSine(time);
			case WaveType.Square:
				return GetSquare(time);
			case WaveType.Sawtooth:
				return GetSawtooth(time);
			case WaveType.Triangle:
				return GetTriangle(time);
			}
			throw new Exception("Unable to recognize waveType(WaveType) : " + waveType);
		}
			
		public static double GetSine (double time) {
			return Math.Sin(time * 2.0 * Math.PI);
		}

		static double[] squareTable = {-1, 1};
		public static double GetSquare (double time) {
			//return GetSine(time) > 0 ? 1 : -1;
			//return Math.Sign(GetSine(frequency, time));
			//return (int)time % 2 > 0 ? 1 : -1;
			return squareTable[(int)(time * 2.0) % 2];
		}
		
		public static double GetSawtooth (double time) {
			return (2 * (time - Math.Floor(time + 0.5)));
		}
		
		public static double GetTriangle (double time) {
			return Math.Abs(GetSawtooth(time)) * 2 - 1;
		}
	}
}