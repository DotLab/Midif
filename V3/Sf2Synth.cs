﻿using Math = System.Math;
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

					volm2Gain[i] = (float)Deci2Gain(40.0 * Math.Log10(i / 127.0));
					pan2Left[i] = (float)Deci2Gain(20.0 * Math.Log10(Math.Cos(Math.PI / 2 * (i / 127.0))));
					pan2Right[i] = (float)Deci2Gain(20.0 * Math.Log10(Math.Sin(Math.PI / 2 * (i / 127.0))));
				}

				for (int i = 0; i < 256; i++) {
					semi2Pitch[i] = (float)Math.Pow(2, (i - Semi2PitchCenter) / 12.0);
					cent2Pitch[i] = (float)Math.Pow(2, (i - Semi2PitchCenter) / 1200.0);
					db2Gain[i] = (float)Deci2Gain(i - Db2GainCenter);
				}
			}

			public static double Deci2Gain (double db) {
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
					// case StageSustain: break;  // not possible
					// case StageRelease: break;  // not possible
					// case StageDone: break;  // no op
					}
					Console.Log("stage", stage, "stageTime", stageTime, "gain", gain, "sustainGain", sustainGain);
				}
			}
		}

		public struct Filter {
			public float fc, q;
    		public float a0, a1, a2, b1, b2;
    		public float z1, z2;

			public void Set(float fc, float q) {
				this.fc = fc;
				this.q  = q;

				float K = (float)Math.Tan(Math.PI * fc);
				float norm = 1f / (1f + K / q + K * K);
				a0 = K * K * norm;
				a1 = 2f * a0;
				a2 = a0;
				b1 = 2f * (K * K - 1f) * norm;
				b2 = (1f - K / q + K * K) * norm;
			}

			public float Process(float i) {
				float o = i * a0 + z1;
				z1 = i * a1 + z2 - b1 * o;
				z2 = i * a2 - b2 * o;
				return o;
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

			public Envelope volEnv;
			public bool useFilter; public Filter filter;

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
				step = sample.sampleRate * table.sampleRateRecip
					* table.semi2Pitch[Table.Semi2PitchCenter + note - root + gs[GenType.coarseTune].value] 
					* table.cent2Pitch[Table.Semi2PitchCenter + sample.correction + gs[GenType.fineTune].value];
				mode = (byte)gs[GenType.sampleModes].value;
				attenuation = (float)Table.Deci2Gain(-gs[GenType.initialAttenuation].value * .1);

				phase = 0;

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
				
//				volEnv.sustainGain = table.db2Gain[Table.Db2GainCenter - gs[GenType.sustainVolEnv].value / 10];
				volEnv.sustainGain = (float)Table.Deci2Gain(-gs[GenType.sustainVolEnv].value * .1);;

				// filter
				short initialFilterFc = gs[GenType.initialFilterFc].value;
				short initialFilterQ = gs[GenType.initialFilterQ].value;

				if (initialFilterQ > 0) {
					useFilter = true;
					filter.z1 = filter.z2 = 0;
					filter.Set((float)Table.AbsCent2Freq(initialFilterFc) * table.sampleRateRecip, (float)Table.Deci2Gain(initialFilterQ * .1));
				} else {
					useFilter = false;
				}
			}

			public void Process(float[] buffer) {
				for (int i = 0, length = buffer.Length; i < length; i += 2) {
					uint uintPhase = (uint)phase;
					float t = (float)(phase - uintPhase);
					float value = data[start + uintPhase] * (1f - t) + data[start + uintPhase + 1] * t;

//					if (useFilter) value = filter.Process(value);

					value = value * attenuation * volEnv.gain;
					
					buffer[i] += value * gainLeft;
					buffer[i + 1] += value * gainRight;
					// buffer[i] += attenuation * volEnv.gain;
					
					// voice
					phase += step;
					if (phase > loopEnd) {
						phase -= loopDuration;
					}

					// volEnv
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
			masterGain = (float)Table.Deci2Gain(volume);
		}

		public void NoteOn(int track, byte channel, byte note, byte velocity) {
			if (channel != 0) return;

			var preset = file.presets[presetIndex];
			for (int i = 0, endI = preset.presetZones.Length; i < endI; i += 1) {
				var presetZone = preset.presetZones[i];
				if (!presetZone.zone.Contains(note, velocity)) continue;

				var instrument = presetZone.instrument;
				for (int j = 0, endJ = instrument.instrumentZones.Length; j < endJ; j += 1) {
					var instrumentZone = instrument.instrumentZones[j];
					if (!instrumentZone.zone.Contains(note, velocity)) continue;

					var zone = Sf2File.GetAppliedZone(preset.globalZone, presetZone.zone, instrument.globalZone, instrumentZone.zone);

					if (firstFreeVoice == -1) {
						UnityEngine.Debug.LogFormat("Not enough notes active {0} free {1}", CountVoices(firstActiveVoice), CountVoices(firstFreeVoice));
						return;
					}
					int k = firstFreeVoice;
					firstFreeVoice = voices[k].next;
					voices[k].next = firstActiveVoice;
					firstActiveVoice = k;

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
			if (channel != 0) return;

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

