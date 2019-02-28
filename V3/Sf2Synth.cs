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

			public readonly int minSampleCount;

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

				minSampleCount = (int)(0.002 * sampleRate);

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

			public static double Cent2Pitch(double cent) {
				return Math.Pow(2, cent / 1200.0);
			}

			public static double Db2Gain (double db) {
				// different with common dB (db / 10.0)
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

			// duration in samples
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

			public void On(Table table) {
				// if too small, set to 0 so that env can just skip stages
				if (delayTime < table.minSampleCount) delayTime = 0; 
				if (attackTime < table.minSampleCount) attackTime = 0; 
				if (holdTime < table.minSampleCount) holdTime = 0; 
				if (decayTime < table.minSampleCount) decayTime = 0; 
				if (releaseTime < table.minSampleCount) releaseTime = 0; 

				stage = StageDelay;
				
				gain = 0;
				gainStep = 0;
				
				time = 0;
				stageTime = delayTime;

				// Console.Log("\tenv on delayTime", delayTime, "attackTime", attackTime, "holdTime", holdTime, "decayTime", decayTime, "releaseTime", releaseTime);
			}

			public void Off() {
				if (stage == StageDone) return;
				stage = StageRelease;
				time = 0;

				stageTime = releaseTime;
				gainStep = -gain / releaseTime;
				// Console.Log("\tenv off stage", stage, "stageTime", stageTime, "gain", gain, "sustainGain", sustainGain);
			}

			// force stop
			public void Kill() {
				if (stage == StageDone) return;
				stage = StageDone;
				time = 0;
				gain = 0;
				stageTime = 0;
				gainStep = 0;
			}

			public void Advance(int count) {
				if (stage == StageSustain || stage == StageDone) return;

				gain += gainStep * count;
				time += count;
				if (time >= stageTime) {
					do {
						// skip stages if env can
						time -= stageTime;
						stage += 1;
						switch (stage) {
						case StageAttack: stageTime = attackTime; break;
						case StageHold: stageTime = holdTime; break;
						case StageDecay: stageTime = decayTime; break;
						case StageSustain: time = -1; stageTime = 0; break;
						// case StageRelease: break;  // not possible
						case StageDone: time = -1; stageTime = 0; break;
						}
					} while (time >= stageTime);
					// then set gain, gainStep, division by 0 should not happen
					switch (stage) {
					case StageAttack: gain = 0; gainStep = 1f / attackTime; break;
					case StageHold: gain = 1; gainStep = 0; break;
					case StageDecay: gain = 1; gainStep = (sustainGain - gain) / decayTime; break;
					case StageSustain: gain = sustainGain; gainStep = 0; break;
					// case StageRelease: break;  // not possible
					case StageDone: gain = 0; gainStep = 0; break;
					}
					// Console.Log("\tenv stage", stage, "time", time, "stageTime", stageTime, "gain", gain, "sustainGain", sustainGain);
				}
				
				// auto done when gain is too low
				if ((stage == StageSustain || stage == StageRelease) && gain < .01f) {
					stage = StageDone;
				}
			}
		}

		public struct Filter {
			public float fc, q;
    		public float a1, a2, b1, b2;
    		public float h1, h2;

			public void Set(Table table, float fc, float q) {
				// Console.Log("\tfilter set fc", this.fc , "->", fc, "q", this.q, "->", q);
				this.fc = fc;
				this.q  = q;

				// https://github.com/FluidSynth/fluidsynth/blob/29c668683f43e3b8b4aab0b8aa73cb02aacd6fcb/src/rvoice/fluid_iir_filter.c#L278
				// previous simple bipolar lowpass is faulty when fc is large and should not be used:
				// http://www.earlevel.com/main/2012/11/26/biquad-c-source-code/
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
			public int delayTime;
			public double phase;
			public float value;

			public void On(Table table, float delay, float freq) {
				// in sample count
				delayTime = (int)(delay * table.sampleRate);
				if (delayTime < table.minSampleCount) delayTime = 0;

				// period is 2 
				step = 2f * freq * table.sampleRateRecip;
				// a triangle:
				// | /|\ |
				// |/ | \|
				// 0  1  2
				// start from 0.5 so that value is 0
				phase = 0.5;
				value = 0;
			}

			public void Advance(int count) {
				if (delayTime > 0) {
					value = 0;
					delayTime -= count;
					return;
				}

				if (phase < 1) value = (float)(phase * 2.0 - 1.0);
				else value = (float)(1.0 - 2.0 * (phase - 1.0));
				
				phase += step * count;
				while (phase > 2) phase -= 2;  
			}
		}

		public struct Voice {
			public const byte ModeNoLoop = 0;
			public const byte ModeContinuousLoop = 1;
			public const byte ModeLoopProceed = 3;

			public int id;
			public int next;

			public byte note;
			public byte velocity;
			public byte channel;

			public float gainLeft;
			public float gainRight;

			public float gain;
			public float curGain;

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

			public short mode;
			public uint start;
			public uint duration;
			public uint loopEnd;
			public uint loopDuration;

			public float step;
			public float curStep;
			public double phase;

			public int count;
			public int lastLfoCount;
			public int lastEnvCount;
			public int lfoMask;
			public int envMask;

			public bool killed;

			public void On(Table table, Sf2File.SampleHeader sample, Sf2Zone zone) {
				count = lastLfoCount = lastEnvCount = 0;
				// when (count & xxxMask) == 0, update xxx and xxx's dependents
				lfoMask = 0xff;
				envMask = 0xff;

				killed = false;

				var gs = zone.gens;

				// voice
				start = (uint)(sample.start + (gs[GenType.startAddrsCoarseOffset].value << 15) + gs[GenType.startAddrsOffset].value);
				uint end = (uint)(sample.end + (gs[GenType.endAddrsCoarseOffset].value << 15) + gs[GenType.endAddrsOffset].value);
				uint startloop = (uint)(sample.startloop + (gs[GenType.startloopAddrsCoarseOffset].value << 15) + gs[GenType.startloopAddrsOffset].value);
				uint endloop = (uint)(sample.endloop + (gs[GenType.endloopAddrsCoarseOffset].value << 15) + gs[GenType.endloopAddrsOffset].value);
				duration = end - start;
				loopEnd = endloop - start;
				loopDuration = endloop - startloop;

				int root = gs[GenType.overridingRootKey].value;
				if (root < 0) root = sample.originalKey;  // root = -1 means not set
				curStep = step = sample.sampleRate * table.sampleRateRecip
					* table.semi2Pitch[Table.Semi2PitchCenter + note - root + gs[GenType.coarseTune].value] 
					* table.cent2Pitch[Table.Semi2PitchCenter + sample.correction + gs[GenType.fineTune].value];
				mode = gs[GenType.sampleModes].value;
				Console.Log(sample.sampleName, mode);
				gain = (float)Table.Db2Gain(-gs[GenType.initialAttenuation].value * .1);  // cB
				phase = 0;

				// filter
				short initialFilterFc = gs[GenType.initialFilterFc].value;  // cent
				short initialFilterQ = gs[GenType.initialFilterQ].value;  // cB
				filterFc = (float)Table.AbsCent2Freq(initialFilterFc);
				filter.h1 = filter.h2 = 0;
				filter.Set(table, filterFc, (float)Table.Db2Gain(initialFilterQ * .1));
				// useFilter may be set by modLfo and/or modEnv if they set the fc, so just init whatsoever
				useFilter = initialFilterFc < 13500;

				// vibLfo
				vibLfoToPitch = gs[GenType.vibLfoToPitch].value;  // cent fs
				if (vibLfoToPitch != 0) {
					useVibLfo = true;
					short delayVibLfo = gs[GenType.delayVibLFO].value;  // timecent
					short freqVibLfo = gs[GenType.freqVibLFO].value;  // cent
					vibLfo.On(table, (float)Table.Timecent2Sec(delayVibLfo), (float)Table.AbsCent2Freq(freqVibLfo));
				} else {
					useVibLfo = false;
				}

				// modLfo
				modLfoToPitch = gs[GenType.modLfoToPitch].value;  // cent fs
				modLfoToFilterFc = gs[GenType.modLfoToFilterFc].value;  // cent fs
				modLfoToVolume = gs[GenType.modLfoToVolume].value;  // cB fs
				useModLfoToPitch = modLfoToPitch != 0;
				useModLfoToFilterFc = modLfoToFilterFc != 0;
				useModLfoToVolume = modLfoToVolume != 0;
				useModLfo = useModLfoToPitch || useModLfoToFilterFc || useModLfoToVolume;
				if (useModLfo) {
					// modLfo affects filter, so use filter
					if (useModLfoToFilterFc) useFilter = true; 
					short delayModLfo = gs[GenType.delayModLFO].value;  // timecent
					short freqModLfo = gs[GenType.freqModLFO].value;  // cent
					modLfo.On(table, (float)Table.Timecent2Sec(delayModLfo), (float)Table.AbsCent2Freq(freqModLfo));
				}

				// volEnv
				short delayVolEnv = gs[GenType.delayVolEnv].value;  // timecent
				short attackVolEnv = gs[GenType.attackVolEnv].value;  // timecent
				short holdVolEnv = gs[GenType.holdVolEnv].value;  // timecent
				short decayVolEnv = gs[GenType.decayVolEnv].value;  // timecent
				short releaseVolEnv = gs[GenType.releaseVolEnv].value;  // timecent
				volEnv.delayTime = (int)(table.sampleRate * Table.Timecent2Sec(delayVolEnv));
				volEnv.attackTime = (int)(table.sampleRate * Table.Timecent2Sec(attackVolEnv));
				volEnv.holdTime = (int)(table.sampleRate * Table.Timecent2Sec(holdVolEnv));
				volEnv.decayTime = (int)(table.sampleRate * Table.Timecent2Sec(decayVolEnv));
				volEnv.releaseTime = (int)(table.sampleRate * Table.Timecent2Sec(releaseVolEnv));
				volEnv.sustainGain = (float)Table.Db2Gain(-gs[GenType.sustainVolEnv].value * .1);  // cB attn
				volEnv.On(table);

				// modEnv
				modEnvToPitch = gs[GenType.modEnvToPitch].value;  // cent fs
				modEnvToFilterFc = gs[GenType.modEnvToFilterFc].value;  // cent fs
				useModEnvToPitch = modEnvToPitch != 0;
				useModEnvToFilterFc = modEnvToFilterFc != 0;
				useModEnv = useModEnvToPitch || useModEnvToFilterFc;
				if (useModEnv) {
					// modEnv affects filter, so use filter
					if (useModEnvToFilterFc) useFilter = true;
					short delayModEnv = gs[GenType.delayModEnv].value;  // timecent
					short attackModEnv = gs[GenType.attackModEnv].value;  // timecent
					short holdModEnv = gs[GenType.holdModEnv].value;  // timecent
					short decayModEnv = gs[GenType.decayModEnv].value;  // timecent
					short releaseModEnv = gs[GenType.releaseModEnv].value;  // timecent
					modEnv.delayTime = (int)(table.sampleRate * Table.Timecent2Sec(delayModEnv));
					modEnv.attackTime = (int)(table.sampleRate * Table.Timecent2Sec(attackModEnv));
					modEnv.holdTime = (int)(table.sampleRate * Table.Timecent2Sec(holdModEnv));
					modEnv.decayTime = (int)(table.sampleRate * Table.Timecent2Sec(decayModEnv));
					modEnv.releaseTime = (int)(table.sampleRate * Table.Timecent2Sec(releaseModEnv));
					modEnv.sustainGain = (float)Table.Db2Gain(-gs[GenType.sustainVolEnv].value * .1);  // cB attn
					modEnv.On(table);
				}

				Console.Log("v", id, "on", note, 
					"vibLfo", vibLfoToPitch, 
					"modLfo", modLfoToPitch, modLfoToFilterFc, modLfoToVolume, 
					"modEnv", modEnvToPitch, modEnvToFilterFc, 
					"useVibLfo", useVibLfo, "useModLfo", useModLfo, "useModEnv", useModEnv, "useFilter", useFilter,
					"filter", filterFc, initialFilterQ, sample.sampleName);
			}

			public void Off() {
				volEnv.Off();
				if (useModEnv) modEnv.Off();
			}

			public void Kill() {
				volEnv.Kill();
				killed = true;
			}

			public void Process(float[] buffer) {
				for (int i = 0, length = buffer.Length; i < length; i += 2) {
					// the first iteration will call update with count = 0 so that filter, lfos, envs can init
					bool lfoFlag = (count & lfoMask) == 0;
					bool envFlag = (count & envMask) == 0;
					// maybe more specific flags will be better?
					if (lfoFlag || envFlag) Update(lfoFlag, envFlag);
					count += 1;

					// simple interpolation
					float value;
					if (killed) {
						value = 0;
					} else {
						uint uintPhase = (uint)phase;
						float t = (float)(phase - uintPhase);
						value = data[start + uintPhase] * (1f - t) + data[start + uintPhase + 1] * t;
					}

					// filter even when fc > 13500 (set fc = 13500 when that happens) so that filter is always ready
					if (useFilter) value = filter.Process(value);

					value = value * curGain;
					buffer[i] += value * gainLeft;
					buffer[i + 1] += value * gainRight;

					phase += curStep;
					switch (mode) {
					case Sf2File.SampleMode.contLoop:
						if (phase > loopEnd) phase -= loopDuration;
						break;
					case Sf2File.SampleMode.contLoopRelease:
						if (volEnv.stage == Envelope.StageRelease) {
							if (phase > duration) Kill();
						} else {
							if (phase > loopEnd) phase -= loopDuration;
						}
						break;
					case Sf2File.SampleMode.noLoop:
					default:
						if (phase > duration) Kill();
						break;
					}
				}
			}

			// will be called with count = 0 to init
			void Update(bool lfoFlag, bool envFlag) {
				// Console.Log("v", id, "update", lfoFlag, envFlag, count);
				if (envFlag || (lfoFlag && useModLfoToVolume)) {
					curGain = gain * volEnv.gain;
					if (useModLfoToVolume) curGain *= (float)Table.Db2Gain(modLfoToVolume * .1 * modLfo.value);
				}

				if (useFilter && ((lfoFlag && useModLfoToFilterFc) || (envFlag && useModEnvToFilterFc))) {
					float curFilterFc = filterFc;
					if (useModLfoToFilterFc) curFilterFc *= (float)Table.Cent2Pitch(modLfoToFilterFc * modLfo.value);
					if (useModEnvToFilterFc) curFilterFc *= (float)Table.Cent2Pitch(modEnvToFilterFc * modEnv.gain);
					// filter will become super unstable if fc > 13500, so just set it back to 13500Hz
					if (curFilterFc > 13500) curFilterFc = 13500;
					if (curFilterFc != filter.fc) {
						// float diff = (float)Math.Abs(curFilterFc - filter.fc);
						float diff = curFilterFc - filter.fc;
						if (diff < 0) diff = -diff;
						// only update filter is diff is larger than 100Hz since set filter is very expensive
						if (diff > 100) filter.Set(table, curFilterFc, filter.q);
					}
				}

				if ((lfoFlag && (useVibLfo || useModLfoToPitch)) || (envFlag && useModEnvToPitch)) {
					curStep = step;
					if (useVibLfo) curStep *= (float)Table.Cent2Pitch(vibLfoToPitch * vibLfo.value);
					if (useModLfoToPitch) curStep *= (float)Table.Cent2Pitch(modLfoToPitch * modLfo.value);
					if (useModEnvToPitch) curStep *= (float)Table.Cent2Pitch(modEnvToPitch * modEnv.gain);
				}
				
				if (lfoFlag) {
					int skip = count - lastLfoCount; lastLfoCount = count;
					if (lfoFlag && useVibLfo) vibLfo.Advance(skip);
					if (lfoFlag && useModLfo) modLfo.Advance(skip);
				}

				if (envFlag) {
					int skip = count - lastEnvCount; lastEnvCount = count;
					if (envFlag && useModEnv) modEnv.Advance(skip);
					if (envFlag) volEnv.Advance(skip);
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
		public int presetIndex;

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
				voices[i].id = i;
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

					// Console.Log("synth noteon", note, velocity, "preset", preset.presetName, "instrument", instrument.instName, "sample", instrumentZone.sampleHeader.sampleName);
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

					UpdateVoicePitch(k);
					UpdateVoiceGain(k);
				}
			}
		}

		public void NoteOff(int track, byte channel, byte note, byte velocity) {
			if (channel == 9) return;

			for (int i = firstActiveVoice; i != -1; i = voices[i].next) {
				if (voices[i].channel == channel && voices[i].note == note) {
					voices[i].Off();
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

