using System.Collections.Generic;

namespace Midif.File.Sfz {
	[System.Serializable]
	public class SfzFile {
		public List<SfzRegion> Regions = new List<SfzRegion>();
	}
}