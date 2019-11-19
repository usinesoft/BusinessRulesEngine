using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.RulesEngine
{
    /// <summary>
    ///     A rule assigns a value to a single property. It can be triggered by a list of properties
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    public class Rule<TParent>
    {
        public ISet<string> TriggerProperties { get; set; } = new HashSet<string>();


        /// <summary>
        ///     Name of the property that will be updated by this rule
        /// </summary>
        public string TargetPropertyName { get; set; }

        /// <summary>
        ///     Compiled method which updates the target property. It return true if value changed
        /// </summary>
        public Func<TParent, bool> Updater { get; set; }

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            foreach (var triggerProperty in TriggerProperties)
            {
                builder.Append(triggerProperty)
                    .Append(" ");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append(")");

            builder.Append("\t => ")
                .Append(TargetPropertyName);

            return builder.ToString();
        }

        #endregion
    }
}