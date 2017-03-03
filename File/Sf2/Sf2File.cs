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


		public void Compile () {
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
				System.Array.Copy(Samples, header.Start, header.Samples, 0, header.Samples.Length);
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