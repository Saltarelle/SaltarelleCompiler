﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler {
	public interface ISourceFile {
		string Filename { get; }
		TextReader Open();
	}
}
