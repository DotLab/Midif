namespace Midif.Synth {
	public abstract class BaseSignalProvider : ISignalProvider {
		public virtual void Init (double sampleRate) {
		}


		public virtual void NoteOn (byte note, byte velocity) {
		}

		public virtual void NoteOff (byte velocity) {
		}

		public virtual bool IsActive () {
			return false;
		}


		public abstract double Render (bool flag);
	}
}

