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

		public int Polyphony;

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
//			DebugConsole.Log(string.Format("Rebase TicksPerBeat from {0} to {1}.", TicksPerBeat, TicksPerBeat / cdTick));

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

		public void Trim () {
			Length = MidiEvents[MidiEvents.Count - 1].Tick + TicksPerBeat;
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
		
			return time;
		}

		public int GetMaxPolyphony () {
			var notes = new List<int>();
			var maxPolyphony = 0;
	
			foreach (var midiEvent in MidiEvents)
				if (midiEvent.Type == MidiEventType.NoteOn) {
					notes.Add(midiEvent.Note);
					if (notes.Count > maxPolyphony)
						maxPolyphony = notes.Count;
				} else if (midiEvent.Type == MidiEventType.NoteOff)
					notes.Remove(midiEvent.Note);

			if (notes.Count != 0)
				throw new System.Exception("Midi Notes not Off.");

			return maxPolyphony;
		}


		public override string ToString () {
			return string.Format("[MidiFile: Format={0}, NumberOfTracks={1}, TicksPerBeat={2}, Length={3}]", Format, NumberOfTracks, TicksPerBeat, Length);
		}
	}
}

