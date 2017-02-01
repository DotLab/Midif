namespace Midif.Synth {
	public sealed class SineTableGenerator : MidiGenerator {
		const int TableLength = 0x800;
		const int TableMod = 0x7FF;
		static readonly double[] Table;

		static SineTableGenerator () {
			Table = new double[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = System.Math.Sin(2 * System.Math.PI * i / TableLength);
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