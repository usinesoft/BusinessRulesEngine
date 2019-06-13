using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BusinessRulesEngine.Tools;

namespace BusinessRulesEngine.RulesEngine
{
    /// <summary>
    ///     Implement a fluent syntax o define business rules
    ///     Each rule has:
    ///     - exactly one target property (that CAN be modified by the rule)
    ///     - one or more trigger properties (when they are changed the rule is triggered)
    ///     - an optional condition; the execution is blocked if it is false
    ///     - a value calculator: pure function that computes the value for the target property
    /// </summary>
    public static class FluentExtensions
    {
        /// <summary>
        ///     Second part of the rule declaration; specifies the function that will compute the value of the target property
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        /// <param name="valueComputer"></param>
        /// <returns></returns>
        public static FluentToken<TParent, TTargetProperty> With<TParent, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, Expression<Func<TParent, TTargetProperty>> valueComputer)
        {
            token.ValueComputer = valueComputer.Compile();

            return token;
        }

        /// <summary>
        ///     Specifies the first trigger property
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static FluentToken<TParent, TTargetProperty> OnChanged<TParent, TProperty, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, Expression<Func<TParent, TProperty>> propertySelector)
        {
            token.PropertyNames.Add(ExpressionTreeHelper.PropertyName(propertySelector));

            return token;
        }

        /// <summary>
        ///     Specifies an extra trigger property. Multiple statements may be chained
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static FluentToken<TParent, TTargetProperty> Or<TParent, TProperty, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, Expression<Func<TParent, TProperty>> propertySelector)
        {
            token.PropertyNames.Add(ExpressionTreeHelper.PropertyName(propertySelector));

            return token;
        }

        /// <summary>
        ///     Specifies an optional applicability condition as a predicate; if false the rule will not be triggered
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        /// <param name="ifPredicate"></param>
        /// <returns></returns>
        public static FluentToken<TParent, TTargetProperty> If<TParent, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, Predicate<TParent> ifPredicate)
        {
            token.IfPredicate = ifPredicate;

            return token;
        }

        /// <summary>
        ///     Always the last NON OPTIONAL statement in a rule declaration
        ///     Internally produces an instance of <see cref="Rule{TParent}" /> and adds it into the rules engine
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        public static void EndRule<TParent, TTargetProperty>(this FluentToken<TParent, TTargetProperty> token)
        {
            if (token.MappingRulesContainer == null)
                throw new NotSupportedException("Error in fluent syntax. Start with a Set() statement");

            var propertyName = ExpressionTreeHelper.PropertyName(token.TargetPropertySelector);

            bool Updater(TParent parent)
            {
                if (token.IfPredicate != null)
                    if (!token.IfPredicate(parent))
                        return false;

                var newValue = token.ValueComputer(parent);

                var oldValue = RuntimeCompiler<TParent, TTargetProperty>.Getter(token.TargetPropertySelector)(parent);
                if (Equals(oldValue, newValue)) return false;

                RuntimeCompiler<TParent, TTargetProperty>.SetterFromGetter(token.TargetPropertySelector)(parent,
                    newValue);

                return true;
            }

            var rule = new Rule<TParent>
            {
                TriggerProperties = token.PropertyNames,
                Updater = Updater,
                TargetPropertyName = propertyName
            };

            foreach (var name in token.PropertyNames)
            {
                if (!token.MappingRulesContainer.RulesByTrigger.TryGetValue(name, out var rules))
                {
                    rules = new List<Rule<TParent>>();
                    token.MappingRulesContainer.RulesByTrigger.Add(name, rules);
                }

                rules.Add(rule);
            }
        }

        /// <summary>
        ///     Internally used to chain the statements of the fluent syntax
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        public class FluentToken<TParent, TTargetProperty>
        {
            private HashSet<string> _propertyNames;

            public MappingRules<TParent> MappingRulesContainer { get; set; }

            public Func<TParent, TTargetProperty> ValueComputer { get; set; }

            public Predicate<TParent> IfPredicate { get; set; }

            public ISet<string> PropertyNames => _propertyNames ?? (_propertyNames = new HashSet<string>());

            public Expression<Func<TParent, TTargetProperty>> TargetPropertySelector { get; set; }
        }
    }
}