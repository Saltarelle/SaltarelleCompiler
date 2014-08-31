using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Plugin {
	public static class MetadataUtils {
		public static string MakeCamelCase(string s) {
			if (String.IsNullOrEmpty(s))
				return s;
			if (s.Equals("ID", StringComparison.Ordinal))
				return "id";

			bool hasNonUppercase = false;
			int numUppercaseChars = 0;
			for (int index = 0; index < s.Length; index++) {
				if (Char.IsUpper(s, index)) {
					numUppercaseChars++;
				}
				else {
					hasNonUppercase = true;
					break;
				}
			}

			if ((!hasNonUppercase && s.Length != 1) || numUppercaseChars == 0)
				return s;
			else if (numUppercaseChars > 1)
				return s.Substring(0, numUppercaseChars - 1).ToLower(CultureInfo.InvariantCulture) + s.Substring(numUppercaseChars - 1);
			else if (s.Length == 1)
				return s.ToLower(CultureInfo.InvariantCulture);
			else
				return Char.ToLower(s[0], CultureInfo.InvariantCulture) + s.Substring(1);
		}

		public static bool? IsAutoProperty(IPropertySymbol property) {
			if (property.Locations.IsEmpty)
				return null;
			return false;
			#warning TODO
			//return property.GetMethod != null && property.SetMethod != null && property.Getmethod.BodyRegion == default(DomRegion) && property.SetMethod.BodyRegion == default(DomRegion);
		}

		public static bool? IsAutoEvent(IEventSymbol evt) {
			if (evt.Locations.IsEmpty)
				return null;
			return false;
			#warning TODO
			//return evt.AddAccessor != null && evt.RemoveAccessor != null && evt.AddAccessor.BodyRegion == default(DomRegion) && evt.RemoveAccessor.BodyRegion == default(DomRegion);
		}

		public static bool IsSerializable(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<ScriptSerializableAttribute>() || (type.GetAllBaseTypes().Any(t => t.FullyQualifiedName() == "System.Record") && type.FullyQualifiedName() != "System.Record");
		}

		public static string GetSerializableTypeCheckCode(INamedTypeSymbol type, IAttributeStore attributeStore) {
			var attr = attributeStore.AttributesFor(type).GetAttribute<ScriptSerializableAttribute>();
			return attr != null ? attr.TypeCheckCode : null;
		}

		public static bool DoesTypeObeyTypeSystem(INamedTypeSymbol type, IAttributeStore attributeStore) {
			var ia = attributeStore.AttributesFor(type).GetAttribute<ImportedAttribute>();
			return ia == null || ia.ObeysTypeSystem;
		}

		public static bool IsMixin(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<MixinAttribute>();
		}

		public static bool IsImported(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<ImportedAttribute>();
		}

		public static bool IsResources(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<ResourcesAttribute>();
		}

		public static bool IsNamedValues(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<NamedValuesAttribute>();
		}

		public static bool IsGlobalMethods(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<GlobalMethodsAttribute>();
		}

		public static bool IsPreserveMemberCase(INamedTypeSymbol type, IAttributeStore attributeStore) {
			var pmca = attributeStore.AttributesFor(type).GetAttribute<PreserveMemberCaseAttribute>() ?? attributeStore.AttributesFor(type.ContainingAssembly).GetAttribute<PreserveMemberCaseAttribute>();
			return pmca != null && pmca.Preserve;
		}

		public static bool IsPreserveMemberNames(INamedTypeSymbol type, IAttributeStore attributeStore) {
			return IsImported(type, attributeStore) || IsGlobalMethods(type, attributeStore);
		}

		public static bool OmitNullableChecks(Compilation compilation, IAttributeStore attributeStore) {
			var sca = attributeStore.AttributesFor(compilation.Assembly).GetAttribute<ScriptSharpCompatibilityAttribute>();
			return sca != null && sca.OmitNullableChecks;
		}

		public static bool OmitDowncasts(Compilation compilation, IAttributeStore attributeStore) {
			var sca = attributeStore.AttributesFor(compilation.Assembly).GetAttribute<ScriptSharpCompatibilityAttribute>();
			return sca != null && sca.OmitDowncasts;
		}

		public static bool IsAsyncModule(IAssemblySymbol assembly, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(assembly).HasAttribute<AsyncModuleAttribute>();
		}

		public static IEnumerable<KeyValuePair<string,string>> GetAdditionalDependencies(IAssemblySymbol assembly, IAttributeStore attributeStore)
		{
			return attributeStore.AttributesFor(assembly).GetAttributes<AdditionalDependencyAttribute>()
				.Select(a => new KeyValuePair<string,string>(a.ModuleName, a.InstanceName));
		}

		public static string GetModuleName(IAssemblySymbol assembly, IAttributeStore attributeStore) {
			var mna = attributeStore.AttributesFor(assembly).GetAttribute<ModuleNameAttribute>();
			return (mna != null && !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null);
		}

		public static string GetModuleName(INamedTypeSymbol type, IAttributeStore attributeStore) {
			for (var current = type; current != null; current = current.ContainingType) {
				var mna = attributeStore.AttributesFor(type).GetAttribute<ModuleNameAttribute>();
				if (mna != null)
					return !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null;
			}
			return GetModuleName(type.ContainingAssembly, attributeStore);
		}

		public static bool? ShouldGenericArgumentsBeIncluded(INamedTypeSymbol type, IAttributeStore attributeStore) {
			var attributes = attributeStore.AttributesFor(type);

			var iga = attributes.GetAttribute<IncludeGenericArgumentsAttribute>();
			if (iga != null)
				return iga.Include;
			var imp = attributes.GetAttribute<ImportedAttribute>();
			if (imp != null)
				return false;
			var def = attributeStore.AttributesFor(type.ContainingAssembly).GetAttribute<IncludeGenericArgumentsDefaultAttribute>();
			switch (def != null ? def.TypeDefault : GenericArgumentsDefault.IncludeExceptImported) {
				case GenericArgumentsDefault.IncludeExceptImported:
					return true;
				case GenericArgumentsDefault.Ignore:
					return false;
				case GenericArgumentsDefault.RequireExplicitSpecification:
					return null;
				default:
					throw new ArgumentException("Invalid generic arguments default " + def.TypeDefault);
			}
		}

		public static bool? ShouldGenericArgumentsBeIncluded(IMethodSymbol method, IAttributeStore attributeStore) {
			var iga = attributeStore.AttributesFor(method).GetAttribute<IncludeGenericArgumentsAttribute>();
			if (iga != null)
				return iga.Include;
			var imp = attributeStore.AttributesFor(method.ContainingType).GetAttribute<ImportedAttribute>();
			if (imp != null)
				return false;
			var def = attributeStore.AttributesFor(method.ContainingAssembly).GetAttribute<IncludeGenericArgumentsDefaultAttribute>();
			switch (def != null ? def.MethodDefault : GenericArgumentsDefault.IncludeExceptImported) {
				case GenericArgumentsDefault.IncludeExceptImported:
					return true;
				case GenericArgumentsDefault.Ignore:
					return false;
				case GenericArgumentsDefault.RequireExplicitSpecification:
					return null;
				default:
					throw new ArgumentException("Invalid generic arguments default " + def.TypeDefault);
			}
		}

		public static ISymbol UnwrapValueTypeConstructor(ISymbol m) {
			if (m is IMethodSymbol && !m.IsStatic && m.ContainingType.TypeKind == TypeKind.Struct && ((IMethodSymbol)m).MethodKind == MethodKind.Constructor && ((IMethodSymbol)m).Parameters.Length == 0) {
				var other = m.ContainingType.GetMembers().OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Constructor).SingleOrDefault(c => c.Parameters.Length == 1 && c.Parameters[0].Type.FullyQualifiedName() == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName);
				if (other != null)
					return other;
			}
			return m;
		}

		public static bool CanBeMinimized(INamedTypeSymbol typeDefinition) {
			return !typeDefinition.IsExternallyVisible();
		}

		public static bool CanBeMinimized(ISymbol member, IAttributeStore attributeStore) {
			return !member.IsExternallyVisible() || attributeStore.AttributesFor(member.ContainingAssembly).HasAttribute<MinimizePublicNamesAttribute>();
		}

		/// <summary>
		/// Determines the preferred name for a member. The first item is the name, the second item is true if the name was explicitly specified.
		/// </summary>
		public static Tuple<string, bool> DeterminePreferredMemberName(ISymbol member, bool minimizeNames, IAttributeStore attributeStore) {
			member = UnwrapValueTypeConstructor(member);

			bool isConstructor = member is IMethodSymbol && ((IMethodSymbol)member).MethodKind == MethodKind.Constructor;
			bool isAccessor = member is IMethodSymbol && ((IMethodSymbol)member).IsAccessor();
			bool isPreserveMemberCase = IsPreserveMemberCase(member.ContainingType, attributeStore);

			string defaultName;
			if (isConstructor) {
				defaultName = "$ctor";
			}
			else if (!CanBeMinimized(member, attributeStore)) {
				defaultName = isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name);
			}
			else {
				if (minimizeNames && member.ContainingType.TypeKind != TypeKind.Interface)
					defaultName = null;
				else
					defaultName = "$" + (isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name));
			}

			var attributes = attributeStore.AttributesFor(member);

			var asa = attributes.GetAttribute<AlternateSignatureAttribute>();
			if (asa != null) {
				var otherMembers = member.ContainingType.GetMembers().OfType<IMethodSymbol>().Where(m => m.Name == member.Name && !attributeStore.AttributesFor(m).HasAttribute<AlternateSignatureAttribute>() && !attributeStore.AttributesFor(m).HasAttribute<NonScriptableAttribute>() && !attributeStore.AttributesFor(m).HasAttribute<InlineCodeAttribute>()).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0], minimizeNames, attributeStore);
				}
				else {
					return Tuple.Create(member.Name, false);	// Error
				}
			}

			var sna = attributes.GetAttribute<ScriptNameAttribute>();
			if (sna != null) {
				string name = sna.Name;
				if (IsNamedValues(member.ContainingType, attributeStore) && (name == "" || !name.IsValidJavaScriptIdentifier())) {
					return Tuple.Create(defaultName, false);	// For named values enum, allow the use to specify an empty or invalid value, which will only be used as the literal value for the field, not for the name.
				}
				if (name == "" && isConstructor)
					name = "$ctor";
				return Tuple.Create(name, true);
			}
			
			if (isConstructor && IsImported(member.ContainingType, attributeStore)) {
				return Tuple.Create("$ctor", true);
			}

			var ica = attributes.GetAttribute<InlineCodeAttribute>();
			if (ica != null) {
				if (ica.GeneratedMethodName != null)
					return Tuple.Create(ica.GeneratedMethodName, true);
			}

			if (attributes.HasAttribute<PreserveCaseAttribute>())
				return Tuple.Create(member.Name, true);

			bool preserveName = (!isConstructor && !isAccessor && (   attributes.HasAttribute<PreserveNameAttribute>()
			                                                       || attributes.HasAttribute<InstanceMethodOnFirstArgumentAttribute>()
			                                                       || IsPreserveMemberNames(member.ContainingType, attributeStore) && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (IsSerializable(member.ContainingType, attributeStore) && !member.IsStatic && (member is IPropertySymbol || member is IFieldSymbol)))
			                                                       || (IsNamedValues(member.ContainingType, attributeStore) && member is IFieldSymbol);

			if (preserveName)
				return Tuple.Create(isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name), true);

			return Tuple.Create(defaultName, false);
		}

		private const string _encodeNumberTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string EncodeNumber(int i, bool ensureValidIdentifier) {
			if (ensureValidIdentifier) {
				string result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1);
				while (i >= _encodeNumberTable.Length - 10) {
					i /= _encodeNumberTable.Length - 10;
					result = _encodeNumberTable.Substring(i % (_encodeNumberTable.Length - 10) + 10, 1) + result;
				}
				return Saltarelle.Compiler.JSModel.Utils.IsJavaScriptReservedWord(result) ? "_" + result : result;
			}
			else {
				string result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1);
				while (i >= _encodeNumberTable.Length) {
					i /= _encodeNumberTable.Length;
					result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1) + result;
				}
				return result;
			}
		}

		public static string GetUniqueName(string preferredName, Func<string, bool> isNameAvailable) {
			string name = preferredName;
			int i = (name == null ? 0 : 1);
			while (name == null || !isNameAvailable(name)) {
				name = preferredName + "$" + EncodeNumber(i, false);
				i++;
			}
			return name;
		}

		public static IMethodSymbol CreateTypeCheckMethod(ITypeSymbol type, Compilation compilation) {
			#warning TODO: Major TODO
			return null;
			//IMethodSymbol method = new DefaultResolvedMethod(new DefaultUnresolvedMethod(type.OriginalDefinition.Parts[0], "IsInstanceOfType"), compilation.TypeResolveContext.WithCurrentTypeDefinition(type.OriginalDefinition));
			//if (type is ParameterizedType)
			//	method = new SpecializedMethod(method, new TypeParameterSubstitution(classTypeArguments: ((ParameterizedType)type).TypeArguments, methodTypeArguments: null));
			//return method;
		}

		public static IMethodSymbol CreateDummyMethodForFieldInitialization(ISymbol member, Compilation compilation) {
			#warning TODO: Major TODO
			return null;
			//var unresolved = new DefaultUnresolvedMethod(member.ContainingType.Parts[0], "initialization for " + member.Name) {
			//	Parameters = { new DefaultUnresolvedParameter(member.ReturnType.ToTypeReference(), "value") },
			//	IsStatic = member.IsStatic,
			//};
			//IMethodSymbol method = new DefaultResolvedMethod(unresolved, compilation.TypeResolveContext.WithCurrentTypeDefinition(member.ContainingType));
			//if (member.ContainingType is ParameterizedType)
			//	method = new SpecializedMethod(method, new TypeParameterSubstitution(classTypeArguments: ((ParameterizedType)member.ContainingType).TypeArguments, methodTypeArguments: null));
			//return method;
		}

		public static bool IsJsGeneric(IMethodSymbol method, IMetadataImporter metadataImporter) {
			return method.TypeParameters.Length > 0 && !metadataImporter.GetMethodSemantics(method).IgnoreGenericArguments;
		}

		public static bool IsJsGeneric(INamedTypeSymbol type, IMetadataImporter metadataImporter) {
			return type.TypeParameters.Length > 0 && !metadataImporter.GetTypeSemantics(type).IgnoreGenericArguments;
		}

		public static bool IsReflectable(ISymbol member, IAttributeStore attributeStore) {
			var ra = attributeStore.AttributesFor(member).GetAttribute<ReflectableAttribute>();
			return ra != null && ra.Reflectable;
		}

		private static ExpressionCompileResult Compile(ResolveResult rr, INamedTypeSymbol currentType, IMethodSymbol currentMethod, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, bool returnValueIsImportant, Dictionary<ISymbol, VariableData> variables, ISet<string> usedVariableNames) {
			variables = variables ?? new Dictionary<ISymbol, VariableData>();
			usedVariableNames = usedVariableNames ?? new HashSet<string>();
			return new ExpressionCompiler(compilation,
			                              metadataImporter,
			                              namer,
			                              runtimeLibrary,
			                              errorReporter,
			                              variables,
			                              new Dictionary<SyntaxNode, NestedFunctionData>(),
			                              t => {
			                                  string name = namer.GetVariableName(null, usedVariableNames);
			                                  ILocalSymbol variable = new SimpleVariable("temporary", Location.None);
			                                  variables[variable] = new VariableData(name, null, false);
			                                  usedVariableNames.Add(name);
			                                  return variable;
			                              },
			                              _ => { throw new Exception("Cannot compile nested functions here"); },
			                              null,
			                              new NestedFunctionContext(ImmutableArray<ISymbol>.Empty),
			                              null,
			                              currentMethod,
			                              currentType
			                             ).Compile(rr, returnValueIsImportant);
		}

		public static ExpressionCompileResult CompileConstructorInvocation(IMethodSymbol constructor, IList<ResolveResult> initializerStatements, INamedTypeSymbol currentType, IMethodSymbol currentMethod, IList<ResolveResult> arguments, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Dictionary<ISymbol, VariableData> variables, ISet<string> usedVariableNames) {
			return Compile(new CSharpInvocationResolveResult(new TypeResolveResult(constructor.ContainingType), constructor, arguments, initializerStatements: initializerStatements), currentType, currentMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, true, variables, usedVariableNames);
		}

		public static JsExpression ConstructAttribute(AttributeData attr, INamedTypeSymbol currentType, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter) {
			errorReporter.Location = attr.ApplicationSyntaxReference.GetSyntax().GetLocation();
			var initializerStatements = attr.NamedArguments.Select(a => new OperatorResolveResult(a.Key.ReturnType, ExpressionType.Assign, new MemberResolveResult(new InitializedObjectResolveResult(attr.AttributeType), a.Key), a.Value)).ToList<ResolveResult>();
			var constructorResult = CompileConstructorInvocation(attr.Constructor, initializerStatements, currentType, null, attr.PositionalArguments, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, null, null);
			if (constructorResult.AdditionalStatements.Count > 0) {
				return JsExpression.Invocation(JsExpression.FunctionDefinition(new string[0], JsStatement.Block(constructorResult.AdditionalStatements.Concat(new[] { JsStatement.Return(constructorResult.Expression) }))));
			}
			else {
				return constructorResult.Expression;
			}
		}

		public static JsExpression ConstructFieldPropertyAccessor(IMethodSymbol m, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, string fieldName, Func<ITypeSymbol, JsExpression> instantiateType, bool isGetter, bool includeDeclaringType) {
			var properties = GetCommonMemberInfoProperties(m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);
			properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Method)));
			properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(m.Parameters.Select(p => instantiateType(p.Type)))));
			properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(m.ReturnType)));
			properties.Add(new JsObjectLiteralProperty(isGetter ? "fget" : "fset", JsExpression.String(fieldName)));
			if (m.IsStatic)
				properties.Add(new JsObjectLiteralProperty("isStatic", JsExpression.True));
			return JsExpression.ObjectLiteral(properties);
		}

		public static IEnumerable<AttributeData> GetScriptableAttributes(IEnumerable<AttributeData> attributes, IMetadataImporter metadataImporter) {
			return attributes.Where(a => !a.IsConditionallyRemoved && metadataImporter.GetTypeSemantics(a.AttributeType.OriginalDefinition).Type != TypeScriptSemantics.ImplType.NotUsableFromScript);
		}

		private static List<JsObjectLiteralProperty> GetCommonMemberInfoProperties(ISymbol m, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<ITypeSymbol, JsExpression> instantiateType, bool includeDeclaringType) {
			var result = new List<JsObjectLiteralProperty>();
			var attr = GetScriptableAttributes(m.GetAttributes(), metadataImporter).ToList();
			if (attr.Count > 0)
				result.Add(new JsObjectLiteralProperty("attr", JsExpression.ArrayLiteral(attr.Select(a => ConstructAttribute(a, m.ContainingType, compilation, metadataImporter, namer, runtimeLibrary, errorReporter)))));
			if (includeDeclaringType)
				result.Add(new JsObjectLiteralProperty("typeDef", instantiateType(m.ContainingType)));

			result.Add(new JsObjectLiteralProperty("name", JsExpression.String(m.Name)));
			return result;
		}

		private static JsExpression ConstructConstructorInfo(IMethodSymbol constructor, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<ITypeSymbol, JsExpression> instantiateType, bool includeDeclaringType) {
			var properties = GetCommonMemberInfoProperties(constructor, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);

			var sem = metadataImporter.GetConstructorSemantics(constructor);
			if (sem.Type == ConstructorScriptSemantics.ImplType.NotUsableFromScript) {
				errorReporter.Message(Messages._7200, constructor.FullyQualifiedName());
				return JsExpression.Null;
			}
			properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Constructor)));
			properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(constructor.Parameters.Select(p => instantiateType(p.Type)))));
			if (sem.Type == ConstructorScriptSemantics.ImplType.NamedConstructor || sem.Type == ConstructorScriptSemantics.ImplType.StaticMethod)
				properties.Add(new JsObjectLiteralProperty("sname", JsExpression.String(sem.Name)));
			if (sem.Type == ConstructorScriptSemantics.ImplType.StaticMethod)
				properties.Add(new JsObjectLiteralProperty("sm", JsExpression.True));
			if ((sem.Type == ConstructorScriptSemantics.ImplType.UnnamedConstructor || sem.Type == ConstructorScriptSemantics.ImplType.NamedConstructor || sem.Type == ConstructorScriptSemantics.ImplType.StaticMethod) && sem.ExpandParams)
				properties.Add(new JsObjectLiteralProperty("exp", JsExpression.True));
			if (sem.Type == ConstructorScriptSemantics.ImplType.Json || sem.Type == ConstructorScriptSemantics.ImplType.InlineCode) {
				var usedNames = new HashSet<string>();
				var parameters = new List<ISymbol>();
				var variables = new Dictionary<ISymbol, VariableData>();
				IList<ResolveResult> constructorParameters = null;
				IList<ResolveResult> initializerStatements = null;
				if (sem.Type == ConstructorScriptSemantics.ImplType.Json && constructor.ContainingType.IsAnonymousType) {
					initializerStatements = new List<ResolveResult>();
					foreach (var p in constructor.ContainingType.GetProperties()) {
						string paramName = MakeCamelCase(p.Name);
						string name = namer.GetVariableName(paramName, usedNames);
						usedNames.Add(name);
						var variable = new SimpleVariable(p.ReturnType, paramName, DomRegion.Empty);
						parameters.Add(variable);
						variables.Add(variable, new VariableData(name, null, false));
						initializerStatements.Add(new OperatorResolveResult(p.ReturnType, ExpressionType.Assign, new MemberResolveResult(new InitializedObjectResolveResult(constructor.ContainingType), p), new LocalResolveResult(variable)));
					}
				}
				else {
					constructorParameters = new List<ResolveResult>();
					foreach (var p in constructor.Parameters) {
						string name = namer.GetVariableName(p.Name, usedNames);
						usedNames.Add(name);
						var variable = new SimpleVariable(p.Name, Location.None);
						parameters.Add(variable);
						variables.Add(variable, new VariableData(name, null, false));
						constructorParameters.Add(new LocalResolveResult(variable));
					}
				}
				var compileResult = CompileConstructorInvocation(constructor, initializerStatements, constructor.ContainingType, constructor, constructorParameters, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, variables, usedNames);
				var definition = JsExpression.FunctionDefinition(parameters.Select(p => variables[p].Name), JsStatement.Block(compileResult.AdditionalStatements.Concat(new[] { JsStatement.Return(compileResult.Expression) })));
				properties.Add(new JsObjectLiteralProperty("def", definition));
			}
			return JsExpression.ObjectLiteral(properties);
		}

		private static JsExpression ConstructMemberInfo(ISymbol m, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<ITypeSymbol, JsExpression> instantiateType, bool includeDeclaringType, MethodScriptSemantics semanticsIfAccessor) {
			if (m is IMethodSymbol && ((IMethodSymbol)m).MethodKind == MethodKind.Constructor)
				return ConstructConstructorInfo((IMethodSymbol)m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);

			var properties = GetCommonMemberInfoProperties(m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);
			if (m.IsStatic)
				properties.Add(new JsObjectLiteralProperty("isStatic", JsExpression.True));

			if (m is IMethodSymbol) {
				var method = (IMethodSymbol)m;
				var sem = semanticsIfAccessor ?? metadataImporter.GetMethodSemantics(method);
				if (sem.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.Type != MethodScriptSemantics.ImplType.InlineCode) {
					errorReporter.Message(Messages._7201, m.FullyQualifiedName(), "method");
					return JsExpression.Null;
				}
				if ((sem.Type == MethodScriptSemantics.ImplType.NormalMethod || sem.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) && sem.ExpandParams)
					properties.Add(new JsObjectLiteralProperty("exp", JsExpression.True));

				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Method)));
				if (sem.Type == MethodScriptSemantics.ImplType.InlineCode) {
					var usedNames = new HashSet<string>();
					var parameters = new List<ISymbol>();
					var variables = new Dictionary<ISymbol, VariableData>();
					var arguments = new List<ResolveResult>();
					foreach (var p in method.Parameters) {
						string name = namer.GetVariableName(p.Name, usedNames);
						usedNames.Add(name);
						var variable = new SimpleVariable(p.Name, Location.None);
						parameters.Add(variable);
						variables.Add(variable, new VariableData(name, null, false));
						arguments.Add(new LocalResolveResult(variable));
					}
					var tokens = InlineCodeMethodCompiler.Tokenize(method, sem.LiteralCode, _ => {});

					var compileResult = Compile(CreateMethodInvocationResolveResult(method, method.IsStatic ? (ResolveResult)new TypeResolveResult(method.ContainingType) : new ThisResolveResult(method.ContainingType), arguments), method.ContainingType, method, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, true, variables, usedNames);
					var definition = JsExpression.FunctionDefinition(parameters.Select(p => variables[p].Name), JsStatement.Block(compileResult.AdditionalStatements.Concat(new[] { JsStatement.Return(compileResult.Expression) })));

					if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.TypeParameter && t.OwnerType == SymbolKind.Method)) {
						definition = JsExpression.FunctionDefinition(method.TypeParameters.Select(namer.GetTypeParameterName), JsStatement.Return(definition));
						properties.Add(new JsObjectLiteralProperty("tpcount", JsExpression.Number(method.TypeParameters.Length)));
					}
					properties.Add(new JsObjectLiteralProperty("def", definition));
				}
				else {
					if (IsJsGeneric(method, metadataImporter)) {
						properties.Add(new JsObjectLiteralProperty("tpcount", JsExpression.Number(method.TypeParameters.Length)));
					}
					if (sem.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
						properties.Add(new JsObjectLiteralProperty("sm", JsExpression.True));
					}
					properties.Add(new JsObjectLiteralProperty("sname", JsExpression.String(sem.Name)));
				}
				properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(method.ReturnType)));
				properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(method.Parameters.Select(p => instantiateType(p.Type)))));
			}
			else if (m is IFieldSymbol) {
				var field = (IFieldSymbol)m;
				var sem = metadataImporter.GetFieldSemantics(field);
				if (sem.Type != FieldScriptSemantics.ImplType.Field) {
					errorReporter.Message(Messages._7201, m.FullyQualifiedName(), "field");
					return JsExpression.Null;
				}
				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Field)));
				properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(field.Type)));
				properties.Add(new JsObjectLiteralProperty("sname", JsExpression.String(sem.Name)));
			}
			else if (m is IPropertySymbol) {
				var prop = (IPropertySymbol)m;
				var sem = metadataImporter.GetPropertySemantics(prop);
				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Property)));
				properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(prop.Type)));
				if (prop.Parameters.Count > 0)
					properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(prop.Parameters.Select(p => instantiateType(p.Type)))));

				switch (sem.Type) {
					case PropertyScriptSemantics.ImplType.GetAndSetMethods:
						if (sem.GetMethod != null && sem.GetMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.GetMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.GetMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
							errorReporter.Message(Messages._7202, m.FullyQualifiedName(), "property", "getter");
							return JsExpression.Null;
						}
						if (sem.SetMethod != null && sem.SetMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.SetMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.SetMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
							errorReporter.Message(Messages._7202, m.FullyQualifiedName(), "property", "setter");
							return JsExpression.Null;
						}
						if (sem.GetMethod != null)
							properties.Add(new JsObjectLiteralProperty("getter", ConstructMemberInfo(prop.GetMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.GetMethod)));
						if (sem.SetMethod != null)
							properties.Add(new JsObjectLiteralProperty("setter", ConstructMemberInfo(prop.SetMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.SetMethod)));
						break;
					case PropertyScriptSemantics.ImplType.Field:
						if (prop.GetMethod != null)
							properties.Add(new JsObjectLiteralProperty("getter", ConstructFieldPropertyAccessor(prop.GetMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, sem.FieldName, instantiateType, isGetter: true, includeDeclaringType: includeDeclaringType)));
						if (prop.SetMethod != null)
							properties.Add(new JsObjectLiteralProperty("setter", ConstructFieldPropertyAccessor(prop.SetMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, sem.FieldName, instantiateType, isGetter: false, includeDeclaringType: includeDeclaringType)));
						properties.Add(new JsObjectLiteralProperty("fname", JsExpression.String(sem.FieldName)));
						break;
					default:
						errorReporter.Message(Messages._7201, m.FullyQualifiedName(), "property");
						return JsExpression.Null;
				}
			}
			else if (m is IEventSymbol) {
				var evt = (IEventSymbol)m;
				var sem = metadataImporter.GetEventSemantics(evt);
				if (sem.Type != EventScriptSemantics.ImplType.AddAndRemoveMethods) {
					errorReporter.Message(Messages._7201, m.FullyQualifiedName(), "event");
					return JsExpression.Null;
				}
				if (sem.AddMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.AddMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.AddMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
					errorReporter.Message(Messages._7202, m.FullyQualifiedName(), "event", "add accessor");
					return JsExpression.Null;
				}
				if (sem.RemoveMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.RemoveMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.RemoveMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
					errorReporter.Message(Messages._7202, m.FullyQualifiedName(), "event", "remove accessor");
					return JsExpression.Null;
				}

				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Event)));
				properties.Add(new JsObjectLiteralProperty("adder", ConstructMemberInfo(evt.AddMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.AddMethod)));
				properties.Add(new JsObjectLiteralProperty("remover", ConstructMemberInfo(evt.RemoveMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.RemoveMethod)));
			}
			else {
				throw new ArgumentException("Invalid member " + m);
			}

			return JsExpression.ObjectLiteral(properties);
		}

		private static ResolveResult CreateMethodInvocationResolveResult(IMethodSymbol method, ResolveResult target, IList<ResolveResult> args) {
			if (method.IsAccessor) {
				var owner = ((IMethodSymbol)method).AccessorOwner;
				var prop = owner as IPropertySymbol;
				if (prop != null) {
					if (ReferenceEquals(method, prop.GetMethod))
						return args.Count == 0 ? new MemberResolveResult(target, owner) : new CSharpInvocationResolveResult(target, prop, args);
					else if (ReferenceEquals(method, prop.SetMethod))
						return new OperatorResolveResult(prop.ReturnType, ExpressionType.Assign, new[] { args.Count == 1 ? new MemberResolveResult(target, prop) : new CSharpInvocationResolveResult(target, prop, args.Take(args.Count - 1).ToList()), args[args.Count - 1] });
					else
						throw new ArgumentException("Invalid member " + method);
				}
				var evt = owner as IEventSymbol;
				if (evt != null) {
					ExpressionType op;
					if (ReferenceEquals(method, ((IEventSymbol)owner).AddAccessor))
						op = ExpressionType.AddAssign;
					else if (ReferenceEquals(method, ((IEventSymbol)owner).RemoveAccessor))
						op = ExpressionType.SubtractAssign;
					else
						throw new ArgumentException("Invalid member " + method);

					return new OperatorResolveResult(evt.ReturnType, op, new[] { new MemberResolveResult(target, evt), args[args.Count - 1] });
				}

				throw new ArgumentException("Invalid owner " + owner);
			}
			else {
				return new CSharpInvocationResolveResult(target, method, args);
			}
		}

		public static JsExpression ConstructMemberInfo(ISymbol m, Compilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<ITypeSymbol, JsExpression> instantiateType, bool includeDeclaringType) {
			MethodScriptSemantics semanticsIfAccessor = null;
			if (m is IMethodSymbol && ((IMethodSymbol)m).IsAccessor()) {
				var owner = ((IMethodSymbol)m).AccessorOwner;
				if (owner is IPropertySymbol) {
					var sem = metadataImporter.GetPropertySemantics((IPropertySymbol)owner);
					if (sem.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods) {
						if (ReferenceEquals(m, ((IPropertySymbol)owner).GetMethod))
							semanticsIfAccessor = sem.GetMethod;
						else if (ReferenceEquals(m, ((IPropertySymbol)owner).SetMethod))
							semanticsIfAccessor = sem.SetMethod;
						else
							throw new ArgumentException("Invalid member " + m);
					}
				}
				else if (owner is IEventSymbol) {
					var sem = metadataImporter.GetEventSemantics((IEventSymbol)owner);
					if (sem.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods) {
						if (ReferenceEquals(m, ((IEventSymbol)owner).AddAccessor))
							semanticsIfAccessor = sem.AddMethod;
						else if (ReferenceEquals(m, ((IEventSymbol)owner).RemoveAccessor))
							semanticsIfAccessor = sem.RemoveMethod;
						else
							throw new ArgumentException("Invalid member " + m);
					}
				}
				else
					throw new ArgumentException("Invalid owner " + owner);
			}

			return ConstructMemberInfo(m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, semanticsIfAccessor);
		}
	}
}
