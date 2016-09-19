using System.IO;
using System.Text;

namespace Midif.File {
	class RiffChunk {
		public string Id;

		public int Size;
		public int Pad;

		public byte[] Data;


		public RiffChunk (Stream stream) {
			Id = StreamHelper.ReadString(stream, 4);

			Size = (int)StreamHelper.ReadUInt32(stream);
			Pad = Size & 0x1;

			Data = new byte[Size];
			stream.Read(Data, 0, Size);
			stream.Position += Pad;

			DebugConsole.WriteLine(this);
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