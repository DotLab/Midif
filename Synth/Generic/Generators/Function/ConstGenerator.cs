namespace Midif.Synth {
	public sealed class ConstGenerator : MidiGenerator {
		public float Constant = 1;

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
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = Constant;
		}
	}
}