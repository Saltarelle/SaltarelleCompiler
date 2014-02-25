using System.Linq;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;

namespace CoreLib.Plugin {
	public class MakeMembersWithScriptableAttributesReflectable : IAutomaticMetadataAttributeApplier {
		private readonly IAttributeStore _attributeStore;

		public MakeMembersWithScriptableAttributesReflectable(IAttributeStore attributeStore) {
			_attributeStore   = attributeStore;
		}

		public void Process(IAssembly assembly) {
		}

		public void Process(ITypeDefinition type) {
			foreach (var m in type.Members) {
				ProcessMember(m);
			}
		}

		private void ProcessMember(IMember member) {
			var attributes = _attributeStore.AttributesFor(member);
			if (!attributes.HasAttribute<ReflectableAttribute>()) {
				if (member.Attributes.Any(a => a.AttributeType.Kind == TypeKind.Class && !_attributeStore.AttributesFor((IEntity) a.AttributeType.GetDefinition()).HasAttribute<NonScriptableAttribute>())) {
					attributes.Add(new ReflectableAttribute(true));
				}
			}
		}
	}
}