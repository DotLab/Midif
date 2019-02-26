using Unsaf;

namespace Midif.V3 {
	public sealed partial class Sf2File {
		struct Chunk {
			// FOURCC ckID; // A chunk ID identifies the type of data within the chunk. 
			public string id;
			// DWORD ckSize; // The size of the chunk data in bytes, excluding any pad byte. 
			public int size;
			// BYTE ckDATA[ckSize]; // The actual data plus a pad byte if req’d to word align.
			public int end;

			public Chunk(byte[] bytes, ref int i) {
				id = Bit.ReadStringAscii(bytes, ref i, 4);
				size = (int)Bit.ReadUInt32(bytes, ref i);
				end = i + size + (size & 0x1);
			}
		}

		// <phdr-rec> -> struct sfPresetHeader
		[System.Serializable]
		public class PresetHeader {
			// CHAR achPresetName[20];
			public string presetName;
			// WORD wPreset;
			public ushort preset;
			// WORD wBank;
			public ushort bank;
			// WORD wPresetBagNdx;
			public ushort presetBagNdx;
			// DWORD dwLibrary;
			public uint library;
			// DWORD dwGenre;
			public uint genre;
			// DWORD dwMorphology;
			public uint morphology;

			public PresetHeader(byte[] bytes, ref int i) {
				presetName = Bit.ReadStringUtf8(bytes, ref i, 20);
				preset = Bit.ReadUInt16(bytes, ref i);
				bank = Bit.ReadUInt16(bytes, ref i);
				presetBagNdx = Bit.ReadUInt16(bytes, ref i);

				library = Bit.ReadUInt32(bytes, ref i);
				genre = Bit.ReadUInt32(bytes, ref i);
				morphology = Bit.ReadUInt32(bytes, ref i);
			}
		}

		// <pbag-rec> -> struct sfPresetBag
		// <ibag-rec> -> struct sfInstBag
		[System.Serializable]
		public class Bag {
			// WORD wGenNdx;
			// WORD wInstGenNdx;
			public ushort genNdx;
			// WORD wModNdx;
			// WORD wInstModNdx;
			public ushort modNdx;

			public Bag(byte[] bytes, ref int i) {
				genNdx = Bit.ReadUInt16(bytes, ref i);
				modNdx = Bit.ReadUInt16(bytes, ref i);
			}
		}

		// <pmod-rec> -> struct sfModList
		// <imod-rec> -> struct sfInstModList
		[System.Serializable]
		public class Modulator {
			// Modulator Transform Enumerators
			public enum Transform {
				Linear = 0,
				AbsoluteValue = 2
			}

			// SFModulator sfModSrcOper;
			public ModulatorType modSrc;
			// SFGenerator sfModDestOper;
			public GeneratorType modDest;
			// SHORT modAmount;
			public short modAmount;
			// SFModulator sfModAmtSrcOper;
			public ModulatorType modAmtSrc;
			// SFTransform sfModTransOper;
			public Transform modTrans;

			public Modulator(byte[] bytes, ref int i) {
				modSrc = new ModulatorType(bytes, ref i);
				modDest = (GeneratorType)Bit.ReadUInt16(bytes, ref i);
				modAmount = Bit.ReadInt16(bytes, ref i);
				modAmtSrc = new ModulatorType(bytes, ref i);
				modTrans = (Transform)Bit.ReadUInt16(bytes, ref i);
			}
		}

		// <pgen-rec> -> struct sfGenList
		// <igen-rec> -> struct sfInstGenList
		[System.Serializable]
		public class Generator {
			[System.Serializable]
			public struct Amount {
				public byte lo;
				public byte hi;
				public short GetShort() {
					return (short)((sbyte)lo << 8 | hi);
				}
				public ushort GetWord() {
					return (ushort)(lo << 8 | hi);
				}
			}

			// SFGenerator sfGenOper;
			public GeneratorType gen;
			// genAmountType genAmount; 
			public Amount amount;

			public Generator(byte[] bytes, ref int i) {
				gen = (GeneratorType)Bit.ReadUInt16(bytes, ref i);
				amount.lo = Bit.ReadByte(bytes, ref i);
				amount.hi = Bit.ReadByte(bytes, ref i);
			}
		}

		// <inst-rec> -> struct sfInst
		[System.Serializable]
		public class InstrumentHeader {
			// CHAR achInstName[20];
			public string instName;
			// WORD wInstBagNdx;
			public ushort instBagNdx;

			public InstrumentHeader(byte[] bytes, ref int i) {
				instName = Bit.ReadStringUtf8(bytes, ref i, 20);
				instBagNdx = Bit.ReadUInt16(bytes, ref i);
			}
		}

		// <shdr-rec> -> struct sfSample
		[System.Serializable]
		public class SampleHeader {
			// Sample Link Enumerators
			public enum SampleLink {
				MonoSample = 1,
				RightSample = 2,
				LeftSample = 4,
				LinkedSample = 8,
				RomMonoSample = 0x8001,
				RomRightSample = 0x8002,
				RomLeftSample = 0x8004,
				RomLinkedSample = 0x8008
			}

			// CHAR achSampleName[20];
			public string sampleName;
			// DWORD dwStart;
			public uint start;
			// DWORD dwEnd;
			public uint end;
			// DWORD dwStartloop;
			public uint startloop;
			// DWORD dwEndloop;
			public uint endloop;
			// DWORD dwSampleRate;
			public uint sampleRate;
			// BYTE byOriginalKey;
			public byte originalKey;
			// CHAR chCorrection;
			public sbyte correction;
			// WORD wSampleLink;
			public ushort sampleLink;
			// SFSampleLink sfSampleType;
			public SampleLink sampleType;

			public SampleHeader(byte[] bytes, ref int i) {
				sampleName = Bit.ReadStringUtf8(bytes, ref i, 20);
				start = Bit.ReadUInt32(bytes, ref i);
				end = Bit.ReadUInt32(bytes, ref i);
				startloop = Bit.ReadUInt32(bytes, ref i);
				endloop = Bit.ReadUInt32(bytes, ref i);
				sampleRate = Bit.ReadUInt32(bytes, ref i);
				originalKey = Bit.ReadByte(bytes, ref i);
				correction = (sbyte)Bit.ReadByte(bytes, ref i);
				sampleLink = Bit.ReadUInt16(bytes, ref i);
				sampleType = (SampleLink)Bit.ReadUInt16(bytes, ref i);
			}
		}

		// Modulator Source Enumerators
		[System.Serializable]
		public class ModulatorType {
			public enum Continuity {
				Linear = 0,
				Concave,
				Convex,
				Switch
			}

			public enum Polarity {
				Unipolar = 0,
				Bipolar
			}

			public enum Direction {
				MinToMax = 0,
				MaxToMin
			}

			public enum Controller {
				NoController = 0,

				NoteOnVelocity = 2,
				NoteOnKeyNumber = 3,
				PolyPressure = 10,
				ChannelPressure = 13,
				PitchWheel = 14,
				PitchWheelSensitivity = 16,

				Link = 127,

				MidiController = 128,
			}

			public Continuity type;
			public Polarity polarity;
			public Direction direction;

			public Controller controller;
			public byte midiController;

			public ModulatorType (byte[] bytes, ref int i) {
				var data = Bit.ReadUInt16(bytes, ref i);

				type = (Continuity)(data >> 10);
				polarity = (Polarity)((data >> 9) & 0x01);
				direction = (Direction)((data >> 8) & 0x01);

				if (((data >> 7) & 0x01) == 0)
					controller = (Controller)(data & 0x3F);
				else {
					controller = Controller.MidiController;
					midiController = (byte)(data & 0x3F);
				}
			}
		}

		// Generator and Modulator Destination Enumerators
		public enum GeneratorType {
			StartAddrsOffset = 0,
			EndAddrsOffset = 1,
			StartloopAddrsOffset = 2,
			EndloopAddrsOffset = 3,
			StartAddrsCoarseOffset = 4,
			ModLfoToPitch = 5,
			VibLfoToPitch = 6,
			ModEnvToPitch = 7,
			InitialFilterFc = 8,
			InitialFilterQ = 9,
			ModLfoToFilterFc = 10,
			ModEnvToFilterFc = 11,
			EndAddrsCoarseOffset = 12,
			ModLfoToVolume = 13,
			ChorusEffectsSend = 15,
			ReverbEffectsSend = 16,
			Pan = 17,
			DelayModLFO = 21,
			FreqModLFO = 22,
			DelayVibLFO = 23,
			FreqVibLFO = 24,
			DelayModEnv = 25,
			AttackModEnv = 26,
			HoldModEnv = 27,
			DecayModEnv = 28,
			SustainModEnv = 29,
			ReleaseModEnv = 30,
			KeynumToModEnvHold = 31,
			KeynumToModEnvDecay = 32,
			DelayVolEnv = 33,
			AttackVolEnv = 34,
			HoldVolEnv = 35,
			DecayVolEnv = 36,
			SustainVolEnv = 37,
			ReleaseVolEnv = 38,
			KeynumToVolEnvHold = 39,
			KeynumToVolEnvDecay = 40,
			Instrument = 41,
			KeyRange = 43,
			VelRange = 44,
			StartloopAddrsCoarseOffset = 45,
			Keynum = 46,
			Velocity = 47,
			InitialAttenuation = 48,
			EndloopAddrsCoarseOffset = 50,
			CoarseTune = 51,
			FineTune = 52,
			SampleID = 53,
			SampleModes = 54,
			ScaleTuning = 56,
			ExclusiveClass = 57,
			OverridingRootKey = 58,
		}
	}
}

