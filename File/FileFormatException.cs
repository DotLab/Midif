using System;

namespace Midif.File {
	class FileFormatException : Exception {
		public FileFormatException (object element, object unexpected, object expecting) : base(String.Format("{0} : unexpected '{1}', expecting '{2}'", element, unexpected, expecting)) {
		}
	}
}

