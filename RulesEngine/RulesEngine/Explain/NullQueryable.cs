using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace RulesEngine.RulesEngine.Explain
{
    internal class NullQueryable<T> : QueryableBase<T>
    {
        public NullQueryable(IQueryParser queryParser, IQueryExecutor executor) : base(queryParser, executor)
        {
        }

        public NullQueryable(IQueryProvider provider) : base(provider)
        {
        }

        public NullQueryable(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }

        public NullQueryable(IQueryExecutor executor) : base(QueryParser.CreateDefault(), executor)
        {
        }


    }
}