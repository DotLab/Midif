namespace Midif.Synth {
	public sealed class SawGenerator : MidiGenerator {
		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = CalcPhaseStep(note, Transpose, Tune, SampleRateRecip);

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
	}
}