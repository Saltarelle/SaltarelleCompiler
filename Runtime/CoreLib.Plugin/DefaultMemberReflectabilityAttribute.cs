using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;

namespace System.Runtime.CompilerServices {
	public partial class DefaultMemberReflectabilityAttribute {
		public override void ApplyTo(ISymbol symbol, IAttributeStore attributeStore, IErrorReporter errorReporter) {
			var assembly = symbol as IAssemblySymbol;
			if (assembly != null) {
				foreach (var tn in assembly.TypeNames) {
					var t = assembly.GetTypeByMetadataName(tn);
					#warning TODO: Check
					if (!attributeStore.AttributesFor(t).HasAttribute<DefaultMemberReflectabilityAttribute>()) {
						ApplyTo(t, attributeStore, errorReporter);
					}
				}
			}

			var type = symbol as INamedTypeSymbol;
			if (type != null) {
				foreach (var m in type.GetMembers()) {
					var attributes = attributeStore.AttributesFor(m);
					if (!attributes.HasAttribute<ReflectableAttribute>()) {
						if (IsMemberReflectable(m)) {
							attributes.Add(new ReflectableAttribute(true));
						}
					}
				}
			}
		}

		private bool IsMemberReflectable(ISymbol member) {
			switch (DefaultReflectability) {
				case MemberReflectability.None:
					return false;
				case MemberReflectability.PublicAndProtected:
					return member.DeclaredAccessibility != Accessibility.Private && member.DeclaredAccessibility != Accessibility.Internal;
				case MemberReflectability.NonPrivate:
					return member.DeclaredAccessibility != Accessibility.Private;
				case MemberReflectability.All:
					return true;
				default:
					throw new ArgumentException("reflectability");
			}
		}
	}
}
