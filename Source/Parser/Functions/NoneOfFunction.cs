﻿using RATools.Parser.Internal;

namespace RATools.Parser.Functions
{
    internal class NoneOfFunction : IterableJoiningFunction
    {
        public NoneOfFunction()
            : base("none_of")
        {
        }

        protected override ExpressionBase Combine(ExpressionBase left, ExpressionBase right)
        {
            right = ConditionalExpression.InvertExpression(right);
            if (right.Type == ExpressionType.Error)
                return right;

            right.IsLogicalUnit = true;

            if (left == null)
                return right;

            return new ConditionalExpression(left, ConditionalOperation.And, right);
        }

        protected override ExpressionBase GenerateEmptyResult()
        {
            return AlwaysTrueFunction.CreateAlwaysTrueFunctionCall();
        }
    }
}
