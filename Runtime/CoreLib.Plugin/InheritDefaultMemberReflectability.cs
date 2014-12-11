using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CoreLib.Plugin
{
    public class InheritDefaultMemberReflectability : IAutomaticMetadataAttributeApplier
    {
		private readonly IAttributeStore _attributeStore;

        public InheritDefaultMemberReflectability(IAttributeStore attributeStore)
        {
			_attributeStore = attributeStore;
		}
        
        public void Process(ITypeDefinition type)
        {
            var attributes = _attributeStore.AttributesFor(type);
            if (attributes.HasAttribute<DefaultMemberReflectabilityAttribute>())
                return;
            var intReflectability = (int)MemberReflectability.None;
            foreach (var bt in type.DirectBaseTypes)
            {
                var btd = bt as ITypeDefinition;
                if (btd == null)
                    continue;
                var attribute = _attributeStore.AttributesFor(btd).GetAttribute<DefaultMemberReflectabilityAttribute>();
                if (attribute == null || !attribute.Inheritable)
                    continue;
                intReflectability = Math.Max(intReflectability, (int)attribute.DefaultReflectability);
            }
            var reflectability = (MemberReflectability)intReflectability;
            if (reflectability == MemberReflectability.None)
                return;
            var a = new DefaultMemberReflectabilityAttribute(reflectability)
            {
                Inheritable = true
            };
            attributes.Add(a);
        }

        public void Process(IAssembly assembly)
        {
            
        }
    }
}
