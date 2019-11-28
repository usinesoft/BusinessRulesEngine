using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RulesEngine.Tools
{
    /// <summary>
    ///     Common expression tree manipulation logic
    /// </summary>
    public static class ExpressionTreeHelper
    {
        /// <summary>
        ///     Get the name of the most specific property expressed as an expression tree
        ///     For example t=>t.Product.PremiumLeg.Coupon return "Coupon"
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static string PropertyName<TParent, TProperty>(Expression<Func<TParent, TProperty>> propertySelector)
        {
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));

            if (propertySelector.Body.NodeType == ExpressionType.Convert)
            {
                if (propertySelector.Body is UnaryExpression convert)
                    if (convert.Operand is MemberExpression memberExpression)
                        return memberExpression.Member.Name;
            }
            else
            {
                if (propertySelector.Body is MemberExpression memberExpression) return memberExpression.Member.Name;
            }

            throw new ArgumentException("propertySelector must be a MemberExpression.", nameof(propertySelector));
        }

        /// <summary>
        ///     Get the full name of the most specific property expressed as an expression tree
        ///     For example t=>t.Product.PremiumLeg.Coupon return "Product.PremiumLeg.Coupon"
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static string FullPropertyName<TParent, TProperty>(Expression<Func<TParent, TProperty>> propertySelector)
        {
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));

            var memberExpression = propertySelector.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("propertySelector must be a MemberExpression.", nameof(propertySelector));

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
                sb.Append(component)
                    .Append(".");

            return sb.ToString()
                .TrimEnd('.');
        }
    }
}