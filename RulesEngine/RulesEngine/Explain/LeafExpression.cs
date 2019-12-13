using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RulesEngine.RulesEngine.Explain
{
    /// <summary>
    ///     The smallest expression we are able to interpret
    /// </summary>

    public sealed class LeafExpression : ExplainExpression
    {
        public string MemberName { get; set; }

        private HashSet<object> _inValues = new HashSet<object>();

        /// <summary>
        ///     Parameter-less constructor used for serialization
        /// </summary>
        public LeafExpression()
        {
        }


        /// <summary>
        ///     Build a simple atomic query (one value and unary operator)
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="value"></param>
        /// <param name="operator"></param>
        public LeafExpression(string memberName, object value, QueryOperator @operator = QueryOperator.Eq)
        {
            MemberName = memberName;
            Value = value;
            Operator = @operator;
        }


        /// <summary>
        ///     Build an IN query
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="values"></param>
        public LeafExpression(string memberName, IEnumerable<object> values)
        {
            MemberName = memberName;
            _inValues = new HashSet<object>(values);
            Operator = QueryOperator.In;
        }


        /// <summary>
        ///     Check if the query is valid
        /// </summary>
        public override bool IsValid
        {
            get
            {
                // IN requires a list of values
                if (Operator == QueryOperator.In && InValues.Count == 0)
                    return false;

                // only IN accepts a list of values
                if (Operator != QueryOperator.In && InValues.Count > 0)
                    return false;

                // any operator except IN requires at least a value
                if (Operator != QueryOperator.In && ReferenceEquals(Value, null))
                    return false;
   
                return true;
            }
        }

        
        public object Value { get; }

        /// <summary>
        ///     used for binary operators
        /// </summary>
      
        /// <summary>
        ///     The operator of the atomic query
        /// </summary>
        public QueryOperator Operator { get; set; }

        
        public ICollection<object> InValues
        {
            get => _inValues;
            set => _inValues = new HashSet<object>(value);
        }

        public IList<object> Values => _inValues.Count > 0
            ? _inValues.ToList(): new List<object> {Value};

        public string MethodCall { get; set; }


        public override string ToString()
        {

            if (MethodCall != null)
                return MethodCall;

            var result = new StringBuilder();

            result.Append(MemberName);

            switch (Operator)
            {
                case QueryOperator.Eq:
                    result.Append( " = ");
                    break;
                case QueryOperator.NotEqual:
                    result.Append(" <> ");
                    break;
                case QueryOperator.Le:
                    result.Append(" <= ");
                    break;
                case QueryOperator.Lt:
                    result.Append(" < ");
                    break;
                case QueryOperator.Gt:
                    result.Append(" > ");
                    break;
                case QueryOperator.Ge:
                    result.Append(" >= ");
                    break;
                case QueryOperator.In:
                    result.Append(" IN ");
                    break;
            }


            if (Operator == QueryOperator.In)
            {
                result.Append("(");

                    result.Append(string.Join(", ", InValues.ToArray()));
                    
                    result.Append(")");

            }
            else
            {
                result.Append(Value ?? "null");
            }
            
            return result.ToString();
        }

    }
}