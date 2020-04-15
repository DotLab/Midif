using System.Collections.Generic;
using Unsaf;
using Debug = UnityEngine.Debug;

namespace Midif.V3 {
	public sealed class MidiSequencer {
		struct Route {
			public int trackIn;
			public byte channelIn;
			public int channelOut;

			public Route(int trackIn, byte channelIn, int channelOut) {
				this.trackIn = trackIn;
				this.channelIn = channelIn;
				this.channelOut = channelOut;
			}
		}

		public MidiFile file;
		public IMidiSynth synth;

		public int[] trackIndices;
		public int[] trackTicks;

		public float beatsPerSecond;
		public float ticks;

		public bool isFinished;
		public bool isMuted;

		readonly List<Route> routes = new List<Route>();

		public MidiSequencer(MidiFile file, IMidiSynth synth) {
			this.file = file;
			this.synth = synth;

			int trackCount = file.trackCount;
			trackIndices = new int[trackCount];
			trackTicks = new int[trackCount];

			beatsPerSecond = 0;
			ticks = 0;
			isFinished = false;
			Reset();
		}

		public void AddRoute(int trackIn, byte channelIn, int channelOut) {
			routes.Add(new Route(trackIn, channelIn, channelOut));
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

		public float ToTicks(float seconds) {
			return seconds * beatsPerSecond * file.ticksPerBeat;
		}

		public float ToSeconds(float seconds) {
			return seconds / (beatsPerSecond * file.ticksPerBeat);
		}

		public void AdvanceTime(float seconds) {
			ticks += seconds * beatsPerSecond * file.ticksPerBeat;

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
				beatsPerSecond = (1000000f / microsecondsPerBeat);
				 Debug.LogFormat("meta: {0} tempo", microsecondsPerBeat);
			}

			byte channel = (byte)(e.status & 0xf);
			DispatchChannelEvent(channel, e.status, e.b1, e.b2);

			for (int i = 0, count = routes.Count; i < count; i += 1) {
				var r = routes[i];
				if (r.trackIn == track && r.channelIn == channel) DispatchChannelEvent(r.channelOut, e.status, e.b1, e.b2);
			}
		}

		void DispatchChannelEvent(int channel, byte status, byte b1, byte b2) {
			switch (status >> 4) {
			case 0x8:  // note off
				// Debug.LogFormat("note off: {0} {1} {2}", channel, e.b1, e.b2);
				if (!isMuted) synth.NoteOff(channel, b1, b2);
				break;
			case 0x9:  // note on
				// Debug.LogFormat("note on: {0} {1} {2}", channel, b1, b2);
				if (!isMuted) synth.NoteOn(channel, b1, b2);
				break;
			case 0xa:  // aftertouch
//				 Debug.LogFormat("aftertouch: {0} {1} {2}", channel, b1, b2);
				break;
			case 0xb:  // controller
				// Debug.LogFormat("controller: {0} {1} {2}", channel, b1, b2);
				synth.Controller(channel, b1, b2);
				break;
			case 0xc:  // program change
				//Debug.LogFormat("program change: {0} {1}", channel, b1);
				synth.ProgramChange(channel, b1);
				break;
			case 0xd:  // channel pressure
				// Debug.LogFormat("channel pressure: {0} {1} {2}", channel, b1, b2);
				break;
			case 0xe:  // pitch bend
				// Debug.LogFormat("pitch bend: {0} {1} {2}", channel, b1, b2);
				synth.PitchBend(channel, b1, b2);
				break;
			default:
				// Debug.LogFormat("?: {0} 0x{1:X} 0x{2:X} 0x{3:X} 0x{", status, type, b1, b2);
				break;
			}
		}
	}
}

