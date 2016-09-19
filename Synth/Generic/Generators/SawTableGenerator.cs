namespace Midif.Synth {
	public class SawTableGenerator : BaseComponent {
		const int TableLength = 0x800;
		const int TableMod = 0x7FF;
		static readonly double[] Table;

		static SawTableGenerator () {
			Table = new double[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = 2 * (((double)i / TableLength) - (int)(((double)i / TableLength) + 0.5));
		}

		public int Transpose;
		public int Tune;

		double phaseStep;
		double phase;


		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			phaseStep = TableLength * SynthTable.Note2Freq[note + Transpose] * SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] * sampleRateRecip;
			phase = 0;
		}

		public override double Render () {
			return Table[(int)(phase += phaseStep) & TableMod];
		}
	}
}