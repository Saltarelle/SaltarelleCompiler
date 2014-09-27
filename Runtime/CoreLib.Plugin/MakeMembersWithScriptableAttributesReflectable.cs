using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;

namespace CoreLib.Plugin {
	public class MakeMembersWithScriptableAttributesReflectable : IAutomaticMetadataAttributeApplier {
		public void Process(IAssemblySymbol assembly, IAttributeStore attributeStore) {
		}

		public void Process(INamedTypeSymbol type, IAttributeStore attributeStore) {
			foreach (var m in type.GetMembers()) {
				ProcessMember(m, attributeStore);
			}
		}

		private void ProcessMember(ISymbol member, IAttributeStore attributeStore) {
			var attributes = attributeStore.AttributesFor(member);
			if (!attributes.HasAttribute<ReflectableAttribute>()) {
				if (member.GetAttributes().Any(a => !attributeStore.AttributesFor(a.AttributeClass).HasAttribute<NonScriptableAttribute>())) {
					attributes.Add(new ReflectableAttribute(true));
				}
			}
		}
	}
}