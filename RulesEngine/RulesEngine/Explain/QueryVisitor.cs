using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace RulesEngine.RulesEngine.Explain
{
    public class QueryVisitor : QueryModelVisitorBase
    {
        
        public QueryVisitor()
        {
            RootExpression = new ExplainOrExpression();
        }

        public ExplainOrExpression RootExpression { get; set; }


        private bool IsLeafExpression(Expression expression)
        {
            return expression.NodeType == ExpressionType.GreaterThan
                   || expression.NodeType == ExpressionType.GreaterThanOrEqual
                   || expression.NodeType == ExpressionType.LessThan
                   || expression.NodeType == ExpressionType.LessThanOrEqual
                   || expression.NodeType == ExpressionType.Equal
                   || expression.NodeType == ExpressionType.NotEqual;
        }


        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            base.VisitSelectClause(selectClause, queryModel);
        }

     

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (whereClause.Predicate is BinaryExpression expression)
            {
                VisitBinaryExpression(expression, RootExpression);
            }
            else
            {
                if (whereClause.Predicate is SubQueryExpression subQuery)
                {
                    ExplainAndExpression andExpression;

                    if (!RootExpression.MultipleWhereClauses)
                    {
                        andExpression = new ExplainAndExpression();
                        RootExpression.Elements.Add(andExpression);
                    }
                    else // multiple where clauses are joined by AND
                    {
                        andExpression = RootExpression.Elements[0];
                    }

                    var leaf = new LeafExpression();
                    andExpression.Elements.Add(leaf);

                    VisitContainsExpression(subQuery, leaf);
                }
                else if (whereClause.Predicate is MethodCallExpression call)
                {
                    ExplainAndExpression andExpression;

                    if (!RootExpression.MultipleWhereClauses)
                    {
                        andExpression = new ExplainAndExpression();
                        RootExpression.Elements.Add(andExpression);
                    }
                    else // multiple where clauses are joined by AND
                    {
                        andExpression = RootExpression.Elements[0];
                    }

                    var leaf = CallToLeafExpression(call);
                    andExpression.Elements.Add(leaf);
                    
                }
                else if(whereClause.Predicate.NodeType != ExpressionType.Constant)
                {
                    throw new NotSupportedException("query too complex");
                }
            }


            RootExpression.MultipleWhereClauses = true;

            base.VisitWhereClause(whereClause, queryModel, index);
        }


       
        private void VisitAndExpression(BinaryExpression binaryExpression, ExplainAndExpression andExpression)
        {
            if (IsLeafExpression(binaryExpression.Left))
            {
                andExpression.Elements.Add(VisitLeafExpression((BinaryExpression)binaryExpression.Left));
            }
            else if (binaryExpression.Left.NodeType == ExpressionType.AndAlso)
            {
                VisitAndExpression((BinaryExpression)binaryExpression.Left, andExpression);
            }
            else if (binaryExpression.Left.NodeType == ExpressionType.Extension)
            {
                if (binaryExpression.Left is SubQueryExpression subQuery)
                {
                    var leaf = new LeafExpression();
                    andExpression.Elements.Add(leaf);
                    VisitContainsExpression(subQuery, leaf);
                }
            }
            else if (binaryExpression.Left is MethodCallExpression callExpression)
            {
                andExpression.Elements.Add(CallToLeafExpression(callExpression));
            }
            else
            {
                throw new NotSupportedException("ExplainExpression too complex");
            }

            if (IsLeafExpression(binaryExpression.Right))
            {
                andExpression.Elements.Add(VisitLeafExpression((BinaryExpression)binaryExpression.Right));
            }
            else if (binaryExpression.Right.NodeType == ExpressionType.Extension)
            {
                if (binaryExpression.Right is SubQueryExpression subQuery)
                {
                    var leaf = new LeafExpression();
                    andExpression.Elements.Add(leaf);
                    VisitContainsExpression(subQuery, leaf);
                }
            }
            else if (binaryExpression.Right is MethodCallExpression callExpression)
            {
                andExpression.Elements.Add(CallToLeafExpression(callExpression));
            }
            else 
            {
                throw new NotSupportedException("ExplainExpression too complex");
            }
        }

        private void VisitContainsExpression(SubQueryExpression subQuery, LeafExpression leaf)
        {
            if (subQuery.QueryModel.ResultOperators.Count != 1)
                throw new NotSupportedException("Only Contains extension is supported");

            var contains = subQuery.QueryModel.ResultOperators[0] as ContainsResultOperator;

            // process collection.Contains(x=>x.Member)
            if (contains?.Item is MemberExpression item)
            {
                var select = subQuery.QueryModel?.SelectClause.Selector as QuerySourceReferenceExpression;

                leaf.MemberName = item.Member.Name;

                if (select?.ReferencedQuerySource is MainFromClause from)
                {
                    var expression = from.FromExpression as ConstantExpression;

                    if (expression?.Value is IEnumerable values)
                    {
                        leaf.Operator = QueryOperator.In;

                        foreach (var value in values)
                        {
                            leaf.InValues.Add(value);
                        }

                        return;
                    }
                }
            }
            // process x=>x.VectorMember.Contains(value)
            else
            {
                var value = contains?.Item;

                if (value != null)
                {
                    var select = subQuery.QueryModel?.SelectClause.Selector as QuerySourceReferenceExpression;
                    var from = select?.ReferencedQuerySource as MainFromClause;


                    if (from?.FromExpression is MemberExpression expression)
                    {
                        // the member must not be a scalar type. A string is a vector of chars but still considered a scalar in this context
                        var isVector = typeof(IEnumerable).IsAssignableFrom(expression.Type) &&
                                       !typeof(string).IsAssignableFrom(expression.Type);

                        if (!isVector)
                            throw new NotSupportedException("Trying to use Contains extension on a scalar member");


                        if (value is ConstantExpression valueExpession)
                        {
                            leaf.Operator = QueryOperator.In;

                            leaf.InValues.Add(valueExpession.Value);

                            return;
                        }
                    }
                }
            }

            throw new NotSupportedException("Only Contains extension is supported");
        }

        private void VisitBinaryExpression(BinaryExpression binaryExpression, ExplainOrExpression rootExpression)
        {
            // manage AND expressions
            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                var andExpression = new ExplainAndExpression();
                rootExpression.Elements.Add(andExpression);

                VisitAndExpression(binaryExpression, andExpression);
            }

            // manage OR expressions
            else if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                VisitOrExpression(binaryExpression, rootExpression);
            }

            // manage simple expressions like a > 10
            else if (IsLeafExpression(binaryExpression))
            {
                ExplainAndExpression andExpression;

                if (!rootExpression.MultipleWhereClauses)
                {
                    andExpression = new ExplainAndExpression();
                    rootExpression.Elements.Add(andExpression);
                }
                else // if multiple where clauses consider them as expressions linked by AND
                {
                    andExpression = rootExpression.Elements[0];
                }


                andExpression.Elements.Add(VisitLeafExpression(binaryExpression));
            }
            else
            {
                throw new NotSupportedException("ExplainExpression too complex");
            }
        }

        //TODO add unit test for OR expression with Contains
        /// <summary>
        ///     OR expression can be present only at root level
        /// </summary>
        /// <param name="binaryExpression"></param>
        /// <param name="rootExpression"></param>
        private void VisitOrExpression(BinaryExpression binaryExpression, ExplainOrExpression rootExpression)
        {
            // visit left part
            if (IsLeafExpression(binaryExpression.Left))
            {
                var andExpression = new ExplainAndExpression();
                rootExpression.Elements.Add(andExpression);

                andExpression.Elements.Add(VisitLeafExpression((BinaryExpression)binaryExpression.Left));
            }
            else if (binaryExpression.Left.NodeType == ExpressionType.AndAlso)
            {
                var andExpression = new ExplainAndExpression();
                rootExpression.Elements.Add(andExpression);
                VisitAndExpression((BinaryExpression)binaryExpression.Left, andExpression);
            }
            else if (binaryExpression.Left.NodeType == ExpressionType.Extension)
            {
                if (binaryExpression.Left is SubQueryExpression subQuery)
                {
                    ExplainAndExpression andExpression;

                    if (!rootExpression.MultipleWhereClauses)
                    {
                        andExpression = new ExplainAndExpression();
                        rootExpression.Elements.Add(andExpression);
                    }
                    else // multiple where clauses are joined by AND
                    {
                        andExpression = rootExpression.Elements[0];
                    }


                    var leaf = new LeafExpression();
                    andExpression.Elements.Add(leaf);

                    VisitContainsExpression(subQuery, leaf);
                }
            }
            else if (binaryExpression.Left.NodeType == ExpressionType.OrElse)
            {
                VisitOrExpression((BinaryExpression)binaryExpression.Left, rootExpression);
            }
            else if (binaryExpression.Left.NodeType == ExpressionType.AndAlso)
            {
                var andExpression = new ExplainAndExpression();
                rootExpression.Elements.Add(andExpression);
                VisitAndExpression((BinaryExpression)binaryExpression.Left, andExpression);
            }
            else if (binaryExpression.Left is MethodCallExpression call)
            {
                ExplainAndExpression andExpression;

                if (!RootExpression.MultipleWhereClauses)
                {
                    andExpression = new ExplainAndExpression();
                    RootExpression.Elements.Add(andExpression);
                }
                else // multiple where clauses are joined by AND
                {
                    andExpression = RootExpression.Elements[0];
                }

                var leaf = CallToLeafExpression(call);
                andExpression.Elements.Add(leaf);

            }
            else
            {
                throw new NotSupportedException("ExplainExpression too complex");
            }

            // visit right part
            if (IsLeafExpression(binaryExpression.Right))
            {
                var andExpression = new ExplainAndExpression();
                rootExpression.Elements.Add(andExpression);

                andExpression.Elements.Add(VisitLeafExpression((BinaryExpression)binaryExpression.Right));
            }
            else if (binaryExpression.Right.NodeType == ExpressionType.Extension)
            {
                if (binaryExpression.Right is SubQueryExpression subQuery)
                {
                    var andExpression = new ExplainAndExpression();
                    rootExpression.Elements.Add(andExpression);

                    if (rootExpression.MultipleWhereClauses)
                        throw new NotSupportedException(
                            "Multiple where clauses can be used only with simple expressions");


                    var leaf = new LeafExpression();
                    andExpression.Elements.Add(leaf);
                    VisitContainsExpression(subQuery, leaf);
                }
            }
            else if (binaryExpression.Right.NodeType == ExpressionType.OrElse)
            {
                VisitOrExpression((BinaryExpression)binaryExpression.Right, rootExpression);
            }
            else if (binaryExpression.Right.NodeType == ExpressionType.AndAlso)
            {
                var andExpression = new ExplainAndExpression();
                rootExpression.Elements.Add(andExpression);
                VisitAndExpression((BinaryExpression)binaryExpression.Right, andExpression);
            }
            else if (binaryExpression.Right is MethodCallExpression call)
            {
                ExplainAndExpression andExpression;

                if (!RootExpression.MultipleWhereClauses)
                {
                    andExpression = new ExplainAndExpression();
                    RootExpression.Elements.Add(andExpression);
                }
                else // multiple where clauses are joined by AND
                {
                    andExpression = RootExpression.Elements[0];
                }

                var leaf = CallToLeafExpression(call);
                andExpression.Elements.Add(leaf);

            }
            else
            {
                throw new NotSupportedException("ExplainExpression too complex");
            }
        }

        private static LeafExpression CallToLeafExpression(MethodCallExpression call)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(call.Method.Name);
            sb.Append("(");

            foreach (var argument in call.Arguments)
            {
                if (argument is ConstantExpression constant)
                {
                    sb.Append(constant.Value);
                }
            }

            sb.Append(")");


            var leaf = new LeafExpression {MethodCall = sb.ToString()};
            return leaf;
        }

        // TODO add unit test for reverted expression : const = member

        /// <summary>
        ///     Manage simple expressions like left operator right
        /// </summary>
        /// <param name="binaryExpression"></param>
        private LeafExpression VisitLeafExpression(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left is MemberExpression left && binaryExpression.Right is ConstantExpression right)
            {
                
                var oper = QueryOperator.Eq;


                if (binaryExpression.NodeType == ExpressionType.GreaterThan) oper = QueryOperator.Gt;

                if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual) oper = QueryOperator.Ge;

                if (binaryExpression.NodeType == ExpressionType.LessThan) oper = QueryOperator.Lt;

                if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual) oper = QueryOperator.Le;
                
                if (binaryExpression.NodeType == ExpressionType.NotEqual) oper = QueryOperator.NotEqual;

                return new LeafExpression(left.Member.Name, right.Value, oper);
            }

            // try to revert the expression
            left = binaryExpression.Right as MemberExpression;
            right = binaryExpression.Left as ConstantExpression;

            if (left != null && right != null)
            {
                

                var oper = QueryOperator.Eq;


                if (binaryExpression.NodeType == ExpressionType.GreaterThan) oper = QueryOperator.Le;

                if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual) oper = QueryOperator.Lt;

                if (binaryExpression.NodeType == ExpressionType.LessThan) oper = QueryOperator.Ge;

                if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual) oper = QueryOperator.Gt;

                return new LeafExpression(left.Member.Name,  right.Value, oper);
            }

            throw new NotSupportedException("Error parsing binary expression");
        }
    }
}