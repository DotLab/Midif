namespace Midif.V2 {
	public unsafe struct Synth {
		public SynthTable *table;

		public float sampleRate;
		public float sampleRateRecip;

		public float time;

		public fixed byte channelPans[16];
		public fixed byte channelVolumes[16];
		public fixed byte channelExpressions[16];
		public fixed ushort channelpitchBends[16];

		public bool isOn;
		public byte note;
		public byte velocity;
		public byte channel;
		public float freq;
		public float gainLeft;
		public float gainRight;

		public static void Init(Synth *self, SynthTable *table, float sampleRate) {
			self->table = table;

			self->sampleRate = sampleRate;
			self->sampleRateRecip = 1f / sampleRate;

			Reset(self);
		}

		public static void Reset(Synth *self) {
			for (int i = 0; i < 16; i += 1) {
				self->channelPans[i] = 64;
				self->channelVolumes[i] = 100;
				self->channelExpressions[i] = 127;
				self->channelpitchBends[i] = 64;
			}
		}

		public static void NoteOn(Synth *self, int track, byte channel, byte note, byte velocity) {
//			UnityEngine.Debug.LogFormat("note on {0} {1} {2} {3}", track, channel, note, velocity);
			if (channel == 9) return;
			if (velocity == 0) {
				NoteOff(self, track, channel, note, velocity);
				return;
			}

			self->isOn = true;
			self->note = note;
			self->velocity = velocity;
			self->channel = channel;

			self->time = 0;
			UpdatePitch(self);
			UpdateGain(self);
		}

		public static void NoteOff(Synth *self, int track, byte channel, byte note, byte velocity) {
//			UnityEngine.Debug.LogFormat("note on {0} {1} {2} {3}", track, channel, note, velocity);
			if (channel == 9) return;
			if (note == self->note) self->isOn = false;
		}

		public static void Controller(Synth *self, int track, byte channel, byte controller, byte value) {
			switch (controller) {
			case 7:  // channel volume
				self->channelVolumes[channel] = value;
				if (channel == self->channel) UpdateGain(self);
				break;
			case 10:  // pan
				self->channelPans[channel] = value;
				if (channel == self->channel) UpdateGain(self);
				break;
			case 11:  // expression
				self->channelExpressions[channel] = value;
				if (channel == self->channel) UpdateGain(self);
				break;
			}
		}

		public static void UpdateGain(Synth *self) {
			SynthTable *table = self->table;
			byte pan = self->channelPans[self->channel];
			byte volume = self->channelVolumes[self->channel];
			byte expression = self->channelExpressions[self->channel];

			float gain = table->volm2Gain[volume] * table->volm2Gain[expression] * table->volm2Gain[self->velocity];
			self->gainLeft = table->pan2Left[pan] * gain;
			self->gainRight = table->pan2Right[pan] * gain;
		}

		public static void PitchBend(Synth *self, int track, byte channel, byte lsb, byte msb) {
			UnityEngine.Debug.LogFormat("pitch bend {0} {1} {2} {3}", track, channel, lsb, msb);
			self->channelpitchBends[channel] = msb;
			if (channel == self->channel) UpdatePitch(self);
		}

		public static void UpdatePitch(Synth *self) {
			SynthTable *table = self->table;
			self->freq = table->note2Freq[self->note] * table->bend2Pitch[self->channelpitchBends[self->channel]];
		}

		public static void Process(Synth *self, float length, float *data) {
			for (int i = 0; i < length; i += 2) {
				if (self->isOn) {
					float value = (float)System.Math.Sin(self->time * self->freq * self->table->pi2);
					data[i] = value * self->gainLeft;
					data[i + 1] = value * self->gainRight;
				} else {
					data[i] = data[i + 1] = 0;
				}

				self->time += self->sampleRateRecip;
			}
		}
	}
}

