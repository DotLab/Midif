using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Midif.File.Sf2 {
	public static class Sf2FileBuilder {
		public static Sf2File Build (byte[] bytes) {
			using (var stream = new MemoryStream(bytes)) {
				return Build(stream);
			}
		}

		public static Sf2File Build (Stream stream) {
			var file = new Sf2File();

			// <SFBK-form> ; SoundFont 2 RIFF File Format 
			var sfbkChunk = new RiffChunk(stream);
			using (var sfbkStream = sfbkChunk.GetStream()) {
				// <SFBK-form> -> RIFF (‘sfbk’ ; RIFF form header
				var sfbkId = StreamHelper.ReadString(sfbkStream, 4);
				if (sfbkId != "sfbk")
					throw new FileFormatException("Sf2.sfbkId", sfbkId, "sfbk");

				// <INFO-list> ; Supplemental Information 
				var infoChunk = new RiffChunk(sfbkStream);
				using (var infoStream = infoChunk.GetStream()) {
					// <INFO-list> -> LIST (‘INFO’ 
					var infoId = StreamHelper.ReadString(infoStream, 4);
					if (infoId != "INFO")
						throw new FileFormatException("Sf2.infoChunk.infoId", infoId, "INFO");

					while (infoStream.Position < infoStream.Length) {
						var chunk = new RiffChunk(infoStream);
						switch (chunk.Id) {
						case "ifil":
							// <ifil-ck> ; Refers to the version of the Sound Font RIFF file 
							file.Version = BitConverter.ToUInt16(chunk.Data, 0) + "." + BitConverter.ToUInt16(chunk.Data, 2);
							break;
						case "isng":
							// <isng-ck> ; Refers to the target Sound Engine
							file.TargetSoundEngine = Encoding.ASCII.GetString(chunk.Data);
							break;
						case "INAM":
							// <INAM-ck> ; Refers to the Sound Font Bank Name
							file.Name = chunk.GetString();
							break;
						case "ICRD":
							// [<ICRD-ck>] ; Refers to the Date of Creation of the Bank
							file.DateOfCreation = chunk.GetString();
							break;
						case "IENG":
							// [<IENG-ck>] ; Sound Designers and Engineers for the Bank
							file.Enginners = chunk.GetString();
							break;
						case "IPRD":
							// [<IPRD-ck>] ; Product for which the Bank was intended
							file.Product = chunk.GetString();
							break;
						case "ICOP":
							// [<ICOP-ck>] ; Contains any Copyright message
							file.Copyright = chunk.GetString();
							break;
						case "ICMT":
							// [<ICMT-ck>] ; Contains any Comments on the Bank
							file.Comments = chunk.GetString();
							break;
						case "ISFT":
							// [<ISFT-ck>] ; The SoundFont tools used to create and alter the bank
							file.Tools = chunk.GetString();
							break;
						}
					}
				}

				// <sdta-list> ; The Sample Binary Data 
				var sdtaChunk = new RiffChunk(sfbkStream);
				using (var sdtaStream = sdtaChunk.GetStream()) {
					// <sdta-ck> -> LIST (‘sdta’
					var sdtaId = StreamHelper.ReadString(sdtaStream, 4);
					if (sdtaId != "sdta")
						throw new FileFormatException("Sf2.sdtaChunk.sdtaId", sdtaId, "sdta");

					// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits 
					if (sdtaStream.Position < sdtaStream.Length) {
						var smplChunk = new RiffChunk(sdtaStream);
						file.Sample = new short[smplChunk.Size / 2];
						for (int i = 0; i < smplChunk.Size; i += 2)
							file.Sample[i / 2] = BitConverter.ToInt16(smplChunk.Data, i);

						// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits 
						if (sdtaStream.Position < sdtaStream.Length) {
							var sm24Chunk = new RiffChunk(sdtaStream);
							file.Sample24 = sm24Chunk.Data;
						}
					}
				}

				// <pdta-list> ; The Preset, Instrument, and Sample Header data
				var pdtaChunk = new RiffChunk(sfbkStream);
				using (var pdtaStream = pdtaChunk.GetStream()) {
					// <pdta-ck> -> LIST (‘pdta’ 
					var pdtaId = StreamHelper.ReadString(pdtaStream, 4);
					if (pdtaId != "pdta")
						throw new FileFormatException("Sf2.pdtaChunk.pdtaId", pdtaId, "pdta");

					// <phdr-ck> ; The Preset Headers
					var phdrChunk = new RiffChunk(pdtaStream);
					using (var phdrStream = phdrChunk.GetStream()) {
						var list = new List<PresetHeader>();
						while (phdrStream.Position < phdrStream.Length)
							list.Add(new PresetHeader(phdrStream));
						file.PresetHeaders = list.ToArray();
					}

					// <pbag-ck> ; The Preset Index list
					var pbagChunk = new RiffChunk(pdtaStream);
					using (var pbagStream = pbagChunk.GetStream()) {
						var list = new List<Bag>();
						while (pbagStream.Position < pbagStream.Length)
							list.Add(new Bag(pbagStream));
						file.PresetBags = list.ToArray();
					}

					// <pmod-ck> ; The Preset Modulator list
					var pmodChunk = new RiffChunk(pdtaStream);
					using (var pmodStream = pmodChunk.GetStream()) {
						var list = new List<Modulator>();
						while (pmodStream.Position < pmodStream.Length)
							list.Add(new Modulator(pmodStream));
						file.PresetModulators = list.ToArray();
					}

					// <pgen-ck> ; The Preset Generator list
					var pgenChunk = new RiffChunk(pdtaStream);
					using (var pgenStream = pgenChunk.GetStream()) {
						var list = new List<Generator>();
						while (pgenStream.Position < pgenStream.Length)
							list.Add(new Generator(pgenStream));
						file.PresetGenerators = list.ToArray();
					}

					// <inst-ck> ; The Instrument Names and Indices
					var instChunk = new RiffChunk(pdtaStream);
					using (var instStream = instChunk.GetStream()) {
						var list = new List<InstrumentHeader>();
						while (instStream.Position < instStream.Length)
							list.Add(new InstrumentHeader(instStream));
						file.InstrumentHeaders = list.ToArray();
					}

					// <ibag-ck> ; The Instrument Index list
					var ibagChunk = new RiffChunk(pdtaStream);
					using (var ibagStream = ibagChunk.GetStream()) {
						var list = new List<Bag>();
						while (ibagStream.Position < ibagStream.Length)
							list.Add(new Bag(ibagStream));
						file.InstrumentBags = list.ToArray();
					}

					// <imod-ck> ; The Instrument Modulator list
					var imodChunk = new RiffChunk(pdtaStream);
					using (var imodStream = imodChunk.GetStream()) {
						var list = new List<Modulator>();
						while (imodStream.Position < imodStream.Length)
							list.Add(new Modulator(imodStream));
						file.InstrumentModulators = list.ToArray();
					}

					// <igen-ck> ; The Instrument Generator list
					var igenChunk = new RiffChunk(pdtaStream);
					using (var igenStream = igenChunk.GetStream()) {
						var list = new List<Generator>();
						while (igenStream.Position < igenStream.Length)
							list.Add(new Generator(igenStream));
						file.InstrumentGenerators = list.ToArray();
					}

					// <shdr-ck> ; The Sample Headers 
					var shdrChunk = new RiffChunk(pdtaStream);
					using (var shdrStream = shdrChunk.GetStream()) {
						var list = new List<SampleHeader>();
						while (shdrStream.Position < shdrStream.Length)
							list.Add(new SampleHeader(shdrStream));
						file.SampleHeaders = list.ToArray();
					}
				}
			}

			return file;
		}
	}
}

