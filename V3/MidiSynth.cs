using Math = System.Math;

namespace Midif.V3 {
	public sealed class MidiSynthTable {
		public const float VelcRecip = 1f / 127f;

		public const float Pi = 3.14159265f;
		public const float Pi2 = Pi * 2;

		public readonly float[] note2Freq = new float[128];
		public readonly float[] bend2Pitch = new float[128];

		public readonly float[] volm2Gain = new float[128];
		public readonly float[] pan2Left = new float[128];
		public readonly float[] pan2Right = new float[128];

		public MidiSynthTable() {
			for (int i = 0; i < 128; i++) {
				note2Freq[i] = (float)(440 * Math.Pow(2, (i - 69) / 12.0));
				bend2Pitch[i] = (float)Math.Pow(2, 2 * ((i - 64) / 127) / 12.0);

				volm2Gain[i] = (float)Deci2Gain(40.0 * Math.Log10(i / 127.0));
				pan2Left[i] = (float)Deci2Gain(20.0 * Math.Log10(Math.Cos(Math.PI / 2 * (i / 127.0))));
				pan2Right[i] = (float)Deci2Gain(20.0 * Math.Log10(Math.Sin(Math.PI / 2 * (i / 127.0))));
			}
		}

		public static double Deci2Gain (double db) {
			return Math.Pow(10.0, (db / 10.0));
		}
	}

	public sealed class MidiSynth {
		public struct Voice {
			public byte note;
			public byte velocity;
			public byte channel;

			public float freq;
			public float gainLeft;
			public float gainRight;

			public float time;

			public bool isOn;

			public int next;
		}

		public MidiSynthTable table;

		public readonly float sampleRate;
		public readonly float sampleRateRecip;

		public readonly byte[] channelPans = new byte[16];
		public readonly byte[] channelVolumes = new byte[16];
		public readonly byte[] channelExpressions = new byte[16];
		public readonly ushort[] channelpitchBends = new ushort[16];

		public float masterGain;

		public readonly int voiceCount;
		public readonly Voice[] voices;
		public int firstFreeVoice;
		public int firstActiveVoice;

		public MidiSynth(MidiSynthTable table, float sampleRate, int voiceCount) {
			WaveVisualizer.Data = new float[(int)sampleRate];

			this.table = table;

			this.sampleRate = sampleRate;
			this.sampleRateRecip = 1f / sampleRate;

			masterGain = 1;

			this.voiceCount = voiceCount;
			voices = new Voice[voiceCount];

			Reset();
		}

		public void Reset() {
			for (int i = 0; i < 16; i += 1) {
				channelPans[i] = 64;
				channelVolumes[i] = 100;
				channelExpressions[i] = 127;
				channelpitchBends[i] = 64;
			}

			for (int i = 0; i < voiceCount; i += 1) {
				voices[i].next = i + 1;
			}
			voices[voiceCount - 1].next = -1;
			firstActiveVoice = -1;
			firstFreeVoice = 0;
		}

		public void SetVolume(float volume) {
			masterGain = (float)MidiSynthTable.Deci2Gain(volume);
		}

		public void NoteOn(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;
			if (velocity == 0) {
				NoteOff(track, channel, note, velocity);
				return;
			}

			if (firstFreeVoice == -1) return;
			int i = firstFreeVoice;
			firstFreeVoice = voices[i].next;
			voices[i].next = firstActiveVoice;
			firstActiveVoice = i;

			voices[i].note = note;
			voices[i].velocity = velocity;
			voices[i].channel = channel;
			voices[i].time = 0;

			UpdateVoicePitch(i);
			UpdateVoiceGain(i);
		}

		public void NoteOff(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;

			for (int prev = -1, i = firstActiveVoice; i != -1;) {
				if (voices[i].channel == channel && voices[i].note == note) {
					if (prev != -1) voices[prev].next = voices[i].next; else firstActiveVoice = voices[i].next;
					int next = voices[i].next;
					voices[i].next = firstFreeVoice;
					firstFreeVoice = i;
					i = next;
				} else {
					prev = i;
					i = voices[i].next;
				}
			}
		}

		public void Controller(int track, byte channel, byte controller, byte value) {
			switch (controller) {
			case 7:  // channel volume
				channelVolumes[channel] = value;
				UpdateChannelGain(channel);
				break;
			case 10:  // pan
				channelPans[channel] = value;
				UpdateChannelGain(channel);
				break;
			case 11:  // expression
				channelExpressions[channel] = value;
				UpdateChannelGain(channel);
				break;
			}
		}

		public void PitchBend(int track, byte channel, byte lsb, byte msb) {
			channelpitchBends[channel] = msb;
			UpdateChannelPitch(channel);
		}

		public void Process(float[] data) {
			for (int i = 0, length = data.Length; i < length; i += 2) {
				float left = 0;
				float right = 0;

				for (int j = firstActiveVoice; j != -1; j = voices[j].next) {
					// float envelopeGain = voices[i].envelope.gain;
					float value = (float)System.Math.Sin(voices[j].time * voices[j].freq * MidiSynthTable.Pi2);
					left += value * voices[j].gainLeft;
					right += value * voices[j].gainRight;
					// Envelope.AdvanceTime(&voices[i].envelope, sampleRateRecip);
					voices[j].time += sampleRateRecip;
				}
				
				// WaveVisualizer.Push(left);
				data[i] = left;
				data[i + 1] = right;
				
				WaveVisualizer.Push(left);
			}
		}

		void UpdateVoiceGain(int i) {
			int channel = voices[i].channel;

			byte pan = channelPans[channel];
			byte volume = channelVolumes[channel];
			byte expression = channelExpressions[channel];
			float channelGain = masterGain * table.volm2Gain[volume] * table.volm2Gain[expression];
			float channelGainLeft=  channelGain * table.pan2Left[pan];
			float channelGainRight=  channelGain * table.pan2Right[pan];

			// float gain = table.volm2Gain[voices[i].velocity];
			float gain = voices[i].velocity * MidiSynthTable.VelcRecip;
			// float gain = 1;
			voices[i].gainLeft = channelGainLeft * gain;
			voices[i].gainRight = channelGainRight * gain;
		}

		void UpdateChannelGain(int channel) {
			byte pan = channelPans[channel];
			byte volume = channelVolumes[channel];
			byte expression = channelExpressions[channel];
			float channelGain = masterGain * table.volm2Gain[volume] * table.volm2Gain[expression];
			float channelGainLeft=  channelGain * table.pan2Left[pan];
			float channelGainRight=  channelGain * table.pan2Right[pan];

			for (int i = firstActiveVoice; i != -1; i = voices[i].next) {
				if (voices[i].channel == channel) {
					// float gain = table.volm2Gain[voices[i].velocity];
					float gain = voices[i].velocity * MidiSynthTable.VelcRecip;
					// float gain = 1;
					voices[i].gainLeft = channelGainLeft * gain;
					voices[i].gainRight = channelGainRight * gain;
				}
			}
		}

		void UpdateVoicePitch(int i) {
			float channelPitch = table.bend2Pitch[channelpitchBends[voices[i].channel]];

			voices[i].freq = table.note2Freq[voices[i].note] * channelPitch;
		}

		void UpdateChannelPitch(int channel) {
			float channelPitch = table.bend2Pitch[channelpitchBends[channel]];

			for (int i = firstActiveVoice; i != -1; i = voices[i].next) {
				if (voices[i].channel == channel) {
					voices[i].freq = table.note2Freq[voices[i].note] * channelPitch;
				}
			}
		}
	}
}

