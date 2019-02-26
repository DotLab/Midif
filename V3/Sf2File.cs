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
		public float[] samples;

		// <phdr-ck> ; The Preset Headers
		public PresetHeader[] PresetHeaders;
		// <pbag-ck> ; The Preset Index list
		public Bag[] PresetBags;
		// <pmod-ck> ; The Preset Modulator list
		public Modulator[] PresetModulators;
		// <pgen-ck> ; The Preset Generator list
		public Generator[] PresetGenerators;

		// <inst-ck> ; The Instrument Names and Indices
		public InstrumentHeader[] InstrumentHeaders;
		// <ibag-ck> ; The Instrument Index list
		public Bag[] InstrumentBags;
		// <imod-ck> ; The Instrument Modulator list
		public Modulator[] InstrumentModulators;
		// <igen-ck> ; The Instrument Generator list
		public Generator[] InstrumentGenerators;

		// <shdr-ck> ; The Sample Headers
		public SampleHeader[] SampleHeaders;

		public Sf2File(byte[] bytes) {
			int i = 0;

			// <SFBK-form> ; SoundFont 2 RIFF File Format 
			var sfbkChunk = new Chunk(bytes, ref i);
			// <SFBK-form> -> RIFF (‘sfbk’ ; RIFF form header
			string sfbkId = Bit.ReadStringAscii(bytes, ref i, 4);
			UnityEngine.Debug.LogFormat("{0}", sfbkId);

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
			UnityEngine.Debug.Log(sdtaId);

			if (sdtaChunk.size > 0) {
				// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits 
				var smplChunk = new Chunk(bytes, ref i);
				int[] intsamples = new int[smplChunk.size >> 1];
				for (int j = 0, length = intsamples.Length; j < length; j += 1) {
					intsamples[j] = Bit.ReadInt16(bytes, ref i);
				}
				i = smplChunk.end;
				
				samples = new float[intsamples.Length];
				if (i < sdtaChunk.end) {
					// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits 
					var sm24Chunk = new Chunk(bytes, ref i);
					if (sm24Chunk.size == (smplChunk.size >> 1)) {
						for (int j = 0, length = intsamples.Length; j < length; j += 1) {
							samples[j] = (float)(intsamples[j] << 8 | Bit.ReadByte(bytes, ref i)) / 0x7FFFFF;
						}
					}
					i = sm24Chunk.end;
				} else {
					for (int j = 0, length = intsamples.Length; j < length; j += 1) {
						samples[j] = (float)intsamples[j] / 0x7FFF;
					}
				}
			}
			i = sdtaChunk.end;

			// <pdta-list> ; The Preset, Instrument, and Sample Header data
			var pdtaChunk = new Chunk(bytes, ref i);
			// <pdta-ck> -> LIST (‘pdta’ 
			var pdtaId = Bit.ReadStringAscii(bytes, ref i, 4);
			UnityEngine.Debug.Log(pdtaId);

			// <phdr-ck> ; The Preset Headers
			var phdrChunk = new Chunk(bytes, ref i);
			var phdrList = new List<PresetHeader>();
			while (i < phdrChunk.end) phdrList.Add(new PresetHeader(bytes, ref i));
			PresetHeaders = phdrList.ToArray();
			i = phdrChunk.end;

			// <pbag-ck> ; The Preset Index list
			var pbagChunk = new Chunk(bytes, ref i);
			var pbagList = new List<Bag>();
			while (i < pbagChunk.end) pbagList.Add(new Bag(bytes, ref i));
			PresetBags = pbagList.ToArray();
			i = pbagChunk.end;

			// <pmod-ck> ; The Preset Modulator list
			var pmodChunk = new Chunk(bytes, ref i);
			var pmodList = new List<Modulator>();
			while (i < pmodChunk.end) pmodList.Add(new Modulator(bytes, ref i));
			PresetModulators = pmodList.ToArray();
			i = pmodChunk.end;

			// <pgen-ck> ; The Preset Generator list
			var pgenChunk = new Chunk(bytes, ref i);
			var pgenList = new List<Generator>();
			while (i < pgenChunk.end) pgenList.Add(new Generator(bytes, ref i));
			PresetGenerators = pgenList.ToArray();
			i = pgenChunk.end;

			// <inst-ck> ; The Instrument Names and Indices
			var instChunk = new Chunk(bytes, ref i);
			var instList = new List<InstrumentHeader>();
			while (i < instChunk.end) instList.Add(new InstrumentHeader(bytes, ref i));
			InstrumentHeaders = instList.ToArray();
			i = instChunk.end;

			// <ibag-ck> ; The Instrument Index list
			var ibagChunk = new Chunk(bytes, ref i);
			var ibagList = new List<Bag>();
			while (i < ibagChunk.end) ibagList.Add(new Bag(bytes, ref i));
			InstrumentBags = ibagList.ToArray();
			i = ibagChunk.end;

			// <imod-ck> ; The Instrument Modulator list
			var imodChunk = new Chunk(bytes, ref i);
			var imodList = new List<Modulator>();
			while (i < imodChunk.end) imodList.Add(new Modulator(bytes, ref i));
			InstrumentModulators = imodList.ToArray();
			i = imodChunk.end;

			// <igen-ck> ; The Instrument Generator list
			var igenChunk = new Chunk(bytes, ref i);
			var igenList = new List<Generator>();
			while (i < igenChunk.end) igenList.Add(new Generator(bytes, ref i));
			InstrumentGenerators = igenList.ToArray();
			i = igenChunk.end;

			// <shdr-ck> ; The Sample Headers 
			var shdrChunk = new Chunk(bytes, ref i);
			var shdrList = new List<SampleHeader>();
			while (i < shdrChunk.end) shdrList.Add(new SampleHeader(bytes, ref i));
			SampleHeaders = shdrList.ToArray();
			UnityEngine.Debug.Log(shdrChunk.id);
		}
	}
}

