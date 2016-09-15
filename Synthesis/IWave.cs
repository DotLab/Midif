namespace Midif {
	public interface IWave {
		double GetWave (double onTime, double offTime, double frequency);

		bool IsEnded (double offTime);
	}
}