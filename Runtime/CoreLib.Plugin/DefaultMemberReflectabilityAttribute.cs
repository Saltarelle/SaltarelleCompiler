using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.ScriptSemantics;

namespace System.Runtime.CompilerServices {
	public partial class DefaultMemberReflectabilityAttribute {
		public override void ApplyTo(IAssembly assembly, IAttributeStore attributeStore) {
			foreach (var t in assembly.GetAllTypeDefinitions()) {
				if (!attributeStore.AttributesFor(t).HasAttribute<DefaultMemberReflectabilityAttribute>()) {
					ApplyTo(t, attributeStore);
				}
			}
		}

		public override void ApplyTo(IEntity entity, IAttributeStore attributeStore) {
			var type = entity as ITypeDefinition;
			if (type == null)
				return;

			foreach (var m in type.Members) {
				var attributes = attributeStore.AttributesFor(m);
				if (!attributes.HasAttribute<ReflectableAttribute>()) {
					if (IsMemberReflectable(m)) {
						attributes.Add(new ReflectableAttribute(true));
					}
				}
			}
		}

		private bool IsMemberReflectable(IMember member) {
			switch (DefaultReflectability) {
				case MemberReflectability.None:
					return false;
				case MemberReflectability.PublicAndProtected:
					return !member.IsPrivate && !member.IsInternal;
				case MemberReflectability.NonPrivate:
					return !member.IsPrivate;
				case MemberReflectability.All:
					return true;
				default:
					throw new ArgumentException("reflectability");
			}
		}
	}
}
