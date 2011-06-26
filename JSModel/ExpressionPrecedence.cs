using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel
{
    public static class ExpressionPrecedence {
        public const int Terminal = 0;

        public const int MemberOrNew  = Terminal     + 1;
        public const int FunctionCall = MemberOrNew  + 1;
        public const int IncrDecr     = FunctionCall + 1;
        public const int OtherUnary   = IncrDecr     + 1;
        public const int Multiply     = OtherUnary   + 1;
        public const int Addition     = Multiply     + 1;
        public const int BitwiseShift = Addition     + 1;
        public const int Relational  = BitwiseShift  + 1;
        public const int Equality    = Relational    + 1;
        public const int BitwiseAnd  = Equality      + 1;
        public const int BitwiseXor  = BitwiseAnd    + 1;
        public const int BitwiseOr   = BitwiseXor    + 1;
        public const int LogicalAnd  = BitwiseOr     + 1;
        public const int LogicalOr   = LogicalAnd    + 1;
        public const int Conditional = LogicalOr     + 1;
        public const int Assignment  = Conditional   + 1;
        public const int Comma       = Assignment    + 1;
    }
}
