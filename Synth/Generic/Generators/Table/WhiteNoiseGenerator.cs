using System;

namespace Midif.Synth {
	public sealed class WhiteNoiseGenerator : MidiGenerator {
		static readonly float[] Table;

		static WhiteNoiseGenerator () {
			var rand = new Random();

			Table = new float[TableLength];
			for (int i = 0; i < TableLength; i++)
				Table[i] = (float)rand.NextDouble() * 2 - 1;
		}


		public double Speed = 1;

		public override void NoteOn (byte note, byte velocity) {
			if (!IsOn) phase = 0;
			phaseStep = Speed * TableLength * CalcPhaseStep(note, Transpose, Tune);

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
