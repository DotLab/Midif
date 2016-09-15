namespace Midif.Synth {
	public abstract class BaseSignalProvider : ISignalProvider {
		public byte Note { get { return note; } }

		public bool IsOn { get { return isOn; } }

		public virtual bool IsActive { get { return isOn; } }


		protected double sampleRate, sampleRateRecip;

		protected byte note, velocity;
		protected bool isOn;


		public virtual void Init (double sampleRate) {
			this.sampleRate = sampleRate;
			sampleRateRecip = 1 / sampleRate;
		}


		public virtual void NoteOn (byte note, byte velocity) {
			this.note = note;
			this.velocity = velocity;

			isOn = true;
		}

		public virtual void NoteOff (byte note, byte velocity) {
			this.note = note;

			isOn = false;
		}


		public abstract double Render ();
	}
}