using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.OOPEmulator {
	public class ScriptSharpOOPEmulator : IOOPEmulator {
		private const string Prototype = "prototype";
		private const string RegisterNamespace = "registerNamespace";
		private const string RegisterClass = "registerClass";
		private const string RegisterInterface = "registerInterface";
		private const string RegisterEnum = "registerEnum";
		private const string TypeNameField = "__typeName";
		private const string RegisterGenericInstance = "registerGenericInstance";
		private const string RegisterGenericClass = "registerGenericClass";
		private const string RegisterGenericInterface = "registerGenericInterface";
		private const string GlobalMethodsAttribute = "GlobalMethodsAttribute";
		private const string InstantiatedGenericTypeVariableName = "$type";
		private const string InstantiatedGenericNameVariableName = "$name";

		private INamingConventionResolver _namingConvention;

		public ScriptSharpOOPEmulator(INamingConventionResolver namingConvention) {
			_namingConvention = namingConvention;
		}

		private IList<object> GetAttributePositionalArgs(IEntity entity, string attributeName) {
			attributeName = "System.Runtime.CompilerServices." + attributeName;
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == attributeName);
			return attr != null ? attr.PositionalArguments.Select(arg => arg.ConstantValue).ToList() : null;
		}

		private string GetNamespace(string name) {
			int lastDot = name.LastIndexOf('.');
			return lastDot >= 0 ? name.Substring(0, lastDot) : "";
		}

		private JsExpression RewriteMethod(JsMethod method) {
			return method.TypeParameterNames.Count == 0 ? method.Definition : JsExpression.FunctionDefinition(method.TypeParameterNames, new JsReturnStatement(method.Definition));
		}

		private JsExpression CreateRegisterClassCall(JsExpression name, JsExpression baseClass, IList<JsExpression> interfaces, JsExpression typeRef) {
			var args = new List<JsExpression> { name };
			if (baseClass != null)
				args.Add(baseClass);
			else if (interfaces.Count > 0)
				args.Add(JsExpression.Null);
			if (interfaces.Count > 0)
				args.AddRange(interfaces);
			return JsExpression.Invocation(JsExpression.MemberAccess(typeRef, RegisterClass), args);
		}

		private JsExpression GetGenericClassNameExpression(string className, IList<string> typeArgumentNames) {
			JsExpression result = JsExpression.Binary(ExpressionNodeType.Add, JsExpression.String(className + "$"), JsExpression.MemberAccess(JsExpression.Identifier(typeArgumentNames[0]), TypeNameField));
			for (int i = 1; i < typeArgumentNames.Count; i++)
				result = JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Binary(ExpressionNodeType.Add, result, JsExpression.String("$")), JsExpression.MemberAccess(JsExpression.Identifier(typeArgumentNames[i]), TypeNameField));
			return result;
		}

		private void AddClassMembers(JsClass c, JsExpression typeRef, List<JsStatement> stmts) {
			if (c.InstanceMethods.Count > 0) {
				stmts.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(typeRef, Prototype), JsExpression.ObjectLiteral(c.InstanceMethods.Select(m => new JsObjectLiteralProperty(m.Name, m.Definition != null ? RewriteMethod(m) : JsExpression.Null))))));
			}

			if (c.NamedConstructors.Count > 0) {
				stmts.AddRange(c.NamedConstructors.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(typeRef, m.Name), m.Definition))));
				stmts.Add(new JsExpressionStatement(c.NamedConstructors.Reverse().Aggregate((JsExpression)JsExpression.MemberAccess(typeRef, Prototype), (right, ctor) => JsExpression.Assign(JsExpression.MemberAccess(JsExpression.MemberAccess(typeRef, ctor.Name), Prototype), right))));	// This generates a statement like {C}.ctor1.prototype = {C}.ctor2.prototype = {C}.prototoype
			}

			stmts.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(typeRef, m.Name), RewriteMethod(m)))));

			if (c.TypeArgumentNames.Count > 0) {
				stmts.Add(new JsVariableDeclarationStatement(InstantiatedGenericNameVariableName, GetGenericClassNameExpression(c.Name, c.TypeArgumentNames)));
				if (c.ClassType == JsClass.ClassTypeEnum.Interface) {
					stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(JsExpression.Identifier(InstantiatedGenericTypeVariableName), RegisterInterface), JsExpression.Identifier(InstantiatedGenericNameVariableName))));
				}
				else {
					stmts.Add(new JsExpressionStatement(CreateRegisterClassCall(JsExpression.Identifier(InstantiatedGenericNameVariableName), c.BaseClass, c.ImplementedInterfaces, JsExpression.Identifier(InstantiatedGenericTypeVariableName))));
				}
				stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(typeRef, RegisterGenericInstance), JsExpression.Identifier(InstantiatedGenericNameVariableName), JsExpression.Identifier(InstantiatedGenericTypeVariableName))));
			}
			else if (c.ClassType == JsClass.ClassTypeEnum.Interface) {
				stmts.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(typeRef, RegisterInterface), JsExpression.String(c.Name))));
			}
		}

		public IList<JsStatement> Rewrite(IEnumerable<JsType> types, ICompilation compilation) {
			var systemType = Utils.CreateJsTypeReferenceExpression(compilation.FindType(KnownTypeCode.Type).GetDefinition(), _namingConvention);

			var result = new List<JsStatement>();

			var orderedTypes = types.OrderBy(t => t.Name).ToList();
			string currentNs = "";
			foreach (var t in orderedTypes) {
				bool globalMethods = GetAttributePositionalArgs(t.CSharpTypeDefinition, GlobalMethodsAttribute) != null;

				string ns = GetNamespace(t.Name);
				if (ns != currentNs && !globalMethods) {
					result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(systemType, RegisterNamespace), JsExpression.String(ns))));
					currentNs = ns;
				}
				result.Add(new JsComment("//////////////////////////////////////////////////////////////////////////////" + Environment.NewLine + " " + t.Name));

				var typeRef = new JsTypeReferenceExpression(compilation.MainAssembly, t.Name);
				if (t is JsClass) {
					var c = (JsClass)t;
					if (globalMethods) {
						result.AddRange(c.StaticMethods.Select(m => new JsExpressionStatement(JsExpression.Binary(ExpressionNodeType.Assign, JsExpression.MemberAccess(JsExpression.Identifier("window"), m.Name), m.Definition))));
					}
					else {
						var unnamedCtor = c.UnnamedConstructor ?? JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement);

						if (c.TypeArgumentNames.Count == 0) {
							result.Add(new JsExpressionStatement(JsExpression.Assign(typeRef, unnamedCtor)));
							AddClassMembers(c, typeRef, result);
						}
						else {
							var stmts = new List<JsStatement> { new JsVariableDeclarationStatement(InstantiatedGenericTypeVariableName, unnamedCtor) };
							AddClassMembers(c, JsExpression.Identifier(InstantiatedGenericTypeVariableName), stmts);
							stmts.AddRange(c.StaticInitStatements);
							stmts.Add(new JsReturnStatement(JsExpression.Identifier(InstantiatedGenericTypeVariableName)));
							result.Add(new JsExpressionStatement(JsExpression.Assign(typeRef, JsExpression.FunctionDefinition(c.TypeArgumentNames, new JsBlockStatement(stmts)))));
							result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(typeRef, c.ClassType == JsClass.ClassTypeEnum.Interface ? RegisterGenericInterface : RegisterGenericClass), JsExpression.String(c.Name), JsExpression.Number(c.TypeArgumentNames.Count))));
						}
					}
				}
				else if (t is JsEnum) {
					var e = (JsEnum)t;
					result.Add(new JsExpressionStatement(JsExpression.Assign(typeRef, JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement))));
					result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.MemberAccess(typeRef, Prototype), JsExpression.ObjectLiteral(e.Values.Select(v => new JsObjectLiteralProperty(v.Name, JsExpression.Number(v.Value)))))));
					result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(typeRef, RegisterEnum), JsExpression.String(t.Name), JsExpression.False)));
				}
			}

			result.AddRange(orderedTypes.OfType<JsClass>().Where(c => c.ClassType == JsClass.ClassTypeEnum.Class && c.TypeArgumentNames.Count == 0 && GetAttributePositionalArgs(c.CSharpTypeDefinition, GlobalMethodsAttribute) == null).Select(c => new JsExpressionStatement(CreateRegisterClassCall(JsExpression.String(c.Name), c.BaseClass, c.ImplementedInterfaces, new JsTypeReferenceExpression(compilation.MainAssembly, c.Name)))));
			result.AddRange(orderedTypes.OfType<JsClass>().Where(c => c.TypeArgumentNames.Count == 0).SelectMany(t => t.StaticInitStatements));

			return result;
		}
	}
}
