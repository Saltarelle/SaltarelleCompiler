using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Moq;

namespace Saltarelle.Compiler.Tests {
	internal static class Common {
		public static readonly string MscorlibPath = Path.GetFullPath(@"../../../Runtime/CoreLib/bin/mscorlib.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => LoadAssemblyFile(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		public static MetadataReference LoadAssemblyFile(string path) {
			return new MetadataFileReference(path);
		}

		#warning TODO

#if 0
		public static Mock<INamedTypeSymbol> CreateTypeMock(string fullName) {
			int dot = fullName.LastIndexOf(".", StringComparison.InvariantCulture);
			string name;
			if (dot >= 0) {
				name = fullName.Substring(dot + 1);
			}
			else {
				name = fullName;
			}

			var result = new Mock<INamedTypeSymbol>(MockBehavior.Strict);
			result.SetupGet(_ => _.Name).Returns(name);
			result.SetupGet(_ => _.Name).Returns(fullName);
			//result.Setup(_ => _.GetDefinition()).Returns(result.Object);
			result.SetupGet(_ => _.ContainingType).Returns((INamedTypeSymbol)null);
			result.SetupGet(_ => _.Region).Returns(FileLinePositionSpan.Empty);
			return result;
		}

		public static IAssembly CreateMockAssembly(IEnumerable<Expression<Func<System.Attribute>>> attributes = null) {
			var result = new Mock<IAssembly>(MockBehavior.Strict);
			result.SetupGet(_ => _.AssemblyAttributes).Returns(CreateMockAttributes(attributes));
			return result.Object;
		}
		
		public static INamedTypeSymbol CreateMockTypeDefinition(string name, IAssembly assembly, Accessibility accessibility = Accessibility.Public, INamedTypeSymbol ContainingType = null, IEnumerable<Expression<Func<System.Attribute>>> attributes = null) {
			var typeDef = Common.CreateTypeMock(name);
			typeDef.SetupGet(_ => _.DirectBaseTypes).Returns(new ITypeSymbol[0]);
			typeDef.SetupGet(_ => _.Accessibility).Returns(accessibility);
			typeDef.SetupGet(_ => _.ContainingType).Returns(ContainingType);
			typeDef.SetupGet(_ => _.ParentAssembly).Returns(assembly);
			typeDef.Setup(_ => _.GetConstructors(It.IsAny<Predicate<IUnresolvedMethod>>(), It.IsAny<GetMemberOptions>())).Returns(new IMethodSymbol[0]);
			typeDef.SetupGet(_ => _.Attributes).Returns(CreateMockAttributes(attributes));

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
						var argType = new Mock<ITypeSymbol>(MockBehavior.Strict);
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
#endif
	}
}
