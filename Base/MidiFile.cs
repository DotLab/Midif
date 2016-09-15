using System.Collections.Generic;

namespace Midif {
	public enum MidiFileFormat {
		SingleTrack = 0x00,
		MultiTrack = 0x01,
		MultiSong = 0x02
	}

	[System.Serializable]
	public class MidiFile {
		public MidiFileFormat Format;
		public int NumberOfTracks;
		public int TicksPerBeat;

		public int Length;

		public List<MidiEvent> MidiEvents = new List<MidiEvent>();
		public List<SysExEvent> SysExEvents = new List<SysExEvent>();
		public List<MetaEvent> MetaEvents = new List<MetaEvent>();

		public void Sort () {
			MidiEvents.Sort();
			SysExEvents.Sort();
			MetaEvents.Sort();
		}

		public void Rebase () {
			int cdTick = 0;
			foreach (var trackEvent in MidiEvents) cdTick = Gcd(cdTick, trackEvent.Tick);
			// foreach (var trackEvent in SysExEvents) cdTick = gcd(cdTick, trackEvent.Tick);
			foreach (var trackEvent in MetaEvents) cdTick = Gcd(cdTick, trackEvent.Tick);

			if (cdTick == 1) return;
			DebugConsole.Log(string.Format("Rebase TicksPerBeat from {0} to {1}.", TicksPerBeat, TicksPerBeat / cdTick));

			TicksPerBeat /= cdTick;
			Length /= cdTick;
			foreach (var trackEvent in MidiEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in SysExEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in MetaEvents) trackEvent.Tick /= cdTick;
		}

		static int Gcd (int a, int b) {
			while (b > 0) {
				int temp = b;
				b = a % b; // % is remainder
				a = temp;
			}

			return a;
		}

		public override string ToString () {
			return string.Format("(MidiFile: Format={0}, TicksPerBeat={1}, NumberOfTracks={2}, Length={3})",
				Format, TicksPerBeat, NumberOfTracks, Length);
		}
	}
}

