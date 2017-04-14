using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Midif.File.Sfz {
	public enum SfzLoopMode {
		no_loop,
		one_shot,
		loop_continuous,
		loop_sustain,
	}

	public enum SfzFilterType {
		lpf_1p,
		hpf_1p,
		lpf_2p,
		hpf_2p,
		bpf_2p,
		brf_2p,
	}

	[Serializable]
	public class SfzRegion {
		#region Sample Definition

		/// <summary>
		/// <para>this opcode defines which sample file the region will play.</para>
		/// <para>the value of this opcode is the filename of the sample file, including the eXtension. the filename must be stored in the same folder where the definition file is, or specified relatively to it.</para>
		/// <para>if the sample file is not found, the player will ignore the whole region contents.</para>
		/// <para>Long names and names with blank spaces and other special characters (excepting the = character) are allowed in the sample definition.</para>
		/// <para>The sample will play unchanged when a note equal to the pitch_keycenter opcode value iS played. if pitch_keycenter is not defined for the region, sample will play unchanged on note 60 (middle C).</para>
		/// </summary>
		public string sample = null;

		#endregion

		#region Input Controls

		/// <summary>
		/// <para>If incoming notes have a MIDI channel between lochan and hichan, the region will play.</para>
		/// </summary>
		public byte lochan = 1;
		public byte hichan = 16;
		/// <summary>
		/// <para>If a note equal to or higher than lokey AND equal to or lower than hikey is played, the region will play.</para>
		/// <para>lokey and hikey can be entered in either MIDI note numbers (0 to 127) or in MIDI note names (C-1 to G9)</para>
		/// <para>The key opcode sets lokey, hikey and pitch_keycenter to the same note.</para>
		/// </summary>
		public byte lokey = 0;
		public byte hikey = 127;
		/// <summary>
		/// If a note with velocity value equal to or higher than lovel AND equal to or lower than hivel is played, the region will play.
		/// </summary>
		public byte lovel = 0;
		public byte hivel = 127;

		#endregion

		#region Sample Player

		/// <summary>
		/// <para>Region delay time, in seconds.</para>
		/// <para>If a delay value is specified, the region playback will be postponed for the specified time.</para>
		/// <para>If the region receives a note-off message before delay time, the region won't play.</para>
		/// <para>All envelope generators delay stage will start counting after region delay time.</para>
		/// </summary>
		public float delay = 0;
		/// <summary>
		/// <para>The offset used to play the sample, in sample units.</para>
		/// <para>The player will reproduce samples starting with the very first sample in the file, unless offset is specified. It will start playing the file at the offset sample in this case.</para>
		/// </summary>
		public int offset = 0;
		/// <summary>
		/// <para>The endpoint of the sample, in sample units.</para>
		/// <para>The player will reproduce the whole sample if end is not specified.</para>
		/// <para>If end value is -1, the sample will not play. Marking a region end with -1 can be used to use a silent region to turn off other regions by using the group and off_by opcodes.</para>
		/// </summary>
		public int end = 0;
		/// <summary>
		/// <para>The number of times the sample will be played. If this opcode is specified, the sample will restart as many times as defined. Envelope generators will not be retriggered on sample restart.</para>
		/// <para>When this opcode is defined, loopmode is automatically set to one_shot.</para>
		/// </summary>
		public int count = 0;
		/// <summary>
		/// <para>If loop_mode is not specified, each sample will play according to its predefined loop mode. That is, the player will play the sample looped using the first defined loop, if available. If no loops are defined, the wave will play unlooped.</para>
		/// <para>The loop_mode opcode allows playing samples with loops defined in the unlooped mode. The possible values are:</para>
		/// <para>no_loop: no looping will be performed. Sample will play straight from start to end, or until note off, whatever reaches first.</para>
		/// <para>one_shot: sample will play from start to end, ignoring note off. </para>
		/// <para>This mode is engaged automatically if the count opcode is defined.</para>
		/// <para>loop_continuous: once the player reaches sample loop point, the loop will play until note expiration.</para>
		/// <para>loop_sustain: the player will play the loop while the note is held, by keeping it depressed or by using the sustain pedal (CC64). The rest of the sample will play after note release.</para>
		/// </summary>
		public SfzLoopMode loopMode = SfzLoopMode.no_loop;
		/// <summary>
		/// <para>The loop start point, in samples. </para>
		/// <para>If loop_start is not specified and the sample has a loop defined, the sample start point will be used. </para>
		/// <para>If loop_start is specified, it will overwrite the loop start point defined in the sample.</para>
		/// <para>This opcode will not have any effect if loopmode is set to no_loop.</para>
		/// </summary>
		public int loopStart = 0;
		/// <summary>
		/// <para>The loop end point, in samples. This opcode will not have any effect if loopmode is set to no_loop.</para>
		/// <para>If loop_end is not specified and the sample have a loop defined, the sample loop end point will be used. </para>
		/// <para>If loop_end is specified, it will overwrite the loop end point defined in the sample.</para>
		/// </summary>
		public int loopEnd = 0;

		/// <summary>
		/// The transposition value for this region which will be applied to the sample.
		/// </summary>
		public short transpose = 0;
		/// <summary>
		/// The fine tuning for the sample, in cents. Range is Â±1 semitone, from -100 to 100. Only negative values must be prefixed with sign.
		/// </summary>
		public short tune = 0;
		/// <summary>
		/// Root key for the sample.
		/// </summary>
		public short pitchKeycenter = 60;
		/// <summary>
		/// <para>Within the region, this value defines how much the pitch changes with every note. Default value is 100, which means pitch will change one hundred cents (one semitone) per played note.</para>
		/// <para>Setting this value to zero means that all notes in the region will play the same pitch, particularly useful when mapping drum sounds.</para>
		/// </summary>
		public short pitchKeytrack = 100;
		/// <summary>
		/// Pitch velocity tracking, represents how much the pitch changes with incoming note velocity, in cents.
		/// </summary>
		public short pitchVeltrack = 0;

		/// <summary>
		/// Pitch EG delay time, in seconds. This is the time elapsed from note on to the start of the Attack stage.
		/// </summary>
		public float pitchegDelay = 0;
		/// <summary>
		/// Pitch EG start level, in percentage.
		/// </summary>
		public float pitchegStart = 0;
		/// <summary>
		/// Pitch EG attack time, in seconds.
		/// </summary>
		public float pitchegAttack = 0;
		/// <summary>
		/// Pitch EG hold time, in seconds. During the hold stage, EG output will remain at its maximum value.
		/// </summary>
		public float pitchegHold = 0;
		/// <summary>
		/// Pitch EG decay time, in seconds.
		/// </summary>
		public float pitchegDecay = 0;
		/// <summary>
		/// Pitch EG release time (after note release), in seconds.
		/// </summary>
		public float pitchegSustain = 100;
		/// <summary>
		/// Pitch EG release time (after note release), in seconds.
		/// </summary>
		public float pitchegRelease = 0;
		/// <summary>
		/// Depth for the pitch EG, in cents.
		/// </summary>
		public short pitchegDepth = 0;

		/// <summary>
		/// The time before the Pitch LFO starts oscillating, in seconds.
		/// </summary>
		public float pitchlfoDelay = 0;
		/// <summary>
		/// Pitch LFO fade-in effect time.
		/// </summary>
		public float pitchlfoFade = 0;
		/// <summary>
		/// Pitch LFO fade-in effect time.
		/// </summary>
		public float pitchlfoFrequency = 0;
		/// <summary>
		/// Pitch LFO depth, in cents.
		/// </summary>
		public short pitchlfoDepth = 0;

		/// <summary>
		/// <para>Filter type. Avaliable types are:</para>
		/// <para>lpf_1p: one-pole low pass filter (6dB/octave).</para>
		/// <para>hpf_1p: one-pole high pass filter (6dB/octave).</para>
		/// <para>lpf_2p: two-pole low pass filter (12dB/octave).</para>
		/// <para>hpf_2p: two-pole high pass filter (12dB/octave).</para>
		/// <para>bpf_2p: two-pole band pass filter (12dB/octave).</para>
		/// <para>brf_2p: two-pole band rejection filter (12dB/octave).</para>
		/// </summary>
		public SfzFilterType filType = SfzFilterType.lpf_2p;
		/// <summary>
		/// <para>The filter cutoff frequency, in Hertz.</para>
		/// <para>If the cutoff is not specified, the filter will be disabled, with the consequent CPU drop in the player.</para>
		/// </summary>
		public float cutOff = 0;
		/// <summary>
		/// The filter cutoff resonance value, in decibels.
		/// </summary>
		public float resonance = 0;
		/// <summary>
		/// Filter keyboard tracking (change on cutoff for each key) in cents.
		/// </summary>
		public short filKeytrack = 0;
		/// <summary>
		/// Center key for filter keyboard tracking. In this key, the filter keyboard tracking will have no effect.
		/// </summary>
		public short filKeycenter = 60;
		/// <summary>
		/// Filter velocity tracking, represents how much the cutoff changes with incoming note velocity.
		/// </summary>
		public short filVeltrack = 0;

		/// <summary>
		/// Filter EG delay time, in seconds. This is the time elapsed from note on to the start of the Attack stage.
		/// </summary>
		public float filegDelay = 0;
		/// <summary>
		/// Filter EG start level, in percentage.
		/// </summary>
		public float filegStart = 0;
		/// <summary>
		/// Filter EG attack time, in seconds.
		/// </summary>
		public float filegAttack = 0;
		/// <summary>
		/// Filter EG hold time, in seconds. During the hold stage, EG output will remain at its maximum value.
		/// </summary>
		public float filegHold = 0;
		/// <summary>
		/// Filter EG decay time, in seconds.
		/// </summary>
		public float filegDecay = 0;
		/// <summary>
		/// Filter EG sustain level, in percentage.
		/// </summary>
		public float filegSustain = 100;
		/// <summary>
		/// Filter EG release time (after note release), in seconds.
		/// </summary>
		public float filegRelease = 0;
		/// <summary>
		/// Depth for the filter EG, in cents.
		/// </summary>
		public short filegDepth = 0;

		/// <summary>
		/// The time before the filter LFO starts oscillating, in seconds.
		/// </summary>
		public float fillfoDelay = 0;
		/// <summary>
		/// Filter LFO fade-in effect time.
		/// </summary>
		public float fillfoFade = 0;
		/// <summary>
		/// Filter LFO frequency, in hertz.
		/// </summary>
		public float fillfoFrequency = 0;
		/// <summary>
		/// Filter LFO depth, in cents.
		/// </summary>
		public float fillfoDepth = 0;

		/// <summary>
		/// The volume for the region, in decibels.
		/// </summary>
		public float volume = 0;
		/// <summary>
		/// <para>The panoramic position for the region.</para>
		/// <para>If a mono sample is used, pan value defines the position in the stereo image where the sample will be placed. </para>
		/// <para>When a stereo sample is used, the pan value the relative amplitude of one channel respect the other.</para>
		/// <para>A value of zero means centered, negative values move the panoramic to the left, positive to the right.</para>
		/// </summary>
		public float pan = 0;
		/// <summary>
		/// Amplifier keyboard tracking (change in amplitude per key) in dB.
		/// </summary>
		public float ampKeytrack = 0;
		/// <summary>
		/// Center key for amplifier keyboard tracking. In this key, the amplifier keyboard tracking will have no effect.
		/// </summary>
		public byte ampKeycenter = 60;
		/// <summary>
		/// <para>Amplifier velocity tracking, represents how much the amplitude changes with incoming note velocity.</para>
		/// <para>Volume changes with incoming velocity in a concave shape according to the following expression:</para>
		/// <para>Amplitude(dB) = 20 log (127^2 / Velocity^2)</para>
		/// </summary>
		public float ampVeltrack = 100;

		/// <summary>
		/// Amplifier EG delay time, in seconds. This is the time elapsed from note on to the start of the Attack stage.
		/// </summary>
		public float ampegDelay = 0;
		/// <summary>
		/// Amplifier EG start level, in percentage.
		/// </summary>
		public float ampegStart = 0;
		/// <summary>
		/// Amplifier EG attack time, in seconds.
		/// </summary>
		public float ampegAttack;
		/// <summary>
		/// Amplifier EG hold time, in seconds. During the hold stage, EG output will remain at its maximum value.
		/// </summary>
		public float ampegHold = 0;
		/// <summary>
		/// Amplifier EG decay time, in seconds.
		/// </summary>
		public float ampegDecay = 0;
		/// <summary>
		/// Amplifier EG sustain level, in percentage.
		/// </summary>
		public float ampegSustain = 100;
		/// <summary>
		/// Amplifier EG release time (after note release), in seconds.
		/// </summary>
		public float ampegRelease;

		/// <summary>
		/// The time before the Amplifier LFO starts oscillating, in seconds.
		/// </summary>
		public float amplfoDelay = 0;
		/// <summary>
		/// Amplifier LFO fade-in effect time.
		/// </summary>
		public float amplfoFade = 0;
		/// <summary>
		/// Amplifier LFO frequency, in hertz.
		/// </summary>
		public float amplfoFrequency = 0;
		/// <summary>
		/// Amplifier LFO depth, in decibels.
		/// </summary>
		public float amplfoDepth = 0;

		#endregion

		#region Set Flags

		public bool sampleSet = false;

		public bool lochanSet = false;
		public bool hichanSet = false;
		public bool lokeySet = false;
		public bool hikeySet = false;
		public bool lovelSet = false;
		public bool hivelSet = false;

		public bool delaySet = false;
		public bool offsetSet = false;
		public bool endSet = false;
		public bool countSet = false;
		public bool loopModeSet = false;
		public bool loopStartSet = false;
		public bool loopEndSet = false;

		public bool transposeSet = false;
		public bool tuneSet = false;
		public bool pitchKeycenterSet = false;
		public bool pitchKeytrackSet = false;
		public bool pitchVeltrackSet = false;

		public bool pitchegDelaySet = false;
		public bool pitchegStartSet = false;
		public bool pitchegAttackSet = false;
		public bool pitchegHoldSet = false;
		public bool pitchegDecaySet = false;
		public bool pitchegSustainSet = false;
		public bool pitchegReleaseSet = false;
		public bool pitchegDepthSet = false;

		public bool pitchlfoDelaySet = false;
		public bool pitchlfoFadeSet = false;
		public bool pitchlfoFrequencySet = false;
		public bool pitchlfoDepthSet = false;

		public bool filTypeSet = false;
		public bool cutOffSet = false;
		public bool resonanceSet = false;
		public bool filKeytrackSet = false;
		public bool filKeycenterSet = false;
		public bool filVeltrackSet = false;

		public bool filegDelaySet = false;
		public bool filegStartSet = false;
		public bool filegAttackSet = false;
		public bool filegHoldSet = false;
		public bool filegDecaySet = false;
		public bool filegSustainSet = false;
		public bool filegReleaseSet = false;
		public bool filegDepthSet = false;

		public bool fillfoDelaySet = false;
		public bool fillfoFadeSet = false;
		public bool fillfoFrequencySet = false;
		public bool fillfoDepthSet = false;

		public bool volumeSet = false;
		public bool panSet = false;
		public bool ampKeytrackSet = false;
		public bool ampKeycenterSet = false;
		public bool ampVeltrackSet = false;

		public bool ampegDelaySet = false;
		public bool ampegStartSet = false;
		public bool ampegAttackSet = false;
		public bool ampegHoldSet = false;
		public bool ampegDecaySet = false;
		public bool ampegSustainSet = false;
		public bool ampegReleaseSet = false;

		public bool amplfoDelaySet = false;
		public bool amplfoFadeSet = false;
		public bool amplfoFrequencySet = false;
		public bool amplfoDepthSet = false;

		#endregion


		static readonly Regex PrepareOpcadeRegex = new Regex(@"_(.)");

		public void SetParam (string line) {
			var division = line.IndexOf('=');
			var opcode = line.Substring(0, division);
			var value = line.Substring(division + 1);

			if (opcode == "key") {
				SetParam("lokey=" + value);
				SetParam("hikey=" + value);
				SetParam("pitch_keycenter=" + value);
				return;
			}

			opcode = PrepareOpcadeRegex.Replace(opcode, match => match.Groups[1].ToString().ToUpper());
			var field = typeof(SfzRegion).GetField(opcode);
			if (field == null) {
				// Ignore unknown opcode without throwing exception;
				UnityEngine.Debug.Log(new FileFormatException("SfzFile.line.opcode", opcode, "[sfz opcode]"));
				return;
			}
			var fieldType = field.FieldType;

			object obj;
			//			if (line.Contains("key") && !line.Contains("track"))
			//				obj = ParseNote(line);
			if (fieldType == typeof(String))
				obj = value;
			else if (fieldType.BaseType == typeof(Enum))
				obj = Enum.Parse(fieldType, value);
			else {
				obj = fieldType.GetMethod("Parse", new [] { typeof(string) }).Invoke(null, new [] { value });
				//				DebugConsole.WriteLine(double.Parse(value));
			}
			// Set the field's Value;
			field.SetValue(this, obj);
			//			DebugConsole.WriteLine(opcode + " : '" + value + "' | " + obj + " ( " + fieldType);
			// Set the field's Set Flag;
			typeof(SfzRegion).GetField(opcode + "Set").SetValue(this, true);
		}

		//		static byte ParseNote (string name) {
		//			int value, i;
		//
		//			if (int.TryParse(name, out value))
		//				return (byte)value;
		//
		//			const string notes = "cdefgab";
		//			int[] noteValues = { 0, 2, 4, 5, 7, 9, 11 };
		//			name = name.ToLower();
		//
		//			for (i = 0; i < name.Length; i++) {
		//				int index = notes.IndexOf(name[i]);
		//				if (index >= 0) {
		//					value = noteValues[index];
		//					i++;
		//					break;
		//				}
		//			}
		//
		//			while (i < name.Length) {
		//				if (name[i] == '#') {
		//					value--;
		//					i++;
		//					break;
		//				}
		//
		//				if (name[i] == 'b') {
		//					value--;
		//					i++;
		//					break;
		//				}
		//
		//				i++;
		//			}
		//
		//			var digit = string.Empty;
		//			while (i < name.Length) {
		//				if (char.IsDigit(name[i])) {
		//					digit += name[i];
		//					i++;
		//				} else
		//					break;
		//			}
		//
		//			if (digit.Equals(string.Empty))
		//				digit = "0";
		//			return (byte)((int.Parse(digit) + 1) * 12 + value);
		//		}

		public bool Validate () {
			// If the sample file is not found, the player will ignore the whole region contents.
			if (string.IsNullOrEmpty(sample)) return false;

			if (offsetSet) {
				if (!loopStartSet)
					loopStart = offset;
			}

			// If end value is -1, the sample will not play. 
			if (endSet) {
				if (end <= offset) return false;

				if (!loopEndSet)
					loopEnd = end;
			}

			// When 'count' is defined, loopmode is automatically set to one_shot.
			if (countSet) {
				if (count == 0) return false;

				loopMode = SfzLoopMode.one_shot;
				loopModeSet = true;

				// one_shot: sample will play from start to end, ignoring note off. 
				loopStartSet = false;
				loopEndSet = false;
			}

			if (!loopModeSet) {
				if (loopStartSet || loopEndSet) {
					// If loop_start is not specified and the sample has a loop defined, the sample start point will be used. 
					if (!loopStartSet)
						loopStartSet = true;
					// If loop_end is not specified and the sample have a loop defined, the sample loop end point will be used. 
					if (!loopEndSet && endSet)
						loopEndSet = true;

					loopMode = SfzLoopMode.loop_continuous;
					loopModeSet = true;
				}
			} else if (loopMode == SfzLoopMode.one_shot) {
				if (!countSet) {
					count = 1;
					countSet = true;
				}
			} else if (loopMode == SfzLoopMode.no_loop) {
				// These opcodes will not have any effect if loopmode is set to no_loop.
				loopStartSet = false;
				loopEndSet = false;
			}

			return true;
		}

		public override string ToString () {
			return string.Format("[SfzRegion: sample={0}, lokey={1}, hikey={2}, pitchKeycenter={3}, volume={4}]", sample, lokey, hikey, pitchKeycenter, volume);
		}
	}
}

