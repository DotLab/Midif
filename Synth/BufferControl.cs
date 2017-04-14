using System.Collections.Generic;

namespace Midif.Synth {
	static class BufferControl {
		/// <summary>
		/// 4 KB L0 Cache; 16 KB L1 Cache; 2MB L2 Cache.
		/// 1024 floats; 4096 floats;
		/// Must use float for buffers.
		/// The buffer is HALF the size of the real buffer.
		/// </summary>
		static readonly Stack<float[]> BufferStack = new Stack<float[]>();

		public static float[] RequestBuffer () {
			if (BufferStack.Count > 0)
				return BufferStack.Pop();

			UnityEngine.Debug.Log("New buffer");
			return new float[SynthConfig.BufferSize];
		}

		public static void FreeBuffer (float[] buffer) {
			BufferStack.Push(buffer);
		}
	}
}
