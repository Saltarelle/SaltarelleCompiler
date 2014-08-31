using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;

namespace CoreLib.Plugin {
	public class MakeMembersWithScriptableAttributesReflectable : IAutomaticMetadataAttributeApplier {
		private readonly IAttributeStore _attributeStore;

		public MakeMembersWithScriptableAttributesReflectable(IAttributeStore attributeStore) {
			_attributeStore   = attributeStore;
		}

		public void Process(IAssemblySymbol assembly) {
		}

		public void Process(INamedTypeSymbol type) {
			foreach (var m in type.GetMembers()) {
				ProcessMember(m);
			}
		}

		private void ProcessMember(ISymbol member) {
			var attributes = _attributeStore.AttributesFor(member);
			if (!attributes.HasAttribute<ReflectableAttribute>()) {
				if (member.GetAttributes().Any(a => !_attributeStore.AttributesFor(a.AttributeClass).HasAttribute<NonScriptableAttribute>())) {
					attributes.Add(new ReflectableAttribute(true));
				}
			}
		}
	}
}