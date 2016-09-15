using System;

namespace Midif.Wave {
	public class NoiseWave : InfiniteWave {
		public static readonly Random Random = new Random();

		public override double GetWave (double onTime, double offTime, double frequency) {
			double rand;
			lock (Random) {
				rand = Random.NextDouble();
			}
			return rand * 2 - 1;
		}
	}
}