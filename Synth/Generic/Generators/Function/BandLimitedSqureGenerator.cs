namespace Midif.Synth {
	public sealed class BandLimitedSqureGenerator : MidiGenerator {
		static double Sins(double x, int h) {
			double y = 0;

			for (int i = 1; i <= h; i += 2) {
				y += System.Math.Sin(x * i) / i;
			}

			return y;
		}

		public double P;
		public int H;

		public override void NoteOn(byte note, byte velocity) {
			if (!IsOn) phase = 0;

			P = SampleRate / SynthTable.Note2Freq[note];
			H = (int)(P / 2);

			phaseStep = SynthTable.Pi2 * CalcPhaseStep(note, Transpose, Tune);

			IsOn = true;
		}

		public override double Render(bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				return RenderCache = Sins(phase += phaseStep, H);
			}

			return RenderCache;
		}

		public override void Process(float[] buffer) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (float)Sins(phase, H);

				phase += phaseStep;
				if (phase >= SynthTable.Pi2) phase -= SynthTable.Pi2;
			}
		}
	}
}