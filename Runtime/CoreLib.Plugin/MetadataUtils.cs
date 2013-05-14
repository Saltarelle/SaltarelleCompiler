using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;

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

		public static bool IsSerializable(ITypeDefinition type) {
			return AttributeReader.HasAttribute<SerializableAttribute>(type) || (type.GetAllBaseTypeDefinitions().Any(td => td.FullName == "System.Record") && type.FullName != "System.Record");
		}

		public static string GetSerializableTypeCheckCode(ITypeDefinition type) {
			var attr = type.Attributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(SerializableAttribute).FullName);
			if (attr != null) {
				var result = attr.NamedArguments.SingleOrDefault(a => a.Key.Name == "TypeCheckCode");
				if (result.Value != null && result.Value.ConstantValue is string)
					return result.Value.ConstantValue as string;
			}
			return null;
		}

		public static bool DoesTypeObeyTypeSystem(ITypeDefinition type) {
			var ia = AttributeReader.ReadAttribute<ImportedAttribute>(type);
			return ia == null || ia.ObeysTypeSystem;
		}

		public static bool IsMixin(ITypeDefinition type) {
			return AttributeReader.HasAttribute<MixinAttribute>(type);
		}

		public static bool IsImported(ITypeDefinition type) {
			return AttributeReader.HasAttribute<ImportedAttribute>(type);
		}

		public static bool IsResources(ITypeDefinition type) {
			return AttributeReader.HasAttribute<ResourcesAttribute>(type);
		}

		public static bool IsNamedValues(ITypeDefinition type) {
			return AttributeReader.HasAttribute<NamedValuesAttribute>(type);
		}

		public static bool IsGlobalMethods(ITypeDefinition type) {
			return AttributeReader.HasAttribute<GlobalMethodsAttribute>(type);
		}

		public static bool IsPreserveMemberCase(ITypeDefinition type) {
			var pmca = AttributeReader.ReadAttribute<PreserveMemberCaseAttribute>(type) ?? AttributeReader.ReadAttribute<PreserveMemberCaseAttribute>(type.ParentAssembly.AssemblyAttributes);
			return pmca != null && pmca.Preserve;
		}

		public static bool IsPreserveMemberNames(ITypeDefinition type) {
			return IsImported(type) || IsGlobalMethods(type);
		}

		public static bool OmitNullableChecks(ICompilation compilation) {
			var sca = AttributeReader.ReadAttribute<ScriptSharpCompatibilityAttribute>(compilation.MainAssembly.AssemblyAttributes);
			return sca != null && sca.OmitNullableChecks;
		}

		public static bool OmitDowncasts(ICompilation compilation) {
			var sca = AttributeReader.ReadAttribute<ScriptSharpCompatibilityAttribute>(compilation.MainAssembly.AssemblyAttributes);
			return sca != null && sca.OmitDowncasts;
		}

		public static bool IsAsyncModule(IAssembly assembly) {
			return AttributeReader.HasAttribute<AsyncModuleAttribute>(assembly.AssemblyAttributes);
		}

		public static IEnumerable<KeyValuePair<string,string>> GetAdditionalDependencies(IAssembly assembly)
		{
			return AttributeReader.ReadAttributes<AdditionalDependencyAttribute>(assembly.AssemblyAttributes)
				.Select(a => new KeyValuePair<string,string>(a.ModuleName, a.InstanceName));
		}

		public static string GetModuleName(IAssembly assembly) {
			var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(assembly.AssemblyAttributes);
			return (mna != null && !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null);
		}

		public static string GetModuleName(ITypeDefinition type) {
			for (var current = type; current != null; current = current.DeclaringTypeDefinition) {
				var mna = AttributeReader.ReadAttribute<ModuleNameAttribute>(type);
				if (mna != null)
					return !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null;
			}
			return GetModuleName(type.ParentAssembly);
		}

		public static bool? ShouldGenericArgumentsBeIncluded(ITypeDefinition type) {
			var iga = AttributeReader.ReadAttribute<IncludeGenericArgumentsAttribute>(type);
			if (iga != null)
				return iga.Include;
			var imp = AttributeReader.ReadAttribute<ImportedAttribute>(type);
			if (imp != null)
				return false;
			var def = AttributeReader.ReadAttribute<IncludeGenericArgumentsDefaultAttribute>(type.ParentAssembly.AssemblyAttributes);
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

		public static bool? ShouldGenericArgumentsBeIncluded(IMethod method) {
			var iga = AttributeReader.ReadAttribute<IncludeGenericArgumentsAttribute>(method);
			if (iga != null)
				return iga.Include;
			var imp = AttributeReader.ReadAttribute<ImportedAttribute>(method.DeclaringTypeDefinition);
			if (imp != null)
				return false;
			var def = AttributeReader.ReadAttribute<IncludeGenericArgumentsDefaultAttribute>(method.ParentAssembly.AssemblyAttributes);
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

		public static IMember UnwrapValueTypeConstructor(IMember m) {
			if (m is IMethod && !m.IsStatic && m.DeclaringType.Kind == TypeKind.Struct && ((IMethod)m).IsConstructor && ((IMethod)m).Parameters.Count == 0) {
				var other = m.DeclaringType.GetConstructors().SingleOrDefault(c => c.Parameters.Count == 1 && c.Parameters[0].Type.FullName == typeof(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor).FullName);
				if (other != null)
					return other;
			}
			return m;
		}

		public static bool CanBeMinimized(ITypeDefinition typeDefinition) {
			return !typeDefinition.IsExternallyVisible();
		}

		public static bool CanBeMinimized(IMember member) {
			return !member.IsExternallyVisible() || AttributeReader.HasAttribute<MinimizePublicNamesAttribute>(member.ParentAssembly.AssemblyAttributes);
		}

		/// <summary>
		/// Determines the preferred name for a member. The first item is the name, the second item is true if the name was explicitly specified.
		/// </summary>
		public static Tuple<string, bool> DeterminePreferredMemberName(IMember member, bool minimizeNames) {
			member = UnwrapValueTypeConstructor(member);

			bool isConstructor = member is IMethod && ((IMethod)member).IsConstructor;
			bool isAccessor = member is IMethod && ((IMethod)member).IsAccessor;
			bool isPreserveMemberCase = IsPreserveMemberCase(member.DeclaringTypeDefinition);

			string defaultName;
			if (isConstructor) {
				defaultName = "$ctor";
			}
			else if (!CanBeMinimized(member)) {
				defaultName = isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name);
			}
			else {
				if (minimizeNames && member.DeclaringType.Kind != TypeKind.Interface)
					defaultName = null;
				else
					defaultName = "$" + (isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name));
			}

			var asa = AttributeReader.ReadAttribute<AlternateSignatureAttribute>(member);
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && !AttributeReader.HasAttribute<AlternateSignatureAttribute>(m) && !AttributeReader.HasAttribute<NonScriptableAttribute>(m) && !AttributeReader.HasAttribute<InlineCodeAttribute>(m)).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0], minimizeNames);
				}
				else {
					return Tuple.Create(member.Name, false);	// Error
				}
			}

			var sna = AttributeReader.ReadAttribute<ScriptNameAttribute>(member);
			if (sna != null) {
				string name = sna.Name;
				if (IsNamedValues(member.DeclaringTypeDefinition) && (name == "" || !name.IsValidJavaScriptIdentifier())) {
					return Tuple.Create(defaultName, false);	// For named values enum, allow the use to specify an empty or invalid value, which will only be used as the literal value for the field, not for the name.
				}
				if (name == "" && isConstructor)
					name = "$ctor";
				return Tuple.Create(name, true);
			}
			
			if (isConstructor && IsImported(member.DeclaringTypeDefinition)) {
				return Tuple.Create("$ctor", true);
			}

			var ica = AttributeReader.ReadAttribute<InlineCodeAttribute>(member);
			if (ica != null) {
				if (ica.GeneratedMethodName != null)
					return Tuple.Create(ica.GeneratedMethodName, true);
			}

			if (AttributeReader.HasAttribute<PreserveCaseAttribute>(member))
				return Tuple.Create(member.Name, true);

			bool preserveName = (!isConstructor && !isAccessor && (   AttributeReader.HasAttribute<PreserveNameAttribute>(member)
			                                                       || AttributeReader.HasAttribute<InstanceMethodOnFirstArgumentAttribute>(member)
			                                                       || AttributeReader.HasAttribute<IntrinsicPropertyAttribute>(member)
			                                                       || IsPreserveMemberNames(member.DeclaringTypeDefinition) && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (IsSerializable(member.DeclaringTypeDefinition) && !member.IsStatic && (member is IProperty || member is IField)))
			                                                       || (IsNamedValues(member.DeclaringTypeDefinition) && member is IField);

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

		public static IMethod CreateTypeCheckMethod(IType type, ICompilation compilation) {
			IMethod method = new DefaultResolvedMethod(new DefaultUnresolvedMethod(type.GetDefinition().Parts[0], "IsInstanceOfType"), compilation.TypeResolveContext.WithCurrentTypeDefinition(type.GetDefinition()));
			if (type is ParameterizedType)
				method = new SpecializedMethod(method, new TypeParameterSubstitution(classTypeArguments: ((ParameterizedType)type).TypeArguments, methodTypeArguments: null));
			return method;
		}

		public static bool IsJsGeneric(IMethod method, IMetadataImporter metadataImporter) {
			return method.TypeParameters.Count > 0 && !metadataImporter.GetMethodSemantics(method).IgnoreGenericArguments;
		}

		public static bool IsJsGeneric(ITypeDefinition type, IMetadataImporter metadataImporter) {
			return type.TypeParameterCount > 0 && !metadataImporter.GetTypeSemantics(type).IgnoreGenericArguments;
		}

		public static bool IsReflectable(IMember member, IMetadataImporter metadataImporter) {
			return member.Attributes.Any(a => a.AttributeType.FullName == typeof(ReflectableAttribute).FullName || metadataImporter.GetTypeSemantics(a.AttributeType.GetDefinition()).Type == TypeScriptSemantics.ImplType.NormalType);
		}

		private static ExpressionCompileResult Compile(ResolveResult rr, ITypeDefinition currentType, IMethod currentMethod, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, bool returnValueIsImportant, Dictionary<IVariable, VariableData> variables, ISet<string> usedVariableNames) {
			variables = variables ?? new Dictionary<IVariable, VariableData>();
			usedVariableNames = usedVariableNames ?? new HashSet<string>();
			return new ExpressionCompiler(compilation,
			                              metadataImporter,
			                              namer,
			                              runtimeLibrary,
			                              errorReporter,
			                              variables,
			                              new Dictionary<LambdaResolveResult, NestedFunctionData>(),
			                              t => {
			                                  string name = namer.GetVariableName(null, usedVariableNames);
			                                  IVariable variable = new SimpleVariable(t, "temporary", DomRegion.Empty);
			                                  variables[variable] = new VariableData(name, null, false);
			                                  usedVariableNames.Add(name);
			                                  return variable;
			                              },
			                              _ => { throw new Exception("Cannot compile nested functions here"); },
			                              null,
			                              new NestedFunctionContext(EmptyList<IVariable>.Instance),
			                              null,
			                              currentMethod,
			                              currentType
			                             ).Compile(rr, returnValueIsImportant);
		}

		public static ExpressionCompileResult CompileConstructorInvocation(IMethod constructor, IList<ResolveResult> initializerStatements, ITypeDefinition currentType, IMethod currentMethod, IList<ResolveResult> arguments, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Dictionary<IVariable, VariableData> variables, ISet<string> usedVariableNames) {
			return Compile(new CSharpInvocationResolveResult(new TypeResolveResult(constructor.DeclaringType), constructor, arguments, initializerStatements: initializerStatements), currentType, currentMethod, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, true, variables, usedVariableNames);
		}

		public static JsExpression ConstructAttribute(IAttribute attr, ITypeDefinition currentType, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter) {
			errorReporter.Region = attr.Region;
			var initializerStatements = attr.NamedArguments.Select(a => new OperatorResolveResult(a.Key.ReturnType, ExpressionType.Assign, new MemberResolveResult(new InitializedObjectResolveResult(attr.AttributeType), a.Key), a.Value)).ToList<ResolveResult>();
			var constructorResult = CompileConstructorInvocation(attr.Constructor, initializerStatements, currentType, null, attr.PositionalArguments, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, null, null);
			if (constructorResult.AdditionalStatements.Count > 0) {
				return JsExpression.Invocation(JsExpression.FunctionDefinition(new string[0], new JsBlockStatement(constructorResult.AdditionalStatements.Concat(new[] { new JsReturnStatement(constructorResult.Expression) }))));
			}
			else {
				return constructorResult.Expression;
			}
		}

		public static JsExpression ConstructFieldPropertyAccessor(IMethod m, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, string fieldName, Func<IType, JsExpression> instantiateType, bool isGetter, bool includeDeclaringType) {
			var properties = GetCommonMemberInfoProperties(m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);
			properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Method)));
			properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(m.Parameters.Select(p => instantiateType(p.Type)))));
			properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(m.ReturnType)));
			properties.Add(new JsObjectLiteralProperty(isGetter ? "fget" : "fset", JsExpression.String(fieldName)));
			if (m.IsStatic)
				properties.Add(new JsObjectLiteralProperty("isStatic", JsExpression.True));
			return JsExpression.ObjectLiteral(properties);
		}

		private static List<JsObjectLiteralProperty> GetCommonMemberInfoProperties(IMember m, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<IType, JsExpression> instantiateType, bool includeDeclaringType) {
			var result = new List<JsObjectLiteralProperty>();
			var attr = m.Attributes.Where(a => !a.IsConditionallyRemoved && metadataImporter.GetTypeSemantics(a.AttributeType.GetDefinition()).Type == TypeScriptSemantics.ImplType.NormalType).ToList();
			if (attr.Count > 0)
				result.Add(new JsObjectLiteralProperty("attr", JsExpression.ArrayLiteral(attr.Select(a => ConstructAttribute(a, m.DeclaringTypeDefinition, compilation, metadataImporter, namer, runtimeLibrary, errorReporter)))));
			if (includeDeclaringType)
				result.Add(new JsObjectLiteralProperty("typeDef", instantiateType(m.DeclaringType)));

			result.Add(new JsObjectLiteralProperty("name", JsExpression.String(m.Name)));
			return result;
		}

		private static JsExpression ConstructConstructorInfo(IMethod constructor, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<IType, JsExpression> instantiateType, bool includeDeclaringType) {
			var properties = GetCommonMemberInfoProperties(constructor, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);

			var sem = metadataImporter.GetConstructorSemantics(constructor);
			if (sem.Type == ConstructorScriptSemantics.ImplType.NotUsableFromScript) {
				errorReporter.Message(Messages._7200, constructor.FullName);
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
				var parameters = new List<IVariable>();
				var variables = new Dictionary<IVariable, VariableData>();
				IList<ResolveResult> constructorParameters = null;
				IList<ResolveResult> initializerStatements = null;
				if (sem.Type == ConstructorScriptSemantics.ImplType.Json && constructor.DeclaringType.Kind == TypeKind.Anonymous) {
					initializerStatements = new List<ResolveResult>();
					foreach (var p in constructor.DeclaringType.GetProperties()) {
						string paramName = MakeCamelCase(p.Name);
						string name = namer.GetVariableName(paramName, usedNames);
						usedNames.Add(name);
						var variable = new SimpleVariable(p.ReturnType, paramName, DomRegion.Empty);
						parameters.Add(variable);
						variables.Add(variable, new VariableData(name, null, false));
						initializerStatements.Add(new OperatorResolveResult(p.ReturnType, ExpressionType.Assign, new MemberResolveResult(new InitializedObjectResolveResult(constructor.DeclaringType), p), new LocalResolveResult(variable)));
					}
				}
				else {
					constructorParameters = new List<ResolveResult>();
					foreach (var p in constructor.Parameters) {
						string name = namer.GetVariableName(p.Name, usedNames);
						usedNames.Add(name);
						var variable = new SimpleVariable(p.Type, p.Name, DomRegion.Empty);
						parameters.Add(variable);
						variables.Add(variable, new VariableData(name, null, false));
						constructorParameters.Add(new LocalResolveResult(variable));
					}
				}
				var compileResult = CompileConstructorInvocation(constructor, initializerStatements, constructor.DeclaringTypeDefinition, constructor, constructorParameters, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, variables, usedNames);
				var definition = JsExpression.FunctionDefinition(parameters.Select(p => variables[p].Name), new JsBlockStatement(compileResult.AdditionalStatements.Concat(new[] { new JsReturnStatement(compileResult.Expression) })));
				properties.Add(new JsObjectLiteralProperty("def", definition));
			}
			return JsExpression.ObjectLiteral(properties);
		}

		private static JsExpression ConstructMemberInfo(IMember m, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<IType, JsExpression> instantiateType, bool includeDeclaringType, MethodScriptSemantics semanticsIfAccessor) {
			if (m is IMethod && ((IMethod)m).IsConstructor)
				return ConstructConstructorInfo((IMethod)m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);

			var properties = GetCommonMemberInfoProperties(m, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType);
			if (m.IsStatic)
				properties.Add(new JsObjectLiteralProperty("isStatic", JsExpression.True));

			if (m is IMethod) {
				var method = (IMethod)m;
				var sem = semanticsIfAccessor ?? metadataImporter.GetMethodSemantics(method);
				if (sem.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.Type != MethodScriptSemantics.ImplType.InlineCode) {
					errorReporter.Message(Messages._7201, m.FullName, "method");
					return JsExpression.Null;
				}
				if ((sem.Type == MethodScriptSemantics.ImplType.NormalMethod || sem.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) && sem.ExpandParams)
					properties.Add(new JsObjectLiteralProperty("exp", JsExpression.True));

				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Method)));
				if (sem.Type == MethodScriptSemantics.ImplType.InlineCode) {
					var usedNames = new HashSet<string>();
					var parameters = new List<IVariable>();
					var variables = new Dictionary<IVariable, VariableData>();
					var arguments = new List<ResolveResult>();
					foreach (var p in method.Parameters) {
						string name = namer.GetVariableName(p.Name, usedNames);
						usedNames.Add(name);
						var variable = new SimpleVariable(p.Type, p.Name, DomRegion.Empty);
						parameters.Add(variable);
						variables.Add(variable, new VariableData(name, null, false));
						arguments.Add(new LocalResolveResult(variable));
					}
					var tokens = InlineCodeMethodCompiler.Tokenize(method, sem.LiteralCode, _ => {});

					var compileResult = Compile(CreateMethodInvocationResolveResult(method, method.IsStatic ? (ResolveResult)new TypeResolveResult(method.DeclaringType) : new ThisResolveResult(method.DeclaringType), arguments), method.DeclaringTypeDefinition, method, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, true, variables, usedNames);
					var definition = JsExpression.FunctionDefinition(parameters.Select(p => variables[p].Name), new JsBlockStatement(compileResult.AdditionalStatements.Concat(new[] { new JsReturnStatement(compileResult.Expression) })));

					if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.TypeParameter && t.OwnerType == EntityType.Method)) {
						definition = JsExpression.FunctionDefinition(method.TypeParameters.Select(namer.GetTypeParameterName), new JsReturnStatement(definition));
						properties.Add(new JsObjectLiteralProperty("tpcount", JsExpression.Number(method.TypeParameters.Count)));
					}
					properties.Add(new JsObjectLiteralProperty("def", definition));
				}
				else {
					if (IsJsGeneric(method, metadataImporter)) {
						properties.Add(new JsObjectLiteralProperty("tpcount", JsExpression.Number(method.TypeParameters.Count)));
					}
					if (sem.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument) {
						properties.Add(new JsObjectLiteralProperty("sm", JsExpression.True));
					}
					properties.Add(new JsObjectLiteralProperty("sname", JsExpression.String(sem.Name)));
				}
				properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(method.ReturnType)));
				properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(method.Parameters.Select(p => instantiateType(p.Type)))));
			}
			else if (m is IField) {
				var field = (IField)m;
				var sem = metadataImporter.GetFieldSemantics(field);
				if (sem.Type != FieldScriptSemantics.ImplType.Field) {
					errorReporter.Message(Messages._7201, m.FullName, "field");
					return JsExpression.Null;
				}
				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Field)));
				properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(field.ReturnType)));
				properties.Add(new JsObjectLiteralProperty("sname", JsExpression.String(sem.Name)));
			}
			else if (m is IProperty) {
				var prop = (IProperty)m;
				var sem = metadataImporter.GetPropertySemantics(prop);
				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Property)));
				properties.Add(new JsObjectLiteralProperty("returnType", instantiateType(prop.ReturnType)));
				if (prop.Parameters.Count > 0)
					properties.Add(new JsObjectLiteralProperty("params", JsExpression.ArrayLiteral(prop.Parameters.Select(p => instantiateType(p.Type)))));

				switch (sem.Type) {
					case PropertyScriptSemantics.ImplType.GetAndSetMethods:
						if (sem.GetMethod != null && sem.GetMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.GetMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.GetMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
							errorReporter.Message(Messages._7202, m.FullName, "property", "getter");
							return JsExpression.Null;
						}
						if (sem.SetMethod != null && sem.SetMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.SetMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.SetMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
							errorReporter.Message(Messages._7202, m.FullName, "property", "setter");
							return JsExpression.Null;
						}
						if (sem.GetMethod != null)
							properties.Add(new JsObjectLiteralProperty("getter", ConstructMemberInfo(prop.Getter, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.GetMethod)));
						if (sem.SetMethod != null)
							properties.Add(new JsObjectLiteralProperty("setter", ConstructMemberInfo(prop.Setter, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.SetMethod)));
						break;
					case PropertyScriptSemantics.ImplType.Field:
						if (prop.CanGet)
							properties.Add(new JsObjectLiteralProperty("getter", ConstructFieldPropertyAccessor(prop.Getter, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, sem.FieldName, instantiateType, isGetter: true, includeDeclaringType: includeDeclaringType)));
						if (prop.CanSet)
							properties.Add(new JsObjectLiteralProperty("setter", ConstructFieldPropertyAccessor(prop.Setter, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, sem.FieldName, instantiateType, isGetter: false, includeDeclaringType: includeDeclaringType)));
						properties.Add(new JsObjectLiteralProperty("fname", JsExpression.String(sem.FieldName)));
						break;
					default:
						errorReporter.Message(Messages._7201, m.FullName, "property");
						return JsExpression.Null;
				}
			}
			else if (m is IEvent) {
				var evt = (IEvent)m;
				var sem = metadataImporter.GetEventSemantics(evt);
				if (sem.Type != EventScriptSemantics.ImplType.AddAndRemoveMethods) {
					errorReporter.Message(Messages._7201, m.FullName, "event");
					return JsExpression.Null;
				}
				if (sem.AddMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.AddMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.AddMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
					errorReporter.Message(Messages._7202, m.FullName, "event", "add accessor");
					return JsExpression.Null;
				}
				if (sem.RemoveMethod.Type != MethodScriptSemantics.ImplType.NormalMethod && sem.RemoveMethod.Type != MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument && sem.RemoveMethod.Type != MethodScriptSemantics.ImplType.InlineCode) {
					errorReporter.Message(Messages._7202, m.FullName, "event", "remove accessor");
					return JsExpression.Null;
				}

				properties.Add(new JsObjectLiteralProperty("type", JsExpression.Number((int)MemberTypes.Event)));
				properties.Add(new JsObjectLiteralProperty("adder", ConstructMemberInfo(evt.AddAccessor, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.AddMethod)));
				properties.Add(new JsObjectLiteralProperty("remover", ConstructMemberInfo(evt.RemoveAccessor, compilation, metadataImporter, namer, runtimeLibrary, errorReporter, instantiateType, includeDeclaringType, sem.RemoveMethod)));
			}
			else {
				throw new ArgumentException("Invalid member " + m);
			}

			return JsExpression.ObjectLiteral(properties);
		}

		private static ResolveResult CreateMethodInvocationResolveResult(IMethod method, ResolveResult target, IList<ResolveResult> args) {
			if (method.IsAccessor) {
				var owner = ((IMethod)method).AccessorOwner;
				var prop = owner as IProperty;
				if (prop != null) {
					if (ReferenceEquals(method, prop.Getter))
						return args.Count == 0 ? new MemberResolveResult(target, owner) : new CSharpInvocationResolveResult(target, prop, args);
					else if (ReferenceEquals(method, prop.Setter))
						return new OperatorResolveResult(prop.ReturnType, ExpressionType.Assign, new[] { args.Count == 1 ? new MemberResolveResult(target, prop) : new CSharpInvocationResolveResult(target, prop, args.Take(args.Count - 1).ToList()), args[args.Count - 1] });
					else
						throw new ArgumentException("Invalid member " + method);
				}
				var evt = owner as IEvent;
				if (evt != null) {
					ExpressionType op;
					if (ReferenceEquals(method, ((IEvent)owner).AddAccessor))
						op = ExpressionType.AddAssign;
					else if (ReferenceEquals(method, ((IEvent)owner).RemoveAccessor))
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

		public static JsExpression ConstructMemberInfo(IMember m, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<IType, JsExpression> instantiateType, bool includeDeclaringType) {
			MethodScriptSemantics semanticsIfAccessor = null;
			if (m is IMethod && ((IMethod)m).IsAccessor) {
				var owner = ((IMethod)m).AccessorOwner;
				if (owner is IProperty) {
					var sem = metadataImporter.GetPropertySemantics((IProperty)owner);
					if (sem.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods) {
						if (ReferenceEquals(m, ((IProperty)owner).Getter))
							semanticsIfAccessor = sem.GetMethod;
						else if (ReferenceEquals(m, ((IProperty)owner).Setter))
							semanticsIfAccessor = sem.SetMethod;
						else
							throw new ArgumentException("Invalid member " + m);
					}
				}
				else if (owner is IEvent) {
					var sem = metadataImporter.GetEventSemantics((IEvent)owner);
					if (sem.Type == EventScriptSemantics.ImplType.AddAndRemoveMethods) {
						if (ReferenceEquals(m, ((IEvent)owner).AddAccessor))
							semanticsIfAccessor = sem.AddMethod;
						else if (ReferenceEquals(m, ((IEvent)owner).RemoveAccessor))
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
