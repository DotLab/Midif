namespace Midif {
	public interface IInstrument {
		double GetSample (int note, double onTime);
		double GetSample (int note, double onTime, double offTime);
	}
}