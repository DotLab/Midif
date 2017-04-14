using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Midif.File.Sfz {
	[System.Serializable]
	public class SfzFile {
		public SfzRegion[] Regions;

		#region Constructor

		static readonly Regex PrepareTextRegex = 
			new Regex(@"(?:[ \n\r\t]|\/.*\n)*([a-z0-9_]+=|<[a-z]+>)");

		public SfzFile (string text) {
			text = PrepareTextRegex.Replace(text, match => string.Format("\n{0}", match.Groups[1]));

			var list = new List<SfzRegion>();
			SfzRegion master = null, region = null;
			var lines = text.Trim().Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines) {
				var lower = line.ToLower();
				if (lower == "<group>") {
					// Start of a new Group
					master = new SfzRegion();

					// Maybe End of a old Region
					if (region != null && region.Validate())
						list.Add(region);
					region = null;
				} else if (lower == "<region>") {
					// Maybe End of a old Region
					if (region != null && region.Validate())
						list.Add(region);

					// Start of a new Region
					region = new SfzRegion();

					// Apply master Group if there's one
					if (master != null)
						foreach (var field in region.GetType().GetFields())
							field.SetValue(region, field.GetValue(master));
				} else if (line.Contains("=")) {
					if (region != null)
						region.SetParam(line);
					else if (master != null)
						master.SetParam(line);
					else
						// Ignore unknown token without throwing exception;
						UnityEngine.Debug.Log(new FileFormatException("SfzFile.line", line, "<group>|<region>|[opcode]=[value]"));
					//						throw new FileFormatException("SfzFile.line", line, "<group>|<region>|[opcode]=[value]");
				}
			}

			// The last Region
			if (region.Validate())
				list.Add(region);

			Regions = list.ToArray();
		}

		#endregion

		public override string ToString () {
			return string.Format("[SfzFile: Regions.Length={0}]", Regions.Length);
		}
	}
}