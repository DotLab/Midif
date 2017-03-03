namespace Midif.Synth {
	public sealed class SawTableGenerator : MidiGenerator {
		const int TableLength = 0x800;
		const int TableMod = 0x7FF;
		static readonly double[] Table;

		static SawTableGenerator () {
			Table = new double[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = 2 * (((double)i / TableLength) - (int)(((double)i / TableLength) + 0.5));
		}


		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = TableLength * CalcPhaseStep(note, Transpose, Tune, SampleRateRecip);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				return RenderCache = Table[(int)(phase += phaseStep) & TableMod];
			}

			return RenderCache;
		}
	}
}