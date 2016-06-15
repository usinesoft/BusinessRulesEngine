using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BusinessRulesEngine.Tools;

namespace BusinessRulesEngine.RulesEngine
{
    /// <summary>
    ///     The business rules engine. It triggers the corresponding rule execution when a property changes from an external
    ///     source
    ///     or from a previous rule execution
    ///     Concrete classes should inherit from this one and declare all the rules in the constructor by using a fluent syntax
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    public abstract class MappingRules<TParent>
    {
        private readonly Dictionary<string, IList<Rule<TParent>>> _rulesByTrigger =
            new Dictionary<string, IList<Rule<TParent>>>();

        protected MappingRules(TParent root)
        {
            Root = root;
        }

        private TParent Root { get; }

        /// <summary>
        ///     If set triggers an exception which prevent a stack overflow if the specified level of recursion is over the
        ///     threshold
        /// </summary>
        protected int RecursionLimit { get; set; }

        public IDictionary<string, IList<Rule<TParent>>> RulesByTrigger => _rulesByTrigger;

        /// <summary>
        ///     First declaration of the fluent syntax. Sets the target property of the rule
        /// </summary>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        protected FluentExtensions.FluentToken<TParent, TTargetProperty> Set<TTargetProperty>(
            Expression<Func<TParent, TTargetProperty>> propertySelector)
        {
            return new FluentExtensions.FluentToken<TParent, TTargetProperty>
            {
                MappingRulesContainer = this,
                TargetPropertySelector = propertySelector
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="parent"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ICollection<string> SetProperty(string propertyName, object parent, object value)
        {
            var modified = new HashSet<string>();

            var smartSetter = CompiledAccessors.CompiledSmartSetter(parent.GetType(), propertyName);

            var hasChanged = smartSetter(parent, value);
            if (hasChanged)
            {
                modified.Add(propertyName);
                Cascade(propertyName, Root, modified, 1);
            }

            return modified;
        }

        private void Cascade(string propertyName, object parent, HashSet<string> modified, int recursionLimit)
        {
            if (RecursionLimit > 0)
            {
                if (recursionLimit > RecursionLimit)
                {
                    throw new NotSupportedException("Recursion limit exceeded: probably circular dependency");
                }
            }

            IList<Rule<TParent>> rules;
            if (!_rulesByTrigger.TryGetValue(propertyName, out rules))
            {
                rules = new List<Rule<TParent>>();
            }

            var modifiedInThisIteration = new HashSet<string>();

            foreach (var rule in rules)
            {
                var targetName = rule.TargetPropertyName;

                if (rule.Updater((TParent) parent))
                {
                    Trace(rule, propertyName, (TParent) parent);
                    modified.Add(targetName);
                    modifiedInThisIteration.Add(targetName);
                }
            }

            foreach (var name in modifiedInThisIteration)
            {
                Cascade(name, Root, modified, recursionLimit + 1);
            }
        }

        protected virtual void Trace(Rule<TParent> triggeredRule, string triggerProperty, TParent instance)
        {
        }
    }
}