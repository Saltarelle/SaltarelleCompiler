using System;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler {
	public abstract class PluginAttributeBase : Attribute {
		public virtual void ApplyTo(ISymbol symbol, IAttributeStore attributeStore, IErrorReporter errorReporter) {
		}
	}
}