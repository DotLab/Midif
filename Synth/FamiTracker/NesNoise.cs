namespace Midif.Synth.FamiTracker {
	public class NesNoise : FamiBase {
		public static readonly double[] PeriodTable = {
			4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068,
		};

		// pitch 2 step 0 ~ 0x7FF
		protected static double[] stepTable;

		protected double gain;
		protected double step;
		protected double phase;

		protected int period;
		protected uint shiftReg = 1;
		protected int noiseMode = 13;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);
		
			if (stepTable == null) {
				stepTable = new double[16];
				for (int i = 0; i < 16; i++)
					stepTable[i] = (ClockFreq / (PeriodTable[i] + 1)) * sampleRateRecip; 	
			}
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
		
			step = stepTable[0xF - (note & 0x0F)];
			phase = 0;
		}

		public override double Render () {
			if ((frameCounter += framesPerSample) > 1) {
				frameCounter = 0;

				AdvanceFrame();

				if (muted) return 0;

				gain = Volm2GainTable[currentVolume];
				step = stepTable[0xF - (currentNote & 0x0F)];

				noiseMode = (currentDuty & 0x80) == 0 ? 13 : 8;
			}
				
			if ((phase += step) > 1) {
				phase = 0;
				shiftReg = (((shiftReg << 14) ^ (shiftReg << noiseMode)) & 0x4000) | (shiftReg >> 1);
			}
			return muted || (shiftReg & 1) == 0 ? 0 : gain;
		}
	}
}