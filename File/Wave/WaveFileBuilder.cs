using System.IO;

using Midif.File;

namespace Midif.File.Wave {
	public static class WaveFileBuilder {
		public static WaveFile Build (byte[] bytes) {
			using (var stream = new MemoryStream(bytes)) {
				return Build(stream);
			}
		}

		public static WaveFile Build (Stream stream) {
			var file = new WaveFile();

			var masterChunk = new RiffChunk(stream);
			if (masterChunk.Id != "RIFF")
				throw new FileFormatException("WaveFile.masterChunk.Id", masterChunk.Id, "RIFF");

			// masterChunk
			using (var masterStream = masterChunk.GetStream()) {
				file.WaveId = StreamHelper.ReadString(masterStream, 4);
				if (file.WaveId != "WAVE")
					throw new FileFormatException("WaveFile.WaveId", file.WaveId, "WAVE");

				// formatChunk
				var formatChunk = new RiffChunk(masterStream);
				if (formatChunk.Id != "fmt ")
					throw new FileFormatException("WaveFile.formatChunk.Id", formatChunk.Id, "fmt ");
				using (var formatStream = formatChunk.GetStream()) {
					file.FormatTag = StreamHelper.ReadUInt16(formatStream);
					file.Channels = StreamHelper.ReadUInt16(formatStream);
					file.SupportStereo = file.Channels > 1;
					file.SamplePerSec = (int)StreamHelper.ReadUInt32(formatStream);
					file.AvgBytesPerSec = (int)StreamHelper.ReadUInt32(formatStream);
					file.BlockAlign = StreamHelper.ReadUInt16(formatStream);
					file.BitsPerSample = StreamHelper.ReadUInt16(formatStream);
				}

				if (file.Format != WaveFileFormat.MicrosoftPcm)
					throw new FileFormatException("WaveFile.Format", file.Format, WaveFileFormat.MicrosoftPcm);

				// dataChunk
				while (masterStream.Position < masterStream.Length) {
					var chunk = new RiffChunk(masterStream);
					if (chunk.Id == "data") {
						file.WaveData = chunk.Data;
						using (var dataStream = chunk.GetStream()) {
							BuildSamples(file, dataStream);
						}
						break;
					}
				}

				if (file.WaveData == null)
					throw new FileFormatException("WaveFile.WaveData", "null", "byte[]");
			}

			return file;
		}

		static void BuildSamples (WaveFile file, Stream stream) {
			if ((file.BitsPerSample & 0x7) > 0)
				throw new FileFormatException("WaveFile.BitsPerSample", file.BitsPerSample, "[multiple of 8]");
			var bytesPerSample = file.BitsPerSample / 8;
			var sampleCount = file.WaveData.Length / bytesPerSample / file.Channels;

			file.Samples = new double[file.Channels][];
			for (int i = 0; i < file.Channels; i++)
				file.Samples[i] = new double[sampleCount];

			switch (bytesPerSample) {
			case 1:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (double)(stream.ReadByte() - 0x80) / 0x7F;
				break;
			case 2:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (double)(stream.ReadByte() | (sbyte)stream.ReadByte() << 8) / 0x7FFF;
				break;
			case 3:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (double)(stream.ReadByte() | stream.ReadByte() << 8 | (sbyte)stream.ReadByte() << 16) / 0x7FFFFF;
				break;
			case 4:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (double)(stream.ReadByte() | stream.ReadByte() << 8 | stream.ReadByte() << 16 | (sbyte)stream.ReadByte() << 24) / 0x7FFFFFFF;
				break;
			}

			if (stream.Position < stream.Length)
				throw new FileFormatException("WaveFile.stream.Position", stream.Position, stream.Length);
		}
	}
}

