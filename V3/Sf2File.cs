using Unsaf;
using System.Collections.Generic;
using Encoding = System.Text.Encoding;

using GenType = Midif.V3.Sf2File.GeneratorType;

namespace Midif.V3 {
	// [System.Serializable]
	public sealed partial class Sf2File {
		// <ifil-ck> ; Refers to the version of the Sound Font RIFF file
		public short majorVersion;
		public short minorVersion;
		// <isng-ck> ; Refers to the target Sound Engine
		public string targetSoundEngine;
		// <INAM-ck> ; Refers to the Sound Font Bank Name
		public string name;
		// [<ICRD-ck>] ; Refers to the Date of Creation of the Bank
		public string dateOfCreation;
		// [<IENG-ck>] ; Sound Designers and Engineers for the Bank
		public string enginners;
		// [<IPRD-ck>] ; Product for which the Bank was intended
		public string product;
		// [<ICOP-ck>] ; Contains any Copyright message
		public string copyright;
		// [<ICMT-ck>] ; Contains any Comments on the Bank
		public string comments;
		// [<ISFT-ck>] ; The SoundFont tools used to create and alter the bank
		public string tools;

		// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits
		// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits
		[System.NonSerialized]
		public float[] data;

		// <shdr-ck> ; The Sample Headers
		public SampleHeader[] sampleHeaders;

		public Sf2Preset[] presets;
		public Sf2Instrument[] instruments;

		public Sf2File(byte[] bytes) {
			int i = 0;

			// <SFBK-form> ; SoundFont 2 RIFF File Format 
			new Chunk(bytes, ref i);
			// <SFBK-form> -> RIFF (‘sfbk’ ; RIFF form header
			Bit.ReadStringAscii(bytes, ref i, 4);

			// <INFO-list> ; Supplemental Information 
			var infoChunk = new Chunk(bytes, ref i);
			// <INFO-list> -> LIST (‘INFO’ 
			Bit.ReadStringAscii(bytes, ref i, 4);

			while (i < infoChunk.end) {
				var chunk = new Chunk(bytes, ref i);
				switch (chunk.id) {
				case "ifil": // <ifil-ck> ; Refers to the version of the Sound Font RIFF file 
					majorVersion = Bit.ReadInt16(bytes, ref i); minorVersion = Bit.ReadInt16(bytes, ref i); break;
				case "isng": // <isng-ck> ; Refers to the target Sound Engine
					targetSoundEngine = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "INAM": // <INAM-ck> ; Refers to the Sound Font Bank Name
					name = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "ICRD": // [<ICRD-ck>] ; Refers to the Date of Creation of the Bank
					dateOfCreation = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "IENG": // [<IENG-ck>] ; Sound Designers and Engineers for the Bank
					enginners = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "IPRD": // [<IPRD-ck>] ; Product for which the Bank was intended
					product = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "ICOP": // [<ICOP-ck>] ; Contains any Copyright message
					copyright = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "ICMT": // [<ICMT-ck>] ; Contains any Comments on the Bank
					comments = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				case "ISFT": // [<ISFT-ck>] ; The SoundFont tools used to create and alter the bank
					tools = Trim(Bit.ReadStringAscii(bytes, ref i, chunk.size)); break;
				}
				i = chunk.end;
			}
			i = infoChunk.end;

			// <sdta-list> ; The Sample Binary Data 
			var sdtaChunk = new Chunk(bytes, ref i);
			// <sdta-ck> -> LIST (‘sdta’
			Bit.ReadStringAscii(bytes, ref i, 4);

			if (sdtaChunk.size > 0) {
				// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits 
				var smplChunk = new Chunk(bytes, ref i);
				short[] intData = new short[smplChunk.size >> 1];
				for (int j = 0, length = intData.Length; j < length; j += 1) {
					intData[j] = Bit.ReadInt16(bytes, ref i);
				}
				i = smplChunk.end;
				
				 data = new float[intData.Length];
				if (i < sdtaChunk.end) {
					UnityEngine.Debug.Log("24bit audio");
					// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits 
					var sm24Chunk = new Chunk(bytes, ref i);
					if (sm24Chunk.size == (smplChunk.size >> 1)) {
						for (int j = 0, length = intData.Length; j < length; j += 1) {
							data[j] = (float)(intData[j] << 8 | Bit.ReadByte(bytes, ref i)) / 0x7FFFFF;
						}
					}
				} else {
					for (int j = 0, length = intData.Length; j < length; j += 1) {
						data[j] = (float)intData[j] / 32767f;
					}
				}
			}
			i = sdtaChunk.end;

			// <pdta-list> ; The Preset, Instrument, and Sample Header data
			new Chunk(bytes, ref i);
			// <pdta-ck> -> LIST (‘pdta’ 
			Bit.ReadStringAscii(bytes, ref i, 4);

			// <phdr-ck> ; The Preset Headers
			var phdrChunk = new Chunk(bytes, ref i);
			var phdrList = new List<PresetHeader>();
			while (i < phdrChunk.end) phdrList.Add(new PresetHeader(bytes, ref i));
			i = phdrChunk.end;

			// <pbag-ck> ; The Preset Index list
			var pbagChunk = new Chunk(bytes, ref i);
			var pbagList = new List<Bag>();
			while (i < pbagChunk.end) pbagList.Add(new Bag(bytes, ref i));
			i = pbagChunk.end;

			// <pmod-ck> ; The Preset Modulator list
			var pmodChunk = new Chunk(bytes, ref i);
			var pmodList = new List<Modulator>();
			while (i < pmodChunk.end) pmodList.Add(new Modulator(bytes, ref i));
			i = pmodChunk.end;

			// <pgen-ck> ; The Preset Generator list
			var pgenChunk = new Chunk(bytes, ref i);
			var pgenList = new List<Generator>();
			while (i < pgenChunk.end) pgenList.Add(new Generator(bytes, ref i));
			i = pgenChunk.end;

			// <inst-ck> ; The Instrument Names and Indices
			var instChunk = new Chunk(bytes, ref i);
			var instList = new List<InstrumentHeader>();
			while (i < instChunk.end) instList.Add(new InstrumentHeader(bytes, ref i));
			i = instChunk.end;

			// <ibag-ck> ; The Instrument Index list
			var ibagChunk = new Chunk(bytes, ref i);
			var ibagList = new List<Bag>();
			while (i < ibagChunk.end) ibagList.Add(new Bag(bytes, ref i));
			i = ibagChunk.end;

			// <imod-ck> ; The Instrument Modulator list
			var imodChunk = new Chunk(bytes, ref i);
			var imodList = new List<Modulator>();
			while (i < imodChunk.end) imodList.Add(new Modulator(bytes, ref i));
			i = imodChunk.end;

			// <igen-ck> ; The Instrument Generator list
			var igenChunk = new Chunk(bytes, ref i);
			var igenList = new List<Generator>();
			while (i < igenChunk.end) igenList.Add(new Generator(bytes, ref i));
			i = igenChunk.end;

			// <shdr-ck> ; The Sample Headers 
			var shdrChunk = new Chunk(bytes, ref i);
			var shdrList = new List<SampleHeader>();
			while (i < shdrChunk.end) shdrList.Add(new SampleHeader(bytes, ref i));
			sampleHeaders = shdrList.ToArray();
			UnityEngine.Debug.Log(shdrChunk.id);

			// compile
			instruments = new Sf2Instrument[instList.Count - 1];
			UnityEngine.Debug.LogFormat("{0} instruments", instruments.Length);
			for (int j = 0; j < instruments.Length; j += 1) {
				instruments[j] = new Sf2Instrument();
				instruments[j].instName = instList[j].instName;

				var instrumentZoneList = new List<Sf2InstrumentZone>();

				for (int k = instList[j].instBagNdx; k < instList[j + 1].instBagNdx; k += 1) {
					int iZoneGenStart = ibagList[k].genNdx;
					int iZoneGenEnd = ibagList[k + 1].genNdx;
					if (iZoneGenStart >= iZoneGenEnd) continue;  // must skip if no generator

					// A global zone is determined by the fact that the last generator in the list is not a sampleID generator.
					if (igenList[iZoneGenEnd - 1].gen != GeneratorType.sampleId) {
						instruments[j].globalZone = new Sf2Zone(igenList, iZoneGenStart, iZoneGenEnd);
						continue;
					}

					var instrumentZone = new Sf2InstrumentZone();
					instrumentZoneList.Add(instrumentZone);
					instrumentZone.sampleHeader = shdrList[igenList[iZoneGenEnd - 1].amount];
					instrumentZone.zone = new Sf2Zone(igenList, iZoneGenStart, iZoneGenEnd);
				}

				instruments[j].instrumentZones = instrumentZoneList.ToArray();
			}

			presets = new Sf2Preset[phdrList.Count - 1];
			UnityEngine.Debug.LogFormat("{0} presets", presets.Length);
			for (int j = 0; j < presets.Length; j += 1) {
				presets[j] = new Sf2Preset();
				presets[j].presetName = Trim(phdrList[j].presetName);
				presets[j].preset = phdrList[j].preset;
				presets[j].bank = phdrList[j].bank;

				var presetZoneList = new List<Sf2PresetZone>();

				for (int k = phdrList[j].presetBagNdx; k < phdrList[j + 1].presetBagNdx; k += 1) {
					int pZoneGenStart = pbagList[k].genNdx;
					int pZoneGenEnd = pbagList[k + 1].genNdx;
					if (pZoneGenStart >= pZoneGenEnd) continue;  // must skip if no generator

					// A global zone is determined by the fact that the last generator in the list is not an Instrument generator.
					if (pgenList[pZoneGenEnd - 1].gen != GeneratorType.instrument) {
						presets[j].globalZone = new Sf2Zone(pgenList, pZoneGenStart, pZoneGenEnd);
						continue;
					}

					var presetZone = new Sf2PresetZone();
					presetZoneList.Add(presetZone);
					presetZone.instrument = instruments[pgenList[pZoneGenEnd - 1].amount];
					presetZone.zone = new Sf2Zone(pgenList, pZoneGenStart, pZoneGenEnd);
				}

				presets[j].presetZones = presetZoneList.ToArray();
			}

			System.Array.Sort(presets);
		}

		public int FindPreset(byte bank, byte preset) {
			for (int i = 0; i < presets.Length; i += 1) {
				if (presets[i].bank < bank) continue;
				if (presets[i].bank > bank) return -1;
				if (presets[i].preset < preset) continue;
				if (presets[i].preset > preset) return -1;
				return i;
			}
			return -1;
		}

		public static Sf2Zone GetAppliedZone(Sf2Zone iGlobalZone, Sf2Zone iZone, Sf2Zone pGlobalZone, Sf2Zone pZone) {
			// short v1 = 0, v2 = 0, v3 = 0, v4 = 0;
			// if (iGlobalZone != null) v1 = iGlobalZone.gens[GeneratorType.sustainVolEnv].value;
			// v2 = iZone.gens[GeneratorType.sustainVolEnv].value;
			// if (pGlobalZone != null) v2 = pGlobalZone.gens[GeneratorType.sustainVolEnv].value;
			// v4 = pZone.gens[GeneratorType.sustainVolEnv].value;
			// Console.Log(v1, v2, v3, v4);

			var zone = new Sf2Zone();
			zone.Default();
			if (iGlobalZone != null) zone.Set(iGlobalZone);
			zone.Set(iZone);

			/**
			 * SF 2.01 Page 65
			 * A generator in a local preset zone that is identical to a generator in a global preset zone
			 * supersedes or replaces that generator in the global preset zone. That generator then has its
			 * effects added to the destination-summing node of all zones in the given instrument.
 			 */
			var zone2 = new Sf2Zone();
			if (pGlobalZone != null) zone2.Set(pGlobalZone);
			zone2.Set(pZone);

			zone.Add(zone2);
			zone.Clamp();
			return zone;
		}

		static string Trim(string str) {
			int end = str.IndexOf('\0');
			if (end != -1) return str.Substring(0, end).Trim();
			return str.Trim();
		}
	}

	// [System.Serializable]
	public sealed class Sf2Preset : System.IComparable<Sf2Preset> {
		public string presetName;
		public int preset;
		public int bank;

		public Sf2Zone globalZone;
		public Sf2PresetZone[] presetZones;

		public int CompareTo(Sf2Preset other) {
			if (bank == other.bank) return preset - other.preset;
			return bank - other.bank;
		}
	}

	// [System.Serializable]
	public sealed class Sf2PresetZone {
		public Sf2Zone zone;
		public Sf2Instrument instrument;
	}

	// [System.Serializable]
	public sealed class Sf2Instrument {
		public string instName;

		public Sf2Zone globalZone;
		public Sf2InstrumentZone[] instrumentZones;
	}

//	[System.Serializable]
	public sealed class Sf2InstrumentZone {
		public Sf2Zone zone;
		public Sf2File.SampleHeader sampleHeader;
	}

//	[System.Serializable]
	public sealed class Sf2Zone {
//		[System.Serializable]
		public struct Gen {
			public bool flag;
			public short value;
		}

		public byte noteLo;
		public byte noteHi = 127;
		public byte velocityLo;
		public byte velocityHi = 127;

		public Gen[] gens = new Gen[Sf2File.GeneratorType.end];

		public Sf2Zone() {}

		public Sf2Zone(List<Sf2File.Generator> gs, int start, int end) {
			for (int i = start; i < end; i += 1) Set(gs[i]);
		}

		public void Set(Sf2File.Generator g) {
			switch (g.gen) {
			case Sf2File.GeneratorType.keyRange:
				noteLo = g.amountLo;
				noteHi = g.amountHi;
				break;
			case Sf2File.GeneratorType.velRange:
				velocityLo = g.amountLo;
				velocityHi = g.amountHi;
				break;
			default:
				gens[g.gen].value = g.amount;
				gens[g.gen].flag = true;
				break;	
			}
		}

		public void Set(Sf2Zone z) {
			noteLo = z.noteLo; noteHi = z.noteHi;
			velocityLo = z.velocityLo; velocityHi = z.velocityHi;
			
			for (int i = 0; i < z.gens.Length; i += 1) {
				if (z.gens[i].flag) {
					gens[i].value = z.gens[i].value;
					gens[i].flag = true;
				}
			}
		}

		public void Add(Sf2Zone z) {
			/**
			 * If the generator operator is a Range Generator, the generator values are NOT ADDED to
			 * those in the instrument level, rather they serve as an intersection filter to those key number or
			 * velocity ranges in the instrument that is used in the preset zone.
			 */
			if (noteLo < z.noteLo) noteLo = z.noteLo;
			if (z.noteHi < noteHi) noteHi = z.noteHi;
			if (velocityLo < z.velocityLo) velocityLo = z.velocityLo;
			if (z.velocityHi < velocityHi) velocityHi = z.velocityHi;

			for (int i = 0; i < z.gens.Length; i += 1) {
				if (z.gens[i].flag) {
					gens[i].value += z.gens[i].value;
					gens[i].flag = true;
				}
			}
		}

		public bool Contains(byte note, byte velocity) {
			return noteLo <= note && note <= noteHi && velocityLo <= velocity && velocity <= velocityHi;
		}

		public void Default() {
			for (int i = 0; i < Sf2File.GeneratorType.end; i += 1) gens[i].value = 0;

			gens[Sf2File.GeneratorType.initialFilterFc].value = 13500;
			gens[Sf2File.GeneratorType.delayModLfo].value = -12000;
			gens[Sf2File.GeneratorType.delayVibLfo].value = -12000;
			gens[Sf2File.GeneratorType.delayModEnv].value = -12000;
			gens[Sf2File.GeneratorType.attackModEnv].value = -12000;
			gens[Sf2File.GeneratorType.holdModEnv].value = -12000;
			gens[Sf2File.GeneratorType.decayModEnv].value = -12000;
			gens[Sf2File.GeneratorType.releaseModEnv].value = -12000;
			gens[Sf2File.GeneratorType.delayVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.attackVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.holdVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.decayVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.releaseVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.keynum].value = -1;
			gens[Sf2File.GeneratorType.velocity].value = -1;
			gens[Sf2File.GeneratorType.scaleTuning].value = 100;
			gens[Sf2File.GeneratorType.overridingRootKey].value = -1;
		}

		public void Clamp() {
			// const short min = short.MinValue;
			// const short max = short.MaxValue;
			
			// Clamp(GenType.startAddrsOffset,           0,      max);
			// Clamp(GenType.endAddrsOffset,             min,    0);
			// Clamp(GenType.startloopAddrsOffset,       min,    max);
			// Clamp(GenType.endloopAddrsOffset,         min,    max);
			// Clamp(GenType.startAddrsCoarseOffset,     0,      max);
			Clamp(GenType.modLfoToPitch,              -12000, 12000);
			Clamp(GenType.vibLfoToPitch,              -12000, 12000);
			Clamp(GenType.modEnvToPitch,              -12000, 12000);
			Clamp(GenType.initialFilterFc,            1500,   13500);
			Clamp(GenType.initialFilterQ,             0,      960);
			Clamp(GenType.modLfoToFilterFc,           -12000, 12000);
			Clamp(GenType.modEnvToFilterFc,           -12000, 12000);
			// Clamp(GenType.endAddrsCoarseOffset,       min,    0);
			Clamp(GenType.modLfoToVolume,             -960,   960);
			Clamp(GenType.chorusEffectsSend,          0,      1000);
			Clamp(GenType.reverbEffectsSend,          0,      1000);
			Clamp(GenType.pan,                        -500,   500);
			Clamp(GenType.delayModLfo,                -12000, 5000);
			Clamp(GenType.freqModLfo,                 -16000, 4500);
			Clamp(GenType.delayVibLfo,                -12000, 5000);
			Clamp(GenType.freqVibLfo,                 -16000, 4500);
			Clamp(GenType.delayModEnv,                -12000, 5000);
			Clamp(GenType.attackModEnv,               -12000, 8000);
			Clamp(GenType.holdModEnv,                 -12000, 5000);
			Clamp(GenType.decayModEnv,                -12000, 8000);
			Clamp(GenType.sustainModEnv,              0,      1000);
			Clamp(GenType.releaseModEnv,              -12000, 8000);
			Clamp(GenType.keynumToModEnvHold,         -1200,  1200);
			Clamp(GenType.keynumToModEnvDecay,        -1200,  1200);
			Clamp(GenType.delayVolEnv,                -12000, 5000);
			Clamp(GenType.attackVolEnv,               -12000, 8000);
			Clamp(GenType.holdVolEnv,                 -12000, 5000);
			Clamp(GenType.decayVolEnv,                -12000, 8000);
			Clamp(GenType.sustainVolEnv,              0,      1440);
			Clamp(GenType.releaseVolEnv,              -12000, 8000);
			Clamp(GenType.keynumToVolEnvHold,         -1200,  1200);
			Clamp(GenType.keynumToVolEnvDecay,        -1200,  1200);
			// Clamp(GenType.instrument);
			Clamp(GenType.keyRange,                   0,      127);
			Clamp(GenType.velRange,                   0,      127);
			// Clamp(GenType.startloopAddrsCoarseOffset, min,    max);
			Clamp(GenType.keynum,                     0,      127);
			Clamp(GenType.velocity,                   1,      127);
			Clamp(GenType.initialAttenuation,         0,      1440);
			// Clamp(GenType.endloopAddrsCoarseOffset,   min,    max);
			Clamp(GenType.coarseTune,                 -120,   120);
			Clamp(GenType.fineTune,                   -99,    99);
			// Clamp(GenType.sampleId);
			// Clamp(GenType.sampleModes,                min,    max);
			Clamp(GenType.scaleTuning,                0,      1200);
			Clamp(GenType.exclusiveClass,             1,      127);
			Clamp(GenType.overridingRootKey,          0,      127);
		}

		void Clamp(int type, short min, short max) {
			if (gens[type].flag) {
				if (gens[type].value < min) {
					Console.Log("clamp < min", type, gens[type].value, min, max);
					gens[type].value = min;
				} else if (gens[type].value > max) {
					Console.Log("clamp > max", type, gens[type].value, min, max);
					gens[type].value = max;
				}
			}
		}
	}
}

