namespace Midif.Synth {
	public class ConstantGenerator : BaseSignalProvider {
		public double Value;

		public override double Render () {
			return Value;
		}
	}
}