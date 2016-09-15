namespace Midif.Sequencer {
	public class MidiSequencer : ISequencer, IMetaEventHandler {
		#region Events

		public delegate void MidiEventDelegate (MidiEvent midiEvent);

		public event MidiEventDelegate OnMidiEvent;

		public delegate void SysExEventDelegate (SysExEvent sysExEvent);

		public event SysExEventDelegate OnSysExEvent;

		public delegate void MetaEventDelegate (MetaEvent metaEvent);

		public event MetaEventDelegate OnMetaEvent;

		#endregion

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
				for (sysExIndex = 0; file.SysExEvents[sysExIndex].Tick < tick;) sysExIndex++;
				for (metaIndex = 0; file.MetaEvents[metaIndex].Tick < tick;) metaIndex++;
			}
		}

		public double InternalTick { get { return Tick; } }

		public int MidiIndex { get { return midiIndex; } }

		public int SysExIndex { get { return sysExIndex; } }

		public int MetaIndex { get { return metaIndex; } }


		public MidiSequencer () {
			OnMetaEvent += MetaEventHandler;
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

			for (; midiIndex < file.MidiEvents.Count && file.MidiEvents[midiIndex].Tick <= tick; midiIndex++)
				OnMidiEvent(file.MidiEvents[midiIndex]);
			if (OnSysExEvent != null)
				for (; sysExIndex < file.SysExEvents.Count && file.SysExEvents[sysExIndex].Tick <= tick; sysExIndex++)
					OnSysExEvent(file.SysExEvents[sysExIndex]);
			for (; metaIndex < file.MetaEvents.Count && file.MetaEvents[metaIndex].Tick <= tick; metaIndex++)
				OnMetaEvent(file.MetaEvents[metaIndex]);
		}

		public bool IsFinished () {
			return tick > file.Length;
		}

		public void MetaEventHandler (MetaEvent metaEvent) {
			if (metaEvent.Type == MetaEventType.Tempo)
				ticksPerSample = file.TicksPerBeat / (metaEvent.Tempo * samplesPerMicrosecond);
		}
	}
}