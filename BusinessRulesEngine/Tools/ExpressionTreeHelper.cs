using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BusinessRulesEngine.Tools
{
    /// <summary>
    ///     Common expression tree manipulation logic
    /// </summary>
    public static class ExpressionTreeHelper
    {
        /// <summary>
        ///     Get the name of the most specific property expressed as an espression tree
        ///     For example t=>t.Product.PremiumLeg.Coupon return "Coupon"
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static string PropertyName<TParent, TProperty>(Expression<Func<TParent, TProperty>> propertySelector)
        {
            if (propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }

            var memberExpression = propertySelector.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("propertySelector must be a MemberExpression.", "propertySelector");
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        ///     Get the full name of the most specific property expressed as an espression tree
        ///     For example t=>t.Product.PremiumLeg.Coupon return "Product.PremiumLeg.Coupon"
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static string FullPropertyName<TParent, TProperty>(Expression<Func<TParent, TProperty>> propertySelector)
        {
            if (propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }

            var memberExpression = propertySelector.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("propertySelector must be a MemberExpression.", "propertySelector");
            }

            var components = new List<string>
            {
                memberExpression.Member.Name
            };
            var inner = memberExpression.Expression as MemberExpression;
            while (inner != null)
            {
                components.Add(inner.Member.Name);
                inner = inner.Expression as MemberExpression;
            }

            var sb = new StringBuilder();
            components.Reverse();

            foreach (var component in components)
            {
                sb.Append(component)
                    .Append(".");
            }

            return sb.ToString()
                .TrimEnd('.');
        }

        /// <summary>
        ///     Create a compiled setter from a getter expression
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static Action<TParent, object> CompileSetter<TParent>(
            Expression<Func<TParent, object>> propertyExpression)
        {
            var propertyInfo = typeof (TParent).GetProperty(PropertyName(propertyExpression));

            var instance = Expression.Parameter(typeof (TParent), "i");
            var argument = Expression.Parameter(typeof (object), "a");
            var cast = Expression.Convert(argument, propertyInfo.PropertyType);
            var setterCall = Expression.Call(instance,
                propertyInfo.GetSetMethod(),
                cast
                );
            return (Action<TParent, object>) Expression.Lambda(setterCall, instance, argument)
                .Compile();
        }
    }
}