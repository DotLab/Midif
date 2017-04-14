namespace Midif.Synth {
	public sealed class SineGenerator : MidiGenerator {
		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = SynthTable.Pi2 * CalcPhaseStep(note, Transpose, Tune);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				return RenderCache = System.Math.Sin(phase += phaseStep);
			}

			return RenderCache;
		}

		public override void Process (float[] buffer) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (float)System.Math.Sin(phase);

				phase += phaseStep;
				if (phase >= SynthTable.Pi2) phase -= SynthTable.Pi2;
			}
		}
	}
}