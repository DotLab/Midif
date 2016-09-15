using System.IO;

namespace Midif.File {
	public class SysExEvent : TrackEvent {
		public byte[] Bytes;

		public bool IsTerminated {
			get { return Bytes[Bytes.Length - 1] == 0xF7; }
		}

		public string Text {
			get { return System.Text.Encoding.ASCII.GetString(Bytes); }
		}

		public SysExEvent (int track, int time, Stream stream) : base(track, time) {
			var length = MidiStreamHelper.ReadVlv(stream);
			Bytes = new byte[length];
			stream.Read(Bytes, 0, length);
		}

		public void Append (Stream stream) {
			var length = MidiStreamHelper.ReadVlv(stream);

			System.Array.Resize<byte>(ref Bytes, Bytes.Length + length);

			stream.Read(Bytes, Bytes.Length - length, length);
		}

		public override string ToString () {
			return string.Format("(SysExEvent: Track={0}, Time={1}, Bytes={2})", Track, Time, System.BitConverter.ToString(Bytes));
		}
	}
}

