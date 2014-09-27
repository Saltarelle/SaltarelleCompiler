using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler {
	public interface IAttributeStore {
		AttributeList AttributesFor(ISymbol symbol);
	}
}