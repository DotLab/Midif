using System.Collections.Generic;
using System.Linq;
using Unsaf;

namespace Midif.V3 {
//	[System.Serializable]
	public sealed class NoteSequenceCollection {
//		[System.Serializable]
		public sealed class Note {
			//public int sequence;

			public int start;
			public int end;
			public int duration;

			public float startSeconds;
			public float endSeconds;
			public float durationSeconds;

			public int track;
			public byte channel;
			public byte note;
			public byte velocity;
		}

//		[System.Serializable]
		public sealed class Sequence {
			public int index;

			public int start;
			public int end;

			public int track;
			public int trackGroup;
			public byte channel;
			public byte channelGroup;
			public byte program;
			public byte programGroup;

			public List<Note> notes = new List<Note>();
		}

		public MidiFile file;

		public int start;
		public int end;
		public int noteCount;
		public int[] trackGroups;
		public byte[] channelGroups;
		public byte[] programGroups;
		public List<Sequence> sequences = new List<Sequence>();

		public NoteSequenceCollection(MidiFile file) {
			this.file = file;

			Parse();
		}

		void Parse() {
			var seq = new Sequence();

			// default tempo 120 bpm
			float beatsPerSecond = 2;
			int ticks = 0;
			float seconds = 0;
			byte[] channelPrograms = new byte[16];

			for (int i = 0; i < file.combinedTrack.Length; i++) {
				var e = file.combinedTrack[i];
				byte channel = (byte)(e.status & 0xf);
				seq = SwitchWorkingSequence(seq, e.track, channel, channelPrograms[channel]);
				int tickDiff = e.tick - ticks;
				ticks = e.tick;
				seconds += tickDiff / (beatsPerSecond * file.ticksPerBeat);


				switch (e.status >> 4) {
					case 0x8:  // note off
						//UnityEngine.Debug.LogFormat("off {3} track {0} tick {1} seconds {2}", e.track, e.tick, seconds, e.b1);
						NoteOff(seq, e.tick, seconds, e.b1);
						break;
					case 0x9:  // note on
						//UnityEngine.Debug.LogFormat("on {3} track {0} tick {1} seconds {2}", e.track, e.tick, seconds, e.b1);
						if (e.b2 == 0) NoteOff(seq, e.tick, seconds, e.b1);
						else seq.notes.Add(new Note { track = e.track, channel = channel, note = e.b1, velocity = e.b2, start = e.tick, startSeconds = seconds });
						break;
					case 0xc:  // program change
						//UnityEngine.Debug.LogFormat("prog track {0} tick {1} seconds {2}", e.track, e.tick, seconds);
						channelPrograms[channel] = e.b1;
						seq = SwitchWorkingSequence(seq, e.track, seq.channel, e.b1);
						break;
					case 0xff:  // meta
						if (e.type == 0x51) {  // tempo
							//UnityEngine.Debug.LogFormat("temp track {0} tick {1} seconds {2}", e.track, e.tick, seconds);
							int start = e.dataLoc;
							// 24-bit value specifying the tempo as the number of microseconds per beat
							int microsecondsPerBeat = BitBe.ReadInt24(file.bytes, ref start);
							beatsPerSecond = 1000000f / microsecondsPerBeat;
						}
						break;
				}
			}
			
			if (seq.notes.Count > 0 && !sequences.Contains(seq)) {
				sequences.Add(seq);
			}
			sequences.Sort((a, b) => {
				if (a.track == b.track) {
					if (a.channel == b.channel) {
						return a.program.CompareTo(b.program);
					}
					return a.channel.CompareTo(b.channel);
				}
				return a.track.CompareTo(b.track);
			});

			int trackGroupIndex = 0;
			byte channelGroupIndex = 0;
			byte programGroupIndex = 0;
			var trackGroupDict = new Dictionary<int, int>();
			var channelGroupDict = new Dictionary<byte, byte>();
			var programGroupDict = new Dictionary<byte, byte>();
			start = end = -1;
			for (int i = 0; i < sequences.Count; i++) {
				seq = sequences[i];

				if (!trackGroupDict.ContainsKey(seq.track)) {
					trackGroupDict.Add(seq.track, trackGroupIndex);
					trackGroupIndex += 1;
				}
				if (!channelGroupDict.ContainsKey(seq.channel)) {
					channelGroupDict.Add(seq.channel, channelGroupIndex);
					channelGroupIndex += 1;
				}
				if (!programGroupDict.ContainsKey(seq.program)) {
					programGroupDict.Add(seq.program, programGroupIndex);
					programGroupIndex += 1;
				}
				seq.trackGroup = trackGroupDict[seq.track];
				seq.channelGroup = channelGroupDict[seq.channel];
				seq.programGroup = programGroupDict[seq.program];

				noteCount += seq.notes.Count;

				seq.start = seq.notes[0].start;
				if (start == -1 || seq.start < start) start = seq.start;
				int seqEnd = seq.notes[0].end;
				for (int j = 0; j < seq.notes.Count; j++) {
					var n = seq.notes[j];
					if (seqEnd < n.end) seqEnd = n.end;
					if (n.end == 0) {
						UnityEngine.Debug.LogErrorFormat("Note is never off: {0} tr {1} ch {2} start {3} seq {4} tr {5} ch {6} prog {7}",
							n.note, n.track, n.channel, n.start, seq.index, seq.track, seq.channel, seq.program);
					}
				}
				seq.end = seqEnd;
				if (end == -1 || seq.end > end) end = seq.end;
			}

			trackGroups = trackGroupDict.Keys.ToArray();
			channelGroups = channelGroupDict.Keys.ToArray();
			programGroups = programGroupDict.Keys.ToArray();
		}

		void NoteOff(Sequence seq, int tick, float seconds, byte note) {
			for (int i = seq.notes.Count - 1; i >= 0; i--) {
				var n = seq.notes[i];
				if (n.note == note && n.end == 0 && n.start != tick) {
					n.end = tick;
					n.duration = tick - n.start;
					n.endSeconds = seconds;
					n.durationSeconds = seconds - n.startSeconds;
					//UnityEngine.Debug.LogFormat("  emit {0} start {1} duration {2} seconds {3:F2}", n.note, n.start, n.duration, n.durationSeconds);
					return;
				}
			}
			UnityEngine.Debug.LogErrorFormat("Cannot find the note to turn off: {0} seq {1} tr {2} ch {3} prog {4} tick {5}",
				note, seq.index, seq.track, seq.channel, seq.program, tick);
		}

		Sequence SwitchWorkingSequence(Sequence seq, int track, byte channel, byte program) {
			if (seq.notes.Count > 0 && !sequences.Contains(seq)) {
				sequences.Add(seq);
			}
			return FindOrCreateSequence(track, channel, program);
		}

		Sequence FindOrCreateSequence(int track, byte channel, byte program) {
			foreach (var sequence in sequences) {
				if (sequence.track == track && sequence.channel == channel && sequence.program == program) {
					return sequence;
				}
			}
			return new Sequence{track = track, channel = channel, program = program};
		}
	}
}

