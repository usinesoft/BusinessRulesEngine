using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;

namespace RulesEngine.RulesEngine.Explain
{
    public class NullExecutor : IQueryExecutor
    {
        
        
        public ExplainOrExpression Expression { get; private set; }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return default(T);
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return default(T);
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var visitor = new QueryVisitor();

            visitor.VisitQueryModel(queryModel);

            Expression = visitor.RootExpression;

            return Enumerable.Empty<T>();
        }
    }
}