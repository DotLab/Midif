namespace Midif.File.Sfz {
	[System.Serializable]
	public class SfzFile {
		public SfzRegion[] Regions;

		public override string ToString () {
			return string.Format("[SfzFile: Regions.Length={0}]", Regions.Length);
		}
	}
}