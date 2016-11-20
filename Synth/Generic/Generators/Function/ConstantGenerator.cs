namespace Midif.Synth {
	public class ConstantGenerator : MidiComponent {
		public double Value;

		public override double Render () {
			return Value;
		}
	}
}