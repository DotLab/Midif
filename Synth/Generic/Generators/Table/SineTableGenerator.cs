namespace Midif.Synth {
	public sealed class SineTableGenerator : MidiGenerator {
		static readonly float[] Table;

		static SineTableGenerator () {
			Table = new float[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = (float)System.Math.Sin(2 * System.Math.PI * i / TableMod);
		}


		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = TableLength * CalcPhaseStep(note, Transpose, Tune);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				return RenderCache = Table[(int)(phase += phaseStep) & TableMod];
			}

			return RenderCache;
		}

		public override void Process (float[] buffer) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = Table[(int)phase & TableMod];

				phase += phaseStep;
				if (phase >= TableLength) phase -= TableLength;
			}
		}
	}
}