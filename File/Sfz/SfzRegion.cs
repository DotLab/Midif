using System;

namespace Midif.File.Sfz {
	public enum SfzOffMode {
		Fast,
		Normal
	}

	public enum SfzLoopMode {
		NoLoop,
		OneShot,
		Continuous,
		Sustain,
	}

	public enum SfzFilterType {
		None,
		OnePoleLowPass,
		OnePoleHighPass,
		BiquadLowPass,
		BiquadHighPass,
		BiquadBandPass,
		BiquadBandReject,
	}

	[Serializable]
	public class SfzRegion {
		// Sample Definition
		public string sample;

		// Input Controls
		public byte loChan;
		public byte hiChan = 15;
		public byte loKey;
		public byte hiKey = 127;
		public byte loVel;
		public byte hiVel = 127;
		public short loBend = -8192;
		public short hiBend = 8192;
		public byte loChanAft;
		public byte hiChanAft = 127;
		public byte loPolyAft;
		public byte hiPolyAft = 127;
		public int group;
		public int offBy;
		public SfzOffMode offMode = SfzOffMode.Fast;

		// Sample Player
		public float delay;
		public int offset;
		public int end = -1;
		public int count = -1;
		public SfzLoopMode loopMode = SfzLoopMode.NoLoop;
		public int loopStart = -1;
		public int loopEnd = -1;

		// Pitch
		public short transpose;
		public short tune;
		public short pitchKeyCenter = 60;
		public short pitchKeyTrack = 100;
		public short pitchVelTrack;

		// Pitch
		public bool pitchEgEnabled;
		public float pitchEgDelay;
		public float pitchEgStart;
		public float pitchEgAttack;
		public float pitchEgHold;
		public float pitchEgDecay;
		public float pitchEgSustain = 100;
		public float pitchEgRelease;
		public short pitchEgDepth;
		public float pitchEgVel2Delay;
		public float pitchEgVel2Attack;
		public float pitchEgVel2Hold;
		public float pitchEgVel2Decay;
		public float pitchEgVel2Sustain;
		public float pitchEgVel2Release;
		public short pitchEgVel2Depth;

		// Pitch Lfo
		public bool pitchLfoEnabled;
		public float pitchLfoDelay;
		public float pitchLfoFrequency;
		public short pitchLfoDepth;

		// Filter
		public bool filterEnabled;
		public SfzFilterType filterType = SfzFilterType.BiquadLowPass;
		public float cutOff;
		public float resonance;
		public short filterKeyCenter = 60;
		public short filterKeyTrack;
		public short filterVelTrack;

		// Filter Eg
		public bool filterEgEnabled;
		public float filterEgDelay;
		public float filterEgStart;
		public float filterEgAttack;
		public float filterEgHold;
		public float filterEgDecay;
		public float filterEgSustain = 100;
		public float filterEgRelease;
		public short filterEgDepth;
		public float filterEgVel2Delay;
		public float filterEgVel2Attack;
		public float filterEgVel2Hold;
		public float filterEgVel2Decay;
		public float filterEgVel2Sustain;
		public float filterEgVel2Release;
		public short filterEgVel2Depth;

		// Filter Lfo
		public bool filterLfoEnabled;
		public float filterLfoDelay;
		public float filterLfoFrequency;
		public float filterLfoDepth;

		// Amplifier
		public bool ampEnabled;
		public float volume;
		public float pan;
		public byte ampKeyCenter = 60;
		public float ampKeyTrack;
		public float ampVelTrack = 1;

		// Amplifier Eg
		public bool ampEgEnabled;
		public float ampEgDelay;
		public float ampEgStart;
		public float ampEgAttack;
		public float ampEgHold;
		public float ampEgDecay;
		public float ampEgSustain = 100;
		public float ampEgRelease;
		public float ampEgVel2Delay;
		public float ampEgVel2Attack;
		public float ampEgVel2Hold;
		public float ampEgVel2Decay;
		public float ampEgVel2Sustain;
		public float ampEgVel2Release;

		// Amplifier Lfo
		public bool ampLfoEnabled;
		public float ampLfoDelay;
		public float ampLfoFrequency;
		public float ampLfoDepth;


		public SfzRegion () {
		}

		public SfzRegion (SfzRegion master) {
			if (master == null) return;

			// Sample Definition
			sample = master.sample;

			// Input Controls
			loChan = master.loChan;
			hiChan = master.hiChan;
			loKey = master.loKey;
			hiKey = master.hiKey;
			loVel = master.loVel;
			hiVel = master.hiVel;
			loBend = master.loBend;
			hiBend = master.hiBend;
			loChanAft = master.loChanAft;
			hiChanAft = master.hiChanAft;
			loPolyAft = master.loPolyAft;
			hiPolyAft = master.hiPolyAft;
			group = master.group;
			offBy = master.offBy;
			offMode = master.offMode;

			// Sample Player
			delay = master.delay;
			offset = master.offset;
			end = master.end;
			count = master.count;
			loopMode = master.loopMode;
			loopStart = master.loopStart;
			loopEnd = master.loopEnd;

			// Pitch
			transpose = master.transpose;
			tune = master.tune;
			pitchKeyCenter = master.pitchKeyCenter;
			pitchKeyTrack = master.pitchKeyTrack;
			pitchVelTrack = master.pitchVelTrack;

			pitchEgEnabled = master.pitchEgEnabled;
			pitchEgDelay = master.pitchEgDelay;
			pitchEgStart = master.pitchEgStart;
			pitchEgAttack = master.pitchEgAttack;
			pitchEgHold = master.pitchEgHold;
			pitchEgDecay = master.pitchEgDecay;
			pitchEgSustain = master.pitchEgSustain;
			pitchEgRelease = master.pitchEgRelease;
			pitchEgDepth = master.pitchEgDepth;
			pitchEgVel2Delay = master.pitchEgVel2Delay;
			pitchEgVel2Attack = master.pitchEgVel2Attack;
			pitchEgVel2Hold = master.pitchEgVel2Hold;
			pitchEgVel2Decay = master.pitchEgVel2Decay;
			pitchEgVel2Sustain = master.pitchEgVel2Sustain;
			pitchEgVel2Release = master.pitchEgVel2Release;
			pitchEgVel2Depth = master.pitchEgVel2Depth;

			// Pitch Lfo
			pitchLfoEnabled = master.pitchLfoEnabled;
			pitchLfoDelay = master.pitchLfoDelay;
			pitchLfoFrequency = master.pitchLfoFrequency;
			pitchLfoDepth = master.pitchLfoDepth;

			// Filter
			filterEnabled = master.filterEnabled;
			filterType = master.filterType;
			cutOff = master.cutOff;
			resonance = master.resonance;
			filterKeyTrack = master.filterKeyTrack;
			filterKeyCenter = master.filterKeyCenter;
			filterVelTrack = master.filterVelTrack;

			// Filter Eg
			filterEgEnabled = master.filterEgEnabled;
			filterEgDelay = master.filterEgDelay;
			filterEgStart = master.filterEgStart;
			filterEgAttack = master.filterEgAttack;
			filterEgHold = master.filterEgHold;
			filterEgDecay = master.filterEgDecay;
			filterEgSustain = master.filterEgSustain;
			filterEgRelease = master.filterEgRelease;
			filterEgDepth = master.filterEgDepth;
			filterEgVel2Delay = master.filterEgVel2Delay;
			filterEgVel2Attack = master.filterEgVel2Attack;
			filterEgVel2Hold = master.filterEgVel2Hold;
			filterEgVel2Decay = master.filterEgVel2Decay;
			filterEgVel2Sustain = master.filterEgVel2Sustain;
			filterEgVel2Release = master.filterEgVel2Release;
			filterEgVel2Depth = master.filterEgVel2Depth;

			// Filter Lfo
			filterLfoEnabled = master.filterLfoEnabled;
			filterLfoDelay = master.filterLfoDelay;
			filterLfoFrequency = master.filterLfoFrequency;
			filterLfoDepth = master.filterLfoDepth;

			// Amplifier
			ampEnabled = master.ampEnabled;
			volume = master.volume;
			pan = master.pan;
			ampKeyTrack = master.ampKeyTrack;
			ampKeyCenter = master.ampKeyCenter;
			ampVelTrack = master.ampVelTrack;

			// Amplifier Eg
			ampEgEnabled = master.ampEgEnabled;
			ampEgDelay = master.ampEgDelay;
			ampEgStart = master.ampEgStart;
			ampEgAttack = master.ampEgAttack;
			ampEgHold = master.ampEgHold;
			ampEgDecay = master.ampEgDecay;
			ampEgSustain = master.ampEgSustain;
			ampEgRelease = master.ampEgRelease;
			ampEgVel2Delay = master.ampEgVel2Delay;
			ampEgVel2Attack = master.ampEgVel2Attack;
			ampEgVel2Hold = master.ampEgVel2Hold;
			ampEgVel2Decay = master.ampEgVel2Decay;
			ampEgVel2Sustain = master.ampEgVel2Sustain;
			ampEgVel2Release = master.ampEgVel2Release;

			// Amplifier Lfo
			ampLfoEnabled = master.ampLfoEnabled;
			ampLfoDelay = master.ampLfoDelay;
			ampLfoFrequency = master.ampLfoFrequency;
			ampLfoDepth = master.ampLfoDepth;
		}
	}
}

