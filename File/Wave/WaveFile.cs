namespace Midif.File.Wave {
	public enum WaveFileFormat {
		MicrosoftPcm = 0x0001,

		IeeeFloat = 0x0003,

		IbmMuLaw = 0x0101,
		IbmALaw = 0x0102,
		IbmAdpcm = 0x0103,

		Extensible = 0xFFFE
	}

	// Only support MicrosoftPcm.
	[System.Serializable]
	public class WaveFile {
		public string WaveId;
		// A number indicating the WAVE format category of the file.
		public int FormatTag;
		// The number of channels represented in the waveform data, such as 1 for mono or 2 for stereo.
		public int Channels;
		public bool SupportStereo;
		// The sampling rate (in samples per second) at which each channel should be played.
		public int SamplePerSec;
		// The average number of bytes per second at which the waveform data should be transferred.
		public int AvgBytesPerSec;
		// The block alignment (in bytes) of the waveform data.
		public int BlockAlign;
		// The number of bits of data used to represent each sample of each channel.
		public int BitsPerSample;
		public byte[] WaveData;

		public double[][] Samples;
		public int Scale = 1;

		public WaveFileFormat Format {
			get { return (WaveFileFormat)FormatTag; }
		}

		public override string ToString () {
			return string.Format("[WaveFile: Channels={0}, SamplePerSec={1}, BitsPerSample={2}, Format={3}]", Channels, SamplePerSec, BitsPerSample, Format);
		}
	}
}