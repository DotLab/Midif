﻿namespace Midif.Synth {
	public abstract class MidiGenerator : MidiComponent {
		protected const int TableLength = 0x80;
		protected const int TableMod = 0x7F;

		/// <summary>
		/// Calculates the phase step with wave length = 1.
		/// </summary>
		/// <returns>The phase step.</returns>
		/// <param name="note">Note.</param>
		/// <param name="transpose">Transpose in semitones.</param>
		/// <param name="tune">Tune in cents.</param>
		/// <param name="sampleRateRecip">Sample rate recipient.</param>
		public static double CalcPhaseStep (byte note, int transpose, int tune) {
			return SynthTable.Note2Freq[note + transpose] * SynthTable.Cent2Pitc[tune + SynthTable.Cent2PitcShif] * SynthConfig.SampleRateRecip;
		}

		public int Transpose;
		public int Tune;

		protected double phaseStep;
		protected double phase;
	}
}