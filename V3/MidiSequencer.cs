using Unsaf;
using Debug = UnityEngine.Debug;

namespace Midif.V3 {
	public sealed class MidiSequencer {
		public MidiFile file;
		public IMidiSynth synth;

		public int[] trackIndices;
		public double[] trackTicks;

		public double beatsPerSecond;
		public double ticks;

		public bool isFinished;

		public MidiSequencer(MidiFile file, IMidiSynth synth) {
			this.file = file;
			this.synth = synth;

			int trackCount = file.trackCount;
			trackIndices = new int[trackCount];
			trackTicks = new double[trackCount];

			beatsPerSecond = 0;
			ticks = 0;
			isFinished = false;
			Reset();
		}

		public void Reset() {
			for (int i = 0, count = file.trackCount; i < count; i += 1) {
				trackIndices[i] = 0;
				trackTicks[i] = 0;
			}
			// default tempo 120 bpm
			beatsPerSecond = 2;
			ticks = 0;
			isFinished = false;
		}

		public void AdvanceTime(double time) {
			ticks += time * beatsPerSecond * file.ticksPerBeat;

			isFinished = true;
			for (int i = 0, count = file.trackCount; i < count; i += 1) {
				int trackLength = file.trackLengths[i];
				if (trackIndices[i] >= trackLength) continue;
				isFinished = false;

				MidiEvent[] track = file.tracks[i];
				MidiEvent e = track[trackIndices[i]];
				while (trackIndices[i] < trackLength && ticks - trackTicks[i] >= e.delta) {
					HandleEvent(i, e);
					trackIndices[i] += 1;
					trackTicks[i] += e.delta;
					if (trackIndices[i] < trackLength) e = track[trackIndices[i]];
				}
			}
		}

		public void HandleEvent(int track, MidiEvent e) {
			if (e.status == 0xff && e.type == 0x51) {  // meta tempo
				int i = e.dataLoc;
				// 24-bit value specifying the tempo as the number of microseconds per beat
				int microsecondsPerBeat = BitBe.ReadInt24(file.bytes, ref i);
				beatsPerSecond = (1000000.0 / (double)microsecondsPerBeat);
				// Debug.LogFormat("meta: {0} tempo {1}", track, microsecondsPerBeat);
			}

			byte channel = (byte)(e.status & 0xf);

			switch (e.status >> 4) {
			case 0x8:  // note off
				// Debug.LogFormat("note off: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				synth.NoteOff(track, channel, e.b1, e.b2);
				break;
			case 0x9:  // note on
				// Debug.LogFormat("note on: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				if (e.b2 == 0) {
					synth.NoteOff(track, channel, e.b1, 0);
				} else {
					synth.NoteOn(track, channel, e.b1, e.b2);
				}
				break;
			case 0xa:  // aftertouch
				// Debug.LogFormat("aftertouch: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				break;
			case 0xb:  // controller
				// Debug.LogFormat("controller: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				synth.Controller(track, channel, e.b1, e.b2);
				break;
			case 0xc:  // program change
				// Debug.LogFormat("program change: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				break;
			case 0xd:  // channel pressure
				// Debug.LogFormat("channel pressure: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				break;
			case 0xe:  // pitch bend
				// Debug.LogFormat("pitch bend: {0} {1} {2} {3}", track, channel, e.b1, e.b2);
				synth.PitchBend(track, channel, e.b1, e.b2);
				break;
			default:
				// Debug.LogFormat("?: {0} 0x{1:X} 0x{2:X} 0x{3:X} 0x{4:X}", track, e.status, e.type, e.b1, e.b2);
				break;
			}
		}
	}
}

