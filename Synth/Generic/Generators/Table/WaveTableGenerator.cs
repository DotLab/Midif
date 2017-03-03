namespace Midif.Synth {
	public sealed class WaveTableGenerator : MidiGenerator {
		public double[] WaveTable {
			set {
				waveTable = value;

				tableLength = value.Length;
				tableMod = tableLength - 1;
			}
		}

		double[] waveTable;
		int tableLength, tableMod;


		public override void Init (double sampleRate) {
			SampleRateRecip = 1 / sampleRate;
		}

		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = tableLength * CalcPhaseStep(note, Transpose, Tune, SampleRateRecip);

			IsOn = true;
		}

		public override double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;

				return RenderCache = waveTable[(int)(phase += phaseStep) & tableMod];
			}

			return RenderCache;
		}
	}
}