
namespace RulesEngine.RulesEngine.Explain
{
    /// <summary>
    ///     Abstract base class for the queries
    /// </summary>
    
    public abstract class ExplainExpression
    {
        /// <summary>
        ///     Check if the current query is valid. Validity rules are specific to each subclass
        /// </summary>
        public abstract bool IsValid { get; }

    }
}