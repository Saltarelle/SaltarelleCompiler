using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsClass : JsType {
        private IList<ScopedName> _implementedInterfaces;
        private IList<Member> _constructors;
        private IList<Member> _instanceMembers;
        private IList<Member> _staticMembers;

        public JsClass BaseClass { get; private set; }
        public IList<ScopedName> ImplementedInterfaces { get { return _implementedInterfaces; } }
        public IList<Member> Constructors { get { return _constructors; } }
        public IList<Member> InstanceMembers { get { return _instanceMembers; } }
        public IList<Member> StaticMembers { get { return _staticMembers; } }

        public JsClass(ScopedName name, JsClass baseClass) : base(name) {
            BaseClass              = baseClass;
            _implementedInterfaces = new List<ScopedName>();
            _constructors          = new List<Member>();
            _instanceMembers       = new List<Member>();
            _staticMembers         = new List<Member>();
        }

        public override void Freeze() {
            _implementedInterfaces = _implementedInterfaces.AsReadOnly();
            _constructors          = _constructors.AsReadOnly();
            _instanceMembers       = _instanceMembers.AsReadOnly();
            _staticMembers         = _instanceMembers.AsReadOnly();
        }
    }
}
