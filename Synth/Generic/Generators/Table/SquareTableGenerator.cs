namespace Midif.Synth {
	public class SquareTableGenerator : MidiComponent {
		const int TableLength = 0x2;
		const int TableMod = 0x1;
		static readonly double[] Table = { -1, 1 };

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