namespace Midif.Note {
	public class MixSequencer {
		public event MixNoteHandler OnProcessMixNote;

		public readonly MixSequence Sequence;

		double tick;
		int index;

		public int Tick {
			get { return (int)tick; }
			set {
				tick = value;

				for (index = 0; Sequence.Notes[index].Tick < tick;) index++;
			}
		}

		public double InternalTick { get { return tick; } }

		public int NoteIndex { get { return index; } }


		public MixSequencer (MixSequence sequence) {
			Sequence = sequence;
		}

		public void Init () {
			tick = 0;
			index = 0;
		}

		public void AdvanceToTick (double ticks) {
			if (ticks < tick)
				Tick = (int)ticks;
			else
				tick = ticks;

			while (index < Sequence.Notes.Count && Sequence.Notes[index].Tick <= tick)
				OnProcessMixNote(Sequence.Notes[index++]);
		}


		public bool IsFinished () {
			return tick >= Sequence.File.Length;
		}
	}
}