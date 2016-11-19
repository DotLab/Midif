namespace Midif.Synth {
	public interface ISynth : IMidiEventHandler {
		void Init (double sampleRate);

		void Reset ();


		double RenderLeft (bool flag);

		double RenderRight (bool flag);
	}
}