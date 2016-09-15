namespace Midif.Synth {
	public class MidiSequencer : ISequencer, IMetaEventHandler {
		public event MidiEventHandler OnProcessMidiEvent;

		public event MetaEventHandler OnProcessMetaEvent;


		double samplesPerMicrosecond;
		double ticksPerSample;

		MidiFile file;

		double tick;

		int midiIndex;
		int sysExIndex;
		int metaIndex;

		public MidiFile File {
			get { return file; }
			set {
				file = value;

				Reset();
			}
		}

		public int Tick {
			get { return (int)tick; }
			set {
				tick = value;

				for (midiIndex = 0; file.MidiEvents[midiIndex].Tick < tick;) midiIndex++;
				for (metaIndex = 0; file.MetaEvents[metaIndex].Tick < tick;) metaIndex++;
			}
		}

		public double InternalTick { get { return Tick; } }

		public int MidiIndex { get { return midiIndex; } }

		public int SysExIndex { get { return sysExIndex; } }

		public int MetaIndex { get { return metaIndex; } }


		public MidiSequencer () {
			OnProcessMetaEvent += MetaEventHandler;
		}

		public void MetaEventHandler (MetaEvent metaEvent) {
			if (metaEvent.Type == MetaEventType.Tempo)
				ticksPerSample = file.TicksPerBeat / (metaEvent.Tempo * samplesPerMicrosecond);
		}

		public void Init (double sampleRate) {
			samplesPerMicrosecond = sampleRate * 0.000001;

			Reset();
		}

		public void Reset () {
			if (file != null)
				ticksPerSample = file.TicksPerBeat / (500000 * samplesPerMicrosecond);

			tick = 0;

			midiIndex = 0;
			sysExIndex = 0;
			metaIndex = 0;
		}

		public void Advance (double count) {
			tick += count * ticksPerSample;

			while (midiIndex < file.MidiEvents.Count && file.MidiEvents[midiIndex].Tick <= tick)
				OnProcessMidiEvent(file.MidiEvents[midiIndex++]);
			while (metaIndex < file.MetaEvents.Count && file.MetaEvents[metaIndex].Tick <= tick)
				OnProcessMetaEvent(file.MetaEvents[metaIndex++]);
		}

		public bool IsFinished () {
			return tick > file.Length;
		}
	}
}