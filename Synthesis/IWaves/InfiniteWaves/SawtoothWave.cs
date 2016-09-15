namespace Midif.Wave {
	public class SawtoothWave : GenericWave {
		public override double GetWave (double time) {
			return (time - (int)(time + 0.5)) * 2;
		}
	}
}