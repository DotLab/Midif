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
		public float[] data;

		// <shdr-ck> ; The Sample Headers
		public SampleHeader[] sampleHeaders;

		public Sf2Preset[] presets;

		public Sf2File(byte[] bytes) {
			int i = 0;

			// <SFBK-form> ; SoundFont 2 RIFF File Format 
			var sfbkChunk = new Chunk(bytes, ref i);
			// <SFBK-form> -> RIFF (‘sfbk’ ; RIFF form header
			string sfbkId = Bit.ReadStringAscii(bytes, ref i, 4);

			// <INFO-list> ; Supplemental Information 
			var infoChunk = new Chunk(bytes, ref i);
			// <INFO-list> -> LIST (‘INFO’ 
			string infoId = Bit.ReadStringAscii(bytes, ref i, 4);

			while (i < infoChunk.end) {
				var chunk = new Chunk(bytes, ref i);
				switch (chunk.id) {
				case "ifil": // <ifil-ck> ; Refers to the version of the Sound Font RIFF file 
					majorVersion = Bit.ReadInt16(bytes, ref i); minorVersion = Bit.ReadInt16(bytes, ref i); break;
				case "isng": // <isng-ck> ; Refers to the target Sound Engine
					targetSoundEngine = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "INAM": // <INAM-ck> ; Refers to the Sound Font Bank Name
					name = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "ICRD": // [<ICRD-ck>] ; Refers to the Date of Creation of the Bank
					dateOfCreation = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "IENG": // [<IENG-ck>] ; Sound Designers and Engineers for the Bank
					enginners = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "IPRD": // [<IPRD-ck>] ; Product for which the Bank was intended
					product = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "ICOP": // [<ICOP-ck>] ; Contains any Copyright message
					copyright = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "ICMT": // [<ICMT-ck>] ; Contains any Comments on the Bank
					comments = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				case "ISFT": // [<ISFT-ck>] ; The SoundFont tools used to create and alter the bank
					tools = Bit.ReadStringUtf8(bytes, ref i, chunk.size); break;
				}
				i = chunk.end;
			}
			i = infoChunk.end;

			// <sdta-list> ; The Sample Binary Data 
			var sdtaChunk = new Chunk(bytes, ref i);
			// <sdta-ck> -> LIST (‘sdta’
			string sdtaId = Bit.ReadStringAscii(bytes, ref i, 4);

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
			var pdtaChunk = new Chunk(bytes, ref i);
			// <pdta-ck> -> LIST (‘pdta’ 
			var pdtaId = Bit.ReadStringAscii(bytes, ref i, 4);

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
			presets = new Sf2Preset[phdrList.Count - 1];
			UnityEngine.Debug.LogFormat("{0} presets", presets.Length);
			for (int j = 0; j < presets.Length; j += 1) {
				presets[j] = new Sf2Preset();
				presets[j].presetName = Trim(phdrList[j].presetName);
				presets[j].preset = phdrList[j].preset;
				presets[j].bank = phdrList[j].bank;

				int pZoneCount = phdrList[j + 1].presetBagNdx - phdrList[j].presetBagNdx;
				UnityEngine.Debug.LogFormat("\tpreset {0}: {1} to {2} ({3} zones) ", presets[j].presetName, phdrList[j].presetBagNdx, phdrList[j + 1].presetBagNdx, pZoneCount);
				for (int k = 0; k < pZoneCount; k += 1) {
					int pZoneGenStart = pbagList[phdrList[j].presetBagNdx + k].genNdx;
					int pZoneGenEnd = pbagList[phdrList[j].presetBagNdx + k + 1].genNdx;

					int instrumentIndex = -1;
					for (int l = pZoneGenStart; l < pZoneGenEnd; l += 1) {
						UnityEngine.Debug.LogFormat("\t\t\tpgen {0}: {3} ({1} - {2})", pgenList[l].gen, pgenList[l].amount.lo, pgenList[l].amount.hi, pgenList[l].amount.GetShort());
						if (pgenList[l].gen == GeneratorType.Instrument) instrumentIndex = pgenList[l].amount.GetShort();
					}

					if (instrumentIndex == -1) {  // not an instrument, but a global preset zone
						UnityEngine.Debug.LogFormat("\t\tglobal preset zone");
					} else {
						int iZoneCount = instList[instrumentIndex + 1].instBagNdx - instList[instrumentIndex].instBagNdx;
						UnityEngine.Debug.LogFormat("\t\tinstrument {0}: {1} to {2} ({3} zones) ", Trim(instList[instrumentIndex].instName), instList[instrumentIndex].instBagNdx, instList[instrumentIndex + 1].instBagNdx, iZoneCount);
						for (int l = 0; l < iZoneCount; l += 1) {
							int iZoneGenStart = ibagList[instList[instrumentIndex].instBagNdx + l].genNdx;
							int iZoneGenEnd = ibagList[instList[instrumentIndex].instBagNdx + l + 1].genNdx;

							int sampleId = -1;
							for (int m = iZoneGenStart; m < iZoneGenEnd; m += 1) {
								UnityEngine.Debug.LogFormat("\t\t\t\tigen {0}: {3} ({1} - {2})", igenList[m].gen, igenList[m].amount.lo, igenList[m].amount.hi, igenList[m].amount.GetShort());
								if (igenList[m].gen == GeneratorType.SampleID) sampleId = igenList[m].amount.GetShort();
							}

							if (sampleId == -1) {  // not a sample, but a global instrument zone
								UnityEngine.Debug.LogFormat("\t\t\tglobal instrument zone");
							} else {
								UnityEngine.Debug.LogFormat("\t\t\tsample {0}", sampleId);
							}
						}
					}
				}
			}
		}

		static string Trim(string str) {
			return str.Replace('\0', ' ').Trim();
		}
	}

	[System.Serializable]
	public sealed class Sf2Preset {
		public string presetName;
		public int preset;
		public int bank;

		public Sf2Instrument[] instruments;
	}

	[System.Serializable]
	public sealed class Sf2Instrument {
		public string instName;

		public Sf2Sample[] samples;
	}

	[System.Serializable]
	public sealed class Sf2Sample {
		public short startAddrsOffset;
		public short endAddrsOffset;
		public short startloopAddrsOffset;
		public short endloopAddrsOffset;
		public short startAddrsCoarseOffset;
		public short modLfoToPitch;
		public short vibLfoToPitch;
		public short modEnvToPitch;
		public short initialFilterFc = 13500;
		public short initialFilterQ;
		public short modLfoToFilterFc;
		public short modEnvToFilterFc;
		public short endAddrsCoarseOffset;
		public short modLfoToVolume;
		public short chorusEffectsSend;
		public short reverbEffectsSend;
		public short pan;
		public short delayModLFO = -12000;
		public short freqModLFO;
		public short delayVibLFO = -12000;
		public short freqVibLFO;
		public short delayModEnv = -12000;
		public short attackModEnv = -12000;
		public short holdModEnv = -12000;
		public short decayModEnv = -12000;
		public short sustainModEnv;
		public short releaseModEnv = -12000;
		public short keynumToModEnvHold;
		public short keynumToModEnvDecay;
		public short delayVolEnv = -12000;
		public short attackVolEnv = -12000;
		public short holdVolEnv = -12000;
		public short decayVolEnv = -12000;
		public short sustainVolEnv;
		public short releaseVolEnv = -12000;
		public short keynumToVolEnvHold;
		public short keynumToVolEnvDecay;
		public short instrument;
		public byte keyRangeLo;
		public byte keyRangeHi = 127;
		public byte velRangeLo;
		public byte velRangeHi = 127;
		public short startloopAddrsCoarseOffset;
		public short keynum = -1;
		public short velocity = -1;
		public short initialAttenuation;
		public short endloopAddrsCoarseOffset;
		public short coarseTune;
		public short fineTune;
		public short sampleID;
		public short sampleModes;
		public short scaleTuning = 100;
		public short exclusiveClass;
		public short overridingRootKey = -1;

		public void ApplyGenerator(Sf2File.Generator g) {
			switch (g.gen) {
			case Sf2File.GeneratorType.StartAddrsOffset:           startAddrsOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.EndAddrsOffset:             endAddrsOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.StartloopAddrsOffset:       startloopAddrsOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.EndloopAddrsOffset:         endloopAddrsOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.StartAddrsCoarseOffset:     startAddrsCoarseOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ModLfoToPitch:              modLfoToPitch = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.VibLfoToPitch:              vibLfoToPitch = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ModEnvToPitch:              modEnvToPitch = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.InitialFilterFc:            initialFilterFc = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.InitialFilterQ:             initialFilterQ = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ModLfoToFilterFc:           modLfoToFilterFc = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ModEnvToFilterFc:           modEnvToFilterFc = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.EndAddrsCoarseOffset:       endAddrsCoarseOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ModLfoToVolume:             modLfoToVolume = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ChorusEffectsSend:          chorusEffectsSend = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ReverbEffectsSend:          reverbEffectsSend = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.Pan:                        pan = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.DelayModLFO:                delayModLFO = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.FreqModLFO:                 freqModLFO = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.DelayVibLFO:                delayVibLFO = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.FreqVibLFO:                 freqVibLFO = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.DelayModEnv:                delayModEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.AttackModEnv:               attackModEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.HoldModEnv:                 holdModEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.DecayModEnv:                decayModEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.SustainModEnv:              sustainModEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ReleaseModEnv:              releaseModEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.KeynumToModEnvHold:         keynumToModEnvHold = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.KeynumToModEnvDecay:        keynumToModEnvDecay = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.DelayVolEnv:                delayVolEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.AttackVolEnv:               attackVolEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.HoldVolEnv:                 holdVolEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.DecayVolEnv:                decayVolEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.SustainVolEnv:              sustainVolEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ReleaseVolEnv:              releaseVolEnv = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.KeynumToVolEnvHold:         keynumToVolEnvHold = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.KeynumToVolEnvDecay:        keynumToVolEnvDecay = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.Instrument:                 instrument = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.KeyRange:                   keyRangeLo = g.amount.lo; keyRangeHi = g.amount.hi; return;
			case Sf2File.GeneratorType.VelRange:                   velRangeLo = g.amount.lo; velRangeHi = g.amount.hi; return;
			case Sf2File.GeneratorType.StartloopAddrsCoarseOffset: startloopAddrsCoarseOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.Keynum:                     keynum = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.Velocity:                   velocity = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.InitialAttenuation:         initialAttenuation = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.EndloopAddrsCoarseOffset:   endloopAddrsCoarseOffset = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.CoarseTune:                 coarseTune = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.FineTune:                   fineTune = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.SampleID:                   sampleID = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.SampleModes:                sampleModes = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ScaleTuning:                scaleTuning = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.ExclusiveClass:             exclusiveClass = g.amount.GetShort(); return;
			case Sf2File.GeneratorType.OverridingRootKey:          overridingRootKey = g.amount.GetShort(); return;
			}
		}
	}
}

