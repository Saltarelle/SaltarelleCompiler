using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CoreLib.Plugin {
	public static class Common {
		private class DummyMethodSymbol : IMethodSymbol {
			public DummyMethodSymbol(string name, INamedTypeSymbol containingType, ITypeSymbol returnType, IEnumerable<ParameterInfo> parameters, bool isStatic, bool isVirtual, bool isAbstract, bool isSealed) {
				Name               = name;
				ContainingType     = containingType;
				ReturnType         = returnType;
				if (containingType.OriginalDefinition != ContainingType) {
					OriginalDefinition = new DummyMethodSymbol(name, containingType.OriginalDefinition, returnType, parameters, isStatic: isStatic, isVirtual: isVirtual, isAbstract: isAbstract, isSealed: isSealed);
					Parameters         = OriginalDefinition.Parameters;
				}
				else {
					OriginalDefinition = this;
					Parameters = ImmutableArray.CreateRange<IParameterSymbol>(parameters.Select((pi, i) => new DummyParameter(pi.Name, i, pi.Type, this, pi.RefKind)));
				}
				IsStatic           = isStatic;
				IsVirtual          = isVirtual;
				IsAbstract         = isAbstract;
				IsSealed           = isSealed;
			}

			public ImmutableArray<AttributeData> GetAttributes() { return ImmutableArray<AttributeData>.Empty; }
			public void Accept(SymbolVisitor visitor) { visitor.VisitMethod(this); }
			public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) { return visitor.VisitMethod(this); }
			public string GetDocumentationCommentId() { return null; }
			public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = new CancellationToken()) { return null; }
			public string ToDisplayString(SymbolDisplayFormat format = null) { return Name; }
			public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.MethodName, this, Name)); }
			public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return Name; }
			public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.MethodName, this, Name)); }
			public SymbolKind Kind { get { return SymbolKind.Method; } }
			public string Language { get { return "None"; } }
			public string Name { get; private set; }
			public string MetadataName { get { return Name; } }
			public ISymbol ContainingSymbol { get { return ContainingType; } }
			public IAssemblySymbol ContainingAssembly { get { return ContainingType.ContainingAssembly; } }
			public IModuleSymbol ContainingModule { get { return ContainingType.ContainingModule; } }
			public INamedTypeSymbol ContainingType { get; private set; }
			public INamespaceSymbol ContainingNamespace { get { return ContainingType.ContainingNamespace; } }
			public bool IsDefinition { get { return true; } }
			public bool IsStatic { get; private set; }
			public bool IsVirtual { get; private set; }
			public bool IsOverride { get { return false; } }
			public bool IsAbstract { get; private set; }
			public bool IsSealed { get; private set; }
			public bool IsExtern { get { return true; } }
			public bool IsImplicitlyDeclared { get { return true; } }
			public bool CanBeReferencedByName { get { return false; } }
			public ImmutableArray<Location> Locations { get { return ContainingType.Locations; } }
			public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get { return ImmutableArray<SyntaxReference>.Empty; } }
			public Accessibility DeclaredAccessibility { get { return Accessibility.Public; } }
			public IMethodSymbol OriginalDefinition { get; private set; }
			public IMethodSymbol OverriddenMethod { get { return null; } }
			public ITypeSymbol ReceiverType { get { return null; } }
			public IMethodSymbol ReducedFrom { get { return null; } }
			public ImmutableArray<IMethodSymbol> ExplicitInterfaceImplementations { get { return ImmutableArray<IMethodSymbol>.Empty; } }
			public ImmutableArray<CustomModifier> ReturnTypeCustomModifiers { get { return ImmutableArray<CustomModifier>.Empty; } }
			public ISymbol AssociatedSymbol { get { return null; } }
			public IMethodSymbol PartialDefinitionPart { get { return null; } }
			public IMethodSymbol PartialImplementationPart { get { return null; } }
			public INamedTypeSymbol AssociatedAnonymousDelegate { get { return null; } }
			public ITypeSymbol GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter) { return null; }
			public IMethodSymbol ReduceExtensionMethod(ITypeSymbol receiverType) { return null; }
			public ImmutableArray<AttributeData> GetReturnTypeAttributes() { return ImmutableArray<AttributeData>.Empty; }
			public IMethodSymbol Construct(params ITypeSymbol[] typeArguments) { throw new NotSupportedException(); }
			public DllImportData GetDllImportData() { return null; }
			public MethodKind MethodKind { get { return MethodKind.Ordinary; } }
			public int Arity { get { return 0; } }
			public bool IsGenericMethod { get { return false; } }
			public bool IsExtensionMethod { get { return false; } }
			public bool IsAsync { get { return false; } }
			public bool IsVararg { get { return false; } }
			public bool IsCheckedBuiltin { get { return false; } }
			public bool HidesBaseMethodsByName { get { return false; } }
			public bool ReturnsVoid { get { return ReturnType.SpecialType == SpecialType.System_Void; } }
			public ITypeSymbol ReturnType { get; private set; }
			public ImmutableArray<ITypeSymbol> TypeArguments { get { return ImmutableArray<ITypeSymbol>.Empty; } }
			public ImmutableArray<ITypeParameterSymbol> TypeParameters { get { return ImmutableArray<ITypeParameterSymbol>.Empty; } }
			public ImmutableArray<IParameterSymbol> Parameters { get; private set; }
			public IMethodSymbol ConstructedFrom { get { return this; } }
			ISymbol ISymbol.OriginalDefinition { get { return OriginalDefinition; } }
			public bool HasUnsupportedMetadata { get { return false; } }
			public bool Equals(ISymbol other) { throw new NotImplementedException(); }
		}

		private class DummyParameter : IParameterSymbol {
			public DummyParameter(string name, int ordinal, ITypeSymbol type, ISymbol containingSymbol, RefKind refKind) {
				Name = name;
				Ordinal = ordinal;
				Type = type;
				ContainingSymbol = containingSymbol;
				RefKind = refKind;
			}

			public ImmutableArray<AttributeData> GetAttributes() { return ImmutableArray<AttributeData>.Empty; }
			public void Accept(SymbolVisitor visitor) { visitor.VisitParameter(this); }
			public TResult Accept<TResult>(SymbolVisitor<TResult> visitor) { return visitor.VisitParameter(this); }
			public string GetDocumentationCommentId() { return null; }
			public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = new CancellationToken()) { return null; }
			public string ToDisplayString(SymbolDisplayFormat format = null) { return Name; }
			public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.ParameterName, this, Name)); }
			public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return Name; }
			public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) { return ImmutableArray.Create(new SymbolDisplayPart(SymbolDisplayPartKind.ParameterName, this, Name)); }
			public SymbolKind Kind { get { return SymbolKind.Parameter; } }
			public string Language { get { return "None"; } }
			public string Name { get; private set; }
			public string MetadataName { get { return Name; } }
			public ISymbol ContainingSymbol { get; private set; }
			public IAssemblySymbol ContainingAssembly { get { return ContainingSymbol.ContainingAssembly; } }
			public IModuleSymbol ContainingModule { get { return ContainingSymbol.ContainingModule; } }
			public INamedTypeSymbol ContainingType { get { return ContainingSymbol.ContainingType; } }
			public INamespaceSymbol ContainingNamespace { get { return ContainingSymbol.ContainingNamespace; } }
			public bool IsDefinition { get { return true; } }
			public bool IsStatic { get { return false; } }
			public bool IsVirtual { get { return false; } }
			public bool IsOverride { get { return false; } }
			public bool IsAbstract { get { return false; } }
			public bool IsSealed { get { return false; } }
			public bool IsExtern { get { return false; } }
			public bool IsImplicitlyDeclared { get { return false; } }
			public bool CanBeReferencedByName { get { return true; } }
			public ImmutableArray<Location> Locations { get { return ContainingSymbol.Locations; } }
			public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get { return ImmutableArray<SyntaxReference>.Empty; } }
			public Accessibility DeclaredAccessibility { get { return Accessibility.NotApplicable; } }
			public IParameterSymbol OriginalDefinition { get { return this; } }
			public RefKind RefKind { get; private set; }
			public bool IsParams { get { return false; } }
			public bool IsOptional { get { return false; } }
			public bool IsThis { get { return false; } }
			public ITypeSymbol Type { get; private set; }
			public ImmutableArray<CustomModifier> CustomModifiers { get { return ImmutableArray<CustomModifier>.Empty; } }
			public int Ordinal { get; private set; }
			public bool HasExplicitDefaultValue { get { return false; } }
			public object ExplicitDefaultValue { get { return false; } }
			ISymbol ISymbol.OriginalDefinition { get { return OriginalDefinition; } }
			public bool HasUnsupportedMetadata { get { return false; } }
			public bool Equals(ISymbol other) { throw new NotImplementedException(); }
		}

		public class ParameterInfo {
			public string Name { get; private set; }
			public ITypeSymbol Type { get; private set; }
			public RefKind RefKind { get; private set; }

			public ParameterInfo(ITypeSymbol type, string name, RefKind refKind = RefKind.None) {
				Type = type;
				Name = name;
				RefKind = refKind;
			}
		}

		public static IMethodSymbol CreateDummyMethod(string name, INamedTypeSymbol containingType, ITypeSymbol returnType, IEnumerable<ParameterInfo> parameters, bool isStatic = false, bool isVirtual = false, bool isAbstract = false, bool isSealed = false) {
			return new DummyMethodSymbol(name, containingType, returnType, parameters, isStatic: isStatic, isVirtual: isVirtual, isAbstract: isAbstract, isSealed: isSealed);
		}
	}
}
