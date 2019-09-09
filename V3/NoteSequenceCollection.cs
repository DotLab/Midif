using System.Collections.Generic;
using System.Linq;

namespace Midif.V3 {
	[System.Serializable]
	public sealed class NoteSequenceCollection {
		[System.Serializable]
		public class Note {
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
			public byte channel;
			public byte program;

			public List<Note> notes = new List<Note>();
		}

		public MidiFile file;

		public int noteCount;
		public int[] tracks;
		public byte[] channels;
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
					int channel = e.status & 0xf;
					seq = SwitchWorkingSequence(seq, i, (byte)channel, seq.program);

					tick += e.delta;
					switch (e.status >> 4) {
					case 0x8:  // note off
						NoteOff(seq, tick, e.b1);
						break;
					case 0x9:  // note on
						if (e.b2 == 0) NoteOff(seq, tick, e.b1);
						else seq.notes.Add(new Note{note = e.b1, velocity = e.b2, start = tick});
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

			var trackSet = new HashSet<int>();
			var channelSet = new HashSet<byte>();
			for (int i = 0; i < sequences.Count; i++) {
				seq = sequences[i];
				noteCount += seq.notes.Count;
				trackSet.Add(seq.track);
				channelSet.Add(seq.channel);

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
			tracks = trackSet.ToArray();
			channels = channelSet.ToArray();
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

