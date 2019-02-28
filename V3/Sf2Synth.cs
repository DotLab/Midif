using Math = System.Math;
using GenType = Midif.V3.Sf2File.GeneratorType;

namespace Midif.V3 {
	public sealed class Sf2Synth : IMidiSynth {
		public sealed class Table {
			public const float VelcRecip = 1f / 127f;

			public const float Pi = 3.14159265f;
			public const float Pi2 = Pi * 2;

			public readonly float sampleRate;
			public readonly float sampleRateRecip;

			public readonly float[] note2Freq = new float[128];
			public readonly float[] bend2Pitch = new float[128];

			public readonly float[] volm2Gain = new float[128];
			public readonly float[] pan2Left = new float[128];
			public readonly float[] pan2Right = new float[128];

			public const int Semi2PitchCenter = 128;
			public readonly float[] semi2Pitch = new float[256];
			public readonly float[] cent2Pitch = new float[256];

			public const int Db2GainCenter = 128;
			public readonly float[] db2Gain = new float[256];

			public Table(float sampleRate) {
				this.sampleRate = sampleRate;
				sampleRateRecip = 1f / sampleRate;

				for (int i = 0; i < 128; i++) {
					note2Freq[i] = (float)(440 * Math.Pow(2, (i - 69) / 12.0));
					bend2Pitch[i] = (float)Math.Pow(2, 2 * ((i - 64) / 127) / 12.0);

					volm2Gain[i] = (float)Db2Gain(40.0 * Math.Log10(i / 127.0));
					pan2Left[i] = (float)Db2Gain(20.0 * Math.Log10(Math.Cos(Math.PI / 2 * (i / 127.0))));
					pan2Right[i] = (float)Db2Gain(20.0 * Math.Log10(Math.Sin(Math.PI / 2 * (i / 127.0))));
				}

				for (int i = 0; i < 256; i++) {
					semi2Pitch[i] = (float)Math.Pow(2, (i - Semi2PitchCenter) / 12.0);
					cent2Pitch[i] = (float)Math.Pow(2, (i - Semi2PitchCenter) / 1200.0);
					db2Gain[i] = (float)Db2Gain(i - Db2GainCenter);
				}
			}

//			public static double Cent2Pitch(double cent) {
//				return 
//			}

			public static double Db2Gain (double db) {
				return Math.Pow(10.0, (db / 20.0));
			}

			// For example, a delay of 10 msec would be 1200log2(.01) = -7973. 
			public static double Timecent2Sec(short timecent) {
				return Math.Pow(2, (double)timecent / 1200.0);
			}

			// For example, a frequency of 10 mHz would be 1200log2(.01/8.176) = -11610. 
			public static double AbsCent2Freq(short absCent) {
				return 8.176 * Math.Pow(2, (double)absCent / 1200.0);
			}
		}

		public struct Envelope {
			public const int StageDelay = 0;
			public const int StageAttack = 1;
			public const int StageHold = 2;
			public const int StageDecay = 3;
			public const int StageSustain = 4;
			public const int StageRelease = 5;
			public const int StageDone = 6;


			public int delayTime;
			public int attackTime;
			public int holdTime;
			public int decayTime;
			public int releaseTime;

			public float sustainGain;

			public byte stage;

			public float gain;
			public float gainStep;

			public int time;
			public int stageTime;

			public void On() {
				stage = StageDelay;
				
				gain = 0;
				gainStep = 0;
				
				time = 0;
				stageTime = delayTime;
			}

			public void Off() {
				stage = StageRelease;
				time = 0;

				stageTime = releaseTime;
				gainStep = -gain / releaseTime;
				// Console.Log("off stage", stage, "stageTime", stageTime / 44100f, "gain", gain, "sustainGain", sustainGain);
			}

			public void Advance() {
				if (stage == StageSustain || stage == StageDone) return;

				gain += gainStep;
				time += 1;
				if (time >= stageTime) {
					time = 0;
					stage += 1;
					switch (stage) {
					case StageAttack: stageTime = attackTime; gainStep = 1f / attackTime; break;
					case StageHold: stageTime = holdTime; gainStep = 0; break;
					case StageDecay: stageTime = decayTime; gainStep = (sustainGain - gain) / decayTime; break;
					// case StageSustain: break;  // no op
					// case StageRelease: break;  // not possible
					// case StageDone: break;  // no op
					}
					// Console.Log("stage", stage, "stageTime", stageTime / 44100f, "gain", gain, "sustainGain", sustainGain);
				} else if (stage == StageRelease && gain < .01f) {
					stage = StageDone;
				}
			}
		}

		public struct Filter {
			public float fc, q;
    		public float a1, a2, b1, b2;
    		public float h1, h2;

			public void Set(Table table, float fc, float q) {
				Console.Log("set lowpass fc", fc, "q", q);
				this.fc = fc;
				this.q  = q;

				float omega = Table.Pi2 * fc * table.sampleRateRecip;
				float sin = (float)Math.Sin(omega);
				float cos = (float)Math.Cos(omega);
				float alpha = sin / (2f * q);
				float a0Recip = 1f / (1 + alpha);

				a1 = -2f * cos * a0Recip;
				a2 = (1f - alpha) * a0Recip;
				b1 = (1f - cos) * a0Recip;
				b2 = b1 * .5f; 
			}

			public float Process(float i) {
                float centerNode = i - a1 * h1 - a2 * h2;
                float o = b2 * (centerNode + h2) + b1 * h1;
                h2 = h1;
                h1 = centerNode;
				return o;
			}
		}

		public struct Lfo {
			public float step;
			public double phase;
			public float value;

			public void On(Table table, float delay, float freq) {
				step = freq * table.sampleRateRecip;
				phase = -delay * freq;
				value = 0;
			}

			public void Advance() {
				if (phase < 0) value = 0;
				else if (phase < 1) value = (float)(phase * 2.0 - 1.0);
				else value = (float)(1.0 - 2.0 * (phase - 1.0));
				
				phase += step;
				if (phase > 2) phase -= 2;  
			}
		}

		public struct Voice {
			public const byte ModeNoLoop = 0;
			public const byte ModeContinuousLoop = 1;
			public const byte ModeLoopProceed = 3;

			public int next;

			public byte note;
			public byte velocity;
			public byte channel;

			public float gainLeft;
			public float gainRight;

			public Lfo vibLfo;
			public Lfo modLfo;
			public bool useVibLfo; 
			public bool useModLfo;
			public bool useModLfoToPitch;
			public bool useModLfoToFilterFc;
			public bool useModLfoToVolume;
			public short vibLfoToPitch;
			public short modLfoToPitch;
			public short modLfoToFilterFc;
			public short modLfoToVolume;

			public Filter filter;
			public bool useFilter; 
			public float filterFc;

			public Envelope volEnv;
			public Envelope modEnv;
			public bool useModEnv;
			public bool useModEnvToPitch;
			public bool useModEnvToFilterFc;
			public short modEnvToPitch;
			public short modEnvToFilterFc;

			public float[] data;
			public Table table;

			public byte mode;
			public uint start;
			public uint end;
			public uint startloop;
			public uint endloop;

			public uint loopEnd;
			public uint loopDuration;
			public float attenuation;
			public float initialStep;
			public float step;
			public double phase;

			public void On(Table table, Sf2File.SampleHeader sample, Sf2Zone zone) {
				var gs = zone.gens;

				// voice
				start =     (uint)(sample.start + (gs[GenType.startAddrsCoarseOffset].value << 15) + gs[GenType.startAddrsOffset].value);
				end =       (uint)(sample.end + (gs[GenType.endAddrsCoarseOffset].value << 15) + gs[GenType.endAddrsOffset].value);
				startloop = (uint)(sample.startloop + (gs[GenType.startloopAddrsCoarseOffset].value << 15) + gs[GenType.startloopAddrsOffset].value);
				endloop =   (uint)(sample.endloop + (gs[GenType.endloopAddrsCoarseOffset].value << 15) + gs[GenType.endloopAddrsOffset].value);
				loopEnd = endloop - start;
				loopDuration = endloop - startloop;

				int root = gs[GenType.overridingRootKey].value;
				if (root < 0) root = sample.originalKey;
				step = initialStep = sample.sampleRate * table.sampleRateRecip
					* table.semi2Pitch[Table.Semi2PitchCenter + note - root + gs[GenType.coarseTune].value] 
					* table.cent2Pitch[Table.Semi2PitchCenter + sample.correction + gs[GenType.fineTune].value];
				mode = (byte)gs[GenType.sampleModes].value;
				attenuation = (float)Table.Db2Gain(-gs[GenType.initialAttenuation].value * .1);
				phase = 0;

				// vibLfo
				vibLfoToPitch = gs[GenType.vibLfoToPitch].value;
				if (vibLfoToPitch != 0) {
					useVibLfo = true;
					short delayVibLfo = gs[GenType.delayVibLFO].value;
					short freqVibLfo = gs[GenType.freqVibLFO].value;
					vibLfo.On(table, (float)Table.Timecent2Sec(delayVibLfo), (float)Table.AbsCent2Freq(freqVibLfo));
				} else {
					useVibLfo = false;
				}

				// modLfo
				modLfoToPitch = gs[GenType.modLfoToPitch].value;
				modLfoToFilterFc = gs[GenType.modLfoToFilterFc].value;
				modLfoToVolume = gs[GenType.modLfoToVolume].value;
				useModLfo = false;
				if (modLfoToPitch != 0) {
					useModLfo = true;
					useModLfoToPitch = true;
				}
				if (modLfoToFilterFc != 0) {
					useModLfo = true;
					useModLfoToFilterFc = true;
				}
				if (modLfoToVolume != 0) {
					useModLfo = true;
					useModLfoToVolume = true;
				}
				if (useModLfo) {
					short delayModLfo = gs[GenType.delayModLFO].value;
					short freqModLfo = gs[GenType.freqModLFO].value;
					modLfo.On(table, (float)Table.Timecent2Sec(delayModLfo), (float)Table.AbsCent2Freq(freqModLfo));
				}

				// filter
				short initialFilterQ = gs[GenType.initialFilterQ].value;
				short initialFilterFc = gs[GenType.initialFilterFc].value;
				if (initialFilterQ != 0 && initialFilterFc < 13500) {
					useFilter = true;
					filter.h1 = filter.h2 = 0;
					filterFc = (float)Table.AbsCent2Freq(initialFilterFc);
					filter.Set(table, filterFc, (float)Table.Db2Gain(initialFilterQ * .1));
				} else {
					useFilter = false;
				}

				// volEnv
				short delayVolEnv = gs[GenType.delayVolEnv].value;
				short attackVolEnv = gs[GenType.attackVolEnv].value;
				short holdVolEnv = gs[GenType.holdVolEnv].value;
				short decayVolEnv = gs[GenType.decayVolEnv].value;
				short releaseVolEnv = gs[GenType.releaseVolEnv].value;

				volEnv.delayTime = (int)(table.sampleRate * Table.Timecent2Sec(delayVolEnv));
				volEnv.attackTime = (int)(table.sampleRate * Table.Timecent2Sec(attackVolEnv));
				volEnv.holdTime = (int)(table.sampleRate * Table.Timecent2Sec(holdVolEnv));
				volEnv.decayTime = (int)(table.sampleRate * Table.Timecent2Sec(decayVolEnv));
				volEnv.releaseTime = (int)(table.sampleRate * Table.Timecent2Sec(releaseVolEnv));
				
				volEnv.sustainGain = (float)Table.Db2Gain(-gs[GenType.sustainVolEnv].value * .1);
			}

			public void Process(float[] buffer) {
				for (int i = 0, length = buffer.Length; i < length; i += 2) {
					uint uintPhase = (uint)phase;
					float t = (float)(phase - uintPhase);
					float value = data[start + uintPhase] * (1f - t) + data[start + uintPhase + 1] * t;

					if (useFilter) {
						if (useModLfoToFilterFc || useModEnvToFilterFc) {
							float curFilterFc = filterFc;
							if (useModLfoToFilterFc) curFilterFc *= table.cent2Pitch[Table.Semi2PitchCenter + (int)(modLfoToFilterFc * modLfo.value)];
							if (useModEnvToFilterFc) curFilterFc *= table.cent2Pitch[Table.Semi2PitchCenter + (int)(modEnvToFilterFc * modEnv.gain)];
							filter.Set(table, curFilterFc, filter.q);
						}
						value = filter.Process(value);
					}

					value = value * attenuation * volEnv.gain;
					
					buffer[i] += value * gainLeft;
					buffer[i + 1] += value * gainRight;
					
					// voice
					float curStep = step;
					if (useVibLfo) curStep *= table.cent2Pitch[Table.Semi2PitchCenter + (int)(vibLfoToPitch * vibLfo.value)];
					if (useModLfoToPitch) curStep *= table.cent2Pitch[Table.Semi2PitchCenter + (int)(modLfoToPitch * modLfo.value)];
					if (useModEnvToPitch) curStep *= table.cent2Pitch[Table.Semi2PitchCenter + (int)(modEnvToPitch * modEnv.gain)];

					phase += curStep;
					if (phase > loopEnd) phase -= loopDuration;

					if (useVibLfo) vibLfo.Advance();
					if (useModLfo) modLfo.Advance();
					if (useModEnv) modEnv.Advance();
					volEnv.Advance();
				}
			}
		}

		public Sf2File file;
		public Table table;

		public readonly byte[] channelPrograms = new byte[16];
		public readonly byte[] channelPans = new byte[16];
		public readonly byte[] channelVolumes = new byte[16];
		public readonly byte[] channelExpressions = new byte[16];
		public readonly ushort[] channelpitchBends = new ushort[16];

		public float masterGain;
		public byte presetIndex = 10;

		public readonly int voiceCount;
		public readonly Voice[] voices;
		public int firstFreeVoice;
		public int firstActiveVoice;

		public Sf2Synth(Sf2File file, Table table, int voiceCount) {
			this.file = file;
			this.table = table;

			masterGain = 1;

			this.voiceCount = voiceCount;
			voices = new Voice[voiceCount];
			for (int i = 0; i < voiceCount; i += 1) {
				voices[i].data = file.data;
				voices[i].table = table;
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
			masterGain = (float)Table.Db2Gain(volume);
		}

		public void NoteOn(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;

			var preset = file.presets[presetIndex];
			for (int i = 0, endI = preset.presetZones.Length; i < endI; i += 1) {
				var presetZone = preset.presetZones[i];
				if (!presetZone.zone.Contains(note, velocity)) continue;

				var instrument = presetZone.instrument;
				for (int j = 0, endJ = instrument.instrumentZones.Length; j < endJ; j += 1) {
					var instrumentZone = instrument.instrumentZones[j];
					if (!instrumentZone.zone.Contains(note, velocity)) continue;

					var zone = Sf2File.GetAppliedZone(preset.globalZone, presetZone.zone, instrument.globalZone, instrumentZone.zone);

					int k = firstFreeVoice;
					if (k == -1) {
						// UnityEngine.Debug.LogFormat("Not enough notes active {0} free {1}", CountVoices(firstActiveVoice), CountVoices(firstFreeVoice));
						// return;

						// find the oldest active voice and move it to front
						for (k = firstActiveVoice; voices[voices[k].next].next != -1; k = voices[k].next);
						int next = voices[k].next;
						voices[k].next = -1;
						k = next;
						voices[k].next = firstActiveVoice;
						firstActiveVoice = k;
					} else {
						firstFreeVoice = voices[k].next;
						voices[k].next = firstActiveVoice;
						firstActiveVoice = k;
					}

					voices[k].note = note;
					voices[k].velocity = velocity;
					voices[k].channel = channel;

					voices[k].On(table, instrumentZone.sampleHeader, zone);
					voices[k].volEnv.On();

					UpdateVoicePitch(k);
					UpdateVoiceGain(k);
				}
			}
		}

		public void NoteOff(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;

			for (int i = firstActiveVoice; i != -1; i = voices[i].next) {
				if (voices[i].channel == channel && voices[i].note == note) {
					voices[i].volEnv.Off();
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

		public int CountVoices(int i) {
			int j = 0;
			for (; i != -1; i = voices[i].next) {
				j += 1;
			}
			return j;
		}

		public void Panic() {
			for (int prev = -1, i = firstActiveVoice; i != -1;) {
				if (voices[i].volEnv.stage == Envelope.StageDone) {
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

		public void Process(float[] buffer) {
			var sb = new System.Text.StringBuilder();
			for (int j = firstActiveVoice; j != -1; j = voices[j].next) {
				voices[j].Process(buffer);
			}

			for (int i = 0, length = buffer.Length; i < length; i += 2) {
				WaveVisualizer.Push(buffer[i]);
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

		}

		void UpdateChannelPitch(int channel) {
			float channelPitch = table.bend2Pitch[channelpitchBends[channel]];

			for (int i = firstActiveVoice; i != -1; i = voices[i].next) {
				if (voices[i].channel == channel) {
				}
			}
		}
	}
}

