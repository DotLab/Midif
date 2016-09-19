using System;
using System.Text;

namespace Midif {
	public enum MetaEventType {
		SequenceNumber = 0x00,
		Text = 0x01,
		Copyright = 0x02,
		SequenceOrTrackName = 0x03,
		InstrumentName = 0x04,
		Lyric = 0x05,
		Marker = 0x06,
		CuePoint = 0x07,
		ProgramName = 0x08,
		DeviceName = 0x09,

		MidiChannelPrefix = 0x20,
		MidiPort = 0x21,
		EndOfTrack = 0x2F,

		Tempo = 0x51,
		SmpteOffset = 0x54,
		TimeSignature = 0x58,
		KeySignature = 0x59,

		SequencerSpecificEvent = 0x7F
	}

	public delegate void MetaEventHandler (MetaEvent metaEvent);

	public interface IMetaEventHandler {
		void MetaEventHandler (MetaEvent metaEvent);
	}


	[Serializable]
	public class MetaEvent : TrackEvent {
		public byte TypeByte;
		public int Length;
		public byte[] Data;

		public MetaEventType Type {
			get { return (MetaEventType)TypeByte; }
		}

		public int SequenceNumber {
			get { return Data[1] << 8 | Data[2]; }
		}

		public string Text {
			get { return Encoding.UTF8.GetString(Data); }
		}

		public byte ChannelPrefix { get { return Data[0]; } }

		public byte Port { get { return Data[0]; } }

		/// <summary>
		/// Gets the tempo in microseconds per quarter note.
		/// </summary>
		public int Tempo { get { return Data[0] << 16 | Data[1] << 8 | Data[2]; } }

		public MetaEvent (int track, int tick, byte typeByte, int length) : base(track, tick) {
			TypeByte = typeByte;
			Length = length;

			Data = new byte[Length];
		}

		public override string ToString () {
			var info = Type + "=";

			switch (Type) {
			case MetaEventType.SequenceNumber:
				info += SequenceNumber;
				break;
			case MetaEventType.Text:
			case MetaEventType.Copyright:
			case MetaEventType.SequenceOrTrackName:
			case MetaEventType.InstrumentName:
			case MetaEventType.Lyric:
			case MetaEventType.Marker:
			case MetaEventType.CuePoint:
			case MetaEventType.ProgramName:
			case MetaEventType.DeviceName:
				info += Encoding.UTF8.GetString(Data);
				break;
			case MetaEventType.MidiChannelPrefix:
			case MetaEventType.MidiPort:
				info += Data[0];
				break;
			case MetaEventType.EndOfTrack:
				return string.Format("(MetaEvent: Track={0}, Time={1}, Type={2})", Track, Tick, Type);
			case MetaEventType.Tempo:
				info = "MicrosecondsPerBeat=" + Tempo;
				break;
			case MetaEventType.SmpteOffset:
				info = string.Format("FrameRate={0}fps, Offset={1}hours {2}minutes {3}seconds {4}frames {5}1/100frames",
					Data[0] >> 5, Data[0] & 0x1F, Data[1], Data[2], Data[3], Data[4]);
				break;
			case MetaEventType.TimeSignature:
				info = string.Format("TimeSignature={0}/{1}, ClocksPerMetronomeClick={2}, 32ndNotesPer4thNote={3}",
					Data[0], (int)Math.Pow(2, Data[1]), Data[2], Data[3]);
				break;
			case MetaEventType.KeySignature:
				info = string.Format("KeySignature={0}{1} {2}",
					Math.Abs((sbyte)Data[0]), (sbyte)Data[0] < 0 ? "flats" : "sharps", Data[1] == 0 ? "major" : "minor");
				break;
			case MetaEventType.SequencerSpecificEvent:
				info += BitConverter.ToString(Data); 
				break;
			}

			return string.Format("[MetaEvent: Track={0}, Time={1}, Type={2}, {3}, Data={4}]", Track, Tick, Type, info, BitConverter.ToString(Data));
		}
	}
}

