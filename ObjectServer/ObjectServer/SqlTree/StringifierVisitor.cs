using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class StringifierVisitor : StackedVisitor
    {
        private StringBuilder sqlBuilder = new StringBuilder();

        public override void VisitOn(Identifier node)
        {
            this.sqlBuilder.Append(" \"");
            this.sqlBuilder.Append(node.Id);
            this.sqlBuilder.Append(" \"");
        }

        public override void VisitOn(ColumnList node)
        {
            bool isFirst = true;
            foreach (var col in node.Columns)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    this.sqlBuilder.Append(",");
                }
                this.sqlBuilder.Append("\"");
                this.sqlBuilder.Append(col);
                this.sqlBuilder.Append("\"");
            }
        }

        public override void VisitOn(FromClause node)
        {
            this.sqlBuilder.Append(" from ");
        }

        public override void VisitOn(SelectStatement node)
        {
            this.sqlBuilder.Append(" select ");
        }

        public override void VisitOn(RawSql node)
        {
            this.sqlBuilder.Append(' ');
            this.sqlBuilder.Append(node.SqlString);
            this.sqlBuilder.Append(' ');
        }

        public override string ToString()
        {
            return this.sqlBuilder.ToString();
        }
    }
}
