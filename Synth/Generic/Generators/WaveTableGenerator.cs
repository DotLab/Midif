namespace Midif.Synth {
	public class WaveTableGenerator : MidiComponent {
		public int Transpose;
		public int Tune;

		public double[] WaveTable {
			set {
				waveTable = value;

				tableLength = value.Length;
				tableMod = tableLength - 1;
			}
		}

		double[] waveTable, stepTable;
		int tableLength, tableMod;

		double phaseStep;
		double phase;

		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			stepTable = new double[SynthTable.Note2FreqLeng];
			for (int i = 0; i < SynthTable.Note2FreqLeng; i++)
				stepTable[i] = tableLength *
				SynthTable.Note2Freq[SynthTable.Clmp2Note(i + Transpose)] *
				SynthTable.Cent2Pitc[Tune + SynthTable.Cent2PitcShif] *
				sampleRateRecip;
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);

			phaseStep = stepTable[note];
			phase = 0;
		}

		public override double Render () {
			return waveTable[(int)(phase += phaseStep) & tableMod];
		}
	}
}