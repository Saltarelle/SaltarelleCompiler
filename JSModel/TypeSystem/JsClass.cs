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
        public IList<string> TypeArgumentNames { get; private set; }
        public IList<JsConstructedType> ImplementedInterfaces { get; private set; }
        public IList<JsConstructor> Constructors { get; private set; }
        public IList<JsMethod> InstanceMethods { get; private set; }
        public IList<JsMethod> StaticMethods { get; private set; }
        public IList<JsField> InstanceFields { get; private set; }
        public IList<JsField> StaticFields { get; private set; }

        public JsClass(ScopedName name, bool isPublic, ClassTypeEnum classType, IEnumerable<string> typeArgumentNames, JsConstructedType baseClass, IEnumerable<JsConstructedType> implementedInterfaces, IEnumerable<JsConstructor> constructors, IEnumerable<JsMethod> instanceMethods, IEnumerable<JsMethod> staticMethods, IEnumerable<JsField> instanceFields, IEnumerable<JsField> staticFields) : base(name, isPublic) {
            BaseClass             = baseClass;
            ClassType             = classType;
            TypeArgumentNames     = new List<string>(typeArgumentNames ?? new string[0]);
            ImplementedInterfaces = new List<JsConstructedType>(implementedInterfaces ?? new JsConstructedType[0]);
            Constructors          = new List<JsConstructor>(constructors ?? new JsConstructor[0]);
            InstanceMethods       = new List<JsMethod>(instanceMethods ?? new JsMethod[0]);
            StaticMethods         = new List<JsMethod>(staticMethods ?? new JsMethod[0]);
            InstanceFields        = new List<JsField>(instanceFields ?? new JsField[0]);
            StaticFields          = new List<JsField>(staticFields ?? new JsField[0]);
        }

        public override void Freeze() {
            base.Freeze();
            Constructors.ForEach(x => x.Freeze());
            InstanceMethods.ForEach(x => x.Freeze());
            StaticMethods.ForEach(x => x.Freeze());

            TypeArgumentNames     = TypeArgumentNames.AsReadOnly();
            ImplementedInterfaces = ImplementedInterfaces.AsReadOnly();
            Constructors          = Constructors.AsReadOnly();
            InstanceMethods       = InstanceMethods.AsReadOnly();
            StaticMethods         = StaticMethods.AsReadOnly();
            InstanceFields        = InstanceFields.AsReadOnly();
            StaticFields          = StaticFields.AsReadOnly();
        }
    }
}
