using System;

namespace Midif.V2 {
	public unsafe struct SynthTable {
		public float pi;
		public float pi2;

		public const float VelcRecip = 1f / 127f;

		public fixed float note2Freq[128];
		public fixed float bend2Pitch[128];

		public fixed float volm2Gain[128];
		public fixed float pan2Left[128];
		public fixed float pan2Right[128];

		public static void Init(SynthTable *self) {
			self->pi = (float)Math.PI;
			self->pi2 = (float)(2 * Math.PI);

			for (int i = 0; i < 128; i++) {
				self->note2Freq[i] = (float)(440 * Math.Pow(2, (i - 69) / 12.0));
				self->bend2Pitch[i] = (float)Math.Pow(2, 2 * ((i - 64) / 127) / 12.0);

				self->volm2Gain[i] = (float)Deci2Gain(40.0 * Math.Log10(i / 127.0));
				self->pan2Left[i] = (float)Deci2Gain(20.0 * Math.Log10(Math.Cos(Math.PI / 2 * (i / 127.0))));
				self->pan2Right[i] = (float)Deci2Gain(20.0 * Math.Log10(Math.Sin(Math.PI / 2 * (i / 127.0))));
			}
		}

		public static double Deci2Gain (double db) {
			return Math.Pow(10.0, (db / 10.0));
		}
	}
}

