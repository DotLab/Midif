namespace Midif.Synth.FamiTracker {
	public class ModSequence {
		public bool IsActive { get { return isActive; } }

		public bool Enabled;

		public int Value;

		public int[] ValueTable;

		public int LoopPoint = -1, ReleasePoint = -1;
		bool loopEnabled, releaseEnabled;

		bool isActive, isOn;
		int length, frame;


		public ModSequence (int value = 0) {
			Value = value;
		}

		public void Parse (string text) {
			var segs = text.Trim().Split(new [] { ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
			var list = new System.Collections.Generic.List<int>();

			for (int i = 0; i < segs.Length; i++)
				if (segs[i] == "|" && i < segs.Length - 1)
					LoopPoint = list.Count;
				else if (segs[i] == "/" && i < segs.Length - 1)
					ReleasePoint = list.Count;
				else list.Add((int)new System.ComponentModel.Int32Converter().ConvertFromString(segs[i]));

			ValueTable = list.ToArray();

			Init();
		}

		public void Init () {
			Enabled = ValueTable != null && ValueTable.Length > 0;

			if (Enabled) {
				length = ValueTable.Length;

				releaseEnabled = 0 <= ReleasePoint && ReleasePoint < length;
				loopEnabled = 0 <= LoopPoint && LoopPoint < length &&
				(!releaseEnabled || LoopPoint < ReleasePoint);

				if (!releaseEnabled)
					ReleasePoint = length - 1;
			}

			frame = int.MaxValue;
		}

		public void NoteOn () {
			Value = 0;

			isActive = isOn = true;
		
			frame = 0;
		}

		public void NoteOff () {
			isOn = false;
		}

		public bool AdvanceFrame () {
			if (isActive) {
				Value = ValueTable[frame++];
				if (isOn && frame > ReleasePoint) {
					if (loopEnabled)
						frame = LoopPoint;
					else if (releaseEnabled)
						frame = ReleasePoint;
				}
				isActive = frame < length;
				return true;
			}

			return false;
		}
	}
}

