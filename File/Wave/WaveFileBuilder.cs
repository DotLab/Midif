using System;
using System.IO;
using System.Text;

namespace Midif.File.Wave {
	public static class WaveFileBuilder {
		/// <summary>
		/// Like MidiStreamHelper, but with little endien
		/// </summary>
		static class StreamHelper {
			const int Int16Length = 2;
			const int Int32Length = 4;

			public static string ReadString (Stream stream, int length) {
				var bytes = new byte[length];
				stream.Read(bytes, 0, length);
				return Encoding.ASCII.GetString(bytes);
			}

			public static int ReadInt16 (Stream stream) {
				var bytes = new byte[Int16Length];
				stream.Read(bytes, 0, Int16Length);
				return (int)BitConverter.ToInt16(bytes, 0);
			}

			public static int ReadInt32 (Stream stream) {
				var bytes = new byte[Int32Length];
				stream.Read(bytes, 0, Int32Length);
				return BitConverter.ToInt32(bytes, 0);
			}

			public static int ReadUInt16 (Stream stream) {
				var bytes = new byte[Int16Length];
				stream.Read(bytes, 0, Int16Length);
				return (int)BitConverter.ToUInt16(bytes, 0);
			}

			public static int ReadUInt32 (Stream stream) {
				var bytes = new byte[Int32Length];
				stream.Read(bytes, 0, Int32Length);
				return (int)BitConverter.ToUInt32(bytes, 0);
			}

			public static double ReadSample8 (Stream stream) {
				return (double)(stream.ReadByte() - 0x80) / 0x7F;
			}

			public static double ReadSample16 (Stream stream) {
				return (double)(stream.ReadByte() | stream.ReadByte() << 8 | (sbyte)stream.ReadByte() << 16) / 0x7FFFFF;
			}

			public static double ReadSample24 (Stream stream) {
				return (double)(stream.ReadByte() | stream.ReadByte() << 8 | stream.ReadByte() << 16 | (sbyte)stream.ReadByte() << 24) / 0x7FFFFFFF;
			}

			public static double ReadSample32 (Stream stream) {
				return (double)(stream.ReadByte() | stream.ReadByte() << 8 | stream.ReadByte() << 16 | (sbyte)stream.ReadByte() << 24) / 0x7FFFFFFF;
			}
		}

		// <chunk> = <ckID> + <ckSize> + <ckData>
		class Chunk {
			public string Id;

			public int Size;
			public int Pad;

			public byte[] Data;


			public Chunk (Stream stream) {
				Id = StreamHelper.ReadString(stream, 4);

				Size = StreamHelper.ReadUInt32(stream);
				Pad = Size & 0x1;

				Data = new byte[Size];
				stream.Read(Data, 0, Size);
				stream.Position += Pad;
			}

			public Stream GetStream () {
				return new MemoryStream(Data);
			}
		}

		public static WaveFile Build (byte[] bytes) {
			using (var stream = new MemoryStream(bytes)) {
				return Build(stream);
			}
		}

		public static WaveFile Build (Stream stream) {
			var file = new WaveFile();

			var masterChunk = new Chunk(stream);
			if (masterChunk.Id != "RIFF")
				throw new Exception(string.Format("Unexpected masterChunk.Id : {0}, 'RIFF' expected.", masterChunk.Id));

			// masterChunk
			using (var masterStream = masterChunk.GetStream()) {
				file.WaveId = StreamHelper.ReadString(masterStream, 4);
				if (file.WaveId != "WAVE")
					throw new Exception(string.Format("Unexpected this.WaveId : {0}, 'WAVE' expected.", file.WaveId));

				// formatChunk
				var formatChunk = new Chunk(masterStream);
				if (formatChunk.Id != "fmt ")
					throw new Exception(string.Format("Unexpected formatChunk.Id : {0}, 'fmt ' expected.", formatChunk.Id));
				using (var formatStream = formatChunk.GetStream()) {
					file.FormatTag = StreamHelper.ReadUInt16(formatStream);
					file.Channels = StreamHelper.ReadUInt16(formatStream);
					file.SupportStereo = file.Channels > 1;
					file.SamplePerSec = StreamHelper.ReadUInt32(formatStream);
					file.AvgBytesPerSec = StreamHelper.ReadUInt32(formatStream);
					file.BlockAlign = StreamHelper.ReadUInt16(formatStream);
					file.BitsPerSample = StreamHelper.ReadUInt16(formatStream);
				}

				if (file.Format != WaveFileFormat.MicrosoftPcm)
					throw new Exception(string.Format("Unexpected this.Format : {0}, 'MicrosoftPcm' expected.", file.Format));

				// dataChunk
				while (masterStream.Position < masterStream.Length) {
					var chunk = new Chunk(masterStream);
					if (chunk.Id == "data") {
						file.WaveData = chunk.Data;
						using (var dataStream = chunk.GetStream()) {
							BuildSamples(file, dataStream);
						}
						break;
					}
				}

				if (file.WaveData == null)
					throw new Exception("Missing DataChunk in this.Chunks.");
			}

			return file;
		}

		static void BuildSamples (WaveFile file, Stream stream) {
			if ((file.BitsPerSample & 0x7) > 0)
				throw new Exception(string.Format("Unexpected this.BitsPerSample : {0}, multiple of 8 expected.", file.BitsPerSample));
			var bytesPerSample = file.BitsPerSample / 8;
			var sampleCount = file.WaveData.Length / bytesPerSample / file.Channels;

			file.Samples = new double[file.Channels][];
			for (int i = 0; i < file.Channels; i++)
				file.Samples[i] = new double[sampleCount];

			switch (bytesPerSample) {
			case 1:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (float)(stream.ReadByte() - 0x80) / 0x7F;
				break;
			case 2:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (float)(stream.ReadByte() | (sbyte)stream.ReadByte() << 8) / 0x7FFF;
				break;
			case 3:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (float)(stream.ReadByte() | stream.ReadByte() << 8 | (sbyte)stream.ReadByte() << 16) / 0x7FFFFF;
				break;
			case 4:
				for (int i = 0; i < sampleCount; i++)
					for (int j = 0; j < file.Channels; j++)
						file.Samples[j][i] = (float)(stream.ReadByte() | stream.ReadByte() << 8 | stream.ReadByte() << 16 | (sbyte)stream.ReadByte() << 24) / 0x7FFFFFFF;
				break;
			}

			if (stream.Position < stream.Length)
				throw new Exception(string.Format("Unexpected stream.Position : {0}, stream.Length expected.", stream.Position));
		}
	}
}

