namespace Midif.Wave {
	public class SquareWave : GenericWave {
		public static readonly double[] WaveTable = { 1, -1 };

		public override double GetWave (double time) {
			return WaveTable[(int)(time * 2) % 2];
			//return time - (int)time < 0.5 ? 1 : -1;
		}
	}
}