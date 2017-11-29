﻿using NUnit.Framework;
using RATools.Parser.Internal;
using System.Text;

namespace RATools.Test.Parser.Internal
{
    [TestFixture]
    class IndexedVariableExpressionTests
    {
        [Test]
        public void TestAppendString()
        {
            var expr = new IndexedVariableExpression("test", new IntegerConstantExpression(1));

            var builder = new StringBuilder();
            expr.AppendString(builder);
            Assert.That(builder.ToString(), Is.EqualTo("test[1]"));
        }

        [Test]
        public void TestAppendStringTwoDimensional()
        {
            var second = new IndexedVariableExpression("test", new IntegerConstantExpression(2));
            var expr = new IndexedVariableExpression(second, new IntegerConstantExpression(1));

            var builder = new StringBuilder();
            expr.AppendString(builder);
            Assert.That(builder.ToString(), Is.EqualTo("test[2][1]"));
        }

        [Test]
        public void TestReplaceVariables()
        {
            var variable = new VariableExpression("variable");
            var key = new StringConstantExpression("key");
            var value = new IntegerConstantExpression(99);
            var dict = new DictionaryExpression();
            dict.Entries.Add(new DictionaryExpression.DictionaryEntry { Key = key, Value = value });
            var expr = new IndexedVariableExpression(variable.Name, key);

            var scope = new InterpreterScope();
            scope.AssignVariable(variable, dict);

            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.True);
            Assert.That(result, Is.InstanceOf<IntegerConstantExpression>());
            Assert.That(((IntegerConstantExpression)result).Value, Is.EqualTo(99));
        }

        [Test]
        public void TestReplaceTwoDimensional()
        {
            var variable = new VariableExpression("variable");
            var key = new StringConstantExpression("key");
            var value = new IntegerConstantExpression(99);
            var dict2 = new DictionaryExpression();
            dict2.Entries.Add(new DictionaryExpression.DictionaryEntry { Key = key, Value = value });
            var dict1 = new DictionaryExpression();
            dict1.Entries.Add(new DictionaryExpression.DictionaryEntry { Key = key, Value = dict2 });
            var expr2 = new IndexedVariableExpression(variable.Name, key);
            var expr = new IndexedVariableExpression(expr2, key);

            var scope = new InterpreterScope();
            scope.AssignVariable(variable, dict1);

            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.True);
            Assert.That(result, Is.InstanceOf<IntegerConstantExpression>());
            Assert.That(((IntegerConstantExpression)result).Value, Is.EqualTo(99));
        }

        [Test]
        public void TestReplaceVariablesIndexVariable()
        {
            var variable = new VariableExpression("variable");
            var key = new StringConstantExpression("key");
            var index = new VariableExpression("index");
            var value = new IntegerConstantExpression(99);
            var dict = new DictionaryExpression();
            dict.Entries.Add(new DictionaryExpression.DictionaryEntry { Key = key, Value = value });
            var expr = new IndexedVariableExpression(variable.Name, index);

            var scope = new InterpreterScope();
            scope.AssignVariable(variable, dict);
            scope.AssignVariable(index, key);

            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.True);
            Assert.That(result, Is.InstanceOf<IntegerConstantExpression>());
            Assert.That(((IntegerConstantExpression)result).Value, Is.EqualTo(99));
        }

        [Test]
        public void TestReplaceVariablesInvalidKey()
        {
            var variable = new VariableExpression("variable");
            var key = new StringConstantExpression("key");
            var dict = new DictionaryExpression();
            var expr = new IndexedVariableExpression(variable.Name, key);

            var scope = new InterpreterScope();
            scope.AssignVariable(variable, dict);

            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.False);
            Assert.That(result, Is.InstanceOf<ParseErrorExpression>());
            Assert.That(((ParseErrorExpression)result).Message, Is.EqualTo("No entry in dictionary for key: \"key\""));
        }

        [Test]
        public void TestReplaceVariablesNonDictionary()
        {
            var variable = new VariableExpression("variable");
            var key = new StringConstantExpression("key");
            var value = new IntegerConstantExpression(99);
            var expr = new IndexedVariableExpression(variable.Name, key);

            var scope = new InterpreterScope();
            scope.AssignVariable(variable, value);

            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.False);
            Assert.That(result, Is.InstanceOf<ParseErrorExpression>());
            Assert.That(((ParseErrorExpression)result).Message, Is.EqualTo("Cannot index: variable (IntegerConstant)"));
        }

        [Test]
        public void TestReplaceVariablesIndexMathematical()
        {
            var variable = new VariableExpression("variable");
            var key = new IntegerConstantExpression(6);
            var index = new MathematicExpression(new IntegerConstantExpression(2), MathematicOperation.Add, new IntegerConstantExpression(4));
            var value = new IntegerConstantExpression(99);
            var dict = new DictionaryExpression();
            dict.Entries.Add(new DictionaryExpression.DictionaryEntry { Key = key, Value = value });
            var expr = new IndexedVariableExpression(variable.Name, index);

            var scope = new InterpreterScope();
            scope.AssignVariable(variable, dict);
            
            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.True);
            Assert.That(result, Is.InstanceOf<IntegerConstantExpression>());
            Assert.That(((IntegerConstantExpression)result).Value, Is.EqualTo(99));
        }
    }
}
