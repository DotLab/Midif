using System;
using System.IO;

namespace Midif.Synthesizer.Dx7 {
	[Serializable]
	public class Dx7Sysex {
		public const int PatchCount = 32;

		public byte StatusByte;
		public byte Id;
		public byte SubStatus;
		public byte FormatNumber;
		public int ByteCount;

		public Dx7Patch[] Patches;

		public byte Checksum;
		public byte EndFlag;


		public Dx7Sysex (Stream stream) {
			StatusByte = (byte)stream.ReadByte();
			Id = (byte)stream.ReadByte();
			SubStatus = (byte)stream.ReadByte();
			FormatNumber = (byte)stream.ReadByte();
			ByteCount = stream.ReadByte() << 7 + stream.ReadByte();

			Patches = new Dx7Patch[PatchCount];
			for (int i = 0; i < PatchCount; i++)
				Patches[i] = new Dx7Patch(stream);

			Checksum = (byte)stream.ReadByte();
			EndFlag = (byte)stream.ReadByte();
		}
	}
}