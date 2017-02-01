namespace Midif.Synth {
	public sealed class SineGenerator : MidiGenerator {
		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = 2 * System.Math.PI * CalcPhaseStep(note, Transpose, Tune, SampleRateRecip);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;
				return RenderCache = System.Math.Sin(phase += phaseStep);
			}

			return RenderCache;
		}
	}
}