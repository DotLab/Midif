namespace Midif {
	public interface IInstrument {
		double GetRawSample (int note, double time);

		double GetSample (int note, double onTime);
		double GetSample (int note, double onTime, double offTime);

		bool IsEnded (double offTime);
	}
}