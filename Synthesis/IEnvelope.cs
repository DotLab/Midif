namespace Midif {
	public interface IEnvelope {
		// onTime : Time From On to Off
		// offTime : Time From Off
		double GetEnvelope (double onTime);
		double GetEnvelope (double onTime, double offTime);
		bool IsEnded (double offTime);
	}
}