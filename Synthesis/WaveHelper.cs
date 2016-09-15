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

		public static double GetWave (WaveType waveType, double frequency, double time) {
			switch (waveType) {
			case WaveType.Sine:
				return GetSine(frequency, time);
			case WaveType.Square:
				return GetSquare(frequency, time);
			case WaveType.Sawtooth:
				return GetSawtooth(frequency, time);
			case WaveType.Triangle:
				return GetTriangle(frequency, time);
			}
			throw new Exception("Unable to recognize waveType(WaveType) : " + waveType);
		}
			
		public static double GetSine (double frequency, double time) {
			return Math.Sin(frequency * time * 2.0 * Math.PI);
		}
		
		public static double GetSquare (double frequency, double time) {
			return GetSine(frequency, time) > 0 ? 1 : -1;
		}
		
		public static double GetSawtooth (double frequency, double time) {
			return (2 * (time * frequency - Math.Floor(time * frequency + 0.5)));
		}
		
		public static double GetTriangle (double frequency, double time) {
			return Math.Abs(GetSawtooth(frequency, time)) * 2 - 1;
		}
	}
}