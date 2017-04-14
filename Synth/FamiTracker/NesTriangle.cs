namespace Midif.Synth.FamiTracker {
	public sealed class NesTriangle : FamiBase {
		public static readonly float[] WaveTable;

		static NesTriangle () {
			WaveTable = new float[64];
			for (int i = 0; i < 64; i++)
				WaveTable[i] = (float)System.Math.Abs(((4.0 * i / 64 + 3) % 4) - 2) - 1;
		}

		// pitch 2 step 0 ~ 0x7FF
		static double[] stepTable;

		double gain;
		double step;
		double phase;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);
		
			if (stepTable == null) {
				stepTable = new double[0x800];
				for (int i = 0; i < 0x800; i++)
					stepTable[i] = 64 * (ClockFreq / 16 / (i + 1)) * SampleRateRecip; 	
			}
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
		
			step = stepTable[currentPitch];
			phase = 0;
		}

		public override double Render () {
			if ((frameCounter += framesPerSample) > 1) {
				frameCounter = 0;

				AdvanceFrame();

				if (muted) return 0;

				gain = Volm2GainTable[currentVolume];
				step = stepTable[currentPitch];
			}

			if (muted) return 0;
			return gain * WaveTable[(int)(phase += step) & 63];
		}

		public override void Process (float[] buffer) {
			AdvanceFrame();

			if (muted) {
				System.Array.Clear(buffer, 0, buffer.Length);
				return;
			}

			gain = Volm2GainTable[currentVolume];
			step = stepTable[currentPitch];

			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (float)gain * WaveTable[(int)(phase += step) & 63];
				phase += step;
			}
		}
	}
}