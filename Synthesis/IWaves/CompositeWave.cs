namespace Midif.Wave {
	public class CompositeWave : IWave {
		public IWave WaveA;
		public IWave WaveB;

		public double A {
			get { return a; }
			set {
				a = value;
				b = 1 - value;
			}
		}

		public double B {
			get { return b; }
			set {
				b = value;
				a = 1 - value;
			}
		}

		double a;
		double b;

		public CompositeWave (IWave waveA, IWave waveB, double a) {
			WaveA = waveA;
			WaveB = waveB;
			A = a;
		}

		public double GetWave (double onTime, double offTime, double frequency) {
			return WaveA.GetWave(onTime, offTime, frequency) * A + WaveB.GetWave(onTime, offTime, frequency) * B;
		}

		public bool IsEnded (double offTime) {
			return WaveA.IsEnded(offTime) && WaveB.IsEnded(offTime);
		}
	}
}