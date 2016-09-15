namespace Midif {
    public class MidiTrack {
        //--Variables
        public uint NoteCount;
        public uint TotalTime;
        public byte[] Programs;
        public byte[] DrumPrograms;
        public MidiEvent[] MidiEvents;

        //--Public Properties
        public int EventCount {
            get { return MidiEvents.Length; }
        }

        //--Public Methods
        public MidiTrack () {
            NoteCount = 0;
            TotalTime = 0;
        }

        public bool ContainsProgram (byte program) {
            for (int x = 0; x < Programs.Length; x++) {
                if (Programs[x] == program) {
					return true;
				}
            }

            return false;
        }

        public bool ContainsDrumProgram (byte drumProgram) {
            for (int x = 0; x < DrumPrograms.Length; x++) {
                if (DrumPrograms[x] == drumProgram) {
					return true;
				}
            }

            return false;
        }
    }
}
