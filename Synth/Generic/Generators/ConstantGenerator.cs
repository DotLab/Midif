namespace Midif.Synth {
	public class ConstantGenerator : BaseComponent {
		public double Value;

		public override double Render () {
			return Value;
		}
	}
}