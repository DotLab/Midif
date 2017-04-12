using System;
using System.IO;
using System.Text;

namespace Midif.File {
	static class StreamHelperLe {
		static readonly byte[] buffer = new byte[8];

		public static string ReadString (Stream stream, int length) {
			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			return Encoding.UTF8.GetString(bytes);
		}

		public static void WriteString (Stream stream, string str) {
			var bytes = Encoding.UTF8.GetBytes(str);
			stream.Write(bytes, 0, bytes.Length);
		}

		public static Int16 ReadInt16 (Stream stream) {
			stream.Read(buffer, 0, 2);
			return (Int16)(buffer[0] | (sbyte)buffer[1] << 8);
		}

		public static void WriteInt16 (Stream stream, Int16 value) {
			buffer[0] = (byte)(value & 0xFF);
			buffer[1] = (byte)((value >> 8) & 0xFF);
			stream.Write(buffer, 0, 2);
		}

		public static UInt16 ReadUInt16 (Stream stream) {
			stream.Read(buffer, 0, 2);
			return (UInt16)(buffer[0] | (sbyte)buffer[1] << 8);
		}

		public static void WriteUInt16 (Stream stream, UInt16 value) {
			buffer[0] = (byte)(value & 0xFF);
			buffer[1] = (byte)((value >> 8) & 0xFF);
			stream.Write(buffer, 0, 2);
		}

		public static Int32 ReadInt24 (Stream stream) {
			stream.Read(buffer, 0, 3);
			return (buffer[0] | buffer[1] << 8 | (sbyte)buffer[2] << 16);
		}

		public static Int32 ReadUInt24 (Stream stream) {
			stream.Read(buffer, 0, 3);
			return (buffer[0] | buffer[1] << 8 | buffer[2] << 16);
		}

		public static Int32 ReadInt32 (Stream stream) {
			stream.Read(buffer, 0, 4);
			return (buffer[0] | buffer[1] << 8 | buffer[2] << 16 | (sbyte)buffer[3] << 24);
		}

		public static void WriteInt24 (Stream stream, Int32 value) {
			buffer[0] = (byte)(value & 0xFF);
			buffer[1] = (byte)((value >> 8) & 0xFF);
			buffer[2] = (byte)((value >> 16) & 0xFF);
			stream.Write(buffer, 0, 3);
		}

		public static void WriteInt32 (Stream stream, Int32 value) {
			buffer[0] = (byte)(value & 0xFF);
			buffer[1] = (byte)((value >> 8) & 0xFF);
			buffer[2] = (byte)((value >> 16) & 0xFF);
			buffer[3] = (byte)((value >> 24) & 0xFF);
			stream.Write(buffer, 0, 4);
		}

		public static UInt32 ReadUInt32 (Stream stream) {
			stream.Read(buffer, 0, 4);
			return (UInt32)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
		}

		public static void WriteUInt32 (Stream stream, UInt32 value) {
			buffer[0] = (byte)(value & 0xFF);
			buffer[1] = (byte)((value >> 8) & 0xFF);
			buffer[2] = (byte)((value >> 16) & 0xFF);
			buffer[3] = (byte)((value >> 24) & 0xFF);
			stream.Write(buffer, 0, 4);
		}
	}

	static class StreamHelperBe {
		const int Int16Length = 2;
		const int Int32Length = 4;

		public static string ReadString (Stream stream, int length) {
			var bytes = new byte[length];
			stream.Read(bytes, 0, length);
			return Encoding.UTF8.GetString(bytes);
		}

		public static Int16 ReadInt16 (Stream stream) {
			return (Int16)((sbyte)stream.ReadByte() << 8 | stream.ReadByte());
		}

		public static UInt16 ReadUInt16 (Stream stream) {
			return (UInt16)(stream.ReadByte() << 8 | stream.ReadByte());
		}

		public static Int32 ReadInt24 (Stream stream) {
			return ((sbyte)stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte());
		}

		public static Int32 ReadUInt24 (Stream stream) {
			return (stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte());
		}

		public static Int32 ReadInt32 (Stream stream) {
			return ((sbyte)stream.ReadByte() << 24 | stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte());
		}

		public static UInt32 ReadUInt32 (Stream stream) {
			return (UInt32)(stream.ReadByte() << 24 | stream.ReadByte() << 16 | stream.ReadByte() << 8 | stream.ReadByte());
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