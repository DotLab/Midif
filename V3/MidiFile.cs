using Unsaf;
using System.Collections.Generic;

namespace Midif.V3 {
	public sealed class MidiFile {
		public readonly short format;
		public readonly short trackCount;
		public readonly short ticksPerBeat;

		public readonly byte[] bytes;
		public readonly MidiEvent[][] tracks;
		public readonly MidiEvent[] combinedTrack;
		public readonly int[] trackLengths;
		public readonly int[] trackTicks;

		public MidiFile(byte[] bytes) {
			this.bytes = bytes;

			int i = 0;
			// "MThd" 4 bytes
			i += 4;
			// <header_length> 4 bytes
			int length = BitBe.ReadInt32(bytes, ref i);
			int ii = i;
			// <format> 2 bytes
			format = BitBe.ReadInt16(bytes, ref i);
			// <n> 2 bytes
			trackCount = BitBe.ReadInt16(bytes, ref i);
			tracks = new MidiEvent[trackCount][];
			trackLengths = new int[trackCount];
			trackTicks = new int[trackCount];
			// <division> 2 bytes
			ticksPerBeat = BitBe.ReadInt16(bytes, ref i);
			// end chunk
			i = ii + length;

			byte runingStatus = 0;
			var combinedEventList = new List<MidiEvent>();
			for (short j = 0, count = trackCount; j < count; j += 1) {
				var trackEventList = new List<MidiEvent>();

				// "MTrk" 4 bytes
				i += 4;
				// <length> 4 bytes
				length = BitBe.ReadInt32(bytes, ref i);
				ii = i;
				// <track_event>
				while (i < ii + length) {
					int delta = BitBe.ReadVlv(bytes, ref i);
					trackTicks[j] += delta;
					int trackTick = trackTicks[j];
					byte statusByte = Bit.ReadByte(bytes, ref i);
					int dataLength;
					if (statusByte < 0x80) {  // running status
						dataLength = GetMidiEventLength(runingStatus);
						trackEventList   .Add(new MidiEvent(j, delta, trackTick, runingStatus, 0, i - 1, dataLength, bytes));
						combinedEventList.Add(new MidiEvent(j, delta, trackTick, runingStatus, 0, i - 1, dataLength, bytes));
						dataLength -= 1;
					} else if (statusByte < 0xf0) {  // midi events
						runingStatus = statusByte;
						dataLength = GetMidiEventLength(statusByte);
						trackEventList   .Add(new MidiEvent(j, delta, trackTick, statusByte, 0, i, dataLength, bytes));
						combinedEventList.Add(new MidiEvent(j, delta, trackTick, statusByte, 0, i, dataLength, bytes));
					} else if (statusByte == 0xf0 || statusByte == 0xf7) {  // sysex events | escape sequences
						dataLength = BitBe.ReadVlv(bytes, ref i);
						trackEventList   .Add(new MidiEvent(j, delta, trackTick, statusByte, 0, i, dataLength, bytes));
						combinedEventList.Add(new MidiEvent(j, delta, trackTick, statusByte, 0, i, dataLength, bytes));
					} else if (statusByte == 0xff) {  // meta events
						byte type = Bit.ReadByte(bytes, ref i);
						dataLength = BitBe.ReadVlv(bytes, ref i);
						trackEventList   .Add(new MidiEvent(j, delta, trackTick, statusByte, type, i, dataLength, bytes));
						combinedEventList.Add(new MidiEvent(j, delta, trackTick, statusByte, type, i, dataLength, bytes));
					} else {
						return;
					}
					i += dataLength;
				}
				tracks[j] = trackEventList.ToArray();
				trackLengths[j] = trackEventList.Count;
				// end chunk
				i = ii + length;
			}

			combinedEventList.Sort((a, b) => {
				if (a.tick == b.tick) {
					return a.track.CompareTo(b.track);
				}
				return a.tick.CompareTo(b.tick);
			});
			combinedTrack = combinedEventList.ToArray();
		}
	
		static int GetMidiEventLength(byte statusByte) {
			switch (statusByte >> 4) {
			case 0x8:  // note off
			case 0x9:  // note on
			case 0xa:  // aftertouch
			case 0xb:  // controller
			case 0xe:  // pitch bend
				return 2;
			case 0xc:  // program change
			case 0xd:  // channel pressure
				return 1;
			}
			return 0;
		}
	}
}