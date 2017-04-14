namespace Midif.Synth {
	[System.Serializable]
	public abstract class MidiComponent {
		public double SampleRate, SampleRateRecip;

		public byte Note, Velocity;
		public bool IsOn;

		public bool RenderFlag;
		public double RenderCache;

		/// <summary>
		/// Initialize the component using a specified sampleRate.
		/// May or may not set SampleRate or SampleRateRecip.
		/// </summary>
		/// <param name="sampleRate">Sample rate.</param>
		public virtual void Init (double sampleRate) {
			SampleRate = sampleRate;
			SampleRateRecip = 1 / sampleRate;
		}

		/// <summary>
		/// Turn on the note.
		/// Must set IsOn.
		/// </summary>
		/// <param name="note">Note.</param>
		/// <param name="velocity">Velocity.</param>
		public virtual void NoteOn (byte note, byte velocity) {
			IsOn = true;
			Note = note;
			Velocity = velocity;
		}

		/// <summary>
		/// Turn off the note.
		/// Must set IsOn.
		/// </summary>
		/// <param name="note">Note.</param>
		/// <param name="velocity">Velocity.</param>
		public virtual void NoteOff (byte note, byte velocity) {
			IsOn = false;
		}

		/// <summary>
		/// Determines whether this component is finished.
		/// Assume that IsOn == false; the component is turned off.
		/// The upper component is responsable for checking IsOn.
		/// </summary>
		/// <returns><c>true</c> if this component is finished; otherwise, <c>false</c>.</returns>
		public virtual bool IsFinished () {
			return true;
		}

		/// <summary>
		/// Render the component.
		/// Must check RenderFlag and set RenderCache.
		/// </summary>
		/// <param name="flag">Flip the flag to render new frame.</param>
		public virtual double Render (bool flag) {
			if (flag ^ RenderFlag) {
				RenderFlag = flag;
				return RenderCache = 0;
			}

			return RenderCache;
		}

		public abstract void Process (float[] buffer);
	}
}