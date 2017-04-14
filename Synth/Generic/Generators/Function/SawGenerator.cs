namespace Midif.Synth {
	public sealed class SawGenerator : MidiGenerator {
		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = CalcPhaseStep(note, Transpose, Tune);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				phase += phaseStep;
				if (phase > 1) phase %= 1;

				return RenderCache = 2 * (phase - (int)(phase + 0.5));
			}

			return RenderCache;
		}

		public override void Process (float[] buffer) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (float)(2 * (phase - (int)(phase + 0.5)));

				phase += phaseStep;
				if (phase >= 1) phase -= 1;
			}
		}
	}
}