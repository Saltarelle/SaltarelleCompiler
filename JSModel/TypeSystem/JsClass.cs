using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsClass : JsType {
        public JsClass ContainingType { get; private set; }
        public JsClass BaseClass { get; private set; }
        public ReadOnlyCollection<ScopedName> ImplementedInterfaces { get; private set; }
        public ReadOnlyCollection<Member> Constructors { get; private set; }
        public ReadOnlyCollection<Member> InstanceMembers { get; private set; }
        public ReadOnlyCollection<Member> StaticMembers { get; private set; }

        public JsClass(ScopedName name, JsClass containingType, JsClass baseType, IEnumerable<ScopedName> implementedInterfaces, IEnumerable<Member> constructors, IEnumerable<Member> instanceMembers, ReadOnlyCollection<Member> staticMembers) : base(name) {
            ContainingType = containingType;
            BaseClass = baseType;
            ImplementedInterfaces = implementedInterfaces.AsReadOnly();
            Constructors = constructors.AsReadOnly();
            InstanceMembers = instanceMembers.AsReadOnly();
            StaticMembers = staticMembers;
        }
    }
}
