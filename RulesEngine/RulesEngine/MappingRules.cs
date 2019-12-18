using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RulesEngine.Tools;

namespace RulesEngine.RulesEngine
{
    /// <summary>
    ///     The business rules engine. It triggers the corresponding rule execution when a property changes from an external
    ///     source
    ///     or from a previous rule execution
    ///     Concrete classes should inherit from this one and declare all the rules in the constructor by using a fluent syntax
    /// </summary>
    /// <typeparam name="TRoot"></typeparam>
    public abstract class MappingRules<TRoot>
    {
        private readonly Dictionary<string, IList<Rule<TRoot>>> _rulesByTrigger =
            new Dictionary<string, IList<Rule<TRoot>>>();


        private readonly List<Rule<TRoot>> _rules = new List<Rule<TRoot>>();

        /// <summary>
        ///     If set triggers an exception which prevent a stack overflow if the specified level of recursion is over the
        ///     threshold
        /// </summary>
        protected int RecursionLimit { get; set; }

        public IDictionary<string, IList<Rule<TRoot>>> RulesByTrigger => _rulesByTrigger;

        public IList<Rule<TRoot>> Rules=> _rules;


        /// <summary>
        ///     First declaration of the fluent syntax. Sets the target property of the rule
        /// </summary>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        protected FluentExtensions.FluentToken<TRoot, TTargetProperty> Set<TTargetProperty>(
            Expression<Func<TRoot, TTargetProperty>> propertySelector)
        {
            
            return new FluentExtensions.FluentToken<TRoot, TTargetProperty>
            {
                MappingRulesContainer = this,
                TargetPropertySelector = propertySelector
            };

        }

        /// <summary>
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="root">the root (entry point) of the object graph</param>
        /// <param name="parent">the owner of the property</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ICollection<string> SetProperty(string propertyName, object root, object parent, object value)
        {
            var modified = new HashSet<string>();

            var smartSetter = CompiledAccessors.CompiledSmartSetter(parent.GetType(), propertyName);

            var hasChanged = smartSetter(parent, value);
            if (hasChanged)
            {
                modified.Add(propertyName);
                Cascade(propertyName, root, modified, 1);
            }

            return modified;
        }

        private void Cascade(string propertyName, object root, HashSet<string> modified,
            int recursionLimit)
        {
            if (RecursionLimit > 0)
                if (recursionLimit > RecursionLimit)
                    throw new NotSupportedException("Recursion limit exceeded: probably circular dependency");

            if (!_rulesByTrigger.TryGetValue(propertyName, out var rules)) rules = new List<Rule<TRoot>>();

            var modifiedInThisIteration = new HashSet<string>();

            foreach (var rule in rules)
            {
                var targetName = rule.TargetPropertyName;

                if (rule.Updater((TRoot) root))
                {
                    Trace(rule, propertyName, (TRoot) root);
                    modified.Add(targetName);
                    modifiedInThisIteration.Add(targetName);
                }
            }

            foreach (var name in modifiedInThisIteration) Cascade(name, root, modified, recursionLimit + 1);
        }


        /// <summary>
        /// Explicitly trigger all the rules. This may be useful if the object is not filled interactively
        /// </summary>
        /// <param name="root">The object on which the rules need to be triggered</param>
        public ICollection<string> TriggerAll(object root)
        {
            var modifiedInThisIteration = new HashSet<string>();
            foreach (var rule in Rules)
            {
                var targetName = rule.TargetPropertyName;

                if (rule.Updater((TRoot)root))
                {
                    Trace(rule, "", (TRoot)root);
                    
                    modifiedInThisIteration.Add(targetName);
                }
            }

            var allUpdates  = new HashSet<string>();

            foreach (var name in modifiedInThisIteration) Cascade(name, root, allUpdates, 1);

            return allUpdates;
        }

        public int RulesCount => Rules.Count;

        protected virtual void Trace(Rule<TRoot> triggeredRule, string triggerProperty, TRoot instance)
        {
        }
    }
}