using System;

namespace Midif.Synth {
	public class TriangleTableGenerator : MidiComponent {
		const int TableLength = 0x800;
		const int TableMod = 0x7FF;
		static readonly double[] Table;

		static TriangleTableGenerator () {
			Table = new double[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = Math.Abs(((4.0 * i / TableLength + 3) % 4) - 2) - 1;
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