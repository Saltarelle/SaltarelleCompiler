using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class JsConstructedType {
        /// <summary>
        /// The unbound generic type (eg. List{T}), or non-generic type (eg. List).
        /// </summary>
        public JsExpression UnboundType { get; private set; }

        public ReadOnlyCollection<JsConstructedType> TypeArguments { get; private set; }

        public JsConstructedType(JsExpression unboundType) : this(unboundType, new JsConstructedType[0]) {
        }

        public JsConstructedType(JsExpression unboundType, IEnumerable<JsConstructedType> typeArguments) {
            UnboundType   = unboundType;
            TypeArguments = typeArguments.AsReadOnly();
        }
    }
}
