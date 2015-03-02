using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.Compiler {
	public class SimpleVariable : ILocalSymbol {
		private readonly Location _location;
		private readonly string _name;
			
		public SimpleVariable(string name, Location location) {
			Debug.Assert(name != null);
			Debug.Assert(location != null);
			this._name = name;
			this._location = location;
		}

		public ImmutableArray<AttributeData> GetAttributes() {
			return ImmutableArray<AttributeData>.Empty;
		}

		public void Accept(SymbolVisitor visitor) {
			visitor.VisitLocal(this);
		}

		public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) {
			return visitor.VisitLocal(this);
		}

		public string GetDocumentationCommentId() {
			return null;
		}

		public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = new CancellationToken()) {
			return null;
		}

		public string ToDisplayString(SymbolDisplayFormat format = null) {
			return _name;
		}

		public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null) {
			return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.LocalName, this, _name));
		}

		public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) {
			return _name;
		}

		public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) {
			return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.LocalName, this, _name));
		}

		public SymbolKind Kind { get { return SymbolKind.Local; } }
		public string Language { get { return ""; } }
		public string Name { get { return _name; } }
		public string MetadataName { get { return _name; } }
		public ISymbol ContainingSymbol { get { return null; } }
		public IAssemblySymbol ContainingAssembly { get { return null; } }
		public IModuleSymbol ContainingModule { get { return null; } }
		public INamedTypeSymbol ContainingType { get { return null; } }
		public INamespaceSymbol ContainingNamespace { get { return null; } }
		public bool IsDefinition { get { return false; } }
		public bool IsStatic { get { return false; } }
		public bool IsVirtual { get { return false; } }
		public bool IsOverride { get { return false; } }
		public bool IsAbstract { get { return false; } }
		public bool IsSealed { get { return false; } }
		public bool IsExtern { get { return false; } }
		public bool IsImplicitlyDeclared { get { return false; } }
		public bool CanBeReferencedByName { get { return false; } }
		public ImmutableArray<Location> Locations { get { return ImmutableArray.Create(_location); } }
		public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get { return ImmutableArray<SyntaxReference>.Empty; } }
		public Accessibility DeclaredAccessibility { get { return Accessibility.NotApplicable; } }
		public ISymbol OriginalDefinition { get { return this; } }
		public bool HasUnsupportedMetadata { get { return false; } }
		public ITypeSymbol Type { get { return null; } }
		public bool IsConst { get { return false; } }
		public bool HasConstantValue { get { return false; } }
		public object ConstantValue { get { return null; } }
		public bool IsFunctionValue { get { return false; } }
		public bool Equals(ISymbol other) {
			return ReferenceEquals(this, other);
		}
	}
}