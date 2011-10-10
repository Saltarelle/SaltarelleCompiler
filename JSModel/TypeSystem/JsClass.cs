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

        private IList<JsMember> _constructors;
        private IList<JsMember> _instanceMembers;
        private IList<JsMember> _staticMembers;

        public ClassTypeEnum ClassType { get; private set; }
        public JsConstructedType BaseClass { get; private set; }
        public ReadOnlyCollection<string> TypeArgumentNames { get; private set; }
        public ReadOnlyCollection<JsConstructedType> ImplementedInterfaces { get; private set; }
        public IList<JsMember> Constructors { get { return _constructors; } }
        public IList<JsMember> InstanceMembers { get { return _instanceMembers; } }
        public IList<JsMember> StaticMembers { get { return _staticMembers; } }

        public JsClass(ScopedName name, bool isPublic, ClassTypeEnum classType, IEnumerable<string> typeArgumentNames, JsConstructedType baseClass, IEnumerable<JsConstructedType> implementedInterfaces) : base(name, isPublic) {
            BaseClass             = baseClass;
            ClassType             = classType;
            TypeArgumentNames     = typeArgumentNames.AsReadOnly();
            ImplementedInterfaces = implementedInterfaces.AsReadOnly();
            _constructors         = new List<JsMember>();
            _instanceMembers      = new List<JsMember>();
            _staticMembers        = new List<JsMember>();
        }

        public override void Freeze() {
            _constructors          = _constructors.AsReadOnly();
            _instanceMembers       = _instanceMembers.AsReadOnly();
            _staticMembers         = _instanceMembers.AsReadOnly();
        }
    }
}
