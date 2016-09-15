using System;
using System.IO;
using System.Text;

namespace Midif {
	static class MidiStreamHelper {
		const int Int16Length = 2;
		const int Int32Length = 4;

		/// <summary>
		/// Reads a string of length length.
		/// </summary>
		public static string ReadString (Stream stream, int length) {
			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			return Encoding.ASCII.GetString(bytes);
		}

		/// <summary>
		/// Reads 2 bytes as a Int16.
		/// </summary>
		public static int ReadInt16 (Stream stream) {
			var bytes = new byte[Int16Length];
			stream.Read(bytes, 0, Int16Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToInt16(bytes, 0);
		}

		/// <summary>
		/// Reads 4 bytes as a Int16.
		/// </summary>
		public static int ReadInt32 (Stream stream) {
			var bytes = new byte[Int32Length];
			stream.Read(bytes, 0, Int32Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToInt32(bytes, 0);
		}

		/// <summary>
		/// Reads 2 bytes as a UInt16.
		/// </summary>
		public static int ReadUInt16 (Stream stream) {
			var bytes = new byte[Int16Length];
			stream.Read(bytes, 0, Int16Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToUInt16(bytes, 0);
		}

		/// <summary>
		/// Reads 4 bytes as a UInt16.
		/// </summary>
		public static int ReadUInt32 (Stream stream) {
			var bytes = new byte[Int32Length];
			stream.Read(bytes, 0, Int32Length);
			Array.Reverse(bytes);
			return (int)BitConverter.ToUInt32(bytes, 0);
		}

		/// <summary>
		/// Reads a variable length value.
		/// </summary>
		public static int ReadVlv (Stream stream) {
			int b, value = 0;

			do {
				b = stream.ReadByte();

				value = value << 7;
				value += b & 0x7F;
			} while ((b & 0x80) != 0) ;

			return value;
		}
	}
}

