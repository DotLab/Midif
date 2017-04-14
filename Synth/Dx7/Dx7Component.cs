namespace Midif.Synth.Dx7 {
	public abstract class Dx7Component : MidiComponent {
		public static readonly double Pi = System.Math.PI;
		public static readonly double Pi2 = Pi * 2;
		public static readonly double Pi2Recip = 1 / Pi2;

		public const double Dx7SampleRate = 49096;

		/// <summary>
		/// The sample rate difference.
		/// Dx7SampleCount * sampleRateDiff = SampleCount.
		/// </summary>
		protected double sampleRateDiff, sampleRateDiffRecip;

		public override void Init (double sampleRate) {
			SampleRate = sampleRate;
			SampleRateRecip = 1 / sampleRate;

			sampleRateDiff = Dx7SampleRate / sampleRate;
			sampleRateDiffRecip = sampleRate / Dx7SampleRate;
		}
	}
}