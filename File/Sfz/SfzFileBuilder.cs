using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Midif.File.Sfz {
	public static class SfzFileBuilder {
		public static SfzFile Build (byte[] bytes) {
			return Build(Encoding.UTF8.GetString(bytes));
		}

		public static SfzFile Build (Stream stream) {
			using (var reader = new StreamReader(stream)) {
				return Build(reader.ReadToEnd());
			}
		}

		static Regex PrepareTextRegex = new Regex(@"(?:[ \n\r\t]|\/.*\n)*([a-z0-9_]+=|<[a-z]+>)");

		public static SfzFile Build (string text) {
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
						SetParam(region, line);
					else if (master != null)
						SetParam(master, line);
					else
						// Ignore unknown token without throwing exception;
						DebugConsole.WriteLine(new FileFormatException("SfzFile.line", line, "<group>|<region>|[opcode]=[value]"));
//						throw new FileFormatException("SfzFile.line", line, "<group>|<region>|[opcode]=[value]");
				}
			}
			// The last Region
			if (region.Validate())
				list.Add(region);

			var file = new SfzFile();
			file.Regions = list.ToArray();

			return file;
		}

		static Regex PrepareOpcadeRegex = new Regex(@"_(.)");

		static void SetParam (SfzRegion region, string line) {
			var division = line.IndexOf('=');
			var opcode = line.Substring(0, division);
			var value = line.Substring(division + 1);

			if (opcode == "key") {
				SetParam(region, "lokey=" + value);
				SetParam(region, "hikey=" + value);
				SetParam(region, "pitch_keycenter=" + value);
				return;
			}

			opcode = PrepareOpcadeRegex.Replace(opcode, match => match.Groups[1].ToString().ToUpper());
			var field = typeof(SfzRegion).GetField(opcode);
			if (field == null) {
				// Ignore unknown opcode without throwing exception;
				DebugConsole.WriteLine(new FileFormatException("SfzFile.line.opcode", opcode, "[sfz opcode]"));
				return;
			}
			var fieldType = field.FieldType;

			object obj;
			if (line.Contains("key") && !line.Contains("track"))
				obj = ParseNote(line);
			else if (fieldType == typeof(String))
				obj = value;
			else if (fieldType.BaseType == typeof(Enum))
				obj = Enum.Parse(fieldType, value);
			else
				obj = fieldType.GetMethod("Parse", new [] { typeof(string) }).Invoke(null, new [] { value });
			// Set the field's Value;
			field.SetValue(region, obj);
			// Set the field's Set Flag;
			typeof(SfzRegion).GetField(opcode + "Set").SetValue(region, true);
		}

		static byte ParseNote (string name) {
			int value, i;

			if (int.TryParse(name, out value))
				return (byte)value;
			
			const string notes = "cdefgab";
			int[] noteValues = { 0, 2, 4, 5, 7, 9, 11 };
			name = name.ToLower();

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

