namespace Midif.Synth {
	public interface IComponent : ISynthesizer {
		byte Note { get; }

		bool IsOn { get; }

		bool IsActive { get; }
	}
}