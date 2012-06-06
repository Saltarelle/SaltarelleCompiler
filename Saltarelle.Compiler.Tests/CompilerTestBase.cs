using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Tests {
    public class CompilerTestBase {
        private class MockSourceFile : ISourceFile {
            private readonly string _fileName;
            private readonly string _content;

            public MockSourceFile(string fileName, string content) {
                _fileName = fileName;
                _content  = content;
            }

            public string FileName {
                get { return _fileName; }
            }

            public TextReader Open() {
                return new StringReader(_content);
            }
        }

        protected class MockNamingConventionResolver : INamingConventionResolver {
            public MockNamingConventionResolver() {
                GetTypeName                     = t => t.Name;
                GetTypeParameterName            = t => "$" + t.Name;
                GetMethodImplementation         = m => MethodImplOptions.NormalMethod(m.Name);
                GetConstructorImplementation    = c => (c.DeclaringType.GetConstructors().Count() == 1 || c.Parameters.Count == 0) ? ConstructorImplOptions.Unnamed() : ConstructorImplOptions.Named("ctor$" + string.Join("$", c.Parameters.Select(p => p.Type.Name)));
                GetPropertyImplementation       = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name), MethodImplOptions.NormalMethod("set_" + p.Name));
                GetAutoPropertyBackingFieldName = p => "$" + p.Name;
                GetFieldImplementation          = f => FieldImplOptions.Field("$" + f.Name);
                GetEventImplementation          = e => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.NormalMethod("add_" + e.Name), MethodImplOptions.NormalMethod("remove_" + e.Name));
                GetAutoEventBackingFieldName    = e => "$" + e.Name;
                GetEnumValueName                = f => "$" + f.Name;
                GetVariableName                 = (v, used) => {
                                                                   string baseName = "$" + v.Name;
                                                                   if (!used.Contains(baseName))
                                                                       return baseName;
                                                                   int i = 2;
                                                                   while (used.Contains(baseName + i.ToString(CultureInfo.InvariantCulture)))
                                                                      i++;
                                                                   return baseName + i.ToString(CultureInfo.InvariantCulture);
                                                               };
				GetTemporaryVariableName        = index => string.Format(CultureInfo.InvariantCulture, "$tmp{0}", index + 1);
				ThisAlias                       = "$this";
            }

            public Func<ITypeDefinition, string> GetTypeName { get; set; }
            public Func<ITypeParameter, string> GetTypeParameterName { get; set; }
            public Func<IMethod, MethodImplOptions> GetMethodImplementation { get; set; }
            public Func<IMethod, ConstructorImplOptions> GetConstructorImplementation { get; set; }
            public Func<IProperty, PropertyImplOptions> GetPropertyImplementation { get; set; }
            public Func<IProperty, string> GetAutoPropertyBackingFieldName { get; set; }
            public Func<IField, FieldImplOptions> GetFieldImplementation { get; set; }
            public Func<IEvent, EventImplOptions> GetEventImplementation { get; set; }
            public Func<IEvent, string> GetAutoEventBackingFieldName { get; set; }
            public Func<IField, string> GetEnumValueName { get; set; }
            public Func<IVariable, ISet<string>, string> GetVariableName { get; set; }
			public Func<int, string> GetTemporaryVariableName { get; set; }
			public string ThisAlias { get; set; }

            string INamingConventionResolver.GetTypeName(ITypeDefinition typeDefinition) {
                return GetTypeName(typeDefinition);
            }

            string INamingConventionResolver.GetTypeParameterName(ITypeParameter typeDefinition) {
                return GetTypeParameterName(typeDefinition);
            }

            MethodImplOptions INamingConventionResolver.GetMethodImplementation(IMethod method) {
                return GetMethodImplementation(method);
            }

            ConstructorImplOptions INamingConventionResolver.GetConstructorImplementation(IMethod method) {
                return GetConstructorImplementation(method);
            }

            PropertyImplOptions INamingConventionResolver.GetPropertyImplementation(IProperty property) {
                return GetPropertyImplementation(property);
            }

            string INamingConventionResolver.GetAutoPropertyBackingFieldName(IProperty property) {
                return GetAutoPropertyBackingFieldName(property);
            }

            FieldImplOptions INamingConventionResolver.GetFieldImplementation(IField field) {
                return GetFieldImplementation(field);
            }

            EventImplOptions INamingConventionResolver.GetEventImplementation(IEvent evt) {
                return GetEventImplementation(evt);
            }

            string INamingConventionResolver.GetAutoEventBackingFieldName(IEvent evt) {
                return GetAutoEventBackingFieldName(evt);
            }

            string INamingConventionResolver.GetEnumValueName(IField value) {
                return GetEnumValueName(value);
            }

            string INamingConventionResolver.GetVariableName(IVariable variable, ISet<string> usedNames) {
                return GetVariableName(variable, usedNames);
            }

			string INamingConventionResolver.GetTemporaryVariableName(int index) {
				return GetTemporaryVariableName(index);
			}

			string INamingConventionResolver.ThisAlias {
				get { return ThisAlias; }
			}
        }

		public class MockRuntimeLibrary : IRuntimeLibrary {
			public MockRuntimeLibrary() {
				TypeIs                      = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$TypeIs"), e, t);
				TryCast                     = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$TryCast"), e, t);
				Downcast                    = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$Cast"), e, t);
				ImplicitReferenceConversion = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$Upcast"), e, t);
				InstantiateGenericType      = (t, a)        => JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { t }.Concat(a));
				InstantiateGenericMethod    = (m, a)        => JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericMethod"), new[] { m }.Concat(a));
				MakeException               = (e)           => JsExpression.Invocation(JsExpression.Identifier("$MakeException"), e);
				IntegerDivision             = (n, d)        => JsExpression.Invocation(JsExpression.Identifier("$IntDiv"), n, d);
				FloatToInt                  = e             => JsExpression.Invocation(JsExpression.Identifier("$Truncate"), e);
				Coalesce                    = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$Coalesce"), a, b);
				Lift                        = (e)           => JsExpression.Invocation(JsExpression.Identifier("$Lift"), e);
				LiftedBooleanAnd            = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanAnd"), a, b);
				LiftedBooleanOr             = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanOr"), a, b);
				Bind                        = (f, t)        => JsExpression.Invocation(JsExpression.Identifier("$Bind"), f, t);
				Default                     = t             => JsExpression.Invocation(JsExpression.Identifier("$Default"), t);
				CreateArray                 = s             => JsExpression.Invocation(JsExpression.Identifier("$CreateArray"), s);
				CallBase                    = (t, n, ta, a) => JsExpression.Invocation(JsExpression.Identifier("$CallBase"), new[] { t, JsExpression.String(n), JsExpression.ArrayLiteral(ta), JsExpression.ArrayLiteral(a) });
				BindBaseCall                = (t, n, ta, a) => JsExpression.Invocation(JsExpression.Identifier("$BindBaseCall"), new[] { t, JsExpression.String(n), JsExpression.ArrayLiteral(ta), a });
			}

			public Func<JsExpression, JsExpression, JsExpression> TypeIs { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> TryCast { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> Downcast { get; set; }
			public Func<JsExpression, IEnumerable<JsExpression>, JsExpression> InstantiateGenericType { get; set; }
			public Func<JsExpression, IEnumerable<JsExpression>, JsExpression> InstantiateGenericMethod { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> ImplicitReferenceConversion { get; set; }
			public Func<JsExpression, JsExpression> MakeException { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> IntegerDivision { get; set; }
			public Func<JsExpression, JsExpression> FloatToInt { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> Coalesce { get; set; }
			public Func<JsExpression, JsExpression> Lift { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> LiftedBooleanAnd { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> LiftedBooleanOr { get; set; }
			public Func<JsExpression, JsExpression, JsExpression> Bind { get; set; }
			public Func<JsExpression, JsExpression> Default { get; set; }
			public Func<JsExpression, JsExpression> CreateArray { get; set; }
			public Func<JsExpression, string, IEnumerable<JsExpression>, IEnumerable<JsExpression>, JsExpression> CallBase { get; set; }
			public Func<JsExpression, string, IEnumerable<JsExpression>, JsExpression, JsExpression> BindBaseCall { get; set; }
			
			JsExpression IRuntimeLibrary.TypeIs(JsExpression expression, JsExpression targetType) {
				return TypeIs(expression, targetType);
			}

			JsExpression IRuntimeLibrary.TryCast(JsExpression expression, JsExpression targetType) {
				return TryCast(expression, targetType);
			}

			JsExpression IRuntimeLibrary.Downcast(JsExpression expression, JsExpression targetType) {
				return Downcast(expression, targetType);
			}

			JsExpression IRuntimeLibrary.ImplicitReferenceConversion(JsExpression expression, JsExpression targetType) {
				return ImplicitReferenceConversion(expression, targetType);
			}

			JsExpression IRuntimeLibrary.InstantiateGenericType(JsExpression type, IEnumerable<JsExpression> typeArguments) {
				return InstantiateGenericType(type, typeArguments);
			}

			JsExpression IRuntimeLibrary.InstantiateGenericMethod(JsExpression type, IEnumerable<JsExpression> typeArguments) {
				return InstantiateGenericMethod(type, typeArguments);
			}

			JsExpression IRuntimeLibrary.MakeException(JsExpression operand) {
				return MakeException(operand);
			}

			JsExpression IRuntimeLibrary.IntegerDivision(JsExpression numerator, JsExpression denominator) {
				return IntegerDivision(numerator, denominator);
			}

			JsExpression IRuntimeLibrary.FloatToInt(JsExpression operand) {
				return FloatToInt(operand);
			}

			JsExpression IRuntimeLibrary.Coalesce(JsExpression a, JsExpression b) {
				return Coalesce(a, b);
			}

			JsExpression IRuntimeLibrary.Lift(JsExpression expression) {
				return Lift(expression);
			}

			JsExpression IRuntimeLibrary.LiftedBooleanAnd(JsExpression a, JsExpression b) {
				return LiftedBooleanAnd(a, b);
			}

			JsExpression IRuntimeLibrary.LiftedBooleanOr(JsExpression a, JsExpression b) {
				return LiftedBooleanOr(a, b);
			}

			JsExpression IRuntimeLibrary.Bind(JsExpression function, JsExpression target) {
				return Bind(function, target);
			}

			JsExpression IRuntimeLibrary.Default(JsExpression type) {
				return Default(type);
			}

			JsExpression IRuntimeLibrary.CreateArray(JsExpression size) {
				return CreateArray(size);
			}

			JsExpression IRuntimeLibrary.CallBase(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
				return CallBase(baseType, methodName, typeArguments, thisAndArguments);
			}

			JsExpression IRuntimeLibrary.BindBaseCall(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, JsExpression @this) {
				return BindBaseCall(baseType, methodName, typeArguments, @this);
			}
		}

        protected class MockErrorReporter : IErrorReporter {
        	private readonly bool _logToConsole;
        	public List<string> AllMessages { get; set; }

            public MockErrorReporter(bool logToConsole) {
            	_logToConsole = logToConsole;
            	AllMessages = new List<string>();
                Error   = s => { s = "Error: " + s; if (logToConsole) Console.WriteLine(s); AllMessages.Add(s); };
                Warning = s => { s = "Warning: " + s; if (logToConsole) Console.WriteLine(s); AllMessages.Add(s); };
            }

            public Action<string> Error { get; set; }
            public Action<string> Warning { get; set; }

            void IErrorReporter.Error(string message) {
                Error(message);
            }

            void IErrorReporter.Warning(string message) {
                Warning(message);
            }
        }

        private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location));
        protected IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

        protected ReadOnlyCollection<JsType> CompiledTypes { get; private set; }

        protected void Compile(IEnumerable<string> sources, INamingConventionResolver namingConvention = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, Action<IMethod, JsFunctionDefinitionExpression, MethodCompiler> methodCompiled = null) {
            var sourceFiles = sources.Select((s, i) => new MockSourceFile("File" + i + ".cs", s)).ToList();
            bool defaultErrorHandling = false;
            if (errorReporter == null) {
                defaultErrorHandling = true;
                errorReporter = new MockErrorReporter(true);
            }

            var compiler = new Compiler(namingConvention ?? new MockNamingConventionResolver(), runtimeLibrary ?? new MockRuntimeLibrary(), errorReporter);
            if (methodCompiled != null)
                compiler.MethodCompiled += methodCompiled;

            CompiledTypes = compiler.Compile(sourceFiles, new[] { Mscorlib }).AsReadOnly();
            if (defaultErrorHandling) {
                ((MockErrorReporter)errorReporter).AllMessages.Should().BeEmpty("Compile should not generate errors");
            }
        }

        protected void Compile(params string[] sources) {
            Compile((IEnumerable<string>)sources);
        }

        protected string Stringify(JsExpression expression) {
            switch (expression.NodeType) {
                case ExpressionNodeType.Identifier: return ((JsIdentifierExpression)expression).Name;
                case ExpressionNodeType.TypeReference: return "{" + ((JsTypeReferenceExpression)expression).TypeDefinition.ReflectionName + "}";
                default: throw new ArgumentException("expression");
            }
        }

        protected string Stringify(JsConstructedType tp) {
            return Stringify(tp.UnboundType) + (tp.TypeArguments.Count > 0 ? "<" + string.Join(",", tp.TypeArguments.Select(x => Stringify(x))) + ">" : "");
        }

        protected JsClass FindClass(string name) {
            var result = CompiledTypes.SingleOrDefault(t => t.Name.ToString() == name);
            if (result == null) Assert.Fail("Could not find type " + name);
            if (!(result is JsClass)) Assert.Fail("Found type is not a JsClass, it is a " + result.GetType().Name);
            return (JsClass)result;
        }

        protected JsEnum FindEnum(string name) {
            var result = CompiledTypes.SingleOrDefault(t => t.Name.ToString() == name);
            if (result == null) Assert.Fail("Could not find type " + name);
            if (!(result is JsEnum)) Assert.Fail("Found type is not a JsEnum, it is a " + result.GetType().Name);
            return (JsEnum)result;
        }

        protected JsMethod FindInstanceMethod(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.InstanceMethods.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
        }

        protected string FindInstanceFieldInitializer(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.UnnamedConstructor.Body.Statements
                                              .OfType<JsExpressionStatement>()
                                              .Select(s => s.Expression)
                                              .OfType<JsBinaryExpression>()
                                              .Where(be =>    be.NodeType == ExpressionNodeType.Assign
                                                           && be.Left is JsMemberAccessExpression
                                                           && ((JsMemberAccessExpression)be.Left).Target is JsThisExpression
                                                           && ((JsMemberAccessExpression)be.Left).Member == name.Substring(lastDot + 1))
                                              .Select(be => OutputFormatter.Format(be.Right, true))
                                              .SingleOrDefault();
        }

        protected string FindStaticFieldInitializer(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.StaticInitStatements.OfType<JsExpressionStatement>()
                                           .Select(s => s.Expression)
                                           .OfType<JsBinaryExpression>()
                                           .Where(be =>    be.NodeType == ExpressionNodeType.Assign
                                                        && be.Left is JsMemberAccessExpression
                                                        && ((JsMemberAccessExpression)be.Left).Target is JsTypeReferenceExpression
                                                        && ((JsMemberAccessExpression)be.Left).Member == name.Substring(lastDot + 1))
                                           .Select(be => OutputFormatter.Format(be.Right, true))
                                           .SingleOrDefault();
        }

        protected JsMethod FindStaticMethod(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.StaticMethods.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
        }

        protected JsNamedConstructor FindNamedConstructor(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindClass(name.Substring(0, lastDot));
            return cls.NamedConstructors.SingleOrDefault(m => m.Name == name.Substring(lastDot + 1));
        }

        protected JsEnumValue FindEnumValue(string name) {
            var lastDot = name.LastIndexOf('.');
            var cls = FindEnum(name.Substring(0, lastDot));
            return cls.Values.SingleOrDefault(f => f.Name == name.Substring(lastDot + 1));
        }
    }
}
