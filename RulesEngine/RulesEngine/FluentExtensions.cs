using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using RulesEngine.RulesEngine.Explain;
using RulesEngine.Tools;

namespace RulesEngine.RulesEngine
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
        /// <param name="manualExplain">optionally provide a human readable description</param>
        /// <returns></returns>
        public static FluentToken<TParent, TTargetProperty> With<TParent, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, Expression<Func<TParent, TTargetProperty>> valueComputer, string manualExplain = null)
        {
            token.ValueComputerExplained = manualExplain ?? valueComputer.TryExplain();
            token.ValueComputer = valueComputer.Compile();

            return token;
        }

        /// <summary>
        ///     Specifies the first trigger property
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        /// <param name="propertySelectors"></param>
        /// <returns></returns>
        public static void OnChanged<TParent, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, params Expression<Func<TParent, object>>[] propertySelectors)
        {

            foreach (var propertySelector in propertySelectors)
            {
                token.PropertyNames.Add(ExpressionTreeHelper.PropertyName(propertySelector));
            }

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
                TargetPropertyName = propertyName,
                IfExplained = token.IfExplained,
                ValueComputerExplained = token.ValueComputerExplained

            };


            token.MappingRulesContainer.Rules.Add(rule);

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
        ///     Specifies an optional applicability condition as a predicate; if false the rule will not be triggered
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="token"></param>
        /// <param name="ifPredicate"></param>
        /// <param name="manualExplain">optionally provide a human readable description</param>
        /// <returns></returns>
        public static FluentToken<TParent, TTargetProperty> If<TParent, TTargetProperty>(
            this FluentToken<TParent, TTargetProperty> token, Expression<Func<TParent, bool>> ifPredicate, string manualExplain = null) 
        {
            token.IfPredicate = ifPredicate.Compile();
            token.IfExplained = manualExplain ?? ifPredicate.TryExplain();

            return token;
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

            public Func<TParent, bool> IfPredicate { get; set; }

            public ISet<string> PropertyNames => _propertyNames ?? (_propertyNames = new HashSet<string>());

            public Expression<Func<TParent, TTargetProperty>> TargetPropertySelector { get; set; }
            public string ValueComputerExplained { get; set; }
            public string IfExplained { get; set; } 
        }
    }
}