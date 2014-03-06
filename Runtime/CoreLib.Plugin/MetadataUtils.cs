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
using Saltarelle.Compiler.JSModel.TypeSystem;

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

		public static bool? IsAutoProperty(this IProperty property) {
			if (property.Region == default(DomRegion))
				return null;
			return property.Getter != null && property.Setter != null && property.Getter.BodyRegion == default(DomRegion) && property.Setter.BodyRegion == default(DomRegion);
		}

		public static bool? IsAutoEvent(IEvent evt) {
			if (evt.Region == default(DomRegion))
				return null;
			return evt.AddAccessor != null && evt.RemoveAccessor != null && evt.AddAccessor.BodyRegion == default(DomRegion) && evt.RemoveAccessor.BodyRegion == default(DomRegion);
		}

		public static bool IsSerializable(ITypeDefinition type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<ScriptSerializableAttribute>() || (type.GetAllBaseTypeDefinitions().Any(td => td.FullName == "System.Record") && type.FullName != "System.Record");
		}

		public static string GetSerializableTypeCheckCode(ITypeDefinition type, IAttributeStore attributeStore) {
			var attr = attributeStore.AttributesFor(type).GetAttribute<ScriptSerializableAttribute>();
			return attr != null ? attr.TypeCheckCode : null;
		}

		public static bool DoesTypeObeyTypeSystem(ITypeDefinition type, IAttributeStore attributeStore) {
			var ia = attributeStore.AttributesFor(type).GetAttribute<ImportedAttribute>();
			return ia == null || ia.ObeysTypeSystem;
		}

		public static bool IsMixin(ITypeDefinition type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<MixinAttribute>();
		}

		public static bool IsImported(ITypeDefinition type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<ImportedAttribute>();
		}

		public static bool IsResources(ITypeDefinition type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<ResourcesAttribute>();
		}

		public static bool IsNamedValues(ITypeDefinition type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<NamedValuesAttribute>();
		}

		public static bool IsGlobalMethods(ITypeDefinition type, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(type).HasAttribute<GlobalMethodsAttribute>();
		}

		public static bool IsPreserveMemberCase(ITypeDefinition type, IAttributeStore attributeStore) {
			var pmca = attributeStore.AttributesFor(type).GetAttribute<PreserveMemberCaseAttribute>() ?? attributeStore.AttributesFor(type.ParentAssembly).GetAttribute<PreserveMemberCaseAttribute>();
			return pmca != null && pmca.Preserve;
		}

		public static bool IsPreserveMemberNames(ITypeDefinition type, IAttributeStore attributeStore) {
			return IsImported(type, attributeStore) || IsGlobalMethods(type, attributeStore);
		}

		public static bool OmitNullableChecks(ICompilation compilation, IAttributeStore attributeStore) {
			var sca = attributeStore.AttributesFor(compilation.MainAssembly).GetAttribute<ScriptSharpCompatibilityAttribute>();
			return sca != null && sca.OmitNullableChecks;
		}

		public static bool OmitDowncasts(ICompilation compilation, IAttributeStore attributeStore) {
			var sca = attributeStore.AttributesFor(compilation.MainAssembly).GetAttribute<ScriptSharpCompatibilityAttribute>();
			return sca != null && sca.OmitDowncasts;
		}

		public static bool IsAsyncModule(IAssembly assembly, IAttributeStore attributeStore) {
			return attributeStore.AttributesFor(assembly).HasAttribute<AsyncModuleAttribute>();
		}

		public static IEnumerable<KeyValuePair<string,string>> GetAdditionalDependencies(IAssembly assembly, IAttributeStore attributeStore)
		{
			return attributeStore.AttributesFor(assembly).GetAttributes<AdditionalDependencyAttribute>()
				.Select(a => new KeyValuePair<string,string>(a.ModuleName, a.InstanceName));
		}

		public static string GetModuleName(IAssembly assembly, IAttributeStore attributeStore) {
			var mna = attributeStore.AttributesFor(assembly).GetAttribute<ModuleNameAttribute>();
			return (mna != null && !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null);
		}

		public static string GetModuleName(ITypeDefinition type, IAttributeStore attributeStore) {
			for (var current = type; current != null; current = current.DeclaringTypeDefinition) {
				var mna = attributeStore.AttributesFor(type).GetAttribute<ModuleNameAttribute>();
				if (mna != null)
					return !String.IsNullOrEmpty(mna.ModuleName) ? mna.ModuleName : null;
			}
			return GetModuleName(type.ParentAssembly, attributeStore);
		}

		public static bool? ShouldGenericArgumentsBeIncluded(ITypeDefinition type, IAttributeStore attributeStore) {
			var attributes = attributeStore.AttributesFor(type);

			var iga = attributes.GetAttribute<IncludeGenericArgumentsAttribute>();
			if (iga != null)
				return iga.Include;
			var imp = attributes.GetAttribute<ImportedAttribute>();
			if (imp != null)
				return false;
			var def = attributeStore.AttributesFor(type.ParentAssembly).GetAttribute<IncludeGenericArgumentsDefaultAttribute>();
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

		public static bool? ShouldGenericArgumentsBeIncluded(IMethod method, IAttributeStore attributeStore) {
			var iga = attributeStore.AttributesFor(method).GetAttribute<IncludeGenericArgumentsAttribute>();
			if (iga != null)
				return iga.Include;
			var imp = attributeStore.AttributesFor(method.DeclaringTypeDefinition).GetAttribute<ImportedAttribute>();
			if (imp != null)
				return false;
			var def = attributeStore.AttributesFor(method.ParentAssembly).GetAttribute<IncludeGenericArgumentsDefaultAttribute>();
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

		public static bool CanBeMinimized(IMember member, IAttributeStore attributeStore) {
			return !member.IsExternallyVisible() || attributeStore.AttributesFor(member.ParentAssembly).HasAttribute<MinimizePublicNamesAttribute>();
		}

		/// <summary>
		/// Determines the preferred name for a member. The first item is the name, the second item is true if the name was explicitly specified.
		/// </summary>
		public static Tuple<string, bool> DeterminePreferredMemberName(IMember member, bool minimizeNames, IAttributeStore attributeStore) {
			member = UnwrapValueTypeConstructor(member);

			bool isConstructor = member is IMethod && ((IMethod)member).IsConstructor;
			bool isAccessor = member is IMethod && ((IMethod)member).IsAccessor;
			bool isPreserveMemberCase = IsPreserveMemberCase(member.DeclaringTypeDefinition, attributeStore);

			string defaultName;
			if (isConstructor) {
				defaultName = "$ctor";
			}
			else if (!CanBeMinimized(member, attributeStore)) {
				defaultName = isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name);
			}
			else {
				if (minimizeNames && member.DeclaringType.Kind != TypeKind.Interface)
					defaultName = null;
				else
					defaultName = "$" + (isPreserveMemberCase ? member.Name : MakeCamelCase(member.Name));
			}

			var attributes = attributeStore.AttributesFor(member);

			var asa = attributes.GetAttribute<AlternateSignatureAttribute>();
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && !attributeStore.AttributesFor(m).HasAttribute<AlternateSignatureAttribute>() && !attributeStore.AttributesFor(m).HasAttribute<NonScriptableAttribute>() && !attributeStore.AttributesFor(m).HasAttribute<InlineCodeAttribute>()).ToList();
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
				if (IsNamedValues(member.DeclaringTypeDefinition, attributeStore) && (name == "" || !name.IsValidJavaScriptIdentifier())) {
					return Tuple.Create(defaultName, false);	// For named values enum, allow the use to specify an empty or invalid value, which will only be used as the literal value for the field, not for the name.
				}
				if (name == "" && isConstructor)
					name = "$ctor";
				return Tuple.Create(name, true);
			}
			
			if (isConstructor && IsImported(member.DeclaringTypeDefinition, attributeStore)) {
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
			                                                       || IsPreserveMemberNames(member.DeclaringTypeDefinition, attributeStore) && member.ImplementedInterfaceMembers.Count == 0 && !member.IsOverride)
			                                                       || (IsSerializable(member.DeclaringTypeDefinition, attributeStore) && !member.IsStatic && (member is IProperty || member is IField)))
			                                                       || (IsNamedValues(member.DeclaringTypeDefinition, attributeStore) && member is IField);

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

		public static IMethod CreateDummyMethodForFieldInitialization(IMember member, ICompilation compilation) {
			var unresolved = new DefaultUnresolvedMethod(member.DeclaringTypeDefinition.Parts[0], "initialization for " + member.Name) {
				Parameters = { new DefaultUnresolvedParameter(member.ReturnType.ToTypeReference(), "value") },
				IsStatic = member.IsStatic,
			};
			IMethod method = new DefaultResolvedMethod(unresolved, compilation.TypeResolveContext.WithCurrentTypeDefinition(member.DeclaringTypeDefinition));
			if (member.DeclaringType is ParameterizedType)
				method = new SpecializedMethod(method, new TypeParameterSubstitution(classTypeArguments: ((ParameterizedType)member.DeclaringType).TypeArguments, methodTypeArguments: null));
			return method;
		}

		public static bool IsJsGeneric(IMethod method, IMetadataImporter metadataImporter) {
			return method.TypeParameters.Count > 0 && !metadataImporter.GetMethodSemantics(method).IgnoreGenericArguments;
		}

		public static bool IsJsGeneric(ITypeDefinition type, IMetadataImporter metadataImporter) {
			return type.TypeParameterCount > 0 && !metadataImporter.GetTypeSemantics(type).IgnoreGenericArguments;
		}

		public static bool IsReflectable(IMember member, IAttributeStore attributeStore) {
			var ra = attributeStore.AttributesFor(member).GetAttribute<ReflectableAttribute>();
			return ra != null && ra.Reflectable;
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
				return JsExpression.Invocation(JsExpression.FunctionDefinition(new string[0], JsStatement.Block(constructorResult.AdditionalStatements.Concat(new[] { JsStatement.Return(constructorResult.Expression) }))));
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

		public static IEnumerable<IAttribute> GetScriptableAttributes(IEnumerable<IAttribute> attributes, IMetadataImporter metadataImporter) {
			return attributes.Where(a => !a.IsConditionallyRemoved && metadataImporter.GetTypeSemantics(a.AttributeType.GetDefinition()).Type != TypeScriptSemantics.ImplType.NotUsableFromScript);
		}

		private static List<JsObjectLiteralProperty> GetCommonMemberInfoProperties(IMember m, ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, Func<IType, JsExpression> instantiateType, bool includeDeclaringType) {
			var result = new List<JsObjectLiteralProperty>();
			var attr = GetScriptableAttributes(m.Attributes, metadataImporter).ToList();
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
				var definition = JsExpression.FunctionDefinition(parameters.Select(p => variables[p].Name), JsStatement.Block(compileResult.AdditionalStatements.Concat(new[] { JsStatement.Return(compileResult.Expression) })));
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
					var definition = JsExpression.FunctionDefinition(parameters.Select(p => variables[p].Name), JsStatement.Block(compileResult.AdditionalStatements.Concat(new[] { JsStatement.Return(compileResult.Expression) })));

					if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.TypeParameter && t.OwnerType == SymbolKind.Method)) {
						definition = JsExpression.FunctionDefinition(method.TypeParameters.Select(namer.GetTypeParameterName), JsStatement.Return(definition));
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

        public static bool HasBaseType(this ITypeDefinition type, string fullTypeName) {
            return type.GetAllBaseTypeDefinitions().Any(t => t.FullName == fullTypeName);
        }

        static JsFunctionDefinitionExpression InsertInitializers(ITypeDefinition type, JsFunctionDefinitionExpression orig, Func<IEnumerable<JsStatement>, IEnumerable<JsStatement>> insert)
        {
            if (orig.Body.Statements.Count > 0 && orig.Body.Statements[0] is JsExpressionStatement)
            {
                // Find out if we are doing constructor chaining. In this case the first statement in the constructor will be {Type}.call(this, ...) or {Type}.namedCtor.call(this, ...)
                var expr = ((JsExpressionStatement)orig.Body.Statements[0]).Expression;
                if (expr is JsInvocationExpression && ((JsInvocationExpression)expr).Method is JsMemberAccessExpression && ((JsInvocationExpression)expr).Arguments.Count > 0 && ((JsInvocationExpression)expr).Arguments[0] is JsThisExpression)
                {
                    expr = ((JsInvocationExpression)expr).Method;
                    if (expr is JsMemberAccessExpression && ((JsMemberAccessExpression)expr).MemberName == "call")
                    {
                        expr = ((JsMemberAccessExpression)expr).Target;
                        if (expr is JsMemberAccessExpression)
                            expr = ((JsMemberAccessExpression)expr).Target;	// Named constructor
                        if (expr is JsTypeReferenceExpression && ((JsTypeReferenceExpression)expr).Type.Equals(type))
                            return orig;	// Yes, we are chaining. Don't initialize the knockout properties.
                    }
                }
            }

            return JsExpression.FunctionDefinition(orig.ParameterNames, JsStatement.Block(insert(orig.Body.Statements)), orig.Name);
        }

        public static JsType InsertInitializers(JsClass c, IEnumerable<JsStatement> initializers, Func<List<JsStatement>, IEnumerable<JsStatement>, IEnumerable<JsStatement>> insert)
        {
            var initList = initializers.Where(i => i != null).ToList();
            if (initList.Count == 0)
                return c;

            var result = c.Clone();
            if (result.UnnamedConstructor != null)
                result.UnnamedConstructor = InsertInitializers(c.CSharpTypeDefinition, result.UnnamedConstructor, body => insert(initList, body));
            var namedConstructors = result.NamedConstructors.Select(x => new JsNamedConstructor(x.Name, InsertInitializers(c.CSharpTypeDefinition, x.Definition, body => insert(initList, body)))).ToList();
            result.NamedConstructors.Clear();
            foreach (var x in namedConstructors)
            {
                result.NamedConstructors.Add(x);
            }
            return result;
        }

        public static JsType PrependInitializers(this JsClass c, IEnumerable<JsStatement> initializers)
        {
            return InsertInitializers(c, initializers, (i, b) => i.Concat(b));
        }

        public static JsType AppendInitializers(this JsClass c, IEnumerable<JsStatement> initializers)
        {
            return InsertInitializers(c, initializers, (i, b) => b.Concat(i));
        }

        public static JsExpression Compile(this JsClass c, string code, IRuntimeLibrary runtimeLibrary, ICompilation compilation, IRuntimeContext context, IErrorReporter errorReporter)
        {
            var method = CreateTypeCheckMethod(Saltarelle.Compiler.Utils.SelfParameterize(c.CSharpTypeDefinition), compilation);

            var errors = new List<string>();
            var tokens = InlineCodeMethodCompiler.Tokenize(method, code, errors.Add);
            if (errors.Count == 0)
            {
                var result = InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, JsExpression.This, new JsExpression[0],
                n =>
                {
                    var type = ReflectionHelper.ParseReflectionName(n).Resolve(compilation);
                    if (type.Kind == TypeKind.Unknown)
                    {
                        errors.Add("Unknown type '" + n + "' specified in inline implementation");
                        return JsExpression.Null;
                    }
                    return runtimeLibrary.InstantiateType(type, context);
                },
                t => runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, context),
                errors.Add);

                if (errors.Count == 0)
                    return result;

                foreach (var e in errors)
                {
                    errorReporter.Message(CoreLib.Plugin.Messages._7157, c.CSharpTypeDefinition.FullName, e);
                }
            }
            return null;
        }
	}
}
