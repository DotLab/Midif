namespace Midif {
	/// <summary>
	/// Cross Platform Debug Console.
	/// </summary>
	static class DebugConsole {
		public static void Write (object o) {
			#if UNITY_EDITOR
			WriteLine(o);
			#endif
		}

		public static void Write (string format, params object[] args) {
			#if UNITY_EDITOR
			WriteLine(format, args);
			#endif
		}

		public static void WriteLine (object o) {
			#if UNITY_EDITOR
			UnityEngine.Debug.Log(o);
			#endif
		}

		public static void WriteLine (string format, params object[] args) {
			#if UNITY_EDITOR
			UnityEngine.Debug.Log(string.Format(format, args));
			#endif
		}
	}
}
