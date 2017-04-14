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
		// A number indicating the WAVE format category of the file.
		public ushort FormatTag;
		// The number of channels represented in the waveform data, such as 1 for mono or 2 for stereo.
		public ushort Channels;
		// The sampling rate (in samples per second) at which each channel should be played.
		public uint SamplePerSec;
		// The average number of bytes per second at which the waveform data should be transferred.
		public uint AvgBytesPerSec;
		// The block alignment (in bytes) of the waveform data.
		public ushort BlockAlign;
		// The number of bits of data used to represent each sample of each channel.
		public ushort BitsPerSample;

		public byte[] Data;

		public double[] Samples;

		public WaveFileFormat Format {
			get { return (WaveFileFormat)FormatTag; }
		}

		#region Constructor

		public WaveFile (ushort channels, uint samplePerSec, byte bytesPerSample) {
			Channels = channels;
			SamplePerSec = samplePerSec;
			BitsPerSample = (ushort)(bytesPerSample << 3);

			AvgBytesPerSec = bytesPerSample * samplePerSec * channels;
			BlockAlign = (ushort)(bytesPerSample * channels);

			FormatTag = (ushort)WaveFileFormat.MicrosoftPcm;
		}

		public WaveFile (Stream stream) {
			Read(stream);
		}

		public void Read (Stream stream) {
			var masterChunk = new RiffChunk(stream);
			if (masterChunk.Id != "RIFF")
				throw new FileFormatException("WaveFile.masterChunk.Id", masterChunk.Id, "RIFF");

			// masterChunk
			using (var masterStream = masterChunk.GetStream()) {
				var WaveId = StreamHelperLe.ReadString(masterStream, 4);
				if (WaveId != "WAVE")
					throw new FileFormatException("WaveFile.WaveId", WaveId, "WAVE");

				// formatChunk
				var formatChunk = new RiffChunk(masterStream);
				if (formatChunk.Id != "fmt ")
					throw new FileFormatException("WaveFile.formatChunk.Id", formatChunk.Id, "fmt ");
				using (var formatStream = formatChunk.GetStream()) {
					FormatTag = StreamHelperLe.ReadUInt16(formatStream);
					Channels = StreamHelperLe.ReadUInt16(formatStream);
					SamplePerSec = StreamHelperLe.ReadUInt32(formatStream);
					AvgBytesPerSec = StreamHelperLe.ReadUInt32(formatStream);
					BlockAlign = StreamHelperLe.ReadUInt16(formatStream);
					BitsPerSample = StreamHelperLe.ReadUInt16(formatStream);
				}

				if (Format != WaveFileFormat.MicrosoftPcm)
					throw new FileFormatException("WaveFile.Format", Format, WaveFileFormat.MicrosoftPcm);

				// dataChunk
				while (masterStream.Position < masterStream.Length) {
					var chunk = new RiffChunk(masterStream);
					if (chunk.Id == "data") {
						Data = chunk.Data;
						using (var dataStream = chunk.GetStream()) {
							BuildSamples(dataStream);
						}
						break;
					}
				}

				if (Data == null)
					throw new FileFormatException("WaveFile.WaveData", "null", "byte[]");
			}
		}

		void BuildSamples (Stream stream) {
			if ((BitsPerSample & 0x7) > 0)
				throw new FileFormatException("WaveFile.BitsPerSample", BitsPerSample, "[multiple of 8]");
			
			var bytesPerSample = BitsPerSample >> 3;
			var sampleLength = Data.Length / bytesPerSample;

			Samples = new double[sampleLength];

			switch (bytesPerSample) {
			case 1:
				for (int i = 0; i < sampleLength; i++)
					Samples[i] = (double)stream.ReadByte() / 0x7F - 1.0;
				break;
			case 2:
				for (int i = 0; i < sampleLength; i++)
					Samples[i] = (double)StreamHelperLe.ReadInt16(stream) / 0x7FFF;
				break;
			case 3:
				for (int i = 0; i < sampleLength; i++)
					Samples[i] = (double)StreamHelperLe.ReadInt24(stream) / 0x7FFFFF;
				break;
			case 4:
				for (int i = 0; i < sampleLength; i++)
					Samples[i] = (double)StreamHelperLe.ReadInt32(stream) / 0x7FFFFFFF;
				break;
			}

			if (stream.Position < stream.Length)
				throw new FileFormatException("WaveFile.stream.Position", stream.Position, stream.Length);
		}

		public void BuildData () {
			var bytesPerSample = BitsPerSample >> 3;
			var sampleLength = Samples.Length;

			using (var dataStream = new MemoryStream()) {
				switch (bytesPerSample) {
				case 1:
					for (int i = 0; i < sampleLength; i++)
						dataStream.WriteByte((byte)System.Math.Round((Samples[i] + 1.0) * 0x7F));
					break;
				case 2:
					for (int i = 0; i < sampleLength; i++)
						StreamHelperLe.WriteInt16(dataStream, (short)System.Math.Round(Samples[i] * 0x7FFF));
					break;
				case 3:
					for (int i = 0; i < sampleLength; i++)
						StreamHelperLe.WriteInt24(dataStream, (int)System.Math.Round(Samples[i] * 0x7FFFFF));
					break;
				case 4:
					for (int i = 0; i < sampleLength; i++)
						StreamHelperLe.WriteInt32(dataStream, (int)System.Math.Round(Samples[i] * 0x7FFFFFFF));
					break;
				}

				Data = dataStream.ToArray();
			}
		}

		public void Write (Stream stream) {
			UnityEngine.Debug.Log("Write " + this);
			// masterChunk
			using (var masterStream = new MemoryStream()) {
				StreamHelperLe.WriteString(masterStream, "WAVE");

				// formatChunk
				using (var formatStream = new MemoryStream()) {
					StreamHelperLe.WriteUInt16(formatStream, FormatTag);
					StreamHelperLe.WriteUInt16(formatStream, Channels);
					StreamHelperLe.WriteUInt32(formatStream, SamplePerSec);
					StreamHelperLe.WriteUInt32(formatStream, AvgBytesPerSec);
					StreamHelperLe.WriteUInt16(formatStream, BlockAlign);
					StreamHelperLe.WriteUInt16(formatStream, BitsPerSample);

					var formatChunk = new RiffChunk("fmt ", formatStream.ToArray());
//					UnityEngine.Debug.Log(formatChunk.Size);
					formatChunk.Write(masterStream);
				}

				// dataChunk
				var dataChunk = new RiffChunk("data", Data);
//				UnityEngine.Debug.Log(dataChunk.Size);
				dataChunk.Write(masterStream);

				var masterChunk = new RiffChunk("RIFF", masterStream.ToArray());
//				UnityEngine.Debug.Log(masterChunk.Size);
				masterChunk.Write(stream);
			}
		}

		#endregion

		public override string ToString () {
			return string.Format("[WaveFile: Channels={0}, SamplePerSec={1}, BitsPerSample={2}, Format={3}]", Channels, SamplePerSec, BitsPerSample, Format);
		}
	}
}