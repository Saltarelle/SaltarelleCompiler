using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	// Done:
	// [ScriptName] (Type | Method)
	// [IgnoreNamespace] (Type)
	// [ScriptNamespaceAttribute] (Type)
	// [PreserveName] (Type | Method)
	// [PreserveCase] (Property | Event | Field)
	// [ScriptSkip] (Method)
	// [AlternateSignature] (Method)

	// To handle:
	// [NonScriptable] (Type | Constructor | Method | Property | Event) (event missed in ScriptSharp)
	// [Imported] (Type | Struct)
	// [ScriptAssembly] (Assembly) ?
	// [ScriptQualifier] (Assembly)
	// [ScriptNamespaceAttribute] (Assembly)
	// [Resources] (Class) ?
	// [GlobalMethods] (Class) - Needs better support in the compiler
	// [Mixin] (Class) ?
	// [NamedValues] (Enum) - Needs better support in the compiler
	// [NumericValues] (Enum)
	// [AlternateSignature] (Constructor)
	// [IntrinsicProperty] (Property (/indexer))
	// [ScriptName] (Field | Property | Event)
	// [PreserveCase] (Property | Event | Field)
	// [PreserveName] (Property | Event | Field)
	// [ScriptAlias] (Method | Property) = Literal code ?
	// Record
	// New attributes:
	// [InstanceMethodOnFirstArgument]

	public class ScriptSharpMetadataImporter : INamingConventionResolver {
		/// <summary>
		/// Used to deterministically order members. It is assumed that all members belong to the same type.
		/// </summary>
		private class MemberOrderer : IComparer<IMember> {
			public static readonly MemberOrderer Instance = new MemberOrderer();

			private MemberOrderer() {
			}

			private int CompareMethods(IMethod x, IMethod y) {
				int result = string.CompareOrdinal(x.Name, y.Name);
				if (result != 0)
					return result;
				if (x.Parameters.Count > y.Parameters.Count)
					return 1;
				else if (x.Parameters.Count < y.Parameters.Count)
					return -1;

				var xparms = string.Join(",", x.Parameters.Select(p => p.Type.FullName));
				var yparms = string.Join(",", y.Parameters.Select(p => p.Type.FullName));

				return string.CompareOrdinal(xparms, yparms);
			}

			public int Compare(IMember x, IMember y) {
				if (x is IField) {
					if (y is IField) {
						return string.CompareOrdinal(x.Name, y.Name);
					}
					else 
						return -1;
				}
				else if (y is IField) {
					return 1;
				}

				if (x is IProperty) {
					if (y is IProperty) {
						return string.CompareOrdinal(x.Name, y.Name);
					}
					else 
						return -1;
				}
				else if (y is IProperty) {
					return 1;
				}

				if (x is IEvent) {
					if (y is IEvent) {
						return string.CompareOrdinal(x.Name, y.Name);
					}
					else 
						return -1;
				}
				else if (y is IEvent) {
					return 1;
				}

				if (x is IMethod) {
					if (y is IMethod) {
						return CompareMethods((IMethod)x, (IMethod)y);
					}
					else
						return -1;
				}
				else if (y is IMethod) {
					return 1;
				}

				throw new ArgumentException("Invalid member type" + x.GetType().FullName);
			}
		}

		private Dictionary<ITypeDefinition, string> _typeNames;
		private Dictionary<ITypeDefinition, Dictionary<string, List<IMember>>> _memberNamesByType;
		private Dictionary<IMethod, MethodScriptSemantics> _methodSemantics;
		private Dictionary<string, string> _errors;
		private int _internalInterfaceMemberCount;
		private bool _minimizeNames;

		public ScriptSharpMetadataImporter(bool minimizeNames) {
			_minimizeNames = minimizeNames;
		}

		private string MakeCamelCase(string s) {
			if (string.IsNullOrEmpty(s))
				return s;
			if (s.Equals("ID", StringComparison.Ordinal))
				return "id";

			bool hasNonUppercase = false;
			int numUppercaseChars = 0;
			for (int index = 0; index < s.Length; index++) {
				if (char.IsUpper(s, index)) {
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
				return char.ToLower(s[0], CultureInfo.InvariantCulture) + s.Substring(1);
		}

		private static readonly string encodeNumberTable = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		private string EncodeNumber(int i) {
			if (encodeNumberTable.Length != 62 || encodeNumberTable.Distinct().Count() != 62)
				throw new ArgumentException("X");
			string result = encodeNumberTable.Substring(i % encodeNumberTable.Length, 1);
			while (i >= encodeNumberTable.Length) {
				i /= encodeNumberTable.Length;
				result = encodeNumberTable.Substring(i % encodeNumberTable.Length, 1) + result;
			}
			return result;
		}

		private string GetDefaultTypeName(ITypeDefinition def) {
			int outerCount = (def.DeclaringTypeDefinition != null ? def.DeclaringTypeDefinition.TypeParameters.Count : 0);
			return def.Name + (def.TypeParameterCount != outerCount ? "$" + (def.TypeParameterCount - outerCount).ToString(CultureInfo.InvariantCulture) : "");
		}

		private IList<object> GetAttributePositionalArgs(IEntity entity, string attributeName) {
			attributeName = "System.Runtime.CompilerServices." + attributeName;
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == attributeName);
			return attr != null ? attr.PositionalArguments.Select(arg => arg.ConstantValue).ToList() : null;
		}

		private string DetermineNamespace(ITypeDefinition typeDefinition) {
			while (typeDefinition.DeclaringTypeDefinition != null) {
				typeDefinition = typeDefinition.DeclaringTypeDefinition;
			}

			var ina = GetAttributePositionalArgs(typeDefinition, "IgnoreNamespaceAttribute");
			var sna = GetAttributePositionalArgs(typeDefinition, "ScriptNamespaceAttribute");
			if (ina != null) {
				if (sna != null) {
					_errors[typeDefinition.FullName + ":Namespace"] = "The type " + typeDefinition.FullName + " has both [IgnoreNamespace] and [ScriptNamespace] specified. At most one of these attributes can be specified for a type.";
					return typeDefinition.FullName;
				}
				else {
					return "";
				}
			}
			else {
				if (sna != null) {
					string arg = (string)sna[0];
					if (arg == null || (arg != "" && !arg.IsValidNestedJavaScriptIdentifier()))
						_errors[typeDefinition.FullName + ":Namespace"] = typeDefinition.FullName + ": The argument for [ScriptNamespace], when applied to a type, must be a valid JavaScript qualified identifier.";
					return arg;
				}
				else {
					return typeDefinition.Namespace;
				}
			}
		}

		private Tuple<string, string> SplitName(string typeName) {
			int dot = typeName.LastIndexOf('.');
			return dot > 0 ? Tuple.Create(typeName.Substring(0, dot), typeName.Substring(dot + 1)) : Tuple.Create("", typeName);
		}

		private string DetermineTypeName(ITypeDefinition typeDefinition) {
			var scriptNameAttr = GetAttributePositionalArgs(typeDefinition, "ScriptNameAttribute");
			string typeName, nmspace;
			if (scriptNameAttr != null && scriptNameAttr[0] != null && ((string)scriptNameAttr[0]).IsValidJavaScriptIdentifier()) {
				typeName = (string)scriptNameAttr[0];
				nmspace = DetermineNamespace(typeDefinition);
			}
			else {
				if (scriptNameAttr != null) {
					_errors[typeDefinition.FullName + ":Name"] = typeDefinition.FullName + ": The argument for [ScriptName], when applied to a type, must be a valid JavaScript identifier.";
				}

				if (_minimizeNames && !IsPublic(typeDefinition) && GetAttributePositionalArgs(typeDefinition, "PreserveNameAttribute") == null) {
					nmspace = DetermineNamespace(typeDefinition);
					int index = _typeNames.Values.Select(tn => SplitName(tn)).Count(tn => tn.Item1 == nmspace && tn.Item2.StartsWith("$"));
					typeName = "$" + index.ToString(CultureInfo.InvariantCulture);
				}
				else {
					typeName = GetDefaultTypeName(typeDefinition);
					if (typeDefinition.DeclaringTypeDefinition != null) {
						if (GetAttributePositionalArgs(typeDefinition, "IgnoreNamespaceAttribute") != null || GetAttributePositionalArgs(typeDefinition, "ScriptNamespaceAttribute") != null) {
							_errors[typeDefinition.FullName + ":Namespace"] = "[IgnoreNamespace] or [ScriptNamespace] cannot be specified for the nested type " + typeDefinition.FullName + ".";
						}

						typeName = GetTypeName(typeDefinition.DeclaringTypeDefinition) + "$" + typeName;
						nmspace = "";
					}
					else {
						nmspace = DetermineNamespace(typeDefinition);
					}
				}
			}

			return !string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName;
		}

		private bool IsPublic(ITypeDefinition type) {
            // A type is public if the type and all its declaring types are public or protected (or protected internal).
            while (type != null) {
                bool isPublic = (type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Protected || type.Accessibility == Accessibility.ProtectedOrInternal);
                if (!isPublic)
                    return false;
                type = type.DeclaringTypeDefinition;
            }
            return true;
		}

		private bool IsPublic(IMember member) {
			return IsPublic(member.DeclaringType.GetDefinition()) && (member.Accessibility == Accessibility.Public || member.Accessibility == Accessibility.Protected || member.Accessibility == Accessibility.ProtectedOrInternal);
		}

		private Dictionary<string, List<IMember>> GetMemberNames(ITypeDefinition typeDefinition) {
			Dictionary<string, List<IMember>> result;
			if (!_memberNamesByType.TryGetValue(typeDefinition, out result))
				_memberNamesByType[typeDefinition] = result = DetermineMemberNames(typeDefinition);
			return result;
		}

		private Dictionary<string, List<IMember>> GetMemberNames(IEnumerable<ITypeDefinition> typeDefinitions) {
			return (from def in typeDefinitions from m in GetMemberNames(def) group m by m.Key into g select new { g.Key, Value = g.SelectMany(x => x.Value).ToList() }).ToDictionary(x => x.Key, x => x.Value);
		}

		private Tuple<string, bool> DeterminePreferredMemberName(IMember member) {
			var asa = GetAttributePositionalArgs(member, "AlternateSignatureAttribute");
			if (asa != null) {
				var otherMembers = member.DeclaringTypeDefinition.Methods.Where(m => m.Name == member.Name && GetAttributePositionalArgs(m, "AlternateSignatureAttribute") == null).ToList();
				if (otherMembers.Count == 1) {
					return DeterminePreferredMemberName(otherMembers[0]);
				}
				else {
					_errors[GetQualifiedMemberName(member) + ":NoMainMethod"] = "The member " + GetQualifiedMemberName(member) + " has an [AlternateSignatureAttribute], but there is not exactly one other method with the same name that does not have that attribute.";
					return Tuple.Create(member.Name, false);
				}
			}

			var sna = GetAttributePositionalArgs(member, "ScriptNameAttribute");
			if (sna != null) {
				string name = (string)sna[0];
				if (name != "" && !name.IsValidJavaScriptIdentifier()) {
					_errors[GetQualifiedMemberName(member) + ":InvalidName"] = "The name specified in the [ScriptName] attribute for type method " + GetQualifiedMemberName(member) + " must be a valid JavaScript identifier, or be blank.";
				}
				return Tuple.Create(name, true);
			}
			var pca = GetAttributePositionalArgs(member, "PreserveCaseAttribute");
			if (pca != null)
				return Tuple.Create(member.Name, true);
			var pna = GetAttributePositionalArgs(member, "PreserveNameAttribute");
			if (pna != null)
				return Tuple.Create(MakeCamelCase(member.Name), true);

			// Handle [AlternateSignature]
			bool minimize = _minimizeNames && !IsPublic(member);

			return Tuple.Create(minimize ? null : MakeCamelCase(member.Name), false);
		}

		public string GetQualifiedMemberName(IMember member) {
			return member.DeclaringType.FullName + "." + member.Name;
		}

		private Dictionary<string, List<IMember>> DetermineMemberNames(ITypeDefinition typeDefinition) {
			var allMembers = GetMemberNames(typeDefinition.GetAllBaseTypeDefinitions().Where(x => x != typeDefinition));
			foreach (var m in allMembers.Where(kvp => kvp.Value.Count > 1)) {
				// TODO: Determine if we need to raise an error here.
			}

			var membersByName =   from m in typeDefinition.GetMembers(options: GetMemberOptions.IgnoreInheritedMembers)
			                       let name = DeterminePreferredMemberName(m)
			                     group new { m, name } by name.Item1 into g
			                    select new { Name = g.Key, Members = g.Select(x => new { Member = x.m, NameSpecified = x.name.Item2 }).ToList() };

			foreach (var current in membersByName) {
				foreach (var m in current.Members.OrderByDescending(x => x.NameSpecified).ThenBy(x => x.Member, MemberOrderer.Instance)) {
					if (m.Member is IMethod) {
						var method = (IMethod)m.Member;

						if (method.IsConstructor) {
							// TODO
						}
						else {
							var ssa = GetAttributePositionalArgs(m.Member, "ScriptSkipAttribute");
							if (ssa != null) {
								// [ScriptSkip] - Skip invocation of the method entirely.
								if (typeDefinition.Kind == TypeKind.Interface) {
									_errors[GetQualifiedMemberName(m.Member) + ":ScriptSkipOnInterfaceMember"] = "The member " + GetQualifiedMemberName(m.Member) + " cannot have a [ScriptSkipAttribute] because it is an interface method.";
									_methodSemantics[method] = MethodScriptSemantics.NormalMethod(m.Member.Name);
								}
								else if (method.IsOverridable) {
									_errors[GetQualifiedMemberName(m.Member) + ":ScriptSkipOnOverridable"] = "The member " + GetQualifiedMemberName(m.Member) + " cannot have a [ScriptSkipAttribute] because it is overridable.";
									_methodSemantics[method] = MethodScriptSemantics.NormalMethod(m.Member.Name);
								}
								else {
									if (method.IsStatic) {
										if (method.Parameters.Count != 1)
											_errors[GetQualifiedMemberName(m.Member) + ":ScriptSkipParameterCount"] = "The static method " + GetQualifiedMemberName(m.Member) + " must have exactly one parameter in order to have a [ScriptSkipAttribute].";
										_methodSemantics[method] = MethodScriptSemantics.InlineCode("{0}");
									}
									else {
										if (method.Parameters.Count != 0)
											_errors[GetQualifiedMemberName(m.Member) + ":ScriptSkipParameterCount"] = "The instance method " + GetQualifiedMemberName(m.Member) + " must have no parameters in order to have a [ScriptSkipAttribute].";
										_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}");
									}
								}
							}
							else {
								if (m.Member.IsOverride) {
									if (m.NameSpecified) {
										_errors[GetQualifiedMemberName(m.Member) + ":CannotSpecifyName"] = "The [ScriptName], [PreserveName] and [PreserveCase] attributes cannot be specified on method the method " + GetQualifiedMemberName(m.Member) + " because it overrides a base member. Specify the attribute on the base member instead.";
									}

									var semantics = _methodSemantics[(IMethod)InheritanceHelper.GetBaseMember(method)];
									_methodSemantics[method] = semantics;
									var errorMethod = m.Member.ImplementedInterfaceMembers.FirstOrDefault(im => GetMethodImplementation((IMethod)im.MemberDefinition).Name != semantics.Name);
									if (errorMethod != null) {
										_errors[GetQualifiedMemberName(m.Member) + ":MultipleInterfaceImplementations"] = "The overriding member " + GetQualifiedMemberName(m.Member) + " cannot implement the interface method " + GetQualifiedMemberName(errorMethod) + " because it has a different script name. Consider using explicit interface implementation";
									}
								}
								else if (m.Member.ImplementedInterfaceMembers.Count > 0) {
									if (m.NameSpecified) {
										_errors[GetQualifiedMemberName(m.Member) + ":CannotSpecifyName"] = "The [ScriptName], [PreserveName] and [PreserveCase] attributes cannot be specified on the method " + GetQualifiedMemberName(m.Member) + " because it implements an interface member. Specify the attribute on the interface member instead, or consider using explicit interface implementation.";
									}

									if (m.Member.ImplementedInterfaceMembers.Select(im => GetMethodImplementation((IMethod)im.MemberDefinition).Name).Distinct().Count() > 1) {
										_errors[GetQualifiedMemberName(m.Member) + ":MultipleInterfaceImplementations"] = "The member " + GetQualifiedMemberName(m.Member) + " cannot implement multiple interface methods with differing script names. Consider using explicit interface implementation.";
									}

									_methodSemantics[method] = _methodSemantics[(IMethod)method.ImplementedInterfaceMembers[0].MemberDefinition];
								}
								else {
									if (current.Name == "") {
										// Special case - Script# supports setting the name of a method to an empty string, which means that it simply removes the name (eg. "x.M(a)" becomes "x(a)"). We model this with literal code.
										if (typeDefinition.Kind == TypeKind.Interface) {
											_errors[GetQualifiedMemberName(m.Member) + ":InterfaceMethodWithEmptyName"] = "The member " + GetQualifiedMemberName(m.Member) + " cannot have an empty name specified in its [ScriptName] because it is an interface method.";
											_methodSemantics[method] = MethodScriptSemantics.NormalMethod(m.Member.Name);
										}
										else if (method.IsOverridable) {
											_errors[GetQualifiedMemberName(m.Member) + ":OverridableWithEmptyName"] = "The member " + GetQualifiedMemberName(m.Member) + " cannot have an empty name specified in its [ScriptName] because it is overridable.";
											_methodSemantics[method] = MethodScriptSemantics.NormalMethod(m.Member.Name);
										}
										else {
											_methodSemantics[method] = MethodScriptSemantics.InlineCode("{this}(" + string.Join(", ", method.Parameters.Select(p => "{" + p.Name + "}")) + ")");
										}
									}
									else {
										string name = current.Name;

										if (!m.NameSpecified) {
											// The name was not explicitly specified, so ensure that we have a unique name.
											if (name == null && typeDefinition.Kind == TypeKind.Interface) {
												// Minimized interface names need to be unique within the assembly, otherwise we have a very high risk of collisions (100% when a type implements more than one internal interface).
												name = "$I" + EncodeNumber(_internalInterfaceMemberCount++);
											}
											else {
												int i = (name == null ? 0 : 1);
												while (name == null || allMembers.ContainsKey(name)) {
													name = current.Name + "$" + EncodeNumber(i);
													i++;
												}
											}
										}

										_methodSemantics[method] = MethodScriptSemantics.NormalMethod(name);

										if (allMembers.ContainsKey(name))
											allMembers[name].Add(m.Member);
										else
											allMembers[name] = new List<IMember> { m.Member };
									}
								}
							}
						}
					}
				}
			}

			return allMembers;
		}

		public bool Prepare(IEnumerable<ITypeDefinition> types, IAssembly mainAssembly, IErrorReporter errorReporter) {
			_internalInterfaceMemberCount = 0;
			_errors = new Dictionary<string, string>();
			var l = types.ToList();
			_typeNames = new Dictionary<ITypeDefinition, string>();
			_memberNamesByType = new Dictionary<ITypeDefinition, Dictionary<string, List<IMember>>>();
			_methodSemantics = new Dictionary<IMethod, MethodScriptSemantics>();
			foreach (var t in l.Where(t => t.ParentAssembly == mainAssembly || IsPublic(t))) {
				_typeNames[t] = DetermineTypeName(t);
				GetMemberNames(t);
			}

			foreach (var e in _errors.Values)
				errorReporter.Error(e);
			return _errors.Count == 0;
		}

		public string GetTypeName(ITypeDefinition typeDefinition) {
			return _typeNames[typeDefinition];
		}

		public string GetTypeParameterName(ITypeParameter typeParameter) {
			throw new NotImplementedException();
		}

		public MethodScriptSemantics GetMethodImplementation(IMethod method) {
			return _methodSemantics[method];
		}

		public ConstructorScriptSemantics GetConstructorImplementation(IMethod method) {
			throw new NotImplementedException();
		}

		public PropertyScriptSemantics GetPropertyImplementation(IProperty property) {
			throw new NotImplementedException();
		}

		public string GetAutoPropertyBackingFieldName(IProperty property) {
			throw new NotImplementedException();
		}

		public FieldScriptSemantics GetFieldImplementation(IField property) {
			throw new NotImplementedException();
		}

		public EventScriptSemantics GetEventImplementation(IEvent evt) {
			throw new NotImplementedException();
		}

		public string GetAutoEventBackingFieldName(IEvent evt) {
			throw new NotImplementedException();
		}

		public string GetEnumValueName(IField value) {
			throw new NotImplementedException();
		}

		public string GetVariableName(IVariable variable, ISet<string> usedNames) {
			throw new NotImplementedException();
		}

		public string GetTemporaryVariableName(int index) {
			throw new NotImplementedException();
		}

		public string ThisAlias {
			get { throw new NotImplementedException(); }
		}
	}
}
