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

		// [<smpl-ck>] ; The Digital Audio Samples for the upper 16 bits
		public short[] Sample;
		// [<sm24-ck>] ; The Digital Audio Samples for the lower 8 bits
		public byte[] Sample24;

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

		public override string ToString () {
			return string.Format("[Sf2File: Version={0}, Name={1}, DateOfCreation={2}, Enginners={3}]", Version, Name, DateOfCreation, Enginners);
		}
	}
}