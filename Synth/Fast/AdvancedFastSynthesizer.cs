using System;

namespace Midif.Synth.Fast {
	[Serializable]
	public class AdvancedFastSynthesizer : MidiSynthesizer {
		[Serializable]
		public class Channel {
			public int Pan = 64;
			public double PanLeft = 1;
			public double PanRight = 1;

			public int Volumn = 100;
			public int Expression = 127;

			public void Reset () {
				Pan = 64;
				PanLeft = 1;
				PanRight = 1;

				Volumn = 100;
				Expression = 127;
			}
		}

		[Serializable]
		public class Voice {
			public int Index;
			public byte Channel;

			public byte Note;
			public byte Velocity;

			public double Gain;
			public double GainLeft;
			public double GainRight;

			public double Step;
			public double Phase;

			public void SetGain (double channelGain, double channelPanLeft, double channelPanRight) {
				Gain = SynthConstants.Recip127[Velocity] * channelGain;
				GainLeft = Gain * channelPanLeft;
				GainRight = Gain * channelPanRight;
			}
		}

		const double PerVoiceLevel = 0.125 / 3;

		static Random rand = new Random();

		int waveTableMod;
		int waveTableLength;
		double[] waveTable;

		double[] phaseStepTable;
		double[,] gainTable;

		public Channel[] channels;

		public int voiceIndex;
		public Voice[] voices;


		public AdvancedFastSynthesizer () {
			InitTables();
			SetWaveTable(new double[]{ 1, -1 });
		}

		public AdvancedFastSynthesizer (double[] table) {
			InitTables();
			SetWaveTable(table);
		}

		public AdvancedFastSynthesizer (int samplePower) {
			var sampleCount = 1 << samplePower;
			var samples = new double[sampleCount];
			for (int i = 0; i < sampleCount; i++)
				samples[i] = rand.NextDouble() * 2 - 1;

			InitTables();
			SetWaveTable(samples);
		}

		void InitTables () {
			gainTable = new double[128, 128];
			for (int i = 0; i < 128; i++)
				for (int j = 0; j < 128; j++) {
					// Two implementation of Volumn and Expression
					// http://dev.midi.org/techspecs/gmguide2.pdf
					var l = 40 * Math.Log10(i * j / Math.Pow(127, 2));
					var gain = Math.Pow(10, l / 20);
					// http://www.blitter.com/~russtopia/MIDI/~jglatt/tech/midispec.htm
//						var l = 40 * Math.Log10((double)i / 127);
//						var gain = Math.Pow(10, l / 20) * MidiTables.Recip127[j];

					gainTable[i, j] = gain * PerVoiceLevel;
				}

			channels = new Channel[16];
			for (int i = 0; i < 16; i++)
				channels[i] = new Channel();
		}

		public void SetWaveTable (double[] newTable) {
			// http://www.exploringbinary.com/ten-ways-to-check-if-an-integer-is-a-power-of-two-in-c/
			var length = newTable.Length;
			if (!((length != 0) && ((length & (~length + 1)) == length)))
				throw new Exception("Unsupported table.Length : " + newTable.Length);

			waveTable = newTable;
			waveTableMod = length - 1;
			waveTableLength = length;
		}

		public override void Init (double sampleRate, int polyphony) {
			base.Init(sampleRate, polyphony);

			phaseStepTable = new double[128];
			for (int i = 0; i < 128; i++)
				phaseStepTable[i] = waveTableLength * 440 * Math.Pow(2, ((double)i - 69) / 12) / sampleRate;

			voiceIndex = 0;
			voices = new Voice[polyphony];
			for (int i = 0; i < polyphony; i++)
				voices[i] = new Voice();
		}

		public override void Reset () {
			foreach (var voice in voices)
				voice.Note = 0;

			foreach (var channel in channels)
				channel.Reset();
		}

		public override void NoteOn (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.Note == 0) {
					voice.Index = voiceIndex++;
					voice.Channel = channel;
					voice.Note = note;
					voice.Velocity = velocity;
					voice.SetGain(
						gainTable[channels[channel].Volumn, channels[channel].Expression],
						channels[channel].PanLeft, channels[channel].PanRight
					);
					voice.Step = phaseStepTable[note];
					voice.Phase = 0;
					return;
				}

//			Voice oldest = voices[0];
//			foreach (var voice in voices)
//				if (voice.Index < oldest.Index) oldest = voice;
//			oldest.Index = voiceIndex++;
//			oldest.Channel = channel;
//			oldest.Note = note;
//			oldest.Velocity = velocity;
			Voice oldest = null;
			foreach (var voice in voices)
				if (voice.Channel == channel && (oldest == null || voice.Index < oldest.Index))
					oldest = voice;
			if (oldest != null) {
				oldest.Index = voiceIndex++;
				oldest.Note = note;
				oldest.Velocity = velocity;
//				oldest.Gain = gainTable[velocity, channels[channel].Volumn, channels[channel].Expression];
				oldest.Step = phaseStepTable[note];
			}
		}

		public override void NoteOff (byte channel, byte note, byte velocity) {
			foreach (var voice in voices)
				if (voice.Note == note && voice.Channel == channel) {
					voice.Note = 0;
					return;
				}
		}

		//		public override void Aftertouch (byte channel, byte note, byte velocity) {
		//			foreach (var voice in voices)
		//				if (voice.Note == note && voice.Channel == channel) {
		//					voice.Velocity = velocity;
		//					voice.Gain = gainTable[velocity, channels[channel].Volumn, channels[channel].Expression];
		//					return;
		//				}
		//		}
		//
		//		public override void ChannelAftertouch (byte channel, byte pressure) {
		//			foreach (var voice in voices)
		//				if (voice.Note != 0 && voice.Channel == channel) {
		//					voice.Velocity = pressure;
		//					voice.Gain = gainTable[pressure, channels[channel].Volumn, channels[channel].Expression];
		//				}
		//		}

		public override void Controller (byte channel, MidiControllerType controller, byte value) {
			switch (controller) {
			case MidiControllerType.MainVolume:
				channels[channel].Volumn = value;
				break;
			case MidiControllerType.Balance:
			case MidiControllerType.Pan:
				channels[channel].Pan = value;
				if (value > 64) {
					channels[channel].PanLeft = 1;
					channels[channel].PanRight = SynthConstants.Recip127[value - 64] * 2;
				} else if (value < 64) {
					channels[channel].PanLeft = SynthConstants.Recip127[64 - value] * 2;
					channels[channel].PanRight = 1;
				}
				break;
			case MidiControllerType.ExpressionController:
				channels[channel].Expression = value;
				// Expression should take effect immediately.
				foreach (var voice in voices)
					if (voice.Channel == channel && voice.Note != 0)
						voice.SetGain(
							gainTable[channels[channel].Volumn, value],
							channels[channel].PanLeft, channels[channel].PanRight
						);
				break;
			case MidiControllerType.AllSoundOff:
			case MidiControllerType.AllNotesOff:
				foreach (var voice in voices)
					if (voice.Channel == channel)
						voice.Note = 0;
				break;
			}
		}

		public override void Render (ref double sample) {
			foreach (var voice in voices)
				if (voice.Note != 0)
					sample += waveTable[(int)(voice.Phase += voice.Step) & waveTableMod] * voice.Gain;
		}

		public override void Render (ref double sampleL, ref double sampleR) {
			foreach (var voice in voices)
				if (voice.Note != 0) {
					sampleL += waveTable[(int)voice.Phase & waveTableMod] * voice.GainLeft;
					sampleR += waveTable[(int)voice.Phase & waveTableMod] * voice.GainRight;
					voice.Phase += phaseStepTable[voice.Note];
				}
		}
	}
}

