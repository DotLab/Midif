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
			foreach (var trackEvent in MidiEvents) cdTick = Mathf.Gcd(cdTick, trackEvent.Tick);
			// foreach (var trackEvent in SysExEvents) cdTick = gcd(cdTick, trackEvent.Tick);
			foreach (var trackEvent in MetaEvents) cdTick = Mathf.Gcd(cdTick, trackEvent.Tick);

			if (cdTick == 1) return;
//			DebugConsole.Log(string.Format("Rebase TicksPerBeat from {0} to {1}.", TicksPerBeat, TicksPerBeat / cdTick));

			TicksPerBeat /= cdTick;
			Length /= cdTick;
			foreach (var trackEvent in MidiEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in SysExEvents) trackEvent.Tick /= cdTick;
			foreach (var trackEvent in MetaEvents) trackEvent.Tick /= cdTick;
		}

		public void Trim () {
			Length = MidiEvents[MidiEvents.Count - 1].Tick + TicksPerBeat;
		}

		public double LengthInSeconds;
		public List<int> Tracks = new List<int>();
		public List<int> Channels = new List<int>();

		public void Analyze () {
			GetLengthInSeconds();

			foreach (var midiEvent in MidiEvents) {
				if (!Tracks.Contains(midiEvent.Track))
					Tracks.Add(midiEvent.Track);

				if (!Channels.Contains(midiEvent.Channel))
					Channels.Add(midiEvent.Channel);
			}

			Tracks.Sort();
			Channels.Sort();
		}

		public double GetLengthInSeconds () {
			const double microsecondPerSecond = 1000000;
			var ticksPerSecond = (double)TicksPerBeat / 500000 * microsecondPerSecond;
			double time = 0;
			int lastTick = 0;

			foreach (var metaEvent in MetaEvents)
				if (metaEvent.Type == MetaEventType.Tempo) {
					time += (metaEvent.Tick - lastTick) / ticksPerSecond;
					lastTick = metaEvent.Tick;
					ticksPerSecond = (double)TicksPerBeat / metaEvent.Tempo * microsecondPerSecond;
				}

			time += (Length - lastTick) / ticksPerSecond;

			return LengthInSeconds = time;
		}

		public override string ToString () {
			return string.Format("[MidiFile: Format={0}, NumberOfTracks={1}, TicksPerBeat={2}, Length={3}]", Format, NumberOfTracks, TicksPerBeat, Length);
		}
	}
}

