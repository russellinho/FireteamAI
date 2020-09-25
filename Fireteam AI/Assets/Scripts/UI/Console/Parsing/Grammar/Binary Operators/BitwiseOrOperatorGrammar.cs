﻿using System;
using System.Linq.Expressions;

namespace Koobando.UI.Console.Grammar
{
    public class BitwiseOrOperatorGrammar : BinaryOperatorGrammar
    {
        public override int Precedence => 5;

        protected override char OperatorToken => '|';
        protected override string OperatorMethodName => "op_bitwiseOr";

        protected override Func<Expression, Expression, BinaryExpression> PrimitiveExpressionGenerator => Expression.Or;
    }
}
