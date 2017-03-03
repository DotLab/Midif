using System;
using System.IO;

using System.Collections.Generic;

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
			GenNdx = StreamHelperLe.ReadUInt16(stream);
			ModNdx = StreamHelperLe.ReadUInt16(stream);
		}

		public override string ToString () {
			return string.Format("[Bag: GenNdx={0}, ModNdx={1}]", GenNdx, ModNdx);
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
			ModDest = (GeneratorType)StreamHelperLe.ReadUInt16(stream);
			ModAmount = StreamHelperLe.ReadInt16(stream);
			ModAmtSrc = new ModulatorType(stream);
			ModTrans = (TransformType)StreamHelperLe.ReadUInt16(stream);
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
			Gen = (GeneratorType)StreamHelperLe.ReadUInt16(stream);

			AmoutLo = (byte)stream.ReadByte();
			AmoutHi = (byte)stream.ReadByte();
		}

		public override string ToString () {
			return string.Format("[Generator: Gen={0}, AmoutLo={1}, AmoutHi={2}, AmoutShort={3}, AmoutWord={4}]", Gen, AmoutLo, AmoutHi, AmoutShort, AmoutWord);
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
			PresetName = StreamHelperLe.ReadString(stream, 20);
			Preset = StreamHelperLe.ReadUInt16(stream);
			Bank = StreamHelperLe.ReadUInt16(stream);
			PresetBagIndex = StreamHelperLe.ReadUInt16(stream);

			Library = StreamHelperLe.ReadUInt32(stream);
			Genre = StreamHelperLe.ReadUInt32(stream);
			Morphology = StreamHelperLe.ReadUInt32(stream);
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
			InstName = StreamHelperLe.ReadString(stream, 20);
			InstBagNdx = StreamHelperLe.ReadUInt16(stream);
		}

		public override string ToString () {
			return string.Format("[InstrumentHeader: InstName={0}, InstBagNdx={1}]", InstName, InstBagNdx);
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

		public double[] Samples;
		public int Scale = 1;


		public SampleHeader (Stream stream) {
			SampleName = StreamHelperLe.ReadString(stream, 20);
			Start = StreamHelperLe.ReadUInt32(stream);
			End = StreamHelperLe.ReadUInt32(stream);
			Startloop = StreamHelperLe.ReadUInt32(stream);
			Endloop = StreamHelperLe.ReadUInt32(stream);
			SampleRate = StreamHelperLe.ReadUInt32(stream);
			OriginalKey = (byte)stream.ReadByte();
			Correction = (sbyte)stream.ReadByte();
			SampleLink = StreamHelperLe.ReadUInt16(stream);
			SampleLinkType = (SampleLinkType)StreamHelperLe.ReadUInt16(stream);
		}
	}

	public enum Sf2SampleMode {
		NoLoop = 0,
		LoopContinuous = 1,
		Unsed = 2,
		LoopSustain = 3,
	}

	[Serializable]
	public class Sf2Zone {
		public List<Generator> Generators = new List<Generator>();
		public List<Modulator> Modulators = new List<Modulator>();

		#region Range Generators

		public byte LoKey {
			get { return GetLo(GeneratorType.KeyRange, 0); }
		}

		public byte HiKey {
			get { return GetHi(GeneratorType.KeyRange, 127); }
		}

		public byte LoVel {
			get { return GetLo(GeneratorType.VelocityRange, 0); }
		}

		public byte HiVel {
			get { return GetHi(GeneratorType.VelocityRange, 127); }
		}

		#endregion

		#region Sample Generators

		public int StartOffset {
			get { return GetAddressOffset("Start"); }
		}

		public int EndOffset {
			get { return GetAddressOffset("End"); }
		}

		public int StartLoopOffset {
			get { return GetAddressOffset("StartLoop"); }
		}

		public int EndLoopOffset {
			get { return GetAddressOffset("EndLoop"); }
		}

		public Sf2SampleMode SampleMode {
			get { return (Sf2SampleMode)GetWord(GeneratorType.SampleModes, (ushort)Sf2SampleMode.NoLoop); }
		}

		#endregion

		#region Value Generators

		public double Attenuation {
			get { 
				var gen = GetGenerator(GeneratorType.InitialAttenuation);
				return ((gen == null) ? 0 : (double)gen.AmoutShort / 10);
			}
		}

		public double CutoffFrequency {
			get {
				var gen = GetGenerator(GeneratorType.InitialFilterCutoffFrequency);
				return 8.176 * Math.Pow(2, ((gen == null) ? 13500 : (double)gen.AmoutShort) / 1200);
			}
		}

		#endregion


		public Generator GetGenerator (GeneratorType type) {
			foreach (var gen in Generators)
				if (gen.Gen == type)
					return gen;
			
			return null;
		}

		public short GetShort (GeneratorType type, short defult) {
			var gen = GetGenerator(type);

			return (gen == null) ? defult : gen.AmoutShort;
		}

		public ushort GetWord (GeneratorType type, ushort defult) {
			var gen = GetGenerator(type);

			return (gen == null) ? defult : gen.AmoutWord;
		}

		public byte GetLo (GeneratorType type, byte defult) {
			var gen = GetGenerator(type);

			return (gen == null) ? defult : gen.AmoutLo;
		}

		public byte GetHi (GeneratorType type, byte defult) {
			var gen = GetGenerator(type);

			return (gen == null) ? defult : gen.AmoutHi;
		}

		public bool HasGenerator (GeneratorType type) {
			return GetGenerator(type) != null;
		}

		public int GetAddressOffset (string name) {
			var offset = GetGenerator((GeneratorType)Enum.Parse(typeof(GeneratorType), name + "AddressOffset"));
			var coarseOffset = GetGenerator((GeneratorType)Enum.Parse(typeof(GeneratorType), name + "AddressCoarseOffset"));

			return ((offset == null || offset.AmoutShort <= 0) ? 0 : (int)offset.AmoutShort) +
			0x8000 * ((coarseOffset == null || coarseOffset.AmoutShort <= 0) ? 0 : (int)coarseOffset.AmoutShort);
		}
	}

	[Serializable]
	public class Sf2Instrument {
		public string Name;

		public List<Sf2Zone> Zones = new List<Sf2Zone>();

		public Sf2Instrument (string name) {
			Name = name;
		}
	}

	[Serializable]
	public class Sf2Preset : Sf2Instrument {
		public Sf2Preset (string name) : base(name) {
		}
	}
}