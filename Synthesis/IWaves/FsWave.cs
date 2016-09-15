using System;

namespace Midif.Wave {
	// Can we move to the Wave?
	public class FsWave : IWave {
		public IWave Wave;

		public double FrequencyShift;
		public double HamoniousShift;
		public bool FollowKey;

		public FsWave (IWave wave, double shiftFrequency, double shiftHamonious, bool followKey = true) {
			Wave = wave;
			FrequencyShift = shiftFrequency;
			HamoniousShift = shiftHamonious;
			FollowKey = followKey;
		}

		public double GetWave (double onTime, double offTime, double frequency) {
			return Wave.GetWave(
				onTime,
				offTime,
				FollowKey ? frequency * Math.Pow(2, HamoniousShift) + FrequencyShift : FrequencyShift);
		}

		public bool IsEnded (double offTime) {
			return Wave.IsEnded(offTime);
		}
	}
}