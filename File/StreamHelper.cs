using System;
using System.IO;
using System.Text;

namespace Midif.File {
	public static class StreamHelper {
		public static string ReadString (Stream stream, int length) {
			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			return Encoding.UTF8.GetString(bytes);
		}

		public static Int16 ReadInt16 (Stream stream) {
			const int size = sizeof(Int16);
			var bytes = new byte[size];
			stream.Read(bytes, 0, size);
			return BitConverter.ToInt16(bytes, 0);
		}

		public static UInt16 ReadUInt16 (Stream stream) {
			const int size = sizeof(UInt16);
			var bytes = new byte[size];
			stream.Read(bytes, 0, size);
			return BitConverter.ToUInt16(bytes, 0);
		}

		public static Int32 ReadInt32 (Stream stream) {
			const int size = sizeof(Int32);
			var bytes = new byte[size];
			stream.Read(bytes, 0, size);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static UInt32 ReadUInt32 (Stream stream) {
			const int size = sizeof(UInt32);
			var bytes = new byte[size];
			stream.Read(bytes, 0, size);
			return BitConverter.ToUInt32(bytes, 0);
		}
	}
}