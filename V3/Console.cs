namespace Midif.V3 {
	static class Console {
		public static void Log(params object[] objs) {
			#if UNITY_EDITOR
			var sb = new System.Text.StringBuilder();
			foreach (var o in objs) {
				sb.Append(o.ToString());
				sb.Append(' ');
			}
			UnityEngine.Debug.Log(sb.ToString());
			#endif
		}
	}
}