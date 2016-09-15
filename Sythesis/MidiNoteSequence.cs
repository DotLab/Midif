using System.Collections.Generic;

namespace Midif {
	public class MidiNoteSequence {
		int sampleRate;
		uint sampleLength;
		
		MidiSequence midiSequence;

		// Sample to MidiEventsList
		Dictionary<uint, List<MidiNote>> sampleDictionary;

		public MidiSequence MidiSequence {
			get { return midiSequence; }
		}
		public uint SampleLength {
			get { return sampleLength; }
		}
		public int SampleRate {
			get { return sampleRate; }
			set {
				if (sampleRate != value) {
					sampleRate = value;
					ParseSampleDictionary(sampleRate);
				}
			}
		}
		
		public MidiNoteSequence (MidiSequence midiSequence) {
			this.midiSequence = midiSequence;
		}
		
		public MidiNoteSequence (MidiSequence midiSequence, int sampleRate) {
			this.midiSequence = midiSequence;
			this.sampleRate = sampleRate;
			
			ParseSampleDictionary(sampleRate);
		}
		
		void ParseSampleDictionary (int sampleRate) {
			sampleDictionary = new Dictionary<uint, List<MidiNote>>();

			uint sample = 0;
			double ppqn = midiSequence.MidiFile.MidiHeader.PulsesPerQuaterNote;
			double mspqn = 500000;
			double spms = (double)sampleRate / 1000000.0;
			double spp = spms * (mspqn / ppqn);
			
			double lastTime = 0;
			LinkedList<MidiNote> openNoteList = new LinkedList<MidiNote>();
			foreach (var pulse in midiSequence.Pulses) {
				MidiEvent[] midiEventsList = midiSequence.GetMidiEventsAtPulse(pulse);

				sample += (uint)((midiEventsList[0].AbsoluteTime - lastTime) * spp);
				lastTime = midiEventsList[0].AbsoluteTime;

				foreach (var midiEvent in midiEventsList) {
					if (midiEvent.ChannelEventType == MidiChannelEventType.NoteOn) {
						MidiNote newMidiNote = new MidiNote(midiEvent.Track, midiEvent.Channel, midiEvent.Parameter1, midiEvent.Parameter2, sample);
						openNoteList.AddLast(newMidiNote);
						if (!sampleDictionary.ContainsKey(sample)) {
							sampleDictionary.Add(sample, new List<MidiNote>());
						}
						sampleDictionary[sample].Add(newMidiNote);
					} else if (midiEvent.ChannelEventType == MidiChannelEventType.NoteOff) {
						foreach (var openNote in openNoteList) {
							if (openNote.Track == midiEvent.Track && openNote.Channel == midiEvent.Channel && openNote.Note == midiEvent.Parameter1) {
								openNote.SetEndSample(sample);
								openNoteList.Remove(openNote);
								break;
							}
						}

					} else if (midiEvent.MetaEventType == MidiMetaEventType.EndOfTrack) {
						foreach (var openNote in openNoteList) {
							openNote.SetEndSample(sample);
						}
						
						sampleLength = sample;
					} else if (midiEvent.MetaEventType == MidiMetaEventType.Tempo) {
						mspqn = System.Convert.ToUInt32(midiEvent.Parameters[0]);
						spp = spms * (mspqn / ppqn);
					}
				}
			}

			if (sampleLength == 0) {
				sampleLength = sample;
			}
		}

		public bool HasMidiNoteAtSample (uint sample) {
			return sampleDictionary.ContainsKey(sample);
		}
		
		public MidiNote[] GetMidiNotesAtSample (uint sample) {
			if (!HasMidiNoteAtSample(sample)) {
				return null;
			}
			return sampleDictionary[sample].ToArray();
		}
	}
}
