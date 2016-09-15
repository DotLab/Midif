using System;

using System.Collections.Generic;

using Midif.File.Wave;

using UnityEngine;

namespace Midif.Synth.Sfz {
	public static class SfzWaveBank {
		const string WavePath = "GM Bank/SAMPLES/";

		static readonly Dictionary<string, WaveFile> Bank = new Dictionary<string, WaveFile>();

		public static WaveFile GetWaveFile (string name) {
			if (Bank.ContainsKey(name))
				return Bank[name];

			var asset = Resources.Load<TextAsset>(WavePath + name);
			var file = WaveFileBuilder.Build(asset.bytes);
			Bank[name] = file;

			return file;
		}
	}
}

