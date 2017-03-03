using System;
using System.IO;
using System.Text;

namespace Midif.File {
	public static class StreamHelperLe {
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

	public static class StreamHelperBe {
		const int Int16Length = 2;
		const int Int32Length = 4;

		public static string ReadString (Stream stream, int length) {
			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			return Encoding.UTF8.GetString(bytes);
		}

		public static int ReadInt16 (Stream stream) {
			var bytes = new byte[Int16Length];
			stream.Read(bytes, 0, Int16Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToInt16(bytes, 0);
		}

		public static int ReadInt32 (Stream stream) {
			var bytes = new byte[Int32Length];
			stream.Read(bytes, 0, Int32Length);
			Array.Reverse(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static int ReadUInt16 (Stream stream) {
			var bytes = new byte[Int16Length];
			stream.Read(bytes, 0, Int16Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToUInt16(bytes, 0);
		}

		public static int ReadUInt32 (Stream stream) {
			var bytes = new byte[Int32Length];
			stream.Read(bytes, 0, Int32Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToUInt32(bytes, 0);
		}

		public static uint ReadVlv (Stream stream) {
			byte b;
			uint value = 0;

			do {
				b = (byte)stream.ReadByte();
				value = (value << 7) | (uint)(b & 0x7F);
			} while ((b & 0x80) != 0) ;

			return value;
		}
	}
}