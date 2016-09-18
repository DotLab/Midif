namespace Midif {
	/// <summary>
	/// Platform Dependent Debug Console.
	/// </summary>
	static class DebugConsole {
		public static void Write (object o) {
			#if UNITY_EDITOR
			WriteLine(o);
			#endif
		}

		public static void WriteLine (object o) {
			#if UNITY_EDITOR
			UnityEngine.Debug.Log(o);
			#endif
		}
	}
}

