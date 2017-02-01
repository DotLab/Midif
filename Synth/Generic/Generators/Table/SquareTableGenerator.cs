namespace Midif.Synth {
	public sealed class SquareTableGenerator : MidiGenerator {
		const int TableLength = 2;
		const int TableMod = 1;
		static readonly double[] Table = { 1, -1 };


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