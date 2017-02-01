namespace Midif.Synth {
	public abstract class MidiGenerator : MidiComponent {
		/// <summary>
		/// Calculates the phase step with wave length = 1.
		/// </summary>
		/// <returns>The phase step.</returns>
		/// <param name="note">Note.</param>
		/// <param name="transpose">Transpose in semitones.</param>
		/// <param name="tune">Tune in cents.</param>
		/// <param name="sampleRateRecip">Sample rate recipient.</param>
		public static double CalcPhaseStep (byte note, int transpose, int tune, double sampleRateRecip) {
			return SynthTable.Note2Freq[note + transpose] * SynthTable.Cent2Pitc[tune + SynthTable.Cent2PitcShif] * sampleRateRecip;
		}

		public int Transpose;
		public int Tune;

		protected double phaseStep;
		protected double phase;
	}
}