using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public class Member {
        public string Name { get; private set; }
        public Expressions.Expression Initializer { get; private set; }

        public Member(string name, Expression initializer) {
            Require.ValidJavaScriptIdentifier(name, "name");
            Require.NotNull(initializer, "initializer");
            Name = name;
            Initializer = initializer;
        }
    }
}
