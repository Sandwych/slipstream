using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class StringifierVisitor : StackedVisitor
    {
        private StringBuilder sqlBuilder = new StringBuilder();

        public override string ToString()
        {
            return this.sqlBuilder.ToString();
        }

        #region Visitor's methods:

        public override void VisitOn(IdentifierExpression node)
        {
            this.sqlBuilder.Append(" \"");
            this.sqlBuilder.Append(node.Id);
            this.sqlBuilder.Append("\" ");
        }


        public override void VisitOn(ExpressionList node)
        {
            /*
bool isFirst = true;
foreach (var col in node.Expressions)
{
    if (isFirst)
    {
        isFirst = false;
    }
    else
    {
        this.sqlBuilder.Append(",");
    }
    this.sqlBuilder.Append(col);
}
 */
        }

        public override void VisitOn(FromClause node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(" from ");
        }

        public override void VisitOn(SelectStatement node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(" select ");
        }

        public override void VisitBefore(JoinClause node)
        {
            base.VisitBefore(node);

            this.sqlBuilder.Append(" ");
            this.sqlBuilder.Append(node.JoinType);
            this.sqlBuilder.Append(" join ");
        }

        public override void VisitOn(JoinClause node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(" on ");
        }

        public override void VisitOn(RawSql node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(' ');
            this.sqlBuilder.Append(node.SqlString);
            this.sqlBuilder.Append(' ');
        }

        public override void VisitOn(BinaryExpression node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(' ');
            this.sqlBuilder.Append(' ');
        }

        public override void VisitOn(ExpressionOperator node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(' ');
            this.sqlBuilder.Append(node.Operator);
            this.sqlBuilder.Append(' ');
        }

        public override void VisitOn(UnaryExpression node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(' ');
            this.sqlBuilder.Append(node.Operator.Operator);
            this.sqlBuilder.Append(' ');
        }

        public override void VisitOn(ValueExpression node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(' ');

            if (node.Value is string)
            {
                var str = (string)node.Value;
                str.Replace("'", "''");
                this.sqlBuilder.Append('\'');
                this.sqlBuilder.Append(str);
                this.sqlBuilder.Append('\'');
            }
            else
            {
                this.sqlBuilder.Append(node.Value.ToString());
            }
            this.sqlBuilder.Append(' ');
        }

        public override void VisitOn(WhereClause node)
        {
            base.VisitOn(node);

            this.sqlBuilder.Append(" where ");
        }

        public override void VisitOn(AliasExpression node)
        {
            base.VisitOn(node);

        }

        public override void VisitAfter(AliasExpression node)
        {
            base.VisitAfter(node);

            if (this.Parent is ExpressionList)
            {
                var coll = (ExpressionList)this.Parent;
                if (!coll.IsLastExpression(node))
                {
                    this.sqlBuilder.Append(", ");
                }
            }
        }

        #endregion
    }
}
