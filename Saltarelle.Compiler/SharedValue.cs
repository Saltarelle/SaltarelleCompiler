using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler {
	public class SharedValue<T> where T : struct {
		public T Value { get; set; }

		public SharedValue(T value) {
			Value = value;
		}
	}
}
