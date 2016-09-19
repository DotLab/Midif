using System;

namespace Midif.Synth {
	public static class SynthTable {
		public const int Note2FreqLeng = 0x80;
		public const int Semi2PitcLeng = 0x100;
		public const int Cent2PitcLeng = 0x100;

		public const int Semi2PitcShif = 0x80;
		public const int Cent2PitcShif = 0x80;

		public static readonly double[] Note2Freq;
		public static readonly double[] Semi2Pitc;
		public static readonly double[] Cent2Pitc;

		static SynthTable () {
			Note2Freq = new double[Note2FreqLeng];
			for (int i = 0; i < Note2FreqLeng; i++)
				Note2Freq[i] = 440 * Math.Pow(2, (i - 69) / 12.0);

			Semi2Pitc = new double[Semi2PitcLeng];
			for (int i = 0; i < Semi2PitcLeng; i++) {
				Semi2Pitc[i] = Math.Pow(2, (i - Semi2PitcShif) / 12.0);
			}

			Cent2Pitc = new double[Cent2PitcLeng];
			for (int i = 0; i < Cent2PitcLeng; i++) {
				Cent2Pitc[i] = Math.Pow(2, (i - Cent2PitcShif) / 1200.0);
			}
		}

		public static double Deci2Gain (double dB) {
			return Math.Pow(10.0, (dB / 20.0));
		}

		public static int Clmp2Note (int note) {
			return 
				note < 0 ? 0 :
				note > Note2FreqLeng - 1 ? Note2FreqLeng - 1 :
				note;
		}
	}
}
