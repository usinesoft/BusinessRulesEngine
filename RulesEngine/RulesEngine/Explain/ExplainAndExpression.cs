#region

using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace RulesEngine.RulesEngine.Explain
{
    /// <summary>
    ///     A list of atomic queries bound by an AND operator
    /// </summary>
    public class ExplainAndExpression : ExplainExpression
    {
        /// <summary>
        ///     Create an empty query (called internally by the query builder)
        /// </summary>
        public ExplainAndExpression()
        {
            Elements = new List<LeafExpression>();
        }

        /// <summary>
        ///     The contained atomic queries should apply to different keys
        /// </summary>
        public override bool IsValid
        {
            get { return Elements.All(atomicQuery => atomicQuery.IsValid); }
        }

        /// <summary>
        ///     Accessor for the underlying elements (<see cref="LeafExpression" />
        /// </summary>
        public List<LeafExpression> Elements { get; private set; }

        
        public override string ToString()
        {
            if (Elements.Count == 0)
                return "<empty>";
            if (Elements.Count == 1)
                return Elements[0].ToString();

            var sb = new StringBuilder();
            sb.Append(string.Join(" AND ", Elements.Select(e => e.ToString()).ToArray()));

            return sb.ToString();
        }


    }
}