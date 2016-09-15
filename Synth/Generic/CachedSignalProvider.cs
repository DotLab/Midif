namespace Midif.Synth {
	public abstract class CachedSignalProvider : BaseSignalProvider {
		protected bool flag;
		protected double cachedSample;

		public override double Render (bool flag) {
			if (this.flag != flag) {
				this.flag = flag;
				cachedSample = Render();
			}
				
			return cachedSample;
		}

		public abstract double Render ();
	}
}

