using Midif.File;

namespace Midif.Sequencer {
	public class MidiSequencer {
		#region Events

		public delegate void MidiEventHandler (MidiEvent midiEvent);

		public event MidiEventHandler OnMidiEvent;

		public delegate void SysExEventHandler (SysExEvent sysExEvent);

		public event SysExEventHandler OnSysExEvent;

		public delegate void MetaEventHandler (MetaEvent metaEvent);

		public event MetaEventHandler OnMetaEvent;

		#endregion

		double samplesPerMicrosecond;

		MidiFile file;

		double tick;
		double ticksPerSample;

		int midiIndex;
		int sysExIndex;
		int metaIndex;

		public MidiFile File {
			get { return file; }
			set {
				file = value;

				ResetTick();
			}
		}

		public int Tick {
			get { return (int)tick; }
			set {
				tick = value;

				for (midiIndex = 0; file.MidiEvents[midiIndex].Time < tick;) midiIndex++;
				for (sysExIndex = 0; file.SysExEvents[sysExIndex].Time < tick;) sysExIndex++;
				for (metaIndex = 0; file.MetaEvents[metaIndex].Time < tick;) metaIndex++;
			}
		}

		public int MidiIndex { get { return midiIndex; } }

		public int SysExIndex { get { return sysExIndex; } }

		public int MetaIndex { get { return metaIndex; } }


		public MidiSequencer (int sampleRate) {
			samplesPerMicrosecond = sampleRate * 0.000001;

			OnMidiEvent += DefaultMidiEventHandler;
			OnSysExEvent += DefaultSysExEventHandler;
			OnMetaEvent += DefaultMetaEventHandler;
		}

		public void ResetTick () {
			ticksPerSample = file.TicksPerBeat / (500000 * samplesPerMicrosecond);

			tick = 0;

			midiIndex = 0;
			sysExIndex = 0;
			metaIndex = 0;
		}

		public void AdvanceTick (double count = 1) {
			tick += count;

			if (tick > file.Length) return;

			for (; midiIndex < file.MidiEvents.Count && file.MidiEvents[midiIndex].Time <= tick; midiIndex++)
				OnMidiEvent(file.MidiEvents[midiIndex]);
			for (; sysExIndex < file.SysExEvents.Count && file.SysExEvents[sysExIndex].Time <= tick; sysExIndex++)
				OnSysExEvent(file.SysExEvents[sysExIndex]);
			for (; metaIndex < file.MetaEvents.Count && file.MetaEvents[metaIndex].Time <= tick; metaIndex++)
				OnMetaEvent(file.MetaEvents[metaIndex]);
		}

		public void AdvanceSample (double count = 1) {
			AdvanceTick(count * ticksPerSample);
		}

		public void DefaultMidiEventHandler (MidiEvent midiEvent) {
			DebugConsole.Log(midiEvent, "cyan");
		}

		public void DefaultSysExEventHandler (SysExEvent sysExEvent) {
			DebugConsole.Log(sysExEvent, "red");
		}

		public void DefaultMetaEventHandler (MetaEvent metaEvent) {
			DebugConsole.Log(metaEvent, "lime");

			if (metaEvent.Type == MetaEventType.Tempo)
				ticksPerSample = file.TicksPerBeat / (metaEvent.Tempo * samplesPerMicrosecond);
		}
	}
}