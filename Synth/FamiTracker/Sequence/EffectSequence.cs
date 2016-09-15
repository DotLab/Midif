namespace Midif.Synth.FamiTracker {
	public class EffectSequence {
		public bool Enabled;

		public int Value;

		public int[] DeltaTable;
		public int[] ValueTable;

		bool isActive;
		int frameCounter, targetFrame;
		int length, counter;

		public void Parse (string text) {
			var segs = text.Trim().Split(new [] { ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
			var list = new System.Collections.Generic.List<int>();
			var deltaList = new System.Collections.Generic.List<int>();

			for (int i = 0; i < segs.Length; i += 2) {
				deltaList.Add((int)new System.ComponentModel.Int32Converter().ConvertFromString(segs[i]));
				list.Add((int)new System.ComponentModel.Int32Converter().ConvertFromString(segs[i + 1]));
			}

			ValueTable = list.ToArray();
			DeltaTable = deltaList.ToArray();

			Init();
		}

		public void Init () {
			Enabled = ValueTable != null && ValueTable.Length > 0;

			if (Enabled)
				length = ValueTable.Length;

			counter = int.MaxValue;
		}

		public void NoteOn () {
			Value = 0;

			isActive = true;

			frameCounter = 0;
			targetFrame = DeltaTable[0];

			counter = 0;
		}

		public bool AdvanceFrame () {
			if (isActive && frameCounter++ >= targetFrame) {
				Value = ValueTable[counter];
				isActive = ++counter < length;
				if (isActive)
					targetFrame += DeltaTable[counter];
				return true;
			}

			return false;
		}
	}
}

