using System.Collections.Generic;

namespace Midif {
	public class MidiNoteSequence {
		MidiSequence midiSequence;

		uint sampleRate;
		uint sampleLength;

		// Sample to MidiEventsList
		Dictionary<uint, List<MidiNote>> sampleDictionary;
		List<uint> samples;

		public MidiSequence MidiSequence {
			get { return midiSequence; }
		}
		public uint SampleLength {
			get { return sampleLength; }
		}
		public uint SampleRate {
			get { return sampleRate; }
		}
		public uint[] Samples {
			get { return samples.ToArray(); }
		}
		
		public MidiNoteSequence (MidiSequence midiSequence) {
			this.midiSequence = midiSequence;
		}
		
		public MidiNoteSequence (MidiSequence midiSequence, uint sampleRate) {
			this.midiSequence = midiSequence;
			this.sampleRate = sampleRate;
			
			ParseSampleDictionary(sampleRate);
		}

		public void SetSampleRate (uint newSampleRate) {
			if (sampleRate != newSampleRate) {
				sampleRate = newSampleRate;
				ParseSampleDictionary(sampleRate);
			}
		}
		
		void ParseSampleDictionary (uint sampleRate) {
			sampleDictionary = new Dictionary<uint, List<MidiNote>>();

			uint sample = 0;
			double ppqn = midiSequence.MidiFile.MidiHeader.PulsesPerQuaterNote;
			double mspqn = 500000;
			double spms = (double)sampleRate / 1000000.0;
			double spp = spms * (mspqn / ppqn);
			
			double lastTime = 0;
			var openNoteList = new LinkedList<MidiNote>();
			foreach (var pulse in midiSequence.Pulses) {
				MidiEvent[] midiEventsList = midiSequence.GetMidiEventsAtPulse(pulse);

				sample += (uint)(((double)midiEventsList[0].AbsoluteTime - lastTime) * spp);
				lastTime = midiEventsList[0].AbsoluteTime;

				foreach (var midiEvent in midiEventsList) {
					if (midiEvent.ChannelEventType == MidiChannelEventType.NoteOn) {
						var newOpenNote = new MidiNote(midiEvent.Track, midiEvent.Channel, midiEvent.Parameter1, midiEvent.Parameter2);
						newOpenNote.SetStartSample(sample);
						openNoteList.AddLast(newOpenNote);
						if (!sampleDictionary.ContainsKey(sample)) {
							sampleDictionary.Add(sample, new List<MidiNote>());
						}
						sampleDictionary[sample].Add(newOpenNote);
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

			samples = new List<uint>(sampleDictionary.Keys);
			samples.Sort();
		}

		public bool HasMidiNoteAtSample (uint sample) {
			return sampleDictionary.ContainsKey(sample);
		}
		
		public MidiNote[] GetMidiNotesAtSample (uint sample) {
			return sampleDictionary[sample].ToArray();
		}
	}
}
