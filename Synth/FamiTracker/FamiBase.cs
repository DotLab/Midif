using System;

namespace Midif.Synth.FamiTracker {
	public abstract class FamiBase : MidiComponent {
		public const double ClockFreq = 1789772.727;

		public static readonly double[] VibratoDepthTable = {
			1.0, 1.5, 2.5, 4.0, 5.0, 7.0, 10.0, 12.0, 14.0, 17.0, 22.0, 30.0, 44.0, 64.0, 96.0, 128.0
		};

		public static readonly int[] VibratoTable;
		public static readonly int[] TremoloTable;

		public static readonly double[] Volm2GainTable;
		public static readonly int[] Note2PitcTable;

		public const int Volm2GainTableMod = 0xF;
		public const int Note2PitcTableMod = 0xFF;
		public const int Pitc2StepTableMod = 0x7FF;

		static FamiBase () {
			VibratoTable = new int[0x400];
			for (int depth = 0; depth < 16; depth++)
				for (int phase = 0; phase < 64; phase++)
					VibratoTable[(depth << 6) | phase] = (int)Math.Round(Math.Sin(2 * Math.PI * phase / 64) * VibratoDepthTable[depth]);

			TremoloTable = new int[0x400];
			for (int depth = 0; depth < 16; depth++)
				for (int phase = 0; phase < 64; phase++)
					TremoloTable[(depth << 6) | phase] = (int)Math.Round(Math.Sin(2 * Math.PI * phase / 128) * VibratoDepthTable[depth] / 2);

			Volm2GainTable = new double[0x10];
			for (int i = 0; i < 16; i++)
				Volm2GainTable[i] = i / 15.0;

			Note2PitcTable = new int[SynthTable.Note2FreqLeng];
			for (int i = 0; i < SynthTable.Note2FreqLeng; i++)
				Note2PitcTable[i] = (int)((ClockFreq / 16 / SynthTable.Note2Freq[i]) - 0.5);
		}

		public override bool IsActive {
			get {
				return isOn || (!muted && VolumeMod.IsActive);
			}
		}

		#region Mod

		// 0 ~ 16
		public readonly ModSequence VolumeMod = new ModSequence();
		// -128 ~ 127
		public readonly ModSequence ArpeggioMod = new ModSequence();
		// -128 ~ 127
		public readonly ModSequence PitchMod = new ModSequence();
		// 0 ~ 3
		public readonly ModSequence DutyMod = new ModSequence();

		#endregion

		#region Effect

		// 0xy - Changes the pitch of the note every tick, with base + x and base + y semitones;
		public readonly EffectSequence ArpeggioEffect = new EffectSequence();
		readonly int[] arpeggioTable = new int[3];
		int arpeggioCounter;

		// Qxy - Triggers a targeted note slide up. x is the speed and y is the number of semitones to slide up.
		public readonly EffectSequence NoteUpEffect = new EffectSequence();

		// Rxy - Triggers a targeted note slide down. x is the speed and y is the number of semitones to slide down.
		public readonly EffectSequence NoteDownEffect = new EffectSequence();

		// Axy - Automatic volume slide. The x and y affects the volume as fractions of 16(originally 8).
		public readonly EffectSequence VolumeSlideEffect = new EffectSequence();
		int volumeSlide;

		// 1xx / 2xx - Continuously slides the pitch up / down, with xx steps in pitch units every tick;
		public readonly EffectSequence PitchSlideEffect = new EffectSequence();
		int pitchSlide, pitchSlideSpeed, pitchSlideTarget;

		// 7xy - Applies sine tremolo to notes, x is speed and y is the depth, value = depth << 6 | phase;
		public readonly EffectSequence TremoloEffect = new EffectSequence();
		int tremoloDepth, tremoloStep, tremoloCounter;

		// 7xy - Applies sine tremolo to notes, x is speed and y is the depth, value = depth << 6 | phase;
		public readonly EffectSequence VibratoEffect = new EffectSequence();
		int vibratoDepth, vibratoStep, vibratoCounter;

		// Exx - Sets the channel volume.
		public readonly EffectSequence VolumeEffect = new EffectSequence();

		// Pxx - Sets the fine pitch in xx pitch units.
		public readonly EffectSequence PitchEffect = new EffectSequence();

		// Vxx - This effect controls the duty period of the pulse channels and noise mode of the noise channel.
		public readonly EffectSequence DutyEffect = new EffectSequence();

		#endregion

		public double FrameRate = 60;

		protected double framesPerSample;
		protected double frameCounter;

		protected bool muted;

		protected int volume;

		protected int currentVolume;
		protected int currentNote;
		protected int currentPitch;
		protected int currentDuty;


		public override void Init (double sampleRate) {
			base.Init(sampleRate);

			framesPerSample = FrameRate * sampleRateRecip;
		
			VolumeMod.Init();
			ArpeggioMod.Init();
			PitchMod.Init();
			DutyMod.Init();

			ArpeggioEffect.Init();
			NoteUpEffect.Init();
			NoteDownEffect.Init();
			PitchSlideEffect.Init();
			VolumeSlideEffect.Init();
			TremoloEffect.Init();
			VibratoEffect.Init();
			VolumeEffect.Init();
			PitchEffect.Init();
			DutyEffect.Init();
		}

		public override void NoteOn (byte note, byte velocity) {
			base.NoteOn(note, velocity);
		
			if (VolumeMod.Enabled) VolumeMod.NoteOn();
			if (ArpeggioMod.Enabled) ArpeggioMod.NoteOn();
			if (PitchMod.Enabled) PitchMod.NoteOn();
			if (DutyMod.Enabled) DutyMod.NoteOn();

			if (ArpeggioEffect.Enabled) {
				ArpeggioEffect.NoteOn();
				arpeggioTable[1] = arpeggioTable[2] = 0;
				arpeggioCounter = 0;
			}
			if (NoteUpEffect.Enabled) NoteUpEffect.NoteOn();
			if (NoteDownEffect.Enabled) NoteDownEffect.NoteOn();
			if (VolumeSlideEffect.Enabled) {
				VolumeSlideEffect.NoteOn();
				volumeSlide = 0;
			}
			if (PitchSlideEffect.Enabled) {
				PitchSlideEffect.NoteOn();
				pitchSlide = 0;
			}
			if (TremoloEffect.Enabled) {
				TremoloEffect.NoteOn();
				tremoloDepth = 0;
				tremoloStep = 0;
				tremoloCounter = 0;
			}
			if (VibratoEffect.Enabled) {
				VibratoEffect.NoteOn();
				vibratoDepth = 0;
				vibratoStep = 0;
				vibratoCounter = 0;
			}
			if (VolumeEffect.Enabled) VolumeEffect.NoteOn();
			if (PitchEffect.Enabled) PitchEffect.NoteOn();
			if (DutyEffect.Enabled) DutyEffect.NoteOn(); 

			pitchSlide = 0;
			pitchSlideSpeed = 0;
			pitchSlideTarget = 0;

			muted = false;
			volume = velocity >> 3;
			currentDuty = DutyMod.Value;

			AdvanceFrame();
		}

		public override void NoteOff (byte note, byte velocity) {
			base.NoteOff(note, velocity);
		
			if (VolumeMod.Enabled) VolumeMod.NoteOff();
			if (ArpeggioMod.Enabled) ArpeggioMod.NoteOff();
			if (PitchMod.Enabled) PitchMod.NoteOff();
			if (DutyMod.Enabled) DutyMod.NoteOff();
		}

		public void AdvanceFrame () {
//			Console.WriteLine("\n(Master) New Frame");

			#region Volume

			if (VolumeMod.Enabled && VolumeMod.AdvanceFrame())
				volume = VolumeMod.Value;

			if (VolumeEffect.Enabled && VolumeEffect.AdvanceFrame())
				volume = VolumeEffect.Value;

			currentVolume = volume;

			if (VolumeSlideEffect.Enabled) {
//				Console.WriteLine("(Master) VolumeSlideEffect");
				if (VolumeSlideEffect.AdvanceFrame()) {
//					Console.WriteLine("(Master) VolumeSlideEffect Key, shift : {0}", VolumeSlideEffect.Value);
				}

				currentVolume += volumeSlide;
				volumeSlide += VolumeSlideEffect.Value;
//				Console.WriteLine("(Master) VolumeSlideEffect Exit, volumeSlide : {0}", volumeSlide);
			}

			if (TremoloEffect.Enabled) {
//				Console.WriteLine("(Master) TremoloEffect");
				if (TremoloEffect.AdvanceFrame()) {
					tremoloStep = TremoloEffect.Value >> 4;
					tremoloDepth = (TremoloEffect.Value & 0x0F) << 6;
//					Console.WriteLine("(Master) TremoloEffect Key, tremoloStep : {0}, tremoloDepth : {1}", tremoloStep, tremoloDepth);
				}

				currentVolume -= TremoloTable[tremoloDepth | tremoloCounter];
//				Console.WriteLine("(Master) TremoloEffect Exit, tremoloCounter : {1}, currentVolume : {0}", currentVolume, tremoloCounter);
				if ((tremoloCounter += tremoloStep) >= 64) tremoloCounter = 0;
			}

			muted = currentVolume <= 0;
//			if (muted) Console.WriteLine("(Master) Muted, currentVolumn < 0");
			if (currentVolume > Volm2GainTableMod) currentVolume = Volm2GainTableMod;

			#endregion

			#region Note 

			currentNote = note;

			if (ArpeggioMod.Enabled) {
				ArpeggioMod.AdvanceFrame();

				currentNote += ArpeggioMod.Value;
			}

			if (ArpeggioEffect.Enabled) {
//				Console.WriteLine("(Master) ArpeggioEffect");
				if (ArpeggioEffect.AdvanceFrame()) {
					arpeggioTable[1] = ArpeggioEffect.Value >> 4;
					arpeggioTable[2] = ArpeggioEffect.Value & 0x0F;
//					Console.WriteLine("(Master) ArpeggioEffect, Key 0 : {0} : {1}", arpeggioTable[1], arpeggioTable[2]);
				}

				currentNote += arpeggioTable[arpeggioCounter];
//				Console.WriteLine("(Master) ArpeggioEffect Exit, arpeggioCounter : {1}, note : {0}", note, arpeggioCounter);
				if ((++arpeggioCounter) >= 3) arpeggioCounter = 0;
			}

			if (currentNote > Note2PitcTableMod) currentPitch = Note2PitcTable[Note2PitcTableMod];
			else if (currentNote < 0) currentPitch = Note2PitcTable[0];
			else currentPitch = Note2PitcTable[currentNote];

			#endregion

			#region Pitch

			if (PitchMod.Enabled) {
				PitchMod.AdvanceFrame();

				currentPitch += PitchMod.Value;
			}

			if (PitchEffect.Enabled) {
				PitchEffect.AdvanceFrame();

				currentPitch += PitchEffect.Value;

//				Console.WriteLine("(Master) PitchEffect Exit, currentPitch : {0}", currentPitch);
			}

			if (PitchSlideEffect.Enabled) {
//				Console.WriteLine("(Master) PitchSlideEffect");
				if (PitchSlideEffect.AdvanceFrame()) {
					pitchSlideSpeed = PitchSlideEffect.Value;
					if (pitchSlideSpeed > 0)
						pitchSlideTarget = Pitc2StepTableMod - currentPitch;
					else if (pitchSlideSpeed < 0)
						pitchSlideTarget = currentPitch - Pitc2StepTableMod;
//					Console.WriteLine("(Master) PitchSlideEffect Key, pitchSlideSpeed : {0}, pitchSlideTarget : {1}", pitchSlideSpeed, pitchSlideTarget);
				}
			}

			if (NoteUpEffect.Enabled) {
//				Console.WriteLine("(Master) NoteUpEffect");
				if (NoteUpEffect.AdvanceFrame()) {
					pitchSlideSpeed = NoteUpEffect.Value >> 4;
					pitchSlideTarget = Note2PitcTable[currentNote + (NoteUpEffect.Value & 0x0F)] - currentPitch;
//					Console.WriteLine("(Master) NoteUpEffect Key, pitchSlideSpeed : {0}, pitchSlideTarget : {1}", pitchSlideSpeed, pitchSlideTarget);
				}
			}

			if (NoteDownEffect.Enabled) {
//				Console.WriteLine("(Master) NoteDownEffect");
				if (NoteDownEffect.AdvanceFrame()) {
					pitchSlideSpeed = NoteDownEffect.Value >> 4;
					pitchSlideTarget = Note2PitcTable[currentNote - (NoteDownEffect.Value & 0x0F)] - currentPitch;
//					Console.WriteLine("(Master) NoteUpEffect Key, pitchSlideSpeed : {0}, pitchSlideTarget : {1}", pitchSlideSpeed, pitchSlideTarget);
				}
			}

			currentPitch += pitchSlide;
			if (pitchSlideTarget > 0) {
				pitchSlide += pitchSlideSpeed;
				if (pitchSlide > pitchSlideTarget) pitchSlide = pitchSlideTarget; 
			} else if (pitchSlideTarget < 0) {
				pitchSlide -= pitchSlideSpeed;
				if (pitchSlide < pitchSlideTarget) pitchSlide = pitchSlideTarget;
			} 

			if (VibratoEffect.Enabled) {
//				Console.WriteLine("(Master) VibratoEffect");
				if (VibratoEffect.AdvanceFrame()) {
					vibratoStep = VibratoEffect.Value >> 4;
					vibratoDepth = (VibratoEffect.Value & 0x0F) << 6;
//					Console.WriteLine("(Master) VibratoEffect Key, vibratoStep : {0}, vibratoDepth : {1}", vibratoStep, vibratoDepth);
				}

				currentPitch += VibratoTable[vibratoDepth | vibratoCounter];
//				Console.WriteLine("(Master) VibratoEffect Exit, vibratoCounter : {1}, currentPitch : {0}", currentPitch, vibratoCounter);
				if ((vibratoCounter += vibratoStep) >= 64) vibratoCounter = 0;
			}

			if (!muted) muted = currentPitch <= 0;
			if (currentPitch > Pitc2StepTableMod) currentPitch = Pitc2StepTableMod;

			#endregion

			#region Duty

			if (DutyMod.Enabled && DutyMod.AdvanceFrame())
				currentDuty = DutyMod.Value << 3;

			if (DutyEffect.Enabled && DutyEffect.AdvanceFrame())
				currentDuty = DutyEffect.Value << 3;

			#endregion

//			Console.WriteLine("(Master) Consolidated, currentVolume : {0} currentNote : {1} currentPitch : {2}", currentVolume, currentNote, currentPitch);

		}
	}
}

