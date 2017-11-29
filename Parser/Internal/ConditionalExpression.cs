﻿using System.Text;

namespace RATools.Parser.Internal
{
    internal class ConditionalExpression : ExpressionBase
    {
        public ConditionalExpression(ExpressionBase left, ConditionalOperation operation, ExpressionBase right)
            : base(ExpressionType.Conditional)
        {
            Left = left;
            Operation = operation;
            Right = right;
        }

        /// <summary>
        /// Gets the left side of the condition.
        /// </summary>
        public ExpressionBase Left { get; internal set; }

        /// <summary>
        /// Gets the conditional operation.
        /// </summary>
        public ConditionalOperation Operation { get; private set; }

        /// <summary>
        /// Gets the right side of the condition.
        /// </summary>
        public ExpressionBase Right { get; private set; }

        /// <summary>
        /// Appends the textual representation of this expression to <paramref name="builder" />.
        /// </summary>
        internal override void AppendString(StringBuilder builder)
        {
            if (Operation == ConditionalOperation.Not)
            {
                builder.Append('!');
                Right.AppendString(builder);
                return;
            }

            Left.AppendString(builder);
            builder.Append(' ');

            switch (Operation)
            {
                case ConditionalOperation.And:
                    builder.Append("&&");
                    break;
                case ConditionalOperation.Or:
                    builder.Append("||");
                    break;
            }

            builder.Append(' ');
            Right.AppendString(builder);
        }

        /// <summary>
        /// Replaces the variables in the expression with values from <paramref name="scope" />.
        /// </summary>
        /// <param name="scope">The scope object containing variable values.</param>
        /// <param name="result">[out] The new expression containing the replaced variables.</param>
        /// <returns>
        ///   <c>true</c> if substitution was successful, <c>false</c> if something went wrong, in which case <paramref name="result" /> will likely be a <see cref="ParseErrorExpression" />.
        /// </returns>
        public override bool ReplaceVariables(InterpreterScope scope, out ExpressionBase result)
        {
            ExpressionBase left = null;
            if (Left != null && !Left.ReplaceVariables(scope, out left))
            {
                result = left;
                return false;
            }

            ExpressionBase right;
            if (!Right.ReplaceVariables(scope, out right))
            {
                result = right;
                return false;
            }

            var condition = new ConditionalExpression(left, Operation, right);
            condition.Line = Line;
            condition.Column = Column;
            result = condition;
            return true;
        }

        /// <summary>
        /// Determines whether the expression evaluates to true for the provided <paramref name="scope" />
        /// </summary>
        /// <param name="scope">The scope object containing variable values.</param>
        /// <param name="error">[out] The error that prevented evaluation (or null if successful).</param>
        /// <returns>
        /// The result of evaluating the expression
        /// </returns>
        public override bool IsTrue(InterpreterScope scope, out ParseErrorExpression error)
        {
            bool result = Left.IsTrue(scope, out error);
            if (error != null)
                return false;

            switch (Operation)
            {
                case ConditionalOperation.And:
                    if (result)
                        result = Right.IsTrue(scope, out error);
                    break;

                case ConditionalOperation.Or:
                    if (!result)
                        result = Right.IsTrue(scope, out error);
                    break;

                case ConditionalOperation.Not:
                    result = !result;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ConditionalExpression" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="ConditionalExpression" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="ConditionalExpression" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        protected override bool Equals(ExpressionBase obj)
        {
            var that = (ConditionalExpression)obj;
            return Operation == that.Operation && Left == that.Left && Right == that.Right;
        }
    }

    /// <summary>
    /// Specifies how the two sides of the <see cref="ConditionalExpression"/> should be compared.
    /// </summary>
    public enum ConditionalOperation
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Both sides must be true.
        /// </summary>
        And,

        /// <summary>
        /// Either side can be true.
        /// </summary>
        Or,

        /// <summary>
        /// Right is not true (Left is ignored)
        /// </summary>
        Not,
    }
}
