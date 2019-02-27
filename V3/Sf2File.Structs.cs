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
		class PresetHeader {
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
				presetName = Trim(Bit.ReadStringAscii(bytes, ref i, 20));
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
		class Bag {
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
		class Modulator {
			// Modulator Transform Enumerators
			public enum Transform {
				Linear = 0,
				AbsoluteValue = 2
			}

			// SFModulator sfModSrcOper;
			public ModulatorType modSrc;
			// SFGenerator sfModDestOper;
			public byte modDest;
			// SHORT modAmount;
			public short modAmount;
			// SFModulator sfModAmtSrcOper;
			public ModulatorType modAmtSrc;
			// SFTransform sfModTransOper;
			public Transform modTrans;

			public Modulator(byte[] bytes, ref int i) {
				modSrc = new ModulatorType(bytes, ref i);
				modDest = (byte)Bit.ReadUInt16(bytes, ref i);
				modAmount = Bit.ReadInt16(bytes, ref i);
				modAmtSrc = new ModulatorType(bytes, ref i);
				modTrans = (Transform)Bit.ReadUInt16(bytes, ref i);
			}
		}

		// <pgen-rec> -> struct sfGenList
		// <igen-rec> -> struct sfInstGenList
		public class Generator {
			// SFGenerator sfGenOper;
			public byte gen;
			// genAmountType genAmount; 
			public byte lo;
			public byte hi;
			public short amount;

			public Generator(byte[] bytes, ref int i) {
				gen = (byte)Bit.ReadUInt16(bytes, ref i);
				lo = Bit.ReadByte(bytes, ref i);
				hi = Bit.ReadByte(bytes, ref i);
				amount = (short)((sbyte)hi << 8 | lo);
			}
		}

		// <inst-rec> -> struct sfInst
		class InstrumentHeader {
			// CHAR achInstName[20];
			public string instName;
			// WORD wInstBagNdx;
			public ushort instBagNdx;

			public InstrumentHeader(byte[] bytes, ref int i) {
				instName = Trim(Bit.ReadStringAscii(bytes, ref i, 20));
				instBagNdx = Bit.ReadUInt16(bytes, ref i);
			}
		}

		// <shdr-rec> -> struct sfSample
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
				sampleName = Trim(Bit.ReadStringAscii(bytes, ref i, 20));
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
		public static class GeneratorType {
			public const int StartAddrsOffset = 0;             // Sample start address offset (0 - 32767)
			public const int EndAddrsOffset = 1;               // Sample end address offset (-32767 - 0)
			public const int StartloopAddrsOffset = 2;         // Sample loop start address offset (-32767 - 32767)
			public const int EndloopAddrsOffset = 3;           // Sample loop end address offset (-32767 - 32767)
			public const int StartAddrsCoarseOffset = 4;       // Sample start address coarse offset (X 32768)
			public const int ModLfoToPitch = 5;                // Modulation LFO to pitch
			public const int VibLfoToPitch = 6;                // Vibrato LFO to pitch
			public const int ModEnvToPitch = 7;                // Modulation envelope to pitch
			public const int InitialFilterFc = 8;              // Filter cutoff
			public const int InitialFilterQ = 9;               // Filter Q
			public const int ModLfoToFilterFc = 10;            // Modulation LFO to filter cutoff
			public const int ModEnvToFilterFc = 11;            // Modulation envelope to filter cutoff
			public const int EndAddrsCoarseOffset = 12;        // Sample end address coarse offset (X 32768)
			public const int ModLfoToVolume = 13;              // Modulation LFO to volume
  			// Unused
			public const int ChorusEffectsSend = 15;           // Chorus send amount
			public const int ReverbEffectsSend = 16;           // Reverb send amount
			public const int Pan = 17;                         // Stereo panning
			// Unused
			// Unused
			// Unused
			public const int DelayModLFO = 21;                 // Modulation LFO delay
			public const int FreqModLFO = 22;                  // Modulation LFO frequency
			public const int DelayVibLFO = 23;                 // Vibrato LFO delay
			public const int FreqVibLFO = 24;                  // Vibrato LFO frequency
			public const int DelayModEnv = 25;                 // Modulation envelope delay
			public const int AttackModEnv = 26;                // Modulation envelope attack
			public const int HoldModEnv = 27;                  // Modulation envelope hold
			public const int DecayModEnv = 28;                 // Modulation envelope decay
			public const int SustainModEnv = 29;               // Modulation envelope sustain
			public const int ReleaseModEnv = 30;               // Modulation envelope release
			public const int KeynumToModEnvHold = 31;          // Key to modulation envelope hold
			public const int KeynumToModEnvDecay = 32;         // Key to modulation envelope decay
			public const int DelayVolEnv = 33;                 // Volume envelope delay
			public const int AttackVolEnv = 34;                // Volume envelope attack
			public const int HoldVolEnv = 35;                  // Volume envelope hold
			public const int DecayVolEnv = 36;                 // Volume envelope decay
			public const int SustainVolEnv = 37;               // Volume envelope sustain
			public const int ReleaseVolEnv = 38;               // Volume envelope release
			public const int KeynumToVolEnvHold = 39;          // Key to volume envelope hold
			public const int KeynumToVolEnvDecay = 40;         // Key to volume envelope decay
			public const int Instrument = 41;                  // Instrument ID (shouldn't be set by user)
			// Reserved
			public const int KeyRange = 43;                    // MIDI note range
			public const int VelRange = 44;                    // MIDI velocity range
			public const int StartloopAddrsCoarseOffset = 45;  // Sample start loop address coarse offset (X 32768)
			public const int Keynum = 46;                      // Fixed MIDI note number
			public const int Velocity = 47;                    // Fixed MIDI velocity value
			public const int InitialAttenuation = 48;          // Initial volume attenuation
			// Reserved
			public const int EndloopAddrsCoarseOffset = 50;    // Sample end loop address coarse offset (X 32768)
			public const int CoarseTune = 51;                  // Coarse tuning
			public const int FineTune = 52;                    // Fine tuning
			public const int SampleID = 53;                    // Sample ID (shouldn't be set by user)
			public const int SampleModes = 54;                 // Sample mode flags
			// Reserved
			public const int ScaleTuning = 56;                 // Scale tuning
			public const int ExclusiveClass = 57;              // Exclusive class number
			public const int OverridingRootKey = 58;           // Sample root note override
			public const int Last = 59;
		}
	}
}

