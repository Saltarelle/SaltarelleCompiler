using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    /// <summary>
    /// A class, interface or struct.
    /// </summary>
    public class JsClass : JsType {
        private JsFunctionDefinitionExpression _unnamedConstructor;

        public enum ClassTypeEnum { Struct, Class, Interface }

    	public ClassTypeEnum ClassType { get; private set; }
        public JsExpression BaseClass { get; private set; }
        public IList<string> TypeArgumentNames { get; private set; }
        public IList<JsExpression> ImplementedInterfaces { get; private set; }
        public IList<JsNamedConstructor> NamedConstructors { get; private set; }
        public IList<JsMethod> InstanceMethods { get; private set; }
        public IList<JsMethod> StaticMethods { get; private set; }
        public IList<JsStatement> StaticInitStatements { get; private set; }

        public JsFunctionDefinitionExpression UnnamedConstructor {
            get { return _unnamedConstructor; }
            set {
                if (Frozen)
                    throw new InvalidOperationException("Object is frozen.");
                _unnamedConstructor = value;
            }
        }

        public JsClass(ITypeDefinition csharpTypeDefinition, ClassTypeEnum classType, IEnumerable<string> typeArgumentNames, JsExpression baseClass, IEnumerable<JsExpression> implementedInterfaces) : base(csharpTypeDefinition) {
            BaseClass             = baseClass;
            ClassType             = classType;
            TypeArgumentNames     = new List<string>(typeArgumentNames ?? new string[0]);
            ImplementedInterfaces = new List<JsExpression>(implementedInterfaces ?? new JsExpression[0]);
            NamedConstructors     = new List<JsNamedConstructor>();
            InstanceMethods       = new List<JsMethod>();
            StaticMethods         = new List<JsMethod>();
            StaticInitStatements  = new List<JsStatement>();
        }

        public override void Freeze() {
            base.Freeze();
            
            TypeArgumentNames     = TypeArgumentNames.AsReadOnly();
            ImplementedInterfaces = ImplementedInterfaces.AsReadOnly();
            NamedConstructors     = NamedConstructors.AsReadOnly();
            InstanceMethods       = InstanceMethods.AsReadOnly();
            StaticMethods         = StaticMethods.AsReadOnly();
            StaticInitStatements  = StaticInitStatements.AsReadOnly();
        }

		public JsClass Clone() {
			var result = new JsClass(this.CSharpTypeDefinition, this.ClassType, this.TypeArgumentNames, this.BaseClass, this.ImplementedInterfaces) {
				UnnamedConstructor = this.UnnamedConstructor
			};
			((List<JsNamedConstructor>)result.NamedConstructors).AddRange(this.NamedConstructors);
			((List<JsMethod>)result.InstanceMethods).AddRange(this.InstanceMethods);
			((List<JsMethod>)result.StaticMethods).AddRange(this.StaticMethods);
			((List<JsStatement>)result.StaticInitStatements).AddRange(this.StaticInitStatements);
			return result;
		}
    }
}
