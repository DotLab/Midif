using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Midif.File.Sf2 {
	[System.Serializable]
	public class Sf2File {
		// <INFO-list> -> LIST (‘INFO’
		// <ifil-ck> ; Refers to the version of the Sound Font RIFF file
		public string Version;
		// <isng-ck> ; Refers to the target Sound Engine
		public string TargetSoundEngine;
		// <INAM-ck> ; Refers to the Sound Font Bank Name
		public string Name;
		// [<ICRD-ck>] ; Refers to the Date of Creation of the Bank
		public string DateOfCreation;
		// [<IENG-ck>] ; Sound Designers and Engineers for the Bank
		public string Enginners;
		// [<IPRD-ck>] ; Product for which the Bank was intended
		public string Product;
		// [<ICOP-ck>] ; Contains any Copyright message
		public string Copyright;
		// [<ICMT-ck>] ; Contains any Comments on the Bank
		public string Comments;
		// [<ISFT-ck>] ; The SoundFont tools used to create and alter the bank
		public string Tools;

		#region Generic Sf2 Structs

		// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits
		public short[] SampleData;
		// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits
		public byte[] SampleData24;

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

		#endregion

		public Sf2Instrument[] Instruments;

		public double[] Samples;

		#region Constructor

		public Sf2File (Stream stream) {
			// <SFBK-form> ; SoundFont 2 RIFF File Format 
			var sfbkChunk = new RiffChunk(stream);
			using (var sfbkStream = sfbkChunk.GetStream()) {
				// <SFBK-form> -> RIFF (‘sfbk’ ; RIFF form header
				var sfbkId = StreamHelperLe.ReadString(sfbkStream, 4);
				if (sfbkId != "sfbk")
					throw new FileFormatException("Sf2.sfbkId", sfbkId, "sfbk");

				// <INFO-list> ; Supplemental Information 
				var infoChunk = new RiffChunk(sfbkStream);
				using (var infoStream = infoChunk.GetStream()) {
					// <INFO-list> -> LIST (‘INFO’ 
					var infoId = StreamHelperLe.ReadString(infoStream, 4);
					if (infoId != "INFO")
						throw new FileFormatException("Sf2.infoChunk.infoId", infoId, "INFO");

					while (infoStream.Position < infoStream.Length) {
						var chunk = new RiffChunk(infoStream);
						switch (chunk.Id) {
						case "ifil":
							// <ifil-ck> ; Refers to the version of the Sound Font RIFF file 
							Version = BitConverter.ToUInt16(chunk.Data, 0) + "." + BitConverter.ToUInt16(chunk.Data, 2);
							break;
						case "isng":
							// <isng-ck> ; Refers to the target Sound Engine
							TargetSoundEngine = Encoding.UTF8.GetString(chunk.Data);
							break;
						case "INAM":
							// <INAM-ck> ; Refers to the Sound Font Bank Name
							Name = chunk.GetString();
							break;
						case "ICRD":
							// [<ICRD-ck>] ; Refers to the Date of Creation of the Bank
							DateOfCreation = chunk.GetString();
							break;
						case "IENG":
							// [<IENG-ck>] ; Sound Designers and Engineers for the Bank
							Enginners = chunk.GetString();
							break;
						case "IPRD":
							// [<IPRD-ck>] ; Product for which the Bank was intended
							Product = chunk.GetString();
							break;
						case "ICOP":
							// [<ICOP-ck>] ; Contains any Copyright message
							Copyright = chunk.GetString();
							break;
						case "ICMT":
							// [<ICMT-ck>] ; Contains any Comments on the Bank
							Comments = chunk.GetString();
							break;
						case "ISFT":
							// [<ISFT-ck>] ; The SoundFont tools used to create and alter the bank
							Tools = chunk.GetString();
							break;
						}
					}
				}

				// <sdta-list> ; The Sample Binary Data 
				var sdtaChunk = new RiffChunk(sfbkStream);
				using (var sdtaStream = sdtaChunk.GetStream()) {
					// <sdta-ck> -> LIST (‘sdta’
					var sdtaId = StreamHelperLe.ReadString(sdtaStream, 4);
					if (sdtaId != "sdta")
						throw new FileFormatException("Sf2.sdtaChunk.sdtaId", sdtaId, "sdta");

					// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits 
					if (sdtaStream.Position < sdtaStream.Length) {
						var smplChunk = new RiffChunk(sdtaStream);
						SampleData = new short[smplChunk.Size / 2];
						for (int i = 0; i < smplChunk.Size; i += 2)
							SampleData[i / 2] = BitConverter.ToInt16(smplChunk.Data, i);

						// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits 
						if (sdtaStream.Position < sdtaStream.Length) {
							var sm24Chunk = new RiffChunk(sdtaStream);
							SampleData24 = sm24Chunk.Data;
						}
					}
				}

				// <pdta-list> ; The Preset, Instrument, and Sample Header data
				var pdtaChunk = new RiffChunk(sfbkStream);
				using (var pdtaStream = pdtaChunk.GetStream()) {
					// <pdta-ck> -> LIST (‘pdta’ 
					var pdtaId = StreamHelperLe.ReadString(pdtaStream, 4);
					if (pdtaId != "pdta")
						throw new FileFormatException("Sf2.pdtaChunk.pdtaId", pdtaId, "pdta");

					// <phdr-ck> ; The Preset Headers
					var phdrChunk = new RiffChunk(pdtaStream);
					using (var phdrStream = phdrChunk.GetStream()) {
						var list = new List<PresetHeader>();
						while (phdrStream.Position < phdrStream.Length)
							list.Add(new PresetHeader(phdrStream));
						PresetHeaders = list.ToArray();
					}

					// <pbag-ck> ; The Preset Index list
					var pbagChunk = new RiffChunk(pdtaStream);
					using (var pbagStream = pbagChunk.GetStream()) {
						var list = new List<Bag>();
						while (pbagStream.Position < pbagStream.Length)
							list.Add(new Bag(pbagStream));
						PresetBags = list.ToArray();
					}

					// <pmod-ck> ; The Preset Modulator list
					var pmodChunk = new RiffChunk(pdtaStream);
					using (var pmodStream = pmodChunk.GetStream()) {
						var list = new List<Modulator>();
						while (pmodStream.Position < pmodStream.Length)
							list.Add(new Modulator(pmodStream));
						PresetModulators = list.ToArray();
					}

					// <pgen-ck> ; The Preset Generator list
					var pgenChunk = new RiffChunk(pdtaStream);
					using (var pgenStream = pgenChunk.GetStream()) {
						var list = new List<Generator>();
						while (pgenStream.Position < pgenStream.Length)
							list.Add(new Generator(pgenStream));
						PresetGenerators = list.ToArray();
					}

					// <inst-ck> ; The Instrument Names and Indices
					var instChunk = new RiffChunk(pdtaStream);
					using (var instStream = instChunk.GetStream()) {
						var list = new List<InstrumentHeader>();
						while (instStream.Position < instStream.Length)
							list.Add(new InstrumentHeader(instStream));
						InstrumentHeaders = list.ToArray();
					}

					// <ibag-ck> ; The Instrument Index list
					var ibagChunk = new RiffChunk(pdtaStream);
					using (var ibagStream = ibagChunk.GetStream()) {
						var list = new List<Bag>();
						while (ibagStream.Position < ibagStream.Length)
							list.Add(new Bag(ibagStream));
						InstrumentBags = list.ToArray();
					}

					// <imod-ck> ; The Instrument Modulator list
					var imodChunk = new RiffChunk(pdtaStream);
					using (var imodStream = imodChunk.GetStream()) {
						var list = new List<Modulator>();
						while (imodStream.Position < imodStream.Length)
							list.Add(new Modulator(imodStream));
						InstrumentModulators = list.ToArray();
					}

					// <igen-ck> ; The Instrument Generator list
					var igenChunk = new RiffChunk(pdtaStream);
					using (var igenStream = igenChunk.GetStream()) {
						var list = new List<Generator>();
						while (igenStream.Position < igenStream.Length)
							list.Add(new Generator(igenStream));
						InstrumentGenerators = list.ToArray();
					}

					// <shdr-ck> ; The Sample Headers 
					var shdrChunk = new RiffChunk(pdtaStream);
					using (var shdrStream = shdrChunk.GetStream()) {
						var list = new List<SampleHeader>();
						while (shdrStream.Position < shdrStream.Length)
							list.Add(new SampleHeader(shdrStream));
						SampleHeaders = list.ToArray();
					}
				}
			}

			Compile();
		}

		#endregion

		void Compile () {
			Samples = new double[SampleData.Length];

			if (SampleData24 == null)
				for (int i = 0; i < SampleData.Length; i++)
					Samples[i] = (double)SampleData[i] / 0x7FFF;
			else
				for (int i = 0; i < SampleData.Length; i++)
					Samples[i] = (double)(SampleData[i] << 8 | SampleData24[i]) / 0x7FFFFF;

			// Build Samples for each SampleHeader;
			foreach (var header in SampleHeaders) {
				header.Samples = new double[header.End - header.Start + 1];
				Array.Copy(Samples, header.Start, header.Samples, 0, header.Samples.Length);
			}

			// The last instrument is EOI.
			Instruments = new Sf2Instrument[InstrumentHeaders.Length - 1];
			for (int instNdx = 0; instNdx < InstrumentHeaders.Length - 1; instNdx++) {
//				DebugConsole.WriteLine(InstrumentHeaders[instNdx]);
				var inst = new Sf2Instrument(InstrumentHeaders[instNdx].InstName);
				Sf2Zone globalZone = null;

				for (int bagNdx = InstrumentHeaders[instNdx].InstBagNdx; 
					bagNdx < InstrumentHeaders[instNdx + 1].InstBagNdx;
					bagNdx++) {
//					DebugConsole.WriteLine("\t" + InstrumentBags[bagNdx]);
					var zone = new Sf2Zone();

					for (int genNdx = InstrumentBags[bagNdx].GenNdx; 
						genNdx < InstrumentBags[bagNdx + 1].GenNdx; 
						genNdx++) {
//						DebugConsole.WriteLine("\t\t" + InstrumentGenerators[genNdx]);
						zone.Generators.Add(InstrumentGenerators[genNdx]);
					}

					// Apply globalZone.
					if (globalZone != null)
						foreach (var globalGen in globalZone.Generators) {
							var notOverride = true;

							foreach (var gen in zone.Generators)
								if (gen.Gen == globalGen.Gen) {
									notOverride = false;
									break;
								}

							if (notOverride)
								zone.Generators.Insert(zone.Generators.Count - 1, globalGen);
						}

					// A global zone is determined by the fact that
					// the last generator in the list is not a sampleID generator.
					if (
						globalZone == null &&
						inst.Zones.Count == 0 &&
						zone.Generators[zone.Generators.Count - 1].Gen != GeneratorType.SampleID)
						globalZone = zone;
					else
						inst.Zones.Add(zone);
				}

				Instruments[instNdx] = inst;
			}
		}

		public override string ToString () {
			return string.Format("[Sf2File: Version={0}, Name={1}, DateOfCreation={2}, Enginners={3}]", Version, Name, DateOfCreation, Enginners);
		}
	}
}