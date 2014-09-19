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

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		internal static MetadataReference ExpressionAssembly { get { return _expressionAssemblyLazy.Value; } }

		private static readonly Lazy<MetadataReference> _expressionAssemblyLazy = new Lazy<MetadataReference>(() => {
			var c = Common.CreateCompilation(@"
using System.Collections.Generic;
using System.Reflection;

namespace System.Linq.Expressions {
	public class Expression {
		public static Expression Assign(Expression left, Expression right, Type type) { return null; }
		public static Expression Equal(Expression left, Expression right, Type type) { return null; }
		public static Expression Equal(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ReferenceEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression NotEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression NotEqual(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ReferenceNotEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression GreaterThan(Expression left, Expression right, Type type) { return null; }
		public static Expression GreaterThan(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LessThan(Expression left, Expression right, Type type) { return null; }
		public static Expression LessThan(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression GreaterThanOrEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression GreaterThanOrEqual(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LessThanOrEqual(Expression left, Expression right, Type type) { return null; }
		public static Expression LessThanOrEqual(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AndAlso(Expression left, Expression right, Type type) { return null; }
		public static Expression AndAlso(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression OrElse(Expression left, Expression right, Type type) { return null; }
		public static Expression OrElse(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression Coalesce(Expression left, Expression right, Type type) { return null; }
		public static Expression Coalesce(Expression left, Expression right, LambdaExpression conversion, Type type) { return null; }
		public static Expression Add(Expression left, Expression right, Type type) { return null; }
		public static Expression Add(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AddAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression AddAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AddAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression AddAssignChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression AddAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AddAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression AddChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression AddChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression Subtract(Expression left, Expression right, Type type) { return null; }
		public static Expression Subtract(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression SubtractAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression SubtractAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression SubtractAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression SubtractAssignChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression SubtractAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression SubtractAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression SubtractChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression SubtractChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression Divide(Expression left, Expression right, Type type) { return null; }
		public static Expression Divide(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression DivideAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression DivideAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression DivideAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Modulo(Expression left, Expression right, Type type) { return null; }
		public static Expression Modulo(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ModuloAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression ModuloAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ModuloAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Multiply(Expression left, Expression right, Type type) { return null; }
		public static Expression Multiply(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression MultiplyAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression MultiplyAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression MultiplyAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression MultiplyAssignChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression MultiplyAssignChecked(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression MultiplyChecked(Expression left, Expression right, Type type) { return null; }
		public static Expression MultiplyChecked(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LeftShift(Expression left, Expression right, Type type) { return null; }
		public static Expression LeftShift(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LeftShiftAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression LeftShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression LeftShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression RightShift(Expression left, Expression right, Type type) { return null; }
		public static Expression RightShift(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression RightShiftAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression RightShiftAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression RightShiftAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression And(Expression left, Expression right, Type type) { return null; }
		public static Expression And(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AndAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression AndAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression AndAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Or(Expression left, Expression right, Type type) { return null; }
		public static Expression Or(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression OrAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression OrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression OrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression ExclusiveOr(Expression left, Expression right, Type type) { return null; }
		public static Expression ExclusiveOr(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ExclusiveOrAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression ExclusiveOrAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }
		public static Expression Power(Expression left, Expression right, Type type) { return null; }
		public static Expression Power(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression PowerAssign(Expression left, Expression right, Type type) { return null; }
		public static Expression PowerAssign(Expression left, Expression right, MethodInfo method) { return null; }
		public static Expression PowerAssign(Expression left, Expression right, MethodInfo method, LambdaExpression conversion) { return null; }

		public static Expression ArrayIndex(Type type, Expression array, Expression index) { return null; }
		public static Expression ArrayIndex(Type type, Expression array, params Expression[] indexes) { return null; }
		public static Expression ArrayIndex(Type type, Expression array, IEnumerable<Expression> indexes) { return null; }

		public static Expression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) { return null; }

		public static Expression Constant(object value, Type type) { return null; }

		public static Expression Default(Type type) { return null; }

		public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) { return null; }
		public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) { return null; }

		public static Expression MakeIndex(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }
		public static Expression ArrayAccess(Type type, Expression array, params Expression[] indexes) { return null; }
		public static Expression ArrayAccess(Type type, Expression array, IEnumerable<Expression> indexes) { return null; }
		public static Expression Property(Type type, Expression instance, string propertyName, params Expression[] arguments) { return null; }
		public static Expression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) { return null; }
		public static Expression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) { return null; }

		public static Expression Invoke(Type type, Expression expression, params Expression[] arguments) { return null; }
		public static Expression Invoke(Type type, Expression expression, IEnumerable<Expression> arguments) { return null; }

		public static Expression Lambda(Expression body, params ParameterExpression[] parameters) { return null; }

		public static Expression ListInit(NewExpression newExpression, params Expression[] initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, params ElementInit[] initializers) { return null; }
		public static Expression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers) { return null; }

		public static MemberBinding Bind(MemberInfo member, Expression expression) { return null; }
		public static MemberBinding Bind(MethodInfo propertyAccessor, Expression expression) { return null; }

		public static Expression Field(Expression expression, FieldInfo field) { return null; }
		public static Expression Field(Expression expression, string fieldName) { return null; }
		public static Expression Field(Expression expression, Type type, string fieldName) { return null; }
		public static Expression Property(Expression expression, string propertyName) { return null; }
		public static Expression Property(Expression expression, Type type, string propertyName) { return null; }
		public static Expression Property(Expression expression, PropertyInfo property) { return null; }
		public static Expression Property(Expression expression, MethodInfo propertyAccessor) { return null; }
		public static Expression PropertyOrField(Expression expression, string propertyOrFieldName) { return null; }
		public static Expression MakeMemberAccess(Expression expression, MemberInfo member) { return null; }

		public static Expression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) { return null; }
		public static Expression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) { return null; }

		public static MemberBinding ListBind(MemberInfo member, params ElementInit[] initializers) { return null; }
		public static MemberBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) { return null; }
		public static MemberBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) { return null; }
		public static MemberBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) { return null; }

		public static MemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings) { return null; }
		public static MemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings) { return null; }
		public static MemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings) { return null; }
		public static MemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings) { return null; }

		public static Expression Call(MethodInfo method, Expression arg0) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3) { return null; }
		public static Expression Call(MethodInfo method, Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4) { return null; }
		public static Expression Call(MethodInfo method, params Expression[] arguments) { return null; }
		public static Expression Call(MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		public static Expression Call(Expression instance, MethodInfo method) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, params Expression[] arguments) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, Expression arg0, Expression arg1, Expression arg2) { return null; }
		public static Expression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments) { return null; }
		public static Expression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments) { return null; }
		public static Expression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments) { return null; }
		public static Expression ArrayIndex(Expression array, params Expression[] indexes) { return null; }
		public static Expression ArrayIndex(Expression array, IEnumerable<Expression> indexes) { return null; }

		public static Expression NewArrayInit(Type type, params Expression[] initializers) { return null; }
		public static Expression NewArrayInit(Type type, IEnumerable<Expression> initializers) { return null; }
		public static Expression NewArrayBounds(Type type, params Expression[] bounds) { return null; }
		public static Expression NewArrayBounds(Type type, IEnumerable<Expression> bounds) { return null; }

		public static NewExpression New(ConstructorInfo constructor) { return null; }
		public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members) { return null; }
		public static NewExpression New(ConstructorInfo constructor, Expression[] arguments, params MemberInfo[] members) { return null; }
		public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo[] members) { return null; }
		public static NewExpression New(Type type) { return null; }

		public static ParameterExpression Parameter(Type type) { return null; }
		public static ParameterExpression Variable(Type type) { return null; }
		public static ParameterExpression Parameter(Type type, string name) { return null; }
		public static ParameterExpression Variable(Type type, string name) { return null; }

		public static Expression TypeIs(Expression expression, Type type) { return null; }
		public static Expression TypeEqual(Expression expression, Type type) { return null; }

		public static Expression MakeUnary(ExpressionType unaryType, Expression operand, Type type) { return null; }
		public static Expression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo method) { return null; }
		public static Expression Negate(Expression expression, Type type) { return null; }
		public static Expression Negate(Expression expression, MethodInfo method) { return null; }
		public static Expression UnaryPlus(Expression expression, Type type) { return null; }
		public static Expression UnaryPlus(Expression expression, MethodInfo method) { return null; }
		public static Expression NegateChecked(Expression expression, Type type) { return null; }
		public static Expression NegateChecked(Expression expression, MethodInfo method) { return null; }
		public static Expression Not(Expression expression, Type type) { return null; }
		public static Expression Not(Expression expression, MethodInfo method) { return null; }
		public static Expression IsFalse(Expression expression, Type type) { return null; }
		public static Expression IsFalse(Expression expression, MethodInfo method) { return null; }
		public static Expression IsTrue(Expression expression, Type type) { return null; }
		public static Expression IsTrue(Expression expression, MethodInfo method) { return null; }
		public static Expression OnesComplement(Expression expression, Type type) { return null; }
		public static Expression OnesComplement(Expression expression, MethodInfo method) { return null; }
		public static Expression TypeAs(Expression expression, Type type) { return null; }
		public static Expression Unbox(Expression expression, Type type) { return null; }
		public static Expression Convert(Expression expression, Type type) { return null; }
		public static Expression Convert(Expression expression, Type type, MethodInfo method) { return null; }
		public static Expression ConvertChecked(Expression expression, Type type) { return null; }
		public static Expression ConvertChecked(Expression expression, Type type, MethodInfo method) { return null; }
		public static Expression ArrayLength(Expression array) { return null; }
		public static Expression Quote(Expression expression) { return null; }
		public static Expression Increment(Expression expression, Type type) { return null; }
		public static Expression Increment(Expression expression, MethodInfo method) { return null; }
		public static Expression Decrement(Expression expression, Type type) { return null; }
		public static Expression Decrement(Expression expression, MethodInfo method) { return null; }
		public static Expression PreIncrementAssign(Expression expression, Type type) { return null; }
		public static Expression PreIncrementAssign(Expression expression, MethodInfo method) { return null; }
		public static Expression PreDecrementAssign(Expression expression, Type type) { return null; }
		public static Expression PreDecrementAssign(Expression expression, MethodInfo method) { return null; }
		public static Expression PostIncrementAssign(Expression expression, Type type) { return null; }
		public static Expression PostIncrementAssign(Expression expression, MethodInfo method) { return null; }
		public static Expression PostDecrementAssign(Expression expression, Type type) { return null; }
		public static Expression PostDecrementAssign(Expression expression, MethodInfo method) { return null; }
	}
	public enum ExpressionType {}
	public class ParameterExpression : Expression {}
	public class NewExpression : Expression {}
	public class LambdaExpression : Expression {}
	public class Expression<T> : LambdaExpression {}
	public class MemberBinding {}
	public class ElementInit {}
}
			", new[] { _mscorlibLazy.Value }, new string[0]);

			return c.ToMetadataReference();
		});

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
