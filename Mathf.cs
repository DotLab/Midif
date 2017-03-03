using System;

namespace Midif {
	public static class Mathf {
		public static int Gcd (int a, int b) {
			while (b > 0) {
				int temp = b;
				b = a % b; // % is remainder
				a = temp;
			}

			return a;
		}
	}
}

