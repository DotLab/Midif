using System;

namespace Midif.Wave {
	public class SineWave : GenericWave {
		public const double PI2 = 2.0 * Math.PI;

		public override double GetWave (double time) {
			return Math.Sin(time * PI2);
		}
	}
}