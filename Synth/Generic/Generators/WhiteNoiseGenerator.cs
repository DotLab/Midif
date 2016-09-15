using System;

namespace Midif.Synth.Generic {
	public class WhiteNoiseGenerator : CachedSignalProvider {
		static readonly Random rand = new Random();

		public override double Render () {
			return rand.NextDouble() - 0.5;
		}
	}
}