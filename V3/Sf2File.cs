using Unsaf;
using System.Collections.Generic;
using Encoding = System.Text.Encoding;

namespace Midif.V3 {
	[System.Serializable]
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
				int[] intData = new int[smplChunk.size >> 1];
				for (int j = 0, length = intData.Length; j < length; j += 1) {
					intData[j] = Bit.ReadInt16(bytes, ref i);
				}
				i = smplChunk.end;
				
				data = new float[intData.Length];
				if (i < sdtaChunk.end) {
					// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits 
					var sm24Chunk = new Chunk(bytes, ref i);
					if (sm24Chunk.size == (smplChunk.size >> 1)) {
						for (int j = 0, length = intData.Length; j < length; j += 1) {
							data[j] = (float)(intData[j] << 8 | Bit.ReadByte(bytes, ref i)) / 0x7FFFFF;
						}
					}
				} else {
					for (int j = 0, length = intData.Length; j < length; j += 1) {
						data[j] = (float)intData[j] / 0x7FFF;
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

				int iZoneCount = instList[j + 1].instBagNdx - instList[j].instBagNdx;
				for (int k = 0; k < iZoneCount; k += 1) {
					int iZoneGenStart = ibagList[instList[j].instBagNdx + k].genNdx;
					int iZoneGenEnd = ibagList[instList[j].instBagNdx + k + 1].genNdx;

					// A global zone is determined by the fact that the last generator in the list is not a sampleID generator.
					if (igenList[iZoneGenEnd - 1].gen != GeneratorType.SampleID) {
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

				int pZoneCount = phdrList[j + 1].presetBagNdx - phdrList[j].presetBagNdx;
				for (int k = 0; k < pZoneCount; k += 1) {
					int pZoneGenStart = pbagList[phdrList[j].presetBagNdx + k].genNdx;
					int pZoneGenEnd = pbagList[phdrList[j].presetBagNdx + k + 1].genNdx;

					// A global zone is determined by the fact that the last generator in the list is not an Instrument generator.
					if (pgenList[pZoneGenEnd - 1].gen != GeneratorType.Instrument) {
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

		public static Sf2Zone GetAppliedZone(Sf2Zone pGlobalZone, Sf2Zone pZone, Sf2Zone iGlobalZone, Sf2Zone iZone) {
			var zone = new Sf2Zone();
			zone.Default();
			if (iGlobalZone != null) zone.Set(iGlobalZone);
			zone.Set(iZone);
			if (pGlobalZone != null) zone.Add(pGlobalZone);
			zone.Add(pZone);
			return zone;
		}

		static string Trim(string str) {
			int end = str.IndexOf('\0');
			if (end != -1) return str.Substring(0, end).Trim();
			return str.Trim();
		}
	}

	[System.Serializable]
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

	[System.Serializable]
	public sealed class Sf2PresetZone {
		public Sf2Zone zone;
		public Sf2Instrument instrument;
	}

	[System.Serializable]
	public sealed class Sf2Instrument {
		public string instName;

		public Sf2Zone globalZone;
		public Sf2InstrumentZone[] instrumentZones;
	}

	public sealed class Sf2InstrumentZone {
		public Sf2Zone zone;
		public Sf2File.SampleHeader sampleHeader;
	}

	public sealed class Sf2Zone {
		public struct Gen {
			public bool flag;
			public short value;
		}

		public byte noteLo;
		public byte noteHi = 127;
		public byte velocityLo;
		public byte velocityHi = 127;

		public Gen[] gens = new Gen[Sf2File.GeneratorType.Last];

		public Sf2Zone() {}

		public Sf2Zone(List<Sf2File.Generator> gs, int start, int end) {
			for (int i = start; i < end; i += 1) Set(gs[i]);
		}

		public void Set(Sf2File.Generator g) {
			switch (g.gen) {
			case Sf2File.GeneratorType.KeyRange:
				noteLo = g.lo;
				noteHi = g.hi;
				break;
			case Sf2File.GeneratorType.VelRange:
				velocityLo = g.lo;
				velocityHi = g.hi;
				break;
			default:
				gens[g.gen].value = g.amount;
				gens[g.gen].flag = true;
				break;	
			}
		}

		public void Set(Sf2Zone z) {
			for (int i = 0; i < z.gens.Length; i += 1) {
				if (z.gens[i].flag) {
					gens[i].value = z.gens[i].value;
					gens[i].flag = true;
				}
			}
		}

		public void Add(Sf2Zone z) {
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
			gens[Sf2File.GeneratorType.InitialFilterFc].value = 13500;
			gens[Sf2File.GeneratorType.DelayModLFO].value = -12000;
			gens[Sf2File.GeneratorType.DelayVibLFO].value = -12000;
			gens[Sf2File.GeneratorType.DelayModEnv].value = -12000;
			gens[Sf2File.GeneratorType.AttackModEnv].value = -12000;
			gens[Sf2File.GeneratorType.HoldModEnv].value = -12000;
			gens[Sf2File.GeneratorType.DecayModEnv].value = -12000;
			gens[Sf2File.GeneratorType.ReleaseModEnv].value = -12000;
			gens[Sf2File.GeneratorType.DelayVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.AttackVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.HoldVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.DecayVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.ReleaseVolEnv].value = -12000;
			gens[Sf2File.GeneratorType.Keynum].value = -1;
			gens[Sf2File.GeneratorType.Velocity].value = -1;
			gens[Sf2File.GeneratorType.ScaleTuning].value = 100;
			gens[Sf2File.GeneratorType.OverridingRootKey].value = -1;
		}
	}
}

