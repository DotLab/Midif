using System;

namespace Midif.Synth {
	public sealed class WhiteNoiseGenerator : MidiGenerator {
		const int TableLength = 0x800;
		const int TableMod = 0x7FF;
		static readonly double[] Table;

		static WhiteNoiseGenerator () {
			var rand = new Random();

			Table = new double[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = rand.NextDouble() * 2 - 1;
		}


		public double Speed = 1;

		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = Speed * TableLength * CalcPhaseStep(note, Transpose, Tune, SampleRateRecip);

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
