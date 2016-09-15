namespace Midif.Synthesizer.Dx7 {
	public class Dx7Algorithm {
		#region Algorithms

		public static readonly Dx7Algorithm[] Algorithms = {
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new int[] { }, new [] { 3 }, new [] { 4 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new [] { 1 }, new [] { 3 }, new [] { 4 }, new [] { 5 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 3 }, new [] { new [] { 1 }, new [] { 2 }, new int[] { }, new [] { 4 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 3 }, new [] { new [] { 1 }, new [] { 2 }, new int[] { }, new [] { 4 }, new [] { 5 }, new [] { 3 } }),
			new Dx7Algorithm(new [] { 0, 2, 4 }, new [] { new [] { 1 }, new int[] { }, new [] { 3 }, new int[] { }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 2, 4 }, new [] { new [] { 1 }, new int[] { }, new [] { 3 }, new int[] { }, new [] { 5 }, new [] { 4 } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new int[] { }, new [] { 3, 4 }, new int[] { }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new int[] { }, new [] { 3, 4 }, new [] { 3 }, new [] { 5 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new [] { 1 }, new [] { 3, 4 }, new int[] { }, new [] { 5 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 3 }, new [] { new [] { 1 }, new [] { 2 }, new [] { 2 }, new [] { 4, 5 }, new int[] { }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 3 }, new [] { new [] { 1 }, new [] { 2 }, new int[] { }, new [] { 4, 5 }, new int[] { }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new [] { 1 }, new [] { 3, 4, 5 }, new int[] { }, new int[] { }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new int[] { }, new [] { 3, 4, 5 }, new int[] { }, new int[] { }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new int[] { }, new [] { 3 }, new [] { 4, 5 }, new int[] { }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 2 }, new [] { new [] { 1 }, new [] { 1 }, new [] { 3 }, new [] { 4, 5 }, new int[] { }, new int[] { } }),
			new Dx7Algorithm(new [] { 0 }, new [] { new [] { 1, 2, 4 }, new int[] { }, new [] { 3 }, new int[] { }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0 }, new [] { new [] { 1, 2, 4 }, new [] { 1 }, new [] { 3 }, new int[] { }, new [] { 5 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0 }, new [] { new [] { 1, 2, 3 }, new int[] { }, new [] { 2 }, new [] { 4 }, new [] { 5 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 3, 4 }, new [] { new [] { 1 }, new [] { 2 }, new int[] { }, new [] { 5 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 3 }, new [] { new [] { 2 }, new [] { 2 }, new [] { 2 }, new [] { 4, 5 }, new int[] { }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 1, 3, 4 }, new [] { new [] { 2 }, new [] { 2 }, new [] { 2 }, new [] { 5 }, new [] { 5 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 2, 3, 4 }, new [] { new [] { 1 }, new int[] { }, new [] { 5 }, new [] { 5 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 3, 4 }, new [] { new int[] { }, new [] { 2 }, new int[] { }, new [] { 5 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 2, 3, 4 }, new [] { new int[] { }, new int[] { }, new [] { 5 }, new [] { 5 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 2, 3, 4 }, new [] { new int[] { }, new int[] { }, new int[] { }, new [] { 5 }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 3 }, new [] { new int[] { }, new [] { 2 }, new int[] { }, new [] { 4, 5 }, new int[] { }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 3 }, new [] { new int[] { }, new [] { 2 }, new [] { 2 }, new [] { 4, 5 }, new int[] { }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 2, 5 }, new [] { new [] { 1 }, new int[] { }, new [] { 3 }, new [] { 4 }, new [] { 4 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 1, 2, 4 }, new [] { new int[] { }, new int[] { }, new [] { 3 }, new int[] { }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 2, 5 }, new [] { new int[] { }, new int[] { }, new [] { 3 }, new [] { 4 }, new [] { 4 }, new int[] { } }),
			new Dx7Algorithm(new [] { 0, 1, 2, 3, 4 }, new [] { new int[] { }, new int[] { }, new int[] { }, new int[] { }, new [] { 5 }, new [] { 5 } }),
			new Dx7Algorithm(new [] { 0, 1, 2, 3, 4, 5 }, new [] { new int[] { }, new int[] { }, new int[] { }, new int[] { }, new int[] { }, new [] { 5 } }),
		};

		#endregion

		public int[] outputMix;
		public int[][] modulationMatrix;

		Dx7Algorithm (int[] outputMix, int[][] modulationMatrix) {
			this.outputMix = outputMix;
			this.modulationMatrix = modulationMatrix;
		}
	}
}