namespace Midif.Synth {
	public interface ISignalProvider : ISynthesizer {
		byte Note { get; }

		bool IsOn { get; }

		bool IsActive { get; }
	}
}