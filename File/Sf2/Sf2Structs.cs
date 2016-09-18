using System;
using System.IO;

namespace Midif.File.Sf2 {
	// <pbag-rec> -> struct sfPresetBag
	// <ibag-rec> -> struct sfInstBag
	[Serializable]
	public class Bag {
		// WORD wGenNdx;
		// WORD wInstGenNdx;
		public ushort GenNdx;
		// WORD wModNdx;
		// WORD wInstModNdx;
		public ushort ModNdx;

		public Bag (Stream stream) {
			GenNdx = StreamHelper.ReadUInt16(stream);
			ModNdx = StreamHelper.ReadUInt16(stream);
		}
	}

	// <pmod-rec> -> struct sfModList
	// <imod-rec> -> struct sfInstModList
	[Serializable]
	public class Modulator {
		// SFModulator sfModSrcOper;
		public ModulatorType ModSrc;
		// SFGenerator sfModDestOper;
		public GeneratorType ModDest;
		// SHORT modAmount;
		public short ModAmount;
		// SFModulator sfModAmtSrcOper;
		public ModulatorType ModAmtSrc;
		// SFTransform sfModTransOper;
		public TransformType ModTrans;

		public Modulator (Stream stream) {
			ModSrc = new ModulatorType(stream);
			ModDest = (GeneratorType)StreamHelper.ReadUInt16(stream);
			ModAmount = StreamHelper.ReadInt16(stream);
			ModAmtSrc = new ModulatorType(stream);
			ModTrans = (TransformType)StreamHelper.ReadUInt16(stream);
		}
	}

	// <pgen-rec> -> struct sfGenList
	// <igen-rec> -> struct sfInstGenList
	[Serializable]
	public class Generator {
		// SFGenerator sfGenOper;
		public GeneratorType Gen;

		// rangesType ranges;
		public byte AmoutLo, AmoutHi;
		// SHORT shAmount;
		public short AmoutShort {
			get { return (short)((sbyte)AmoutHi << 8 | AmoutLo); }
		}
		// WORD wAmount;
		public ushort AmoutWord {
			get { return (ushort)(AmoutHi << 8 | AmoutLo); }
		}

		public Generator (Stream stream) {
			Gen = (GeneratorType)StreamHelper.ReadUInt16(stream);

			AmoutLo = (byte)stream.ReadByte();
			AmoutHi = (byte)stream.ReadByte();
		}
	}

	// <phdr-rec> -> struct sfPresetHeader
	[Serializable]
	public class PresetHeader {
		// CHAR achPresetName[20];
		public string PresetName;
		// WORD wPreset;
		public ushort Preset;
		// WORD wBank;
		public ushort Bank;
		// WORD wPresetBagNdx;
		public ushort PresetBagIndex;

		// DWORD dwLibrary;
		protected uint Library;
		// DWORD dwGenre;
		protected uint Genre;
		// DWORD dwMorphology;
		protected uint Morphology;

		public PresetHeader (Stream stream) {
			PresetName = StreamHelper.ReadString(stream, 20);
			Preset = StreamHelper.ReadUInt16(stream);
			Bank = StreamHelper.ReadUInt16(stream);
			PresetBagIndex = StreamHelper.ReadUInt16(stream);

			Library = StreamHelper.ReadUInt32(stream);
			Genre = StreamHelper.ReadUInt32(stream);
			Morphology = StreamHelper.ReadUInt32(stream);
		}
	}

	// <inst-rec> -> struct sfInst
	[Serializable]
	public class InstrumentHeader {
		// CHAR achInstName[20];
		public string InstName;
		// WORD wInstBagNdx;
		public ushort InstBagNdx;

		public InstrumentHeader (Stream stream) {
			InstName = StreamHelper.ReadString(stream, 20);
			InstBagNdx = StreamHelper.ReadUInt16(stream);
		}
	}

	// <shdr-rec> -> struct sfSample
	[Serializable]
	public class SampleHeader {
		// CHAR achSampleName[20];
		public string SampleName;
		// DWORD dwStart;
		public uint Start;
		// DWORD dwEnd;
		public uint End;
		// DWORD dwStartloop;
		public uint Startloop;
		// DWORD dwEndloop;
		public uint Endloop;
		// DWORD dwSampleRate;
		public uint SampleRate;
		// BYTE byOriginalKey;
		public byte OriginalKey;
		// CHAR chCorrection;
		public sbyte Correction;
		// WORD wSampleLink;
		public ushort SampleLink;
		// SFSampleLink sfSampleType;
		public SampleLinkType SampleLinkType;

		public SampleHeader (Stream stream) {
			SampleName = StreamHelper.ReadString(stream, 20);
			Start = StreamHelper.ReadUInt32(stream);
			End = StreamHelper.ReadUInt32(stream);
			Startloop = StreamHelper.ReadUInt32(stream);
			Endloop = StreamHelper.ReadUInt32(stream);
			SampleRate = StreamHelper.ReadUInt32(stream);
			OriginalKey = (byte)stream.ReadByte();
			Correction = (sbyte)stream.ReadByte();
			SampleLink = StreamHelper.ReadUInt16(stream);
			SampleLinkType = (SampleLinkType)StreamHelper.ReadUInt16(stream);
		}
	}
}