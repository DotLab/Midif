using System;
using System.Collections.Generic;

namespace Midif.Note {
	public class MixSequence {
		public readonly MidiTrack Track;
		public readonly MidiChannel Channel;

		public readonly MidiFile File;

		public readonly List<MixNote> Notes = new List<MixNote>();


		public MixSequence (
			MidiFile midiFile, MidiTrack track = MidiTrack.All, MidiChannel channel = MidiChannel.All) {
			File = midiFile;

			Track = track;
			Channel = channel;

			var cachedEvents = new LinkedList<MidiEvent>();
			foreach (var midiEvent in midiFile.MidiEvents)
				if (midiEvent.Type == MidiEventType.NoteOn) {
					if (
						(midiEvent.MidiTrack & track) == midiEvent.MidiTrack &&
						(midiEvent.MidiChannel & channel) == midiEvent.MidiChannel)
						cachedEvents.AddLast(midiEvent);
				} else if (midiEvent.Type == MidiEventType.NoteOff)
					foreach (var cachedEvent in cachedEvents) {
						if (
							cachedEvent.Note == midiEvent.Note &&
							cachedEvent.Channel == midiEvent.Channel &&
							cachedEvent.Track == midiEvent.Track) {
							Notes.Add(
								new MixNote(
									cachedEvent.Track,
									cachedEvent.Channel,
									cachedEvent.Note, 
									cachedEvent.Velocity,
									cachedEvent.Tick,
									midiEvent.Tick));

							cachedEvents.Remove(cachedEvent);
							break;
						}
					}

			Notes.Sort();

//			if (cachedEvents.Count > 0)
//				throw new Exception("Unfinished NoteOn : " + cachedEvents.Count);
		}
	}
}

