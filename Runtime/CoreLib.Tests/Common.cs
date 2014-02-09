using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;

namespace CoreLib.Tests {
	internal class Common {
		public static Mock<ITypeDefinition> CreateTypeMock(string fullName) {
			int dot = fullName.LastIndexOf(".", StringComparison.InvariantCulture);
			string name, nmspace;
			if (dot >= 0) {
				name = fullName.Substring(dot + 1);
				nmspace = fullName.Substring(0, dot);
			}
			else {
				name = fullName;
				nmspace = "";
			}

			var result = new Mock<ITypeDefinition>(MockBehavior.Strict);
			var named = result.As<INamedElement>();
			named.SetupGet(_ => _.Namespace).Returns(nmspace);
			named.SetupGet(_ => _.Name).Returns(name);

			result.SetupGet(_ => _.Namespace).Returns(nmspace);
			result.SetupGet(_ => _.Name).Returns(name);
			result.SetupGet(_ => _.FullName).Returns(fullName);
			result.Setup(_ => _.GetDefinition()).Returns(result.Object);
			result.SetupGet(_ => _.DeclaringTypeDefinition).Returns((ITypeDefinition)null);
			result.SetupGet(_ => _.Region).Returns(DomRegion.Empty);
			return result;
		}

		public static IAssembly CreateMockAssembly(IEnumerable<Expression<Func<System.Attribute>>> attributes = null) {
			var result = new Mock<IAssembly>(MockBehavior.Strict);
			result.SetupGet(_ => _.AssemblyAttributes).Returns(CreateMockAttributes(attributes));
			result.SetupGet(_ => _.AssemblyName).Returns("The.AssemblyName");
			result.SetupGet(_ => _.TopLevelTypeDefinitions).Returns(new List<ITypeDefinition>());
			return result.Object;
		}
		
		public static ITypeDefinition CreateMockTypeDefinition(string name, IAssembly assembly, Accessibility accessibility = Accessibility.Public, ITypeDefinition declaringType = null, IEnumerable<Expression<Func<System.Attribute>>> attributes = null) {
			var typeDef = CreateTypeMock(name);
			typeDef.SetupGet(_ => _.DirectBaseTypes).Returns(new IType[0]);
			typeDef.SetupGet(_ => _.Accessibility).Returns(accessibility);
			typeDef.SetupGet(_ => _.DeclaringTypeDefinition).Returns(declaringType);
			typeDef.SetupGet(_ => _.ParentAssembly).Returns(assembly);
			typeDef.Setup(_ => _.GetConstructors(It.IsAny<Predicate<IUnresolvedMethod>>(), It.IsAny<GetMemberOptions>())).Returns(new IMethod[0]);
			typeDef.SetupGet(_ => _.Attributes).Returns(CreateMockAttributes(attributes));
			typeDef.SetupGet(_ => _.NestedTypes).Returns(new List<ITypeDefinition>());
			typeDef.SetupGet(_ => _.Methods).Returns(new List<IMethod>());
			typeDef.SetupGet(_ => _.Fields).Returns(new List<IField>());
			typeDef.SetupGet(_ => _.Properties).Returns(new List<IProperty>());
			typeDef.SetupGet(_ => _.Events).Returns(new List<IEvent>());
			if (assembly != null)
				((List<ITypeDefinition>)assembly.TopLevelTypeDefinitions).Add(typeDef.Object);

			return typeDef.Object;
		}

		private static IList<IAttribute> CreateMockAttributes(IEnumerable<Expression<Func<System.Attribute>>> attributes) {
			var result = new List<IAttribute>();
			if (attributes != null) {
				foreach (var attrExpression in attributes) {
					var attr = new Mock<IAttribute>(MockBehavior.Strict);
					var body = (NewExpression)attrExpression.Body;
					attr.SetupGet(_ => _.AttributeType).Returns(CreateMockTypeDefinition(body.Type.FullName, null));
					var posArgs = new List<ResolveResult>();
					foreach (var argExpression in body.Arguments) {
						var argType = new Mock<IType>(MockBehavior.Strict);
						argType.SetupGet(_ => _.Namespace).Returns(argExpression.Type.Namespace);
						argType.SetupGet(_ => _.Name).Returns(argExpression.Type.Name);
						argType.SetupGet(_ => _.FullName).Returns(argExpression.Type.FullName);
						var arg = new ConstantResolveResult(argType.Object, ((ConstantExpression)argExpression).Value);
						posArgs.Add(arg);
					}
					attr.SetupGet(_ => _.PositionalArguments).Returns(posArgs);

					if (body.Members != null && body.Members.Count > 0)
						throw new InvalidOperationException("Named attribute args are not supported");

					attr.SetupGet(_ => _.NamedArguments).Returns(new KeyValuePair<IMember, ResolveResult>[0]);

					result.Add(attr.Object);
				}
			}
			return result;
		}
	}
}
