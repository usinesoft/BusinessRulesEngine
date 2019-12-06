using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RulesEngine.Tools
{
    /// <summary>
    ///     Caches compiled versions of expression trees
    /// </summary>
    /// <typeparam name="TComplexObject"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public static class RuntimeCompiler<TComplexObject, TProperty>
    {
        private static readonly Dictionary<string, Action<TComplexObject, TProperty>> PrecompiledActions =
            new Dictionary<string, Action<TComplexObject, TProperty>>();

        private static readonly Dictionary<string, Func<TComplexObject, TProperty>> PrecompiledGetters =
            new Dictionary<string, Func<TComplexObject, TProperty>>();

        public static Action<TComplexObject, TProperty> SetterFromGetter(
            Expression<Func<TComplexObject, TProperty>> expression)
        {
            var propertyName = ExpressionTreeHelper.FullPropertyName(expression);

            lock (PrecompiledActions)
            {
                if (PrecompiledActions.TryGetValue(propertyName, out var setter))
                {
                    return setter;
                }

                var valueParameterExpression = Expression.Parameter(typeof (TProperty));
                var targetExpression = expression.Body is UnaryExpression unaryExpression
                    ? unaryExpression.Operand
                    : expression.Body;

                var assign = Expression.Lambda<Action<TComplexObject, TProperty>>(
                    Expression.Assign(targetExpression, Expression.Convert(valueParameterExpression, targetExpression.Type)),
                    expression.Parameters.Single(), valueParameterExpression
                );

                return PrecompiledActions[propertyName] = assign.Compile();
            }
        }

        public static Func<TComplexObject, TProperty> Getter(Expression<Func<TComplexObject, TProperty>> expression)
        {
            var propertyName = ExpressionTreeHelper.FullPropertyName(expression);

            lock (PrecompiledGetters)
            {
                if (PrecompiledGetters.TryGetValue(propertyName, out var getter))
                {
                    return getter;
                }

                return PrecompiledGetters[propertyName] = expression.Compile();
            }
        }
    }
}