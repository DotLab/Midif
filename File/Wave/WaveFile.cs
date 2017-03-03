using System.IO;

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

		#region Constructor

		public WaveFile (Stream stream) {
			var masterChunk = new RiffChunk(stream);
			if (masterChunk.Id != "RIFF")
				throw new FileFormatException("WaveFile.masterChunk.Id", masterChunk.Id, "RIFF");

			// masterChunk
			using (var masterStream = masterChunk.GetStream()) {
				WaveId = StreamHelperLe.ReadString(masterStream, 4);
				if (WaveId != "WAVE")
					throw new FileFormatException("WaveFile.WaveId", WaveId, "WAVE");

				// formatChunk
				var formatChunk = new RiffChunk(masterStream);
				if (formatChunk.Id != "fmt ")
					throw new FileFormatException("WaveFile.formatChunk.Id", formatChunk.Id, "fmt ");
				using (var formatStream = formatChunk.GetStream()) {
					FormatTag = StreamHelperLe.ReadUInt16(formatStream);
					Channels = StreamHelperLe.ReadUInt16(formatStream);
					SupportStereo = Channels > 1;
					SamplePerSec = (int)StreamHelperLe.ReadUInt32(formatStream);
					AvgBytesPerSec = (int)StreamHelperLe.ReadUInt32(formatStream);
					BlockAlign = StreamHelperLe.ReadUInt16(formatStream);
					BitsPerSample = StreamHelperLe.ReadUInt16(formatStream);
				}

				if (Format != WaveFileFormat.MicrosoftPcm)
					throw new FileFormatException("WaveFile.Format", Format, WaveFileFormat.MicrosoftPcm);

				// dataChunk
				while (masterStream.Position < masterStream.Length) {
					var chunk = new RiffChunk(masterStream);
					if (chunk.Id == "data") {
						WaveData = chunk.Data;
						using (var dataStream = chunk.GetStream()) {
							BuildSamples(dataStream);
						}
						break;
					}
				}

				if (WaveData == null)
					throw new FileFormatException("WaveFile.WaveData", "null", "byte[]");
			}
		}

		void BuildSamples (Stream stream) {
			if ((BitsPerSample & 0x7) > 0)
				throw new FileFormatException("WaveFile.BitsPerSample", BitsPerSample, "[multiple of 8]");
			var bytesPerSample = BitsPerSample / 8;
			var sampleCount = WaveData.Length / bytesPerSample / Channels;

			Samples = new double[Channels][];
			for (int i = 0; i < Channels; i++)
				Samples[i] = new double[sampleCount];

			switch (bytesPerSample) {
			case 1:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < Channels; j++)
						Samples[j][i] = (double)(stream.ReadByte() - 0x80) / 0x7F;
				break;
			case 2:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < Channels; j++)
						Samples[j][i] = (double)(stream.ReadByte() | (sbyte)stream.ReadByte() << 8) / 0x7FFF;
				break;
			case 3:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < Channels; j++)
						Samples[j][i] = (double)(stream.ReadByte() | stream.ReadByte() << 8 | (sbyte)stream.ReadByte() << 16) / 0x7FFFFF;
				break;
			case 4:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < Channels; j++)
						Samples[j][i] = (double)(stream.ReadByte() | stream.ReadByte() << 8 | stream.ReadByte() << 16 | (sbyte)stream.ReadByte() << 24) / 0x7FFFFFFF;
				break;
			}

			if (stream.Position < stream.Length)
				throw new FileFormatException("WaveFile.stream.Position", stream.Position, stream.Length);
		}

		#endregion

		public override string ToString () {
			return string.Format("[WaveFile: Channels={0}, SamplePerSec={1}, BitsPerSample={2}, Format={3}]", Channels, SamplePerSec, BitsPerSample, Format);
		}
	}
}