using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BusinessRulesEngine.Tools
{
    /// <summary>
    ///     Caches compiled versiond of expression trees
    /// </summary>
    /// <typeparam name="TComplexObject"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public static class RuntimeCompiler<TComplexObject, TProperty>
    {
        private static readonly Dictionary<string, Action<TComplexObject, TProperty>> _precompiledActions =
            new Dictionary<string, Action<TComplexObject, TProperty>>();

        private static readonly Dictionary<string, Func<TComplexObject, TProperty>> _precompiledGetters =
            new Dictionary<string, Func<TComplexObject, TProperty>>();

        public static Action<TComplexObject, TProperty> SetterFromGetter(
            Expression<Func<TComplexObject, TProperty>> expression)
        {
            var propertyName = ExpressionTreeHelper.FullPropertyName(expression);

            Action<TComplexObject, TProperty> setter;
            if (_precompiledActions.TryGetValue(propertyName, out setter))
            {
                return setter;
            }

            var valueParameterExpression = Expression.Parameter(typeof (TProperty));
            var targetExpression = expression.Body is UnaryExpression
                ? ((UnaryExpression) expression.Body).Operand
                : expression.Body;

            var assign = Expression.Lambda<Action<TComplexObject, TProperty>>(
                Expression.Assign(targetExpression, Expression.Convert(valueParameterExpression, targetExpression.Type)),
                expression.Parameters.Single(), valueParameterExpression
                );

            return _precompiledActions[propertyName] = assign.Compile();
        }

        public static Func<TComplexObject, TProperty> Getter(Expression<Func<TComplexObject, TProperty>> expression)
        {
            var propertyName = ExpressionTreeHelper.FullPropertyName(expression);

            Func<TComplexObject, TProperty> getter;
            if (_precompiledGetters.TryGetValue(propertyName, out getter))
            {
                return getter;
            }

            return _precompiledGetters[propertyName] = expression.Compile();
        }
    }
}