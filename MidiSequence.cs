using System.Collections.Generic;

namespace Midif {
    public class MidiSequence {
		MidiFile midiFile;

		uint pulseLength;

		// MidiPulse to MidiEventsList
		Dictionary<uint, List<MidiEvent>> pulseDictionary;
		List<uint> pulses;

		public MidiFile MidiFile {
			get{ return midiFile; }
		}
		public uint PulseLength {
			get { return pulseLength; }
		}
		public List<uint> Pulses {
			get { return pulses; }
		}

		public MidiSequence (MidiFile file) {
			this.midiFile = file;

			ParsePulseDictionary();
		}

		void ParsePulseDictionary () {
			pulseDictionary = new Dictionary<uint, List<MidiEvent>>();
			foreach (var midiEvent in midiFile.MidiEvents) {
				if (midiEvent.AbsoluteTime > pulseLength) {
					pulseLength = midiEvent.AbsoluteTime;
				}
				
				if (!pulseDictionary.ContainsKey(midiEvent.AbsoluteTime)) {
					pulseDictionary.Add(midiEvent.AbsoluteTime, new List<MidiEvent>());
				}
				pulseDictionary[midiEvent.AbsoluteTime].Add(midiEvent);
			}

			pulses = new List<uint>(pulseDictionary.Keys);
			pulses.Sort();
		}

		public bool HasMidiEventAtPulse (uint absoluteTime) {
			return pulseDictionary.ContainsKey(absoluteTime);
		}

		public MidiEvent[] GetMidiEventsAtPulse (uint absoluteTime) {
			if (!HasMidiEventAtPulse(absoluteTime)) {
				return null;
			}
			return pulseDictionary[absoluteTime].ToArray();
		}
    }
}
