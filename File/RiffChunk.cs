using System.IO;
using System.Text;

namespace Midif.File {
	class RiffChunk {
		public string Id;

		public uint Size;
		public byte Pad;

		public byte[] Data;


		public RiffChunk (string id, byte[] data) {
			Id = id;

			Size = (uint)data.Length;
			Pad = (byte)(Size & 0x01);

			Data = data;
		}

		public RiffChunk (Stream stream) {
			Read(stream);
		}

		public void Read (Stream stream) {
			Id = StreamHelperLe.ReadString(stream, 4);

			Size = StreamHelperLe.ReadUInt32(stream);
			Pad = (byte)(Size & 0x1);

			Data = new byte[Size];
			stream.Read(Data, 0, (int)Size);
			stream.Position += Pad;
		}

		public void Write (Stream stream) {
			StreamHelperLe.WriteString(stream, Id);

			StreamHelperLe.WriteUInt32(stream, Size);

			stream.Write(Data, 0, (int)Size);
			//if (Pad > 0) stream.WriteByte(0);
		}

		public Stream GetStream () {
			return new MemoryStream(Data);
		}

		public string GetString () {
			return Encoding.UTF8.GetString(Data);
		}

		public override string ToString () {
			return string.Format("[RiffChunk: Id={0}, Size={1}]", Id, Size);
		}
	}
}