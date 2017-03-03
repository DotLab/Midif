namespace Midif.Synth {
	public sealed class TriangleGenerator : MidiGenerator {
		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 3;
			phaseStep = 4 * CalcPhaseStep(note, Transpose, Tune, SampleRateRecip);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				phase += phaseStep;
				if (phase > 4) phase %= 4;
				
				return RenderCache = System.Math.Abs(phase - 2) - 1;
			}

			return RenderCache;
		}
	}
}