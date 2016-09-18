﻿namespace Midif.Synth {
	public interface IMixer {
		void Init (double sampleRate);


		void Render (ref double sample);

		void Render (ref double sampleL, ref double sampleR);
	}
}