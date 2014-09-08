using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests {
	internal static class Common {
		public static readonly string MscorlibPath = Path.GetFullPath(@"../../../Runtime/CoreLib/bin/mscorlib.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => LoadAssemblyFile(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		public static MetadataReference LoadAssemblyFile(string path) {
			return new MetadataFileReference(path);
		}

		private class MockAssembly : IAssemblySymbol, INamespaceSymbol, IModuleSymbol {
			private readonly string _name;
			private readonly Dictionary<string, INamedTypeSymbol> _types = new Dictionary<string, INamedTypeSymbol>();

			public MockAssembly(string name) {
				_name = name;
			}

			public void AddType(INamedTypeSymbol type) {
				_types[type.Name] = type;
			}

			public ImmutableArray<AttributeData> GetAttributes() { return ImmutableArray<AttributeData>.Empty; }
			public void Accept(SymbolVisitor visitor) { throw new NotSupportedException(); }
			public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) { throw new NotSupportedException(); }
			public string GetDocumentationCommentId() { return null; }
			public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = new CancellationToken()) { return null; }
			public string ToDisplayString(SymbolDisplayFormat format = null) { return _name; }
			public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.AssemblyName, this, _name)); }
			public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return _name; }
			public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.AssemblyName, this, _name)); }
			public SymbolKind Kind { get { return SymbolKind.Assembly; } }
			public string Language { get { return "C#"; } }
			public string Name { get { return _name; } }
			public string MetadataName { get { return _name; } }
			public ISymbol ContainingSymbol { get { return null; } }
			public IAssemblySymbol ContainingAssembly { get { return null; } }
			public IModuleSymbol ContainingModule { get { return null; } }
			public INamedTypeSymbol ContainingType { get { return null; } }
			public INamespaceSymbol ContainingNamespace { get { return null; } }
			public bool IsDefinition { get { return true; } }
			public bool IsStatic { get { return false; } }
			public bool IsVirtual { get { return false; } }
			public bool IsOverride { get { return false; } }
			public bool IsAbstract { get { return false; } }
			public bool IsSealed { get { return false; } }
			public bool IsExtern { get { return false; } }
			public bool IsImplicitlyDeclared { get { return false; } }
			public bool CanBeReferencedByName { get { return false; } }
			public ImmutableArray<Location> Locations { get { return ImmutableArray<Location>.Empty; } }
			public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get { return ImmutableArray<SyntaxReference>.Empty; } }
			public Accessibility DeclaredAccessibility { get { return Accessibility.NotApplicable; } }
			public ISymbol OriginalDefinition { get { return this; } }
			public bool HasUnsupportedMetadata { get { return false; } }
			public bool GivesAccessTo(IAssemblySymbol toAssembly) { return false; }
			public INamedTypeSymbol GetTypeByMetadataName(string fullyQualifiedMetadataName) { return null; }
			public INamedTypeSymbol ResolveForwardedType(string fullyQualifiedMetadataName) { return null; }
			public bool IsInteractive { get { return false; } }
			public AssemblyIdentity Identity { get { return new AssemblyIdentity(_name); } }
			public INamespaceSymbol GetModuleNamespace(INamespaceSymbol namespaceSymbol) { return this; }
			public INamespaceSymbol GlobalNamespace { get { return this; } }
			public ImmutableArray<AssemblyIdentity> ReferencedAssemblies { get; private set; }
			public ImmutableArray<IAssemblySymbol> ReferencedAssemblySymbols { get; private set; }
			public IEnumerable<IModuleSymbol> Modules { get { yield return this; } }
			public ICollection<string> TypeNames { get { return _types.Keys; } }
			public ICollection<string> NamespaceNames { get { return ImmutableArray<string>.Empty; } }
			public bool MightContainExtensionMethods { get { return false; } }
			ImmutableArray<ISymbol> INamespaceOrTypeSymbol.GetMembers() { return _types.Values.ToImmutableArray<ISymbol>(); }
			IEnumerable<INamespaceOrTypeSymbol> INamespaceSymbol.GetMembers(string name) { if (_types.ContainsKey(name)) yield return _types[name]; }
			public IEnumerable<INamespaceSymbol> GetNamespaceMembers() { yield break; }
			public bool IsGlobalNamespace { get { return true; } }
			public NamespaceKind NamespaceKind { get { return NamespaceKind.Assembly; } }
			public Compilation ContainingCompilation { get { return null; } }
			public ImmutableArray<INamespaceSymbol> ConstituentNamespaces { get { return ImmutableArray<INamespaceSymbol>.Empty; } }
			IEnumerable<INamespaceOrTypeSymbol> INamespaceSymbol.GetMembers() { return _types.Values; }
			ImmutableArray<ISymbol> INamespaceOrTypeSymbol.GetMembers(string name) { return _types.ContainsKey(name) ? ImmutableArray.Create<ISymbol>(_types[name]) : ImmutableArray<ISymbol>.Empty; }
			public ImmutableArray<INamedTypeSymbol> GetTypeMembers() { return ImmutableArray.CreateRange(_types.Values); }
			public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name) { return ImmutableArray.CreateRange(_types.Values.Where(t => t.Name == name)); }
			public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity) { return ImmutableArray.CreateRange(_types.Values.Where(t => t.Name == name && t.TypeParameters.Length == arity)); }
			bool INamespaceOrTypeSymbol.IsNamespace { get { return true; } }
			bool INamespaceOrTypeSymbol.IsType { get { return false; } }
		}

		private class MockType : INamedTypeSymbol {
			private readonly IAssemblySymbol _assembly;
			private readonly string _name;
			private readonly Accessibility _accessibility;
			private readonly INamedTypeSymbol _containingType;

			public MockType(string name, IAssemblySymbol assembly, Accessibility accessibility, INamedTypeSymbol containingType) {
				_name = name;
				_assembly = assembly;
				_accessibility = accessibility;
				_containingType = containingType;
			}

			public ImmutableArray<AttributeData> GetAttributes() { return ImmutableArray<AttributeData>.Empty; }
			public void Accept(SymbolVisitor visitor) { visitor.VisitNamedType(this); }
			public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) { return visitor.VisitNamedType(this); }
			public string GetDocumentationCommentId() { return null; }
			public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = new CancellationToken()) { return null; }
			public string ToDisplayString(SymbolDisplayFormat format = null) { return _name; }
			public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.ClassName, this, _name)); }
			public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return _name; }
			public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.ClassName, this, _name)); }
			public SymbolKind Kind { get { return SymbolKind.NamedType; } }
			public string Language { get { return "C#"; } }
			public string Name { get { return _name; } }
			public string MetadataName { get { return _name; } }
			public ISymbol ContainingSymbol { get { return _containingType; } }
			public IAssemblySymbol ContainingAssembly { get { return _assembly; } }
			public IModuleSymbol ContainingModule { get { return null; } }
			public INamedTypeSymbol ContainingType { get { return _containingType; } }
			public INamespaceSymbol ContainingNamespace { get { return _assembly.GlobalNamespace; } }
			public bool IsDefinition { get { return true; } }
			public bool IsStatic { get { return false; } }
			public bool IsVirtual { get { return false; } }
			public bool IsOverride { get { return false; } }
			public bool IsAbstract { get { return false; } }
			public bool IsSealed { get { return false; } }
			public bool IsExtern { get { return false; } }
			public bool IsImplicitlyDeclared { get { return false; } }
			public bool CanBeReferencedByName { get { return true; } }
			public ImmutableArray<Location> Locations { get { return ImmutableArray<Location>.Empty; } }
			public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get { return ImmutableArray<SyntaxReference>.Empty; } }
			public Accessibility DeclaredAccessibility { get { return _accessibility; } }
			public INamedTypeSymbol OriginalDefinition { get { return this; } }
			public IMethodSymbol DelegateInvokeMethod { get { return null; } }
			public INamedTypeSymbol EnumUnderlyingType { get { return null; } }
			public INamedTypeSymbol ConstructedFrom { get { return this; } }
			public ImmutableArray<IMethodSymbol> InstanceConstructors { get { return ImmutableArray<IMethodSymbol>.Empty; } }
			public ImmutableArray<IMethodSymbol> StaticConstructors { get { return ImmutableArray<IMethodSymbol>.Empty; } }
			public ImmutableArray<IMethodSymbol> Constructors { get { return ImmutableArray<IMethodSymbol>.Empty; } }
			public ISymbol AssociatedSymbol { get { return null; } }
			public bool MightContainExtensionMethods { get { return false; } }
			public INamedTypeSymbol Construct(params ITypeSymbol[] typeArguments) { return this; }
			public INamedTypeSymbol ConstructUnboundGenericType() { return this; }
			public int Arity { get { return 0; } }
			public bool IsGenericType { get { return false; } }
			public bool IsUnboundGenericType { get { return false; } }
			public bool IsScriptClass { get { return false; } }
			public bool IsImplicitClass { get { return false; } }
			public IEnumerable<string> MemberNames { get { return ImmutableArray<string>.Empty; } }
			public ImmutableArray<ITypeParameterSymbol> TypeParameters { get { return ImmutableArray<ITypeParameterSymbol>.Empty; } }
			public ImmutableArray<ITypeSymbol> TypeArguments { get { return ImmutableArray<ITypeSymbol>.Empty; } }
			ITypeSymbol ITypeSymbol.OriginalDefinition { get { return OriginalDefinition; } }
			public SpecialType SpecialType { get { return SpecialType.None; } }
			public ISymbol FindImplementationForInterfaceMember(ISymbol interfaceMember) { return null; }
			public TypeKind TypeKind { get { return TypeKind.Class; } }
			public INamedTypeSymbol BaseType { get { return null; } }
			public ImmutableArray<INamedTypeSymbol> Interfaces { get { return ImmutableArray<INamedTypeSymbol>.Empty; } }
			public ImmutableArray<INamedTypeSymbol> AllInterfaces { get { return ImmutableArray<INamedTypeSymbol>.Empty; } }
			public bool IsReferenceType { get { return true; } }
			public bool IsValueType { get { return false; } }
			public bool IsAnonymousType { get { return false;  } }
			ISymbol ISymbol.OriginalDefinition { get { return OriginalDefinition; } }
			public bool HasUnsupportedMetadata { get { return false; } }
			public ImmutableArray<ISymbol> GetMembers() { return ImmutableArray<ISymbol>.Empty; }
			public ImmutableArray<ISymbol> GetMembers(string name) { return ImmutableArray<ISymbol>.Empty; }
			public ImmutableArray<INamedTypeSymbol> GetTypeMembers() { return ImmutableArray<INamedTypeSymbol>.Empty; }
			public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name) { return ImmutableArray<INamedTypeSymbol>.Empty; }
			public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity) { return ImmutableArray<INamedTypeSymbol>.Empty; }
			public bool IsNamespace { get { return false; } }
			public bool IsType { get { return true; } }
		}

		public static IAssemblySymbol CreateMockAssembly() {
			return new MockAssembly("Mock");
		}

		public static INamedTypeSymbol CreateMockTypeDefinition(string name, IAssemblySymbol assembly, Accessibility accessibility = Accessibility.Public, INamedTypeSymbol containingType = null) {
			var result = new MockType(name, assembly, accessibility, containingType);
			if (assembly is MockAssembly)
				((MockAssembly)assembly).AddType(result);
			return result;
		}

		public static CSharpCompilation CreateCompilation(string source, IEnumerable<MetadataReference> references = null, IList<string> defineConstants = null, string assemblyName = null) {
			return CreateCompilation(new[] { source }, references, defineConstants);
		}

		public static CSharpCompilation CreateCompilation(IEnumerable<string> sources, IEnumerable<MetadataReference> references = null, IList<string> defineConstants = null, string assemblyName = null) {
			references = references ?? new[] { Common.Mscorlib };
			var defineConstantsArr = ImmutableArray.CreateRange(defineConstants ?? new string[0]);
			var syntaxTrees = sources.Select((s, i) => SyntaxFactory.ParseSyntaxTree(s, new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.None, SourceCodeKind.Regular, defineConstantsArr), "File" + i.ToString(CultureInfo.InvariantCulture) + ".cs"));
			var compilation = CSharpCompilation.Create(assemblyName ?? "Test", syntaxTrees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var diagnostics = string.Join(Environment.NewLine, compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage()));
			if (!string.IsNullOrEmpty(diagnostics))
				Assert.Fail("Errors in source:" + Environment.NewLine + diagnostics);
			return compilation;
		}
	}
}
