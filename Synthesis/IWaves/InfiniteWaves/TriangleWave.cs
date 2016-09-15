using System;

namespace Midif.Wave {
	public class TriangleWave : GenericWave {
		public override double GetWave (double time) {
			time += 0.25;
			return Math.Abs((time - (int)(time + 0.5)) * 4) - 1;
		}
	}
}