using Unsaf;

namespace Midif.V2 {
	public unsafe struct Synth {
		public unsafe struct EnvelopeConfig {
			public const byte LevelCount = 4;

			// 0 - 127
			public fixed byte levels[LevelCount];
			// seconds
			public fixed float durations[LevelCount];

			public SynthTable *table;
			public fixed float gains[LevelCount];
			public fixed float gainsPerSecond[LevelCount];

			public static void Init(EnvelopeConfig *self, SynthTable *table, byte l1, float d1, byte l2, float d2, byte l3, float d3, byte l4, float d4) {
				self->table = table;
				self->levels[0] = l1;
				self->levels[1] = l2;
				self->levels[2] = l3;
				self->levels[3] = l4;
				self->durations[0] = d1;
				self->durations[1] = d2;
				self->durations[2] = d3;
				self->durations[3] = d4;
				Reset(self);
			}

			public static void Reset(EnvelopeConfig *self) {
				float prev = self->table->volm2Gain[self->levels[LevelCount - 1]];
				for (int i = 0; i < LevelCount; i += 1) {
					self->gains[i] = self->table->volm2Gain[self->levels[i]];
					self->gainsPerSecond[i] = (self->gains[i] - prev) / self->durations[i];
					prev = self->gains[i];
				}
			}
		}

		public unsafe struct Envelope {
			public EnvelopeConfig *config;

			public bool isOff;
			public bool isFinished;

			public byte stage;
			public float time;
			public float duration;
			public float gain;

			public static void Init(Envelope *self, EnvelopeConfig *config) {
				self->config = config;
			}

			public static void Reset(Envelope *self) {
				self->isOff = false;
				self->isFinished = false;

				self->stage = 0;
				self->time = 0;
				self->duration = self->config->durations[0];
				self->gain = self->config->gains[EnvelopeConfig.LevelCount - 1];
			}

			public static void Off(Envelope *self) {
				self->isOff = true;
				const byte lastIndex = EnvelopeConfig.LevelCount - 1;
				if (self->stage < lastIndex) {
					self->stage = lastIndex;
					EnvelopeConfig *config = self->config;
					self->time = 0;
					self->duration = (config->gains[lastIndex] - self->gain) / config->gainsPerSecond[lastIndex];
				}
			}

			public static void AdvanceTime(Envelope *self, float time) {
				if (self->stage >= EnvelopeConfig.LevelCount) return;
				if (self->stage == EnvelopeConfig.LevelCount - 1 && !self->isOff) return; 

				EnvelopeConfig *config = self->config;

				self->gain += time * config->gainsPerSecond[self->stage];
				self->time += time;

				if (self->time > self->duration) {
					int stage = self->stage += 1;
					if (self->stage < EnvelopeConfig.LevelCount) {
						self->time -= config->durations[stage - 1];
						self->duration = config->durations[stage];
						self->gain = config->gains[stage - 1] + self->time * config->gainsPerSecond[self->stage];
//						Fdb.Log("finish {0}", stage - 1);
					} else {
						self->isFinished = true;
						self->gain = config->gains[EnvelopeConfig.LevelCount - 1];
					}
				}
			}
		}

		public unsafe struct Voice {
			public byte note;
			public byte velocity;
			public byte channel;

			public float freq;
			public float gainLeft;
			public float gainRight;

			public float time;

			public Envelope envelope;

			public Voice *next;
		}

		public SynthTable *table;

		public float sampleRate;
		public float sampleRateRecip;

		public fixed byte channelPans[16];
		public fixed byte channelVolumes[16];
		public fixed byte channelExpressions[16];
		public fixed ushort channelpitchBends[16];

		public float masterGain;

		public int voiceCount;
		public Voice *voices;
		public Voice *firstFreeVoice;
		public Voice *firstActiveVoice;

		public EnvelopeConfig envelopeConfig;

		public static void Init(Synth *self, SynthTable *table, float sampleRate, int voiceCount = 12) {
			self->table = table;

			self->sampleRate = sampleRate;
			self->sampleRateRecip = 1f / sampleRate;

			self->masterGain = 1;

			EnvelopeConfig.Init(&self->envelopeConfig, self->table, 120, .01f, 55, .1f, 100, .4f, 0, .05f);

			self->voiceCount = voiceCount;
			self->voices = (Voice *)Mem.Malloc(voiceCount * sizeof(Voice));
			for (int i = 0, count = self->voiceCount; i < count; i += 1) {
				Voice *voice = self->voices + i;
				Envelope.Init(&voice->envelope, &self->envelopeConfig);
			}
			WaveVisualizer.Data = new float[(int)sampleRate];
			Reset(self);
		}

		public static void SetVolume(Synth *self, float volume) {
			self->masterGain = (float)SynthTable.Deci2Gain(volume);
		}

		public static void Reset(Synth *self) {
			for (int i = 0; i < 16; i += 1) {
				self->channelPans[i] = 64;
				self->channelVolumes[i] = 100;
				self->channelExpressions[i] = 127;
				self->channelpitchBends[i] = 64;
			}

			Voice *voice = null;
			for (int i = 0, count = self->voiceCount; i < count; i += 1) {
				voice = self->voices + i;
				voice->next = voice + 1;
			}
			voice->next = null;
			self->firstActiveVoice = null;
			self->firstFreeVoice = self->voices;
		}

		public static void NoteOn(Synth *self, int track, byte channel, byte note, byte velocity) {
//			UnityEngine.Debug.LogFormat("note on {0} {1} {2} {3}", track, channel, note, velocity);
			if (channel == 9) return;
			if (velocity == 0) {
				NoteOff(self, track, channel, note, velocity);
				return;
			}

			if (self->firstFreeVoice == null) return;
			Voice *voice = self->firstFreeVoice;
			self->firstFreeVoice = voice->next;
			voice->next = self->firstActiveVoice;
			self->firstActiveVoice = voice;

			voice->note = note;
			voice->velocity = velocity;
			voice->channel = channel;
			voice->time = 0;

			Envelope.Reset(&voice->envelope);

			UpdatePitch(self, voice);
			UpdateGain(self, voice);
		}

		public static void NoteOff(Synth *self, int track, byte channel, byte note, byte velocity) {
//			UnityEngine.Debug.LogFormat("note on {0} {1} {2} {3}", track, channel, note, velocity);
			if (channel == 9) return;

			Voice *voice = self->firstActiveVoice;
			while (voice != null) {
				if (voice->channel == channel && voice->note == note) {
					Envelope.Off(&voice->envelope);
				}
				voice = voice->next;
			}
		}

		public static void Controller(Synth *self, int track, byte channel, byte controller, byte value) {
			switch (controller) {
			case 7:  // channel volume
				self->channelVolumes[channel] = value;
				UpdateGain(self, channel);
				break;
			case 10:  // pan
				self->channelPans[channel] = value;
				UpdateGain(self, channel);
				break;
			case 11:  // expression
				self->channelExpressions[channel] = value;
				UpdateGain(self, channel);
				break;
//			default:
//				UnityEngine.Debug.LogFormat("controller {0} {1} {2} {3}", track, channel, controller, value);
//				break;
			}
		}

		public static void UpdateGain(Synth *self, Voice *voice) {
			int channel = voice->channel;

			SynthTable *table = self->table;
			byte pan = self->channelPans[channel];
			byte volume = self->channelVolumes[channel];
			byte expression = self->channelExpressions[channel];
			float channelGain = self->masterGain * table->volm2Gain[volume] * table->volm2Gain[expression];
			float channelGainLeft=  channelGain * table->pan2Left[pan];
			float channelGainRight=  channelGain * table->pan2Right[pan];

//			float gain = table->volm2Gain[voice->velocity];
			float gain = voice->velocity * SynthTable.VelcRecip;
//			float gain = 1;
			voice->gainLeft = channelGainLeft * gain;
			voice->gainRight = channelGainRight * gain;
		}

		public static void UpdateGain(Synth *self, int channel) {
			SynthTable *table = self->table;
			byte pan = self->channelPans[channel];
			byte volume = self->channelVolumes[channel];
			byte expression = self->channelExpressions[channel];
			float channelGain = self->masterGain * table->volm2Gain[volume] * table->volm2Gain[expression];
			float channelGainLeft=  channelGain * table->pan2Left[pan];
			float channelGainRight=  channelGain * table->pan2Right[pan];

			Voice *voice = self->firstActiveVoice;
			while (voice != null) {
				if (voice->channel == channel) {
//					float gain = table->volm2Gain[voice->velocity];
					float gain = voice->velocity * SynthTable.VelcRecip;
//					float gain = 1;
					voice->gainLeft = channelGainLeft * gain;
					voice->gainRight = channelGainRight * gain;
				}
				voice = voice->next;
			}
		}

		public static void PitchBend(Synth *self, int track, byte channel, byte lsb, byte msb) {
//			UnityEngine.Debug.LogFormat("pitch bend {0} {1} {2} {3}", track, channel, lsb, msb);
			self->channelpitchBends[channel] = msb;
			UpdatePitch(self, channel);
		}

		public static void UpdatePitch(Synth *self, Voice *voice) {
			SynthTable *table = self->table;
			float channelPitch = table->bend2Pitch[self->channelpitchBends[voice->channel]];

			voice->freq = table->note2Freq[voice->note] * channelPitch;
		}

		public static void UpdatePitch(Synth *self, int channel) {
			SynthTable *table = self->table;
			float channelPitch = table->bend2Pitch[self->channelpitchBends[channel]];

			Voice *voice = self->firstActiveVoice;
			while (voice != null) {
				if (voice->channel == channel) {
					voice->freq = table->note2Freq[voice->note] * channelPitch;
				}
				voice = voice->next;
			}
		}

		public static void Process(Synth *self, float length, float *data) {
			float pi2 = self->table->pi2;
			Voice *voice;

			for (int i = 0; i < length; i += 2) {
				float left = 0;
				float right = 0;

				voice = self->firstActiveVoice;
				while (voice != null) {
					float envelopeGain = voice->envelope.gain;
					float value = envelopeGain * (float)System.Math.Sin(voice->time * voice->freq * pi2);
					left += value * voice->gainLeft;
					right += value * voice->gainRight;
					Envelope.AdvanceTime(&voice->envelope, self->sampleRateRecip);
					voice->time += self->sampleRateRecip;
					voice = voice->next;
				}
				WaveVisualizer.Push(left);
				data[i] = left;
				data[i + 1] = right;
			}

			Voice **ptr = &self->firstActiveVoice;
			voice = self->firstActiveVoice;
			float time = length * self->sampleRateRecip;
			while (voice != null) {
				if (voice->envelope.isFinished) {
					*ptr = voice->next;
					Voice *next = voice->next;
					voice->next = self->firstFreeVoice;
					self->firstFreeVoice = voice;
					voice = next;
				} else {
					ptr = &voice->next;
					voice = voice->next;
				}
			}
		}
	}
}

