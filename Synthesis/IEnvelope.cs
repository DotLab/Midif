namespace Midif {
	public interface IEnvelope {
		// onTime : Time Since On
		// offTime : Time Since Off
		double GetEnvelope (double onTime);
		double GetEnvelope (double onTime, double offTime);
	}
}