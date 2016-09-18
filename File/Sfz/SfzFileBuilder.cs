using System;
using System.IO;
using System.Collections.Generic;

namespace Midif.File.Sfz {
	public static class SfzFileBuilder {
		public static SfzFile Build (byte[] bytes) {
			using (var stream = new MemoryStream(bytes)) {
				return Build(stream);
			}
		}

		public static SfzFile Build (Stream stream) {
			var file = new SfzFile();

			var list = new List<SfzRegion>();
			using (var reader = new StreamReader(stream)) {
				SfzRegion master = null, region = null;
				while (!reader.EndOfStream) {
					var line = reader.ReadLine().Trim();
					if (string.IsNullOrEmpty(line) || line.Substring(0, 2) == "//") continue;

					var elements = line.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var element in elements) {
						switch (element.ToLower()) {
						case "<group>":
							master = new SfzRegion();
							region = null;
							break;
						case "<region>":
							region = new SfzRegion(master);
							list.Add(region);
							break;
						default:
							if (region != null)
								SetParam(region, element);
							else if (master != null)
								SetParam(master, element);
							else
								throw new Exception("Unexpected param before any region.");
							break;
						}
					}
				}
			}
			file.Regions = list.ToArray();

			return file;
		}

		static void SetParam (SfzRegion region, string line) {
			var param = line.Split('=');
			if (param.Length != 2)
				throw new Exception(string.Format("Unexpected param.Length : {0}, '2' expected.", param.Length));
			var opcode = param[0].Trim();
			var value = param[1].Trim();

			switch (opcode) {
			case "sample":
				region.sample = value;
				break;
			case "lochan":
				region.loChan = (byte)(int.Parse(value) - 1);
				break;
			case "hichan":
				region.hiChan = (byte)(int.Parse(value) - 1);
				break;
			case "lokey":
				region.loKey = ParseNote(value);
				break;
			case "hikey":
				region.hiKey = ParseNote(value);
				break;
			case "key":
				region.loKey = ParseNote(value);
				region.hiKey = region.loKey;
				region.pitchKeyCenter = region.loKey;
				break;
			case "lovel":
				region.loVel = byte.Parse(value);
				break;
			case "hivel":
				region.hiVel = byte.Parse(value);
				break;
			case "lobend":
				region.loBend = short.Parse(value);
				break;
			case "hibend":
				region.hiBend = short.Parse(value);
				break;
			case "lochanaft":
				region.loChanAft = byte.Parse(value);
				break;
			case "hichanaft":
				region.hiChanAft = byte.Parse(value);
				break;
			case "lopolyaft":
				region.loPolyAft = byte.Parse(value);
				break;
			case "hipolyaft":
				region.hiPolyAft = byte.Parse(value);
				break;
			case "group":
				region.group = int.Parse(value);
				break;
			case "off_by":
				region.offBy = int.Parse(value);
				break;
			case "off_mode":
				switch (value) {
				case "fast":
					region.offMode = SfzOffMode.Fast;
					break;
				case "normal":
					region.offMode = SfzOffMode.Normal;
					break;
				}
				break;
			case "delay":
				region.delay = float.Parse(value);
				break;
			case "offset":
				region.offset = int.Parse(value);
				break;
			case "end":
				region.end = int.Parse(value);
				break;
			case "count":
				region.count = int.Parse(value);
				region.loopMode = SfzLoopMode.OneShot;
				break;
			case "loop_mode":
				switch (value) {
				case "no_loop":
					region.loopMode = SfzLoopMode.NoLoop;
					break;
				case "one_shot":
					region.loopMode = SfzLoopMode.OneShot;
					break;
				case "loop_continuous":
					region.loopMode = SfzLoopMode.Continuous;
					break;
				case "loop_sustain":
					region.loopMode = SfzLoopMode.Sustain;
					break;
				}
				break;
			case "loop_start":
				region.loopStart = int.Parse(value);
				break;
			case "loop_end":
				region.loopEnd = int.Parse(value);
				break;
			case "transpose":
				region.transpose = short.Parse(value);
				break;
			case "tune":
				region.tune = short.Parse(value);
				break;
			case "pitch_keycenter":
				region.pitchKeyCenter = ParseNote(value);
				break;
			case "pitch_keytrack":
				region.pitchKeyTrack = short.Parse(value);
				break;
			case "pitch_veltrack":
				region.pitchVelTrack = short.Parse(value);
				break;
			case "pitcheg_delay":
				region.pitchEgEnabled = true;
				region.pitchEgDelay = float.Parse(value);
				break;
			case "pitcheg_start":
				region.pitchEgEnabled = true;
				region.pitchEgStart = float.Parse(value);
				break;
			case "pitcheg_attack":
				region.pitchEgEnabled = true;
				region.pitchEgAttack = float.Parse(value);
				break;
			case "pitcheg_hold":
				region.pitchEgEnabled = true;
				region.pitchEgHold = float.Parse(value);
				break;
			case "pitcheg_decay":
				region.pitchEgEnabled = true;
				region.pitchEgDecay = float.Parse(value);
				break;
			case "pitcheg_sustain":
				region.pitchEgEnabled = true;
				region.pitchEgSustain = float.Parse(value);
				break;
			case "pitcheg_release":
				region.pitchEgEnabled = true;
				region.pitchEgRelease = float.Parse(value);
				break;
			case "pitcheg_depth":
				region.pitchEgEnabled = true;
				region.pitchEgDepth = short.Parse(value);
				break;
			case "pitcheg_vel2delay":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Delay = float.Parse(value);
				break;
			case "pitcheg_vel2attack":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Attack = float.Parse(value);
				break;
			case "pitcheg_vel2hold":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Hold = float.Parse(value);
				break;
			case "pitcheg_vel2decay":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Decay = float.Parse(value);
				break;
			case "pitcheg_vel2sustain":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Sustain = float.Parse(value);
				break;
			case "pitcheg_vel2release":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Release = float.Parse(value);
				break;
			case "pitcheg_vel2depth":
				region.pitchEgEnabled = true;
				region.pitchEgVel2Depth = short.Parse(value);
				break;
			case "pitchlfo_delay":
				region.pitchLfoEnabled = true;
				region.pitchLfoDelay = float.Parse(value);
				break;
			case "pitchlfo_freq":
				region.pitchLfoEnabled = true;
				region.pitchLfoFrequency = float.Parse(value);
				break;
			case "pitchlfo_depth":
				region.pitchLfoEnabled = true;
				region.pitchLfoDepth = short.Parse(value);
				break;
			case "fil_type":
				region.pitchLfoEnabled = true;
				switch (value) {
				case "lpf_1p":
					region.filterType = SfzFilterType.OnePoleLowPass;
					break;
				case "hpf_1p":
					region.filterType = SfzFilterType.OnePoleHighPass; // Unsupported
					break;
				case "lpf_2p":
					region.filterType = SfzFilterType.BiquadLowPass;
					break;
				case "hpf_2p":
					region.filterType = SfzFilterType.BiquadHighPass;
					break;
				case "bpf_2p":
					region.filterType = SfzFilterType.BiquadBandPass; // Unsupported
					break;
				case "brf_2p":
					region.filterType = SfzFilterType.BiquadBandReject; // Unsupported
					break;
				}
				break;
			case "cutoff":
				region.pitchLfoEnabled = true;
				region.cutOff = float.Parse(value);
				break;
			case "resonance":
				region.pitchLfoEnabled = true;
				region.resonance = float.Parse(value);
				break;
			case "fil_keytrack":
				region.filterEnabled = true;
				region.filterKeyTrack = short.Parse(value);
				break;
			case "fil_keycenter":
				region.filterEnabled = true;
				region.filterKeyCenter = byte.Parse(value);
				break;
			case "fil_veltrack":
				region.filterEnabled = true;
				region.filterVelTrack = short.Parse(value);
				break;
			case "fileg_delay":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgDelay = float.Parse(value);
				break;
			case "fileg_start":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgStart = float.Parse(value);
				break;
			case "fileg_attack":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgAttack = float.Parse(value);
				break;
			case "fileg_hold":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgHold = float.Parse(value);
				break;
			case "fileg_decay":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgDecay = float.Parse(value);
				break;
			case "fileg_sustain":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgSustain = float.Parse(value);
				break;
			case "fileg_release":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgRelease = float.Parse(value);
				break;
			case "fileg_depth":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgDepth = short.Parse(value);
				break;
			case "fileg_vel2delay":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Delay = float.Parse(value);
				break;
			case "fileg_vel2attack":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Attack = float.Parse(value);
				break;
			case "fileg_vel2hold":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Hold = float.Parse(value);
				break;
			case "fileg_vel2decay":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Decay = float.Parse(value);
				break;
			case "fileg_vel2sustain":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Sustain = float.Parse(value);
				break;
			case "fileg_vel2release":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Release = float.Parse(value);
				break;
			case "fileg_vel2depth":
				region.filterEnabled = true;
				region.filterEgEnabled = true;
				region.filterEgVel2Depth = short.Parse(value);
				break;
			case "fillfo_delay":
				region.filterEnabled = true;
				region.filterLfoEnabled = true;
				region.filterLfoDelay = float.Parse(value);
				break;
			case "fillfo_freq":
				region.filterEnabled = true;
				region.filterLfoEnabled = true;
				region.filterLfoFrequency = float.Parse(value);
				break;
			case "fillfo_depth":
				region.filterEnabled = true;
				region.filterLfoEnabled = true;
				region.filterLfoDepth = float.Parse(value);
				break;
			case "volume":
				region.ampEnabled = true;
				region.volume = float.Parse(value);
				break;
			case "pan":
				region.ampEnabled = true;
				region.pan = float.Parse(value);
				break;
			case "amp_keytrack":
				region.ampEnabled = true;
				region.ampKeyTrack = float.Parse(value);
				break;
			case "amp_keycenter":
				region.ampEnabled = true;
				region.ampKeyCenter = byte.Parse(value);
				break;
			case "amp_veltrack":
				region.ampEnabled = true;
				region.ampVelTrack = float.Parse(value);
				break;
			case "ampeg_delay":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgDelay = float.Parse(value);
				break;
			case "ampeg_start":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgStart = float.Parse(value);
				break;
			case "ampeg_attack":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgAttack = float.Parse(value);
				break;
			case "ampeg_hold":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgHold = float.Parse(value);
				break;
			case "ampeg_decay":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgDecay = float.Parse(value);
				break;
			case "ampeg_sustain":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgSustain = float.Parse(value);
				break;
			case "ampeg_release":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgRelease = float.Parse(value);
				break;
			case "ampeg_vel2delay":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgVel2Delay = float.Parse(value);
				break;
			case "ampeg_vel2attack":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgVel2Attack = float.Parse(value);
				break;
			case "ampeg_vel2hold":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgVel2Hold = float.Parse(value);
				break;
			case "ampeg_vel2decay":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgVel2Decay = float.Parse(value);
				break;
			case "ampeg_vel2sustain":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgVel2Sustain = float.Parse(value);
				break;
			case "ampeg_vel2release":
				region.ampEnabled = true;
				region.ampEgEnabled = true;
				region.ampEgVel2Release = float.Parse(value);
				break;
			case "amplfo_delay":
				region.ampEnabled = true;
				region.ampLfoEnabled = true;
				region.ampLfoDelay = float.Parse(value);
				break;
			case "amplfo_freq":
				region.ampEnabled = true;
				region.ampLfoEnabled = true;
				region.ampLfoFrequency = float.Parse(value);
				break;
			case "amplfo_depth":
				region.ampEnabled = true;
				region.ampLfoEnabled = true;
				region.ampLfoDepth = float.Parse(value);
				break;
			}
		}

		static byte ParseNote (string name) {
			int value, i;

			if (int.TryParse(name, out value))
				return (byte)value;
			
			const string notes = "cdefgab";
			int[] noteValues = { 0, 2, 4, 5, 7, 9, 11 };
			name = name.Trim().ToLower();

			for (i = 0; i < name.Length; i++) {
				int index = notes.IndexOf(name[i]);
				if (index >= 0) {
					value = noteValues[index];
					i++;
					break;
				}
			}

			while (i < name.Length) {
				if (name[i] == '#') {
					value--;
					i++;
					break;
				}

				if (name[i] == 'b') {
					value--;
					i++;
					break;
				}

				i++;
			}

			var digit = string.Empty;
			while (i < name.Length) {
				if (char.IsDigit(name[i])) {
					digit += name[i];
					i++;
				} else
					break;
			}

			if (digit.Equals(string.Empty))
				digit = "0";
			return (byte)((int.Parse(digit) + 1) * 12 + value);
		}
	}
}

