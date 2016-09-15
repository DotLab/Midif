using System;

namespace Midif.Synth.Generic {
	public class ConstantGenerator : BaseSignalProvider {
		public double Value;

		public override double Render (bool flag) {
			return Value;
		}
	}
}

