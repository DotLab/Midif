namespace Midif {
	public interface IInstrument {
		double GetEnvelopedSample (int note, double onTime, double offTime);
		double GetReleaseTime ();
	}
}