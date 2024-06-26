﻿using Jamiras.Components;
using RATools.Parser.Internal;
using System.Collections.Generic;
using System.Text;

namespace RATools.Parser.Expressions
{
    public class ArrayExpression : ExpressionBase, INestedExpressions,
        IIterableExpression, IValueExpression
    {
        public ArrayExpression()
            : base(ExpressionType.Array)
        {
            Entries = new List<ExpressionBase>();
        }

        /// <summary>
        /// Gets the entries in the array.
        /// </summary>
        public List<ExpressionBase> Entries { get; private set; }

        /// <summary>
        /// Appends the textual representation of this expression to <paramref name="builder" />.
        /// </summary>
        internal override void AppendString(StringBuilder builder)
        {
            builder.Append('[');

            if (Entries.Count > 0)
            {
                foreach (var entry in Entries)
                {
                    entry.AppendString(builder);
                    builder.Append(", ");
                }
                builder.Length -= 2;
            }

            builder.Append(']');
        }

        internal static new ExpressionBase Parse(PositionalTokenizer tokenizer)
        {
            if (tokenizer.NextChar != '[')
                return null;
            tokenizer.Advance();

            SkipWhitespace(tokenizer);

            var array = new ArrayExpression();
            while (tokenizer.NextChar != ']')
            {
                var value = ParseValueClause(tokenizer);
                if (value is not IValueExpression)
                {
                    if (value.Type == ExpressionType.Error)
                        return value;
                    return new ErrorExpression("Invalid array entry", value);
                }

                array.Entries.Add(value);

                SkipWhitespace(tokenizer);
                if (tokenizer.NextChar == ']')
                    break;

                if (tokenizer.NextChar != ',')
                    return ParseError(tokenizer, "Expecting comma between entries");
                tokenizer.Advance();
                SkipWhitespace(tokenizer);
            }

            tokenizer.Advance();
            return array;
        }

        /// <summary>
        /// Evaluates an expression
        /// </summary>
        /// <returns><see cref="ErrorExpression"/> indicating the failure, or the result of evaluating the expression.</returns>
        public ExpressionBase Evaluate(InterpreterScope scope)
        {
            ExpressionBase result;
            ReplaceVariables(scope, out result);
            return result;
        }

        /// <summary>
        /// Replaces the variables in the expression with values from <paramref name="scope" />.
        /// </summary>
        /// <param name="scope">The scope object containing variable values.</param>
        /// <param name="result">[out] The new expression containing the replaced variables.</param>
        /// <returns>
        ///   <c>true</c> if substitution was successful, <c>false</c> if something went wrong, in which case <paramref name="result" /> will likely be a <see cref="ErrorExpression" />.
        /// </returns>
        public override bool ReplaceVariables(InterpreterScope scope, out ExpressionBase result)
        {
            if (Entries.Count == 0)
            {
                result = new ArrayExpression { Entries = new List<ExpressionBase>() };
                CopyLocation(result);
                return true;
            }

            var entries = new List<ExpressionBase>();
            foreach (var entry in Entries)
            {
                ExpressionBase value;

                var valueExpression = entry as IValueExpression;
                if (valueExpression != null)
                {
                    value = valueExpression.Evaluate(scope);
                    if (value is ErrorExpression)
                    {
                        result = value;
                        return false;
                    }
                }
                else if (!entry.ReplaceVariables(scope, out value))
                {
                    result = value;
                    return false;
                }

                entries.Add(value);
            }

            result = new ArrayExpression { Entries = entries };
            CopyLocation(result);
            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ArrayExpression" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="ArrayExpression" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="ArrayExpression" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        protected override bool Equals(ExpressionBase obj)
        {
            var that = obj as ArrayExpression;
            if (that == null || Entries.Count != that.Entries.Count)
                return false;

            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] != that.Entries[i])
                    return false;
            }

            return true;
        }

        IEnumerable<ExpressionBase> INestedExpressions.NestedExpressions
        {
            get
            {
                return Entries;
            }
        }

        void INestedExpressions.GetDependencies(HashSet<string> dependencies)
        {
            foreach (var entry in Entries)
            {
                var nested = entry as INestedExpressions;
                if (nested != null)
                    nested.GetDependencies(dependencies);
            }
        }

        void INestedExpressions.GetModifications(HashSet<string> modifies)
        {
        }

        IEnumerable<ExpressionBase> IIterableExpression.IterableExpressions()
        {
            return Entries;
        }
    }
}
