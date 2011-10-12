using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    /// <summary>
    /// A class, interface or struct.
    /// </summary>
    public class JsClass : JsType {
        public enum ClassTypeEnum { Struct, Class, Interface }

        public ClassTypeEnum ClassType { get; private set; }
        public JsConstructedType BaseClass { get; private set; }
        public ReadOnlyCollection<string> TypeArgumentNames { get; private set; }
        public ReadOnlyCollection<JsConstructedType> ImplementedInterfaces { get; private set; }
        public ReadOnlyCollection<JsMember> Constructors { get; private set; }
        public ReadOnlyCollection<JsMember> InstanceMembers { get; private set; }
        public ReadOnlyCollection<JsMember> StaticMembers { get; private set; }

        public JsClass(ScopedName name, bool isPublic, ClassTypeEnum classType, IEnumerable<string> typeArgumentNames, JsConstructedType baseClass, IEnumerable<JsConstructedType> implementedInterfaces, IEnumerable<JsMember> constructors, IEnumerable<JsMember> instanceMembers, IEnumerable<JsMember> staticMembers) : base(name, isPublic) {
            BaseClass             = baseClass;
            ClassType             = classType;
            TypeArgumentNames     = typeArgumentNames.AsReadOnly();
            ImplementedInterfaces = implementedInterfaces.AsReadOnly();
            Constructors          = constructors.AsReadOnly();
            InstanceMembers       = instanceMembers.AsReadOnly();
            StaticMembers         = staticMembers.AsReadOnly();
        }
    }
}
