#region

using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace RulesEngine.RulesEngine.Explain
{
    /// <summary>
    ///     A list of and queries bound by an OR operator
    /// </summary>

    public class ExplainOrExpression : ExplainExpression
    {
        private readonly List<ExplainAndExpression> _elements = new List<ExplainAndExpression>();

        public override bool IsValid
        {
            get { return Elements.All(element => element.IsValid); }
        }

        /// <summary>
        ///     The elements of type <see cref="ExplainAndExpression" />
        /// </summary>
        public IList<ExplainAndExpression> Elements => _elements;


        public bool MultipleWhereClauses { get; set; }
        

        public override string ToString()
        {
            if (_elements.Count == 0)
                return "<empty>";
            
            var sb = new StringBuilder();
            sb.Append(string.Join(" OR ",_elements.Select(e => e.ToString()).ToArray()));

            return sb.ToString();
        }

    }
}