using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel {
    public interface IContainsJsFunctionDefinition {
        JsFunctionDefinitionExpression Definition { get; set; }
    }
}
