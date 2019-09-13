using System.Collections.Generic;
using System.Linq;

namespace Midif.V3 {
	[System.Serializable]
	public sealed class NoteSequenceCollection {
		[System.Serializable]
		public class Note {
			public byte channel;
			public byte note;
			public byte velocity;

			public int start;
			public int end;
			public int duration;
		}

		[System.Serializable]
		public class Sequence {
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

			for (int i = 0; i < file.tracks.Length; i++) {
				var track = file.tracks[i];
				var tick = 0;
				seq = SwitchWorkingSequence(seq, i, 0, 0);

				for (int j = 0; j < track.Length; j++) {
					var e = track[j];
					byte channel = (byte)(e.status & 0xf);
					seq = SwitchWorkingSequence(seq, i, channel, seq.program);

					tick += e.delta;
					switch (e.status >> 4) {
					case 0x8:  // note off
						NoteOff(seq, tick, e.b1);
						break;
					case 0x9:  // note on
						if (e.b2 == 0) NoteOff(seq, tick, e.b1);
						else seq.notes.Add(new Note{channel = channel, note = e.b1, velocity = e.b2, start = tick});
						break;
					case 0xc:  // program change
						seq = SwitchWorkingSequence(seq, i, seq.channel, e.b1);
						break;
					}
				}
			}

			if (seq.notes.Count > 0 && !sequences.Contains(seq)) {
				sequences.Add(seq);
			}

			int trackGroupIndex = 0;
			byte channelGroupIndex = 0;
			byte programGroupIndex = 0;
			var trackGroupDict = new Dictionary<int, int>();
			var channelGroupDict = new Dictionary<byte, byte>();
			var programGroupDict = new Dictionary<byte, byte>();
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
				int end = seq.notes[0].end;
				for (int j = 0; j < seq.notes.Count; j++) {
					var n = seq.notes[j];
					if (end < n.end) end = n.end;
					if (n.end == 0) {
						UnityEngine.Debug.LogError("Note is never off: " + n.note);
					}
				}
				seq.end = end;
			}

			trackGroups = trackGroupDict.Keys.ToArray();
			channelGroups = channelGroupDict.Keys.ToArray();
			programGroups = programGroupDict.Keys.ToArray();
		}

		void NoteOff(Sequence seq, int tick, byte note) {
			for (int i = seq.notes.Count - 1; i >= 0; i--) {
				var n = seq.notes[i];
				if (n.note == note && n.end == 0) {
					n.end = tick;
					n.duration = tick - n.start;
					return;
				}
			}
			UnityEngine.Debug.LogError("Cannot find the note to turn off: " + note);
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

