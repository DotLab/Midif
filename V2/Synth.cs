using Unsaf;

namespace Midif.V2 {
	public unsafe struct Synth {
		public unsafe struct Voice {
			public bool isOn;
			public byte note;
			public byte velocity;
			public byte channel;

			public float freq;
			public float gainLeft;
			public float gainRight;

			public float time;

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

		public static void Init(Synth *self, SynthTable *table, float sampleRate, int voiceCount = 12) {
			self->table = table;

			self->sampleRate = sampleRate;
			self->sampleRateRecip = 1f / sampleRate;

			self->masterGain = 1;

			self->voiceCount = voiceCount;
			self->voices = (Voice *)Mem.Malloc(voiceCount * sizeof(Voice));

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
				voice->isOn = false;
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

			voice->isOn = true;
			voice->note = note;
			voice->velocity = velocity;
			voice->channel = channel;
			voice->time = 0;

			UpdatePitch(self, voice);
			UpdateGain(self, voice);
		}

		public static void NoteOff(Synth *self, int track, byte channel, byte note, byte velocity) {
//			UnityEngine.Debug.LogFormat("note on {0} {1} {2} {3}", track, channel, note, velocity);
			if (channel == 9) return;

			Voice *voice = self->firstActiveVoice;
			Voice **ptr = &self->firstActiveVoice;
			while (voice != null) {
				if (voice->channel == channel && voice->note == note) {
					// note off
					voice->isOn = false;
					*ptr = voice->next;
					Voice *next = voice->next;
					voice->next = self->firstFreeVoice;
					self->firstFreeVoice = voice;
					// do not update ptr
					voice = next;
				} else {
					ptr = &voice->next;
					voice = voice->next;
				}
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

			for (int i = 0; i < length; i += 2) {
				float left = 0;
				float right = 0;

				Voice *voice = self->firstActiveVoice;
				while (voice != null) {
					float value = (float)System.Math.Sin(voice->time * voice->freq * pi2);
					left += value * voice->gainLeft;
					right += value * voice->gainRight;
					voice->time += self->sampleRateRecip;
					voice = voice->next;
				}
					
				data[i] = left;
				data[i + 1] = right;
			}
		}
	}
}

