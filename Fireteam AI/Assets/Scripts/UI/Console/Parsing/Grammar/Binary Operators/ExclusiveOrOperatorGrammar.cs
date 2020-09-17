﻿using System;
using System.Linq.Expressions;

namespace Koobando.UI.Console.Grammar
{
    public class ExclusiveOrOperatorGrammar : BinaryOperatorGrammar
    {
        public override int Precedence => 7;

        protected override char OperatorToken => '^';
        protected override string OperatorMethodName => "op_ExclusiveOr";

        protected override Func<Expression, Expression, BinaryExpression> PrimitiveExpressionGenerator => Expression.ExclusiveOr;
    }
}
