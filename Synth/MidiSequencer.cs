namespace Midif.Synth {
	public class MidiSequencer : ISequencer, IMetaEventHandler {
		public event MidiEventHandler OnProcessMidiEvent;

		public event MetaEventHandler OnProcessMetaEvent;

		const double microsecondPerSecond = 1000000;
		double samplesPerMicrosecond;

		double ticksPerSample;
		double ticksPerSecond;

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

		public double InternalTick { get { return tick; } }

		public int MidiIndex { get { return midiIndex; } }

		public int SysExIndex { get { return sysExIndex; } }

		public int MetaIndex { get { return metaIndex; } }

		public double TicksPerSample { get { return ticksPerSample; } }

		public double TicksPerSecond { get { return ticksPerSecond; } }


		public MidiSequencer () {
			OnProcessMetaEvent += MetaEventHandler;
		}


		public void MetaEventHandler (MetaEvent metaEvent) {
			if (metaEvent.Type == MetaEventType.Tempo) {
				ticksPerSample = (double)file.TicksPerBeat / (metaEvent.Tempo * samplesPerMicrosecond);
				// ticks per sample = ticks per beat / ms per beat * ms per sample;
				//                  = ticks per ms * ms per sample;
				//                  = ticks per sample;
				ticksPerSecond = (double)file.TicksPerBeat / metaEvent.Tempo * microsecondPerSecond;
				// ticks per second = ticks per beat / ms per beat * ms per second;
				//                  = ticks per ms * ms per second;
				//                  = ticks per second;
			}
		}


		public void Init (double sampleRate) {
			samplesPerMicrosecond = sampleRate / microsecondPerSecond;

			Reset();
		}

		public void Reset () {
			if (file != null) {
				ticksPerSample = (double)file.TicksPerBeat / (500000 * samplesPerMicrosecond);
				ticksPerSecond = (double)file.TicksPerBeat / 500000 * microsecondPerSecond;
			}

			tick = 0;

			midiIndex = 0;
			sysExIndex = 0;
			metaIndex = 0;
		}


		public void AdvanceSamples (double samples) {
			AdvanceTicks(samples * ticksPerSample);
		}

		public void AdvanceSeconds (double seconds) {
			AdvanceTicks(seconds * ticksPerSecond);
		}

		public void AdvanceTicks (double ticks) {
			tick += ticks;

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