using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler {
	public class ReferenceMetadataImporter : IMetadataImporter {
		private const string AttributeNamespace = "System.Runtime.CompilerServices.Internal";
		private const string ScriptSemanticsAttribute = "ScriptSemanticsAttribute";
		private const string UsedMemberNamesAttribute = "UsedMemberNamesAttribute";

		private readonly Dictionary<INamedTypeSymbol, TypeScriptSemantics> _typeSemantics = new Dictionary<INamedTypeSymbol, TypeScriptSemantics>();
		private readonly Dictionary<INamedTypeSymbol, DelegateScriptSemantics> _delegateSemantics = new Dictionary<INamedTypeSymbol, DelegateScriptSemantics>();
		private readonly Dictionary<INamedTypeSymbol, IReadOnlyList<string>> _usedInstanceMemberNames = new Dictionary<INamedTypeSymbol, IReadOnlyList<string>>();
		private readonly Dictionary<IMethodSymbol, MethodScriptSemantics> _methodSemantics = new Dictionary<IMethodSymbol, MethodScriptSemantics>();
		private readonly Dictionary<IPropertySymbol, PropertyScriptSemantics> _propertySemantics = new Dictionary<IPropertySymbol, PropertyScriptSemantics>();
		private readonly Dictionary<IFieldSymbol, FieldScriptSemantics> _fieldSemantics = new Dictionary<IFieldSymbol, FieldScriptSemantics>();
		private readonly Dictionary<IEventSymbol, EventScriptSemantics> _eventSemantics = new Dictionary<IEventSymbol, EventScriptSemantics>();
		private readonly Dictionary<IMethodSymbol, ConstructorScriptSemantics> _constructorSemantics = new Dictionary<IMethodSymbol, ConstructorScriptSemantics>();
		private readonly IErrorReporter _errorReporter;

		public ReferenceMetadataImporter(IErrorReporter errorReporter) {
			_errorReporter  = errorReporter;
		}

		public void Prepare(INamedTypeSymbol type) {
		}

		private TypeScriptSemantics LoadTypeSemantics(INamedTypeSymbol typeDefinition) {
			try {
				var data = GetData(typeDefinition, ScriptSemanticsAttribute);
				switch ((byte)data[0]) {
					case (byte)TypeScriptSemantics.ImplType.NormalType:
						return TypeScriptSemantics.NormalType((string)data[1], (bool)data[2], (bool)data[3]);
					case (byte)TypeScriptSemantics.ImplType.MutableValueType:
						return TypeScriptSemantics.MutableValueType((string)data[1], (bool)data[2], (bool)data[3]);
					case (byte)TypeScriptSemantics.ImplType.NotUsableFromScript:
						return TypeScriptSemantics.NotUsableFromScript();
					default:
						throw new Exception();
				}
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, typeDefinition.FullyQualifiedName(), typeDefinition.ContainingAssembly.Name);
				return TypeScriptSemantics.NotUsableFromScript();
			}
		}

		private static object[] SerializeTypeSemantics(TypeScriptSemantics semantics) {
			switch (semantics.Type) {
				case TypeScriptSemantics.ImplType.NormalType:
					return new object[] { (byte)semantics.Type, semantics.Name, semantics.IgnoreGenericArguments, semantics.GenerateCode };
				case TypeScriptSemantics.ImplType.MutableValueType:
					return new object[] { (byte)semantics.Type, semantics.Name, semantics.IgnoreGenericArguments, semantics.GenerateCode };
				case TypeScriptSemantics.ImplType.NotUsableFromScript:
					return new object[] { (byte)semantics.Type };
				default:
					throw new ArgumentException("Invalid script semantics " + semantics.Type);
			}
		}

		private MethodScriptSemantics LoadMethodSemantics(IMethodSymbol method) {
			try {
				var data = GetData(method, ScriptSemanticsAttribute);
				switch ((byte)data[0]) {
					case (byte)MethodScriptSemantics.ImplType.NormalMethod:
						return MethodScriptSemantics.NormalMethod((string)data[1], (bool)data[2], (bool)data[3], (bool)data[4], (bool)data[5]);
					case (byte)MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument:
						return MethodScriptSemantics.StaticMethodWithThisAsFirstArgument((string)data[1], (bool)data[2], (bool)data[3], (bool)data[4], (bool)data[5]);
					case (byte)MethodScriptSemantics.ImplType.InlineCode:
						return MethodScriptSemantics.InlineCode((string)data[1], (bool)data[2], (string)data[3], (string)data[4], (string)data[5]);
					case (byte)MethodScriptSemantics.ImplType.NativeIndexer:
						return MethodScriptSemantics.NativeIndexer();
					case (byte)MethodScriptSemantics.ImplType.NativeOperator:
						return MethodScriptSemantics.NativeOperator();
					case (byte)MethodScriptSemantics.ImplType.NotUsableFromScript:
						return MethodScriptSemantics.NotUsableFromScript();
					default:
						throw new Exception();
				}
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, method.FullyQualifiedName(), method.ContainingAssembly.Name);
				return MethodScriptSemantics.NotUsableFromScript();
			}
		}

		private static object[] SerializeMethodSemantics(MethodScriptSemantics semantics) {
			switch (semantics.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod:
					return new object[] { (byte)semantics.Type, semantics.Name, semantics.IgnoreGenericArguments, semantics.GeneratedMethodName != null, semantics.ExpandParams, semantics.EnumerateAsArray };
				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument:
					return new object[] { (byte)semantics.Type, semantics.Name, semantics.IgnoreGenericArguments, semantics.GeneratedMethodName != null, semantics.ExpandParams, semantics.EnumerateAsArray };
				case MethodScriptSemantics.ImplType.InlineCode:
					return new object[] { (byte)semantics.Type, semantics.LiteralCode, semantics.EnumerateAsArray, semantics.GeneratedMethodName, semantics.NonVirtualInvocationLiteralCode, semantics.NonExpandedFormLiteralCode };
				case MethodScriptSemantics.ImplType.NativeIndexer:
					return new object[] { (byte)semantics.Type };
				case MethodScriptSemantics.ImplType.NativeOperator:
					return new object[] { (byte)semantics.Type };
				case MethodScriptSemantics.ImplType.NotUsableFromScript:
					return new object[] { (byte)semantics.Type };
				default:
					throw new ArgumentException("Invalid script semantics " + semantics.Type);
			}
		}

		private ConstructorScriptSemantics LoadConstructorSemantics(IMethodSymbol method) {
			try {
				var data = GetData(method, ScriptSemanticsAttribute);
				switch ((byte)data[0]) {
					case (byte)ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						return ConstructorScriptSemantics.Unnamed((bool)data[1], (bool)data[2], (bool)data[3]);
					case (byte)ConstructorScriptSemantics.ImplType.NamedConstructor:
						return ConstructorScriptSemantics.Named((string)data[1], (bool)data[2], (bool)data[3], (bool)data[4]);
					case (byte)ConstructorScriptSemantics.ImplType.StaticMethod:
						return ConstructorScriptSemantics.StaticMethod((string)data[1], (bool)data[2], (bool)data[3], (bool)data[4]);
					case (byte)ConstructorScriptSemantics.ImplType.InlineCode:
						return ConstructorScriptSemantics.InlineCode((string)data[1], (bool)data[2], (string)data[3]);
					case (byte)ConstructorScriptSemantics.ImplType.Json:
						return ConstructorScriptSemantics.Json(data.Skip(2).Select(d => method.ContainingType.GetMembers((string)d).Single()), (bool)data[1]);
					case (byte)ConstructorScriptSemantics.ImplType.NotUsableFromScript:
						return ConstructorScriptSemantics.NotUsableFromScript();
					default:
						throw new Exception();
				}
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, method.FullyQualifiedName(), method.ContainingAssembly.Name);
				return ConstructorScriptSemantics.NotUsableFromScript();
			}
		}

		private static object[] SerializeConstructorSemantics(ConstructorScriptSemantics semantics) {
			switch (semantics.Type) {
				case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
					return new object[] { (byte)semantics.Type, semantics.GenerateCode, semantics.ExpandParams, semantics.SkipInInitializer };
				case ConstructorScriptSemantics.ImplType.NamedConstructor:
					return new object[] { (byte)semantics.Type, semantics.Name, semantics.GenerateCode, semantics.ExpandParams, semantics.SkipInInitializer };
				case ConstructorScriptSemantics.ImplType.StaticMethod:
					return new object[] { (byte)semantics.Type, semantics.Name, semantics.GenerateCode, semantics.ExpandParams, semantics.SkipInInitializer };
				case ConstructorScriptSemantics.ImplType.InlineCode:
					return new object[] { (byte)semantics.Type, semantics.LiteralCode, semantics.SkipInInitializer, semantics.NonExpandedFormLiteralCode };
				case ConstructorScriptSemantics.ImplType.Json:
					return new object[] { (byte)semantics.Type, semantics.SkipInInitializer }.Concat(semantics.ParameterToMemberMap.Select(m => m.Name)).ToArray();
				case ConstructorScriptSemantics.ImplType.NotUsableFromScript:
					return new object[] { (byte)semantics.Type };
				default:
					throw new ArgumentException("Invalid script semantics " + semantics.Type);
			}
		}

		private PropertyScriptSemantics LoadPropertySemantics(IPropertySymbol property) {
			try {
				var data = GetData(property, ScriptSemanticsAttribute);
				switch ((byte)data[0]) {
					case (byte)PropertyScriptSemantics.ImplType.GetAndSetMethods:
						return PropertyScriptSemantics.GetAndSetMethods(property.GetMethod != null ? GetMethodSemantics(property.GetMethod) : null, property.SetMethod != null ? GetMethodSemantics(property.SetMethod) : null);
					case (byte)PropertyScriptSemantics.ImplType.Field:
						return PropertyScriptSemantics.Field((string)data[1]);
					case (byte)PropertyScriptSemantics.ImplType.NotUsableFromScript:
						return PropertyScriptSemantics.NotUsableFromScript();
					default:
						throw new Exception();
				}
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, property.FullyQualifiedName(), property.ContainingAssembly.Name);
				return PropertyScriptSemantics.NotUsableFromScript();
			}
		}

		private static object[] SerializePropertySemantics(PropertyScriptSemantics semantics) {
			switch (semantics.Type) {
				case PropertyScriptSemantics.ImplType.GetAndSetMethods:
					return new object[] { (byte)semantics.Type };
				case PropertyScriptSemantics.ImplType.Field:
					return new object[] { (byte)semantics.Type, semantics.FieldName };
				case PropertyScriptSemantics.ImplType.NotUsableFromScript:
					return new object[] { (byte)semantics.Type };
				default:
					throw new ArgumentException("Invalid script semantics " + semantics.Type);
			}
		}

		private DelegateScriptSemantics LoadDelegateSemantics(INamedTypeSymbol delegateType) {
			try {
				var data = GetData(delegateType, ScriptSemanticsAttribute);
				return new DelegateScriptSemantics((bool)data[0], (bool)data[1]);
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, delegateType.FullyQualifiedName(), delegateType.ContainingAssembly.Name);
				return new DelegateScriptSemantics();
			}
		}

		private static object[] SerializeDelegateSemantics(DelegateScriptSemantics semantics) {
			return new object[] { semantics.ExpandParams, semantics.BindThisToFirstParameter };
		}

		private FieldScriptSemantics LoadFieldSemantics(IFieldSymbol field) {
			try {
				var data = GetData(field, ScriptSemanticsAttribute);
				switch ((byte)data[0]) {
					case (byte)FieldScriptSemantics.ImplType.Field:
						return FieldScriptSemantics.Field((string)data[1]);
					case (byte)FieldScriptSemantics.ImplType.Constant:
						if (data[1] is bool?)
							return FieldScriptSemantics.BooleanConstant((bool)data[1], (string)data[2]);
						else if (data[1] is double?)
							return FieldScriptSemantics.NumericConstant((double)data[1], (string)data[2]);
						else if (data[1] is string)
							return FieldScriptSemantics.StringConstant((string)data[1], (string)data[2]);
						else if (data[1] == null)
							return FieldScriptSemantics.NullConstant((string)data[2]);
						else
							throw new Exception();
					case (byte)PropertyScriptSemantics.ImplType.NotUsableFromScript:
						return FieldScriptSemantics.NotUsableFromScript();
					default:
						throw new Exception();
				}
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, field.FullyQualifiedName(), field.ContainingAssembly.Name);
				return FieldScriptSemantics.NotUsableFromScript();
			}
		}

		private static object[] SerializeFieldSemantics(FieldScriptSemantics semantics) {
			switch (semantics.Type) {
				case FieldScriptSemantics.ImplType.Field:
					return new object[] { (byte)semantics.Type, semantics.Name };
				case FieldScriptSemantics.ImplType.Constant:
					if (semantics.Value is bool || semantics.Value is double || semantics.Value is string || semantics.Value == null)
						return new object[] { (byte)semantics.Type, semantics.Value, semantics.Name };
					else
						throw new ArgumentException("Invalid script constant " + semantics.Value);
				case FieldScriptSemantics.ImplType.NotUsableFromScript:
					return new object[] { (byte)semantics.Type };
				default:
					throw new ArgumentException("Invalid script semantics " + semantics.Type);
			}
		}

		private EventScriptSemantics LoadEventSemantics(IEventSymbol evt) {
			try {
				var data = GetData(evt, ScriptSemanticsAttribute);
				switch ((byte)data[0]) {
					case (byte)EventScriptSemantics.ImplType.AddAndRemoveMethods:
						return EventScriptSemantics.AddAndRemoveMethods(GetMethodSemantics(evt.AddMethod), GetMethodSemantics(evt.RemoveMethod));
					case (byte)EventScriptSemantics.ImplType.NotUsableFromScript:
						return EventScriptSemantics.NotUsableFromScript();
					default:
						throw new Exception();
				}
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, evt.FullyQualifiedName(), evt.ContainingAssembly.Name);
				return EventScriptSemantics.NotUsableFromScript();
			}
		}

		private static object[] SerializeEventSemantics(EventScriptSemantics semantics) {
			switch (semantics.Type) {
				case EventScriptSemantics.ImplType.AddAndRemoveMethods:
					return new object[] { (byte)semantics.Type };
				case EventScriptSemantics.ImplType.NotUsableFromScript:
					return new object[] { (byte)semantics.Type };
				default:
					throw new ArgumentException("Invalid script semantics " + semantics.Type);
			}
		}

		private IReadOnlyList<string> LoadUsedInstanceMemberNames(INamedTypeSymbol type) {
			try {
				var data = GetData(type, UsedMemberNamesAttribute);
				return ImmutableArray.CreateRange(data.Cast<string>());
			}
			catch (Exception) {
				_errorReporter.Message(Messages._7995, type.FullyQualifiedName(), type.ContainingAssembly.Name);
				return ImmutableArray<string>.Empty;
			}
		}

		private object[] GetData(ISymbol symbol, string attributeClass) {
			var attr = symbol.GetAttributes().SingleOrDefault(a => a.AttributeClass.Name == attributeClass && a.AttributeClass.ContainingNamespace.FullyQualifiedName() == AttributeNamespace);
			return attr.ConstructorArguments[0].Values.Select(c => c.Value).ToArray();
		}

		public bool IsMemberNameAvailable(INamedTypeSymbol type, string name, bool isStatic) {
			if (isStatic) {
				_errorReporter.InternalError("Cannot check for used static names in types from referenced assemblies");
				return true;
			}

			return !GetUsedInstanceMemberNames(type).Contains(name);
		}

		public TypeScriptSemantics GetTypeSemantics(INamedTypeSymbol typeDefinition) {
			TypeScriptSemantics result;
			if (!_typeSemantics.TryGetValue(typeDefinition, out result)) {
				if (!typeDefinition.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + typeDefinition.FullyQualifiedName());
					result = TypeScriptSemantics.NotUsableFromScript();
				}
				else
					result = _typeSemantics[typeDefinition] = LoadTypeSemantics(typeDefinition);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + typeDefinition.FullyQualifiedName() + " could not be found");
				result = TypeScriptSemantics.NotUsableFromScript();
			}
			return result;
		}

		public MethodScriptSemantics GetMethodSemantics(IMethodSymbol method) {
			MethodScriptSemantics result;
			if (!_methodSemantics.TryGetValue(method, out result)) {
				if (!method.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + method.FullyQualifiedName());
					result = MethodScriptSemantics.NotUsableFromScript();
				}
				else
					result = _methodSemantics[method] = LoadMethodSemantics(method);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + method.FullyQualifiedName() + " could not be found");
				result = MethodScriptSemantics.NotUsableFromScript();
			}
			return result;
		}

		public ConstructorScriptSemantics GetConstructorSemantics(IMethodSymbol method) {
			ConstructorScriptSemantics result;
			if (!_constructorSemantics.TryGetValue(method, out result)) {
				if (!method.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + method.FullyQualifiedName());
					result = ConstructorScriptSemantics.NotUsableFromScript();
				}
				else
					result = _constructorSemantics[method] = LoadConstructorSemantics(method);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + method.FullyQualifiedName() + " could not be found");
				result = ConstructorScriptSemantics.NotUsableFromScript();
			}
			return result;
		}

		public PropertyScriptSemantics GetPropertySemantics(IPropertySymbol property) {
			PropertyScriptSemantics result;
			if (!_propertySemantics.TryGetValue(property, out result)) {
				if (!property.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + property.FullyQualifiedName());
					result = PropertyScriptSemantics.NotUsableFromScript();
				}
				else
					result = _propertySemantics[property] = LoadPropertySemantics(property);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + property.FullyQualifiedName() + " could not be found");
				result = PropertyScriptSemantics.NotUsableFromScript();
			}
			return result;
		}

		public DelegateScriptSemantics GetDelegateSemantics(INamedTypeSymbol delegateType) {
			DelegateScriptSemantics result;
			if (!_delegateSemantics.TryGetValue(delegateType, out result)) {
				if (!delegateType.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + delegateType.FullyQualifiedName());
					result = new DelegateScriptSemantics();
				}
				else
					result = _delegateSemantics[delegateType] = LoadDelegateSemantics(delegateType);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + delegateType.FullyQualifiedName() + " could not be found");
				result = new DelegateScriptSemantics();
			}
			return result;
		}

		public FieldScriptSemantics GetFieldSemantics(IFieldSymbol field) {
			FieldScriptSemantics result;
			if (!_fieldSemantics.TryGetValue(field, out result)) {
				if (!field.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + field.FullyQualifiedName());
					result = FieldScriptSemantics.NotUsableFromScript();
				}
				else
					result = _fieldSemantics[field] = LoadFieldSemantics(field);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + field.FullyQualifiedName() + " could not be found");
				result = FieldScriptSemantics.NotUsableFromScript();
			}
			return result;
		}

		public EventScriptSemantics GetEventSemantics(IEventSymbol evt) {
			EventScriptSemantics result;
			if (!_eventSemantics.TryGetValue(evt, out result)) {
				if (!evt.IsExternallyVisible()) {
					_errorReporter.InternalError("Should not get information for non-public symbol " + evt.FullyQualifiedName());
					result = EventScriptSemantics.NotUsableFromScript();
				}
				else
					result = _eventSemantics[evt] = LoadEventSemantics(evt);
			}
			if (result == null) {
				_errorReporter.InternalError("Semantics for " + evt.FullyQualifiedName() + " could not be found");
				result = EventScriptSemantics.NotUsableFromScript();
			}
			return result;
		}

		public IReadOnlyList<string> GetUsedInstanceMemberNames(INamedTypeSymbol type) {
			IReadOnlyList<string> result;
			if (!_usedInstanceMemberNames.TryGetValue(type, out result)) {
				if (!type.IsExternallyVisible())
					_errorReporter.InternalError("Should not get information for non-public symbol " + type.FullyQualifiedName());
				result = _usedInstanceMemberNames[type] = LoadUsedInstanceMemberNames(type);
			}
			if (result == null) {
				_errorReporter.InternalError("Used instance member names for " + type.FullyQualifiedName() + " could not be found");
				result = ImmutableArray<string>.Empty;
			}
			return result;
		}

		public void ReserveMemberName(INamedTypeSymbol type, string name, bool isStatic) {
			throw new NotSupportedException();
		}

		public void SetMethodSemantics(IMethodSymbol method, MethodScriptSemantics semantics) {
			throw new NotSupportedException();
		}

		public void SetConstructorSemantics(IMethodSymbol method, ConstructorScriptSemantics semantics) {
			throw new NotSupportedException();
		}

		public void SetPropertySemantics(IPropertySymbol property, PropertyScriptSemantics semantics) {
			throw new NotSupportedException();
		}

		public void SetFieldSemantics(IFieldSymbol field, FieldScriptSemantics semantics) {
			throw new NotSupportedException();
		}

		public void SetEventSemantics(IEventSymbol evt, EventScriptSemantics semantics) {
			throw new NotSupportedException();
		}

		public string GetAutoPropertyBackingFieldName(IPropertySymbol property) {
			throw new NotSupportedException();
		}

		public bool ShouldGenerateAutoPropertyBackingField(IPropertySymbol property) {
			throw new NotSupportedException();
		}

		public string GetAutoEventBackingFieldName(IEventSymbol evt) {
			throw new NotSupportedException();
		}

		public bool ShouldGenerateAutoEventBackingField(IEventSymbol evt) {
			throw new NotSupportedException();
		}

		#region Write

		private static TypeReference CreateTypeReference(ModuleDefinition module, INamedTypeSymbol type) {
			var assemblyName = type.ContainingAssembly.Identity.ToAssemblyName().FullName;
			if (module.Assembly.Name.FullName == assemblyName)
				return new TypeReference(type.ContainingNamespace.FullyQualifiedName(), type.MetadataName, module, module);

			var asm = module.AssemblyReferences.SingleOrDefault(n => n.FullName == assemblyName);
			if (asm == null)
				throw new InvalidOperationException("The processed module does not reference the assembly " + assemblyName);
			return new TypeReference(type.ContainingNamespace.FullyQualifiedName(), type.MetadataName, module, asm);
		}

		private static CustomAttributeArgument MakeAttributeArgument(object arg, TypeSystem typeSystem) {
			if (arg == null)
				return new CustomAttributeArgument(typeSystem.Object, new CustomAttributeArgument(typeSystem.String, null));	// Don't know why null values haved to be typed as string, but those are the only null values we use.
			else if (arg is byte)
				return new CustomAttributeArgument(typeSystem.Object, new CustomAttributeArgument(typeSystem.Byte, arg));
			else if (arg is string)
				return new CustomAttributeArgument(typeSystem.Object, new CustomAttributeArgument(typeSystem.String, arg));
			else if (arg is bool)
				return new CustomAttributeArgument(typeSystem.Object, new CustomAttributeArgument(typeSystem.Boolean, arg));
			else if (arg is double)
				return new CustomAttributeArgument(typeSystem.Object, new CustomAttributeArgument(typeSystem.Double, arg));
			else
				throw new ArgumentException("Unsupported attribute argument " + arg);
		}

		private static CustomAttributeArgument[] MakeAttributeArguments(object[] data, TypeSystem typeSystem) {
			var result = new CustomAttributeArgument[data.Length];
			for (int i = 0; i < data.Length; i++) {
				result[i] = MakeAttributeArgument(data[i], typeSystem);
			}
			return result;
		}

		private static CustomAttribute CreateAttribute(MethodReference ctor, ArrayType arrayOfObject, object[] data) {
			var result = new CustomAttribute(ctor);
			result.ConstructorArguments.Add(new CustomAttributeArgument(arrayOfObject, MakeAttributeArguments(data, ctor.Module.TypeSystem)));
			return result;
		}

		private static MethodReference GetAttributeCtor(INamedTypeSymbol attrType, AssemblyDefinition assembly, ArrayType arrayOfObject) {
			var attr = CreateTypeReference(assembly.MainModule, attrType);
			var ctor = new MethodReference(".ctor", assembly.MainModule.TypeSystem.Void, attr);
			ctor.Parameters.Add(new ParameterDefinition(arrayOfObject));
			return ctor;
		}

		private static string GetCecilReferenceAssemblyName(IMetadataScope scope) {
			switch (scope.MetadataScopeType) {
				case MetadataScopeType.ModuleDefinition:
					return ((ModuleDefinition)scope).Assembly.FullName;
				case MetadataScopeType.AssemblyNameReference:
					return ((AssemblyNameReference)scope).FullName;
				case MetadataScopeType.ModuleReference:
					throw new NotSupportedException("Module references are not supported");
				default:
					throw new NotSupportedException("Unknown reference type");
			}
		}

		private static bool TypesMatch(TypeReference cecilType, Tuple<ITypeSymbol, bool> typeSymbol) {
			if (cecilType.IsByReference) {
				return typeSymbol.Item2 && TypesMatch(cecilType.GetElementType(), Tuple.Create(typeSymbol.Item1, false));
			}
			else if (typeSymbol.Item2) {
				return false;
			}

			if (cecilType.IsArray) {
				var cecilArray = (ArrayType)cecilType;
				var at = typeSymbol.Item1 as IArrayTypeSymbol;
				return at != null && at.Rank == cecilArray.Rank && TypesMatch(cecilArray.ElementType, Tuple.Create(at.ElementType, false));
			}
			else if (typeSymbol.Item1.TypeKind == TypeKind.ArrayType) {
				return false;
			}

 			if (cecilType.IsGenericParameter) {
				var cecilParam = (GenericParameter)cecilType;
				var tp = typeSymbol.Item1 as ITypeParameterSymbol;
				return tp != null && ((tp.TypeParameterKind == TypeParameterKind.Type) == (cecilParam.Type == GenericParameterType.Type)) && tp.Ordinal == cecilParam.Position;
 			}
 			else if (typeSymbol.Item1.TypeKind == TypeKind.TypeParameter) {
				return false;
			}

			if (cecilType.IsGenericInstance) {
				var nt = typeSymbol.Item1 as INamedTypeSymbol;
				var cecilInstance = (GenericInstanceType)cecilType;
				if (nt == null || !TypesMatch(cecilInstance.ElementType, Tuple.Create(typeSymbol.Item1.OriginalDefinition, false)))
					return false;
				var typeArguments = nt.GetAllTypeArguments();
				for (int i = 0; i < typeArguments.Count; i++) {
					if (!TypesMatch(cecilInstance.GenericArguments[i], Tuple.Create(typeArguments[i], false)))
						return false;
				}
				return true;
			}
			else if (!Equals(typeSymbol.Item1, typeSymbol.Item1.OriginalDefinition)) {
				return false;
			}

			if (cecilType.DeclaringType != null) {
				if (typeSymbol.Item1.ContainingType == null)
					return false;
				return TypesMatch(cecilType.DeclaringType, Tuple.Create((ITypeSymbol)typeSymbol.Item1.ContainingType, false)) && cecilType.Name == typeSymbol.Item1.MetadataName;
			}
			else if (typeSymbol.Item1.ContainingType != null) {
				return false;
			}

			return cecilType.Name == typeSymbol.Item1.MetadataName && typeSymbol.Item1.ContainingNamespace.FullyQualifiedName() == cecilType.Namespace && typeSymbol.Item1.ContainingAssembly.Identity.GetDisplayName() == GetCecilReferenceAssemblyName(cecilType.Scope);
		}

		private static bool ParametersMatch(ICollection<ParameterDefinition> cecilParameters, ICollection<IParameterSymbol> parameterSymbols) {
			if (cecilParameters.Count != parameterSymbols.Count)
				return false;

			using (var cecilEnumerator = cecilParameters.GetEnumerator())
			using (var symbolEnumerator = parameterSymbols.GetEnumerator()) {
				while (cecilEnumerator.MoveNext()) {
					symbolEnumerator.MoveNext();
					var cecilParam = cecilEnumerator.Current;
					var paramSymbol = symbolEnumerator.Current;
					if (!TypesMatch(cecilParam.ParameterType, Tuple.Create(paramSymbol.Type, paramSymbol.RefKind != RefKind.None)))
						return false;
				}
			}
			return true;
		}

		private static IMethodSymbol MapMember(MethodDefinition member, IEnumerable<ISymbol> candidates) {
			return candidates.OfType<IMethodSymbol>().Single(m => m.MetadataName == member.Name && m.Arity == member.GenericParameters.Count && TypesMatch(member.ReturnType, Tuple.Create(m.ReturnType, false)) && ParametersMatch(member.Parameters, m.Parameters));
		}

		private static IFieldSymbol MapMember(FieldDefinition member, IEnumerable<ISymbol> candidates) {
			return candidates.OfType<IFieldSymbol>().Single(f => f.MetadataName == member.Name);
		}

		private static IPropertySymbol MapMember(PropertyDefinition member, IEnumerable<ISymbol> candidates) {
			return candidates.OfType<IPropertySymbol>().Single(m => m.MetadataName == member.Name && TypesMatch(member.PropertyType, Tuple.Create(m.Type, false)) && ParametersMatch(member.Parameters, m.Parameters));
		}

		private static IEventSymbol MapMember(EventDefinition member, IEnumerable<ISymbol> candidates) {
			return candidates.OfType<IEventSymbol>().Single(f => f.MetadataName == member.Name);
		}

		private static object[] GetSemanticData(ISymbol symbol, IMetadataImporter importer) {
			if (symbol is IMethodSymbol) {
				var method = (IMethodSymbol)symbol;
				if (method.MethodKind == MethodKind.Constructor)
					return SerializeConstructorSemantics(importer.GetConstructorSemantics(method));
				else
					return SerializeMethodSemantics(importer.GetMethodSemantics(method));
			}
			else if (symbol is IFieldSymbol) {
				return SerializeFieldSemantics(importer.GetFieldSemantics((IFieldSymbol)symbol));
			}
			else if (symbol is IPropertySymbol) {
				return SerializePropertySemantics(importer.GetPropertySemantics((IPropertySymbol)symbol));
			}
			else if (symbol is IEventSymbol) {
				return SerializeEventSemantics(importer.GetEventSemantics((IEventSymbol)symbol));
			}
			else {
				throw new ArgumentException("Unsupported symbol " + symbol);
			}
		}

		private static bool IsExternallyVisible(MethodDefinition m) {
			return m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
		}

		private static bool IsExternallyVisible(FieldDefinition m) {
			return (m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly) && (!m.IsRuntimeSpecialName || !string.Equals(m.Name, "value__", StringComparison.Ordinal));
		}

		private static bool IsExternallyVisible(PropertyDefinition p) {
			return (p.GetMethod != null && IsExternallyVisible(p.GetMethod)) || (p.SetMethod != null && IsExternallyVisible(p.SetMethod));
		}

		private static bool IsExternallyVisible(EventDefinition e) {
			return (e.AddMethod != null && IsExternallyVisible(e.AddMethod)) || (e.RemoveMethod != null && IsExternallyVisible(e.RemoveMethod));
		}

		private static string GetCecilName(INamedTypeSymbol symbol) {
			if (symbol.ContainingType != null)
				return GetCecilName(symbol.ContainingType) + "/" + symbol.MetadataName;
			else if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
				return symbol.ContainingNamespace.FullyQualifiedName() + "." + symbol.MetadataName;
			else
				return symbol.MetadataName;
		}

		private static CustomAttribute CreateSerializableAttribute(Compilation compilation, AttributeData data, AssemblyDefinition assembly) {
			var type = compilation.GetTypeByMetadataName(AttributeStore.ScriptSerializableAttribute);
			var serializableTypeRef = CreateTypeReference(assembly.MainModule, type);
			var ctorRef = new MethodReference(".ctor", assembly.MainModule.TypeSystem.Void, serializableTypeRef);
			ctorRef.Parameters.Add(new ParameterDefinition(assembly.MainModule.TypeSystem.String));
			var result = new CustomAttribute(ctorRef);
			var typeCheckCode = data.NamedArguments.FirstOrDefault(a => a.Key == "TypeCheckCode").Value.Value;
			result.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.String, typeCheckCode));
			return result;
		}

		public static void Write(Compilation compilation, AssemblyDefinition assembly, IMetadataImporter importer) {
			var arrayOfObject = new ArrayType(assembly.MainModule.TypeSystem.Object);
			var scriptSemanticsAttributeCtor = GetAttributeCtor(compilation.GetTypeByMetadataName(AttributeNamespace + "." + ScriptSemanticsAttribute), assembly, arrayOfObject);
			var usedMemberNamesAttributeCtor = GetAttributeCtor(compilation.GetTypeByMetadataName(AttributeNamespace + "." + UsedMemberNamesAttribute), assembly, arrayOfObject);

			foreach (var type in compilation.Assembly.GetAllTypes().Where(type => type.IsExternallyVisible())) {
				var typeDef = assembly.MainModule.GetType(GetCecilName(type));
				if (type.TypeKind == TypeKind.Delegate) {
					typeDef.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, SerializeDelegateSemantics(importer.GetDelegateSemantics(type))));
				}
				else {
					typeDef.CustomAttributes.Add(CreateAttribute(usedMemberNamesAttributeCtor, arrayOfObject, importer.GetUsedInstanceMemberNames(type).ToArray<object>()));
					typeDef.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, SerializeTypeSemantics(importer.GetTypeSemantics(type))));

					var serializableAttr = type.GetAttributes().SingleOrDefault(a => a.AttributeClass.Name == typeof(SerializableAttribute).Name && a.AttributeClass.ContainingNamespace.FullyQualifiedName() == typeof(SerializableAttribute).Namespace);
					if (serializableAttr != null) {
						typeDef.CustomAttributes.Add(CreateSerializableAttribute(compilation, serializableAttr, assembly));
					}
				}

				var candidates = type.GetMembers();

				bool hasDefaultConstructor = false;
				foreach (var m in typeDef.Methods.Where(IsExternallyVisible)) {
					var symbol = MapMember(m, candidates);
					var data = GetSemanticData(symbol, importer);
					m.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, data));
					if (m.IsConstructor && m.Parameters.Count == 0)
						hasDefaultConstructor = true;
				}

				if (type.TypeKind == TypeKind.Struct && !hasDefaultConstructor) {
					var typeCtor = type.InstanceConstructors.Single(c => c.Parameters.Length == 0);
					var ctor = new MethodDefinition(".ctor", MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Public, typeDef.Module.TypeSystem.Void);
					ctor.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, GetSemanticData(typeCtor, importer)));
					ctor.Body = new MethodBody(ctor);
					typeDef.Methods.Add(ctor);
				}

				foreach (var f in typeDef.Fields.Where(IsExternallyVisible)) {
					var symbol = MapMember(f, candidates);
					var data = GetSemanticData(symbol, importer);
					f.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, data));
				}

				foreach (var p in typeDef.Properties.Where(IsExternallyVisible)) {
					var symbol = MapMember(p, candidates);
					var data = GetSemanticData(symbol, importer);
					p.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, data));
				}

				foreach (var e in typeDef.Events.Where(IsExternallyVisible)) {
					var symbol = MapMember(e, candidates);
					var data = GetSemanticData(symbol, importer);
					e.CustomAttributes.Add(CreateAttribute(scriptSemanticsAttributeCtor, arrayOfObject, data));
				}
			}
		}

		#endregion
	}
}
