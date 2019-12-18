using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace RulesEngine.RulesEngine.Explain
{
    public static class Explain
    {
        public static string TryExplain<T>(this Expression<Func<T, bool>> ifExpression)
        {
            //create a fake queryable to force query parsing and capture resolution

            var executor = new NullExecutor();
            var queryable = new NullQueryable<T>(executor);

            try
            {
                var unused = queryable.Where(ifExpression).ToList();

                return executor.Expression.ToString();
            }
            catch
            {
                // expression too complex
                return null;
            }

        }

        public static string TryExplain<T, TR>(this Expression<Func<T, TR>> expression)
        {
            // create a fake queryable to force query parsing and capture resolution

            return ExplainSimple(expression.Body) ?? ExplainBinaryExpression(expression.Body) ?? "?";
            
        }

        private static string ExplainSimple(Expression expression)
        {
            if (expression is MemberExpression member)
            {
                return member.Member.Name;
            }

            if (expression is MethodCallExpression call)
            {
                return call.Method.Name + "()";
            }

            if (expression is ConstantExpression constant)
            {
                return constant.Value.ToString();
            }

            return null;
        }



        private static string ExplainBinaryExpression(Expression expr)
        {
            if (expr is BinaryExpression expression)
            {
                string left = ExplainSimple(expression.Left) ?? ExplainBinaryExpression(expression.Left) ??"?";
                string right = ExplainSimple(expression.Right) ?? ExplainBinaryExpression(expression.Right) ?? "?";

                if (expression.NodeType == ExpressionType.Add)
                    return left + " + " +right;

                if (expression.NodeType == ExpressionType.Subtract)
                    return left + " - " + right;

                if (expression.NodeType == ExpressionType.Multiply)
                    return left + " * " + right;

                if (expression.NodeType == ExpressionType.Divide)
                    return left + " / " + right;

                if (expression.NodeType == ExpressionType.LessThan)
                    return left + " < " + right;

                if (expression.NodeType == ExpressionType.LessThanOrEqual)
                    return left + " <= " + right;

                if (expression.NodeType == ExpressionType.GreaterThan)
                    return left + " > " + right;

                if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
                    return left + " >= " + right;

                if (expression.NodeType == ExpressionType.Equal)
                    return left + " = " + right;

                if (expression.NodeType == ExpressionType.NotEqual)
                    return left + " <> " + right;

                if (expression.NodeType == ExpressionType.AndAlso)
                    return left + " AND " + right;

                if (expression.NodeType == ExpressionType.Or)
                    return left + " OR " + right;


            }

            return null;
        }

    }
}