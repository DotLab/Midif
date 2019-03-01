using Math = System.Math;

namespace Midif.V3 {
	public sealed class MidiSynth : IMidiSynth {
		public sealed class Table {
			public const float VelcRecip = 1f / 127f;

			public const float Pi = 3.14159265f;
			public const float Pi2 = Pi * 2;

			public readonly float[] note2Freq = new float[128];
			public readonly float[] bend2Pitch = new float[128];

			public readonly float[] volm2Gain = new float[128];
			public readonly float[] pan2Left = new float[128];
			public readonly float[] pan2Right = new float[128];

			public Table() {
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

		public sealed class EnvelopeConfig {
			public readonly Table table;
			// 0 - 127
			public readonly byte[] levels = new byte[4];
			// seconds
			public readonly float[] durations = new float[4];

			public readonly float[] gains = new float[4];
			public readonly float[] gainsPerSecond = new float[4];

			public EnvelopeConfig(Table table, byte l1, float d1, byte l2, float d2, byte l3, float d3, byte l4, float d4) {
				this.table = table;
				levels[0] = l1;
				levels[1] = l2;
				levels[2] = l3;
				levels[3] = l4;
				durations[0] = d1;
				durations[1] = d2;
				durations[2] = d3;
				durations[3] = d4;
				Reset();
			}

			public void Reset() {
				float prev = table.volm2Gain[levels[3]];
				for (int i = 0; i < 4; i += 1) {
					gains[i] = table.volm2Gain[levels[i]];
					gainsPerSecond[i] = (gains[i] - prev) / durations[i];
					prev = gains[i];
				}
			}
		}

		public struct Envelope {
			public EnvelopeConfig config;

			public bool isOff;
			public bool isFinished;

			public byte stage;
			public float time;
			public float duration;
			public float gainsPerSecond;
			public float gain;

			public Envelope(EnvelopeConfig config) {
				this.config = config;

				isOff = false;
				isFinished = false;

				stage = 0;
				time = 0;
				duration = 0;
				gainsPerSecond = 0;
				gain = 0;
			}

			public void Reset() {
				isOff = false;
				isFinished = false;

				stage = 0;
				time = 0;
				duration = config.durations[0];
				gainsPerSecond = config.gainsPerSecond[0];
				gain = config.gains[3];
			}

			public void Off() {
				isOff = true;
				if (stage < 3) {
					stage = 3;
					time = 0;
					duration = (config.gains[3] - gain) / config.gainsPerSecond[3];
					gainsPerSecond = config.gainsPerSecond[3];
//					duration = config.durations[3];
//					gainsPerSecond = (config.gains[3] - gain) / config.durations[3];
				}
			}

			public void AdvanceTime(float delta) {
				if (stage >= 4) return;
				if (stage == 3 && !isOff) return; 

				gain += delta * gainsPerSecond;
				time += delta;

				if (time > duration) {
					stage += 1;
					if (stage < 4) {
						time -= config.durations[stage - 1];
						duration = config.durations[stage];
						gainsPerSecond = config.gainsPerSecond[stage];
						gain = config.gains[stage - 1] + time * gainsPerSecond;
//						Fdb.Log("finish {0}", stage - 1);
					} else {
						isFinished = true;
						gain = config.gains[4 - 1];
					}
				}
			}
		}

		public struct Voice {
			public byte note;
			public byte velocity;
			public byte channel;

			public float freq;
			public float gainLeft;
			public float gainRight;

			public float time;

			public Envelope envelope;

			public int next;
		}

		public Table table;

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

		public EnvelopeConfig envelopeConfig;

		public MidiSynth(Table table, float sampleRate, int voiceCount) {
			#if MIDIF_DEBUG_VISUALIZER
			WaveVisualizer.Request(0, 1024);
			#endif

			this.table = table;

			this.sampleRate = sampleRate;
			sampleRateRecip = 1f / sampleRate;

			masterGain = 1;

			envelopeConfig = new EnvelopeConfig(table, 120, .01f, 60, .1f, 100, .5f, 0, .01f);

			this.voiceCount = voiceCount;
			voices = new Voice[voiceCount];
			for (int i = 0; i < voiceCount; i += 1) {
				voices[i].envelope = new Envelope(envelopeConfig);
			}

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
			masterGain = (float)Table.Deci2Gain(volume);
		}

		public void NoteOn(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;
			if (velocity == 0) {
				NoteOff(track, channel, note, velocity);
				return;
			}

			if (firstFreeVoice == -1) {
				UnityEngine.Debug.LogFormat("Not enough notes active {0} free {1}", CountVoices(firstActiveVoice), CountVoices(firstFreeVoice));
				return;
			}
			int i = firstFreeVoice;
			firstFreeVoice = voices[i].next;
			voices[i].next = firstActiveVoice;
			firstActiveVoice = i;

			voices[i].note = note;
			voices[i].velocity = velocity;
			voices[i].channel = channel;
			voices[i].time = 0;
			voices[i].envelope.Reset();

			UpdateVoicePitch(i);
			UpdateVoiceGain(i);
		}

		public void NoteOff(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;

			for (int i = firstActiveVoice; i != -1; i = voices[i].next) {
				if (voices[i].channel == channel && voices[i].note == note) {
					voices[i].envelope.Off();
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

		public void ProgramChange(int track, byte channel, byte program) {
		}

		public void PitchBend(int track, byte channel, byte lsb, byte msb) {
			channelpitchBends[channel] = msb;
			UpdateChannelPitch(channel);
		}

		public int CountVoices(int i) {
			int j = 0;
			for (; i != -1; i = voices[i].next) {
				j += 1;
			}
			return j;
		}

		public void Panic() {
			for (int prev = -1, i = firstActiveVoice; i != -1;) {
				if (voices[i].envelope.isFinished) {
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

		public void Process(float[] data) {
			for (int i = 0, length = data.Length; i < length; i += 2) {
				float left = 0;
				float right = 0;

				for (int j = firstActiveVoice; j != -1; j = voices[j].next) {
					 float envelopeGain = voices[j].envelope.gain;
					voices[j].envelope.AdvanceTime(sampleRateRecip);
					float value = envelopeGain * (float)Math.Sin(voices[j].time * voices[j].freq * Table.Pi2);
					left += value * voices[j].gainLeft;
					right += value * voices[j].gainRight;
					voices[j].time += sampleRateRecip;
				}
				
				data[i] = left;
				data[i + 1] = right;

				#if MIDIF_DEBUG_VISUALIZER
				WaveVisualizer.Push(0, left);
				#endif
			}

			Panic();
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
			float gain = voices[i].velocity * Table.VelcRecip;
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
					float gain = voices[i].velocity * Table.VelcRecip;
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

