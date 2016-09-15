using System;

namespace Midif.Synth {
	public static class SynthConstants {
		public static readonly double[] Recip127;

		public static readonly double[] Note2Freq;

		public static readonly double[] Semitone2Pitch;

		public static readonly double[] Cent2Pitch;

		public static readonly int SampleRate;


		static SynthConstants () {
			Recip127 = new double[128];
			for (int i = 0; i < Recip127.Length; i++)
				Recip127[i] = (double)i / 127;

			Note2Freq = new double[256];
			for (int i = 0; i < Note2Freq.Length; i++)
				Note2Freq[i] = 440.0 * Math.Pow(2, (i - 69) / 12.0);

			Semitone2Pitch = new double[256];
			for (int i = 0; i < Semitone2Pitch.Length; i++)
				Semitone2Pitch[i] = Math.Pow(2, (i - 127) / 12.0);

			Cent2Pitch = new double[256];
			for (int i = 0; i < Cent2Pitch.Length; i++)
				Cent2Pitch[i] = Math.Pow(2, (i - 127) / 1200.0);

			var config = UnityEngine.AudioSettings.GetConfiguration();
			SampleRate = config.sampleRate;
		}


		public static double Cents2Pitch (int cents) {
			return Semitone2Pitch[cents / 100 + 127] * Cent2Pitch[cents % 100 + 127];
		}

		public static double Decibel2Gain (double dB) {
			return Math.Pow(10.0, (dB / 20.0));
		}
	}
}

