using System;

namespace Midif.Synth {
	public class WhiteNoiseGenerator : BaseSignalProvider {
		const int SampleLength = 0x10000;
		const int SampleMod = 0xFFFF;

		static  double[] samples;
		uint counter;

		static WhiteNoiseGenerator () {
			var rand = new Random();

			samples = new double[SampleLength];
			for (int i = 0; i < SampleLength; i++)
				samples[i] = rand.NextDouble() * 2 - 1;
		}

		public override double Render () {
			// counter will overflow automatically.
			return samples[counter++ & SampleMod];
		}
	}
}
