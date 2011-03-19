using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class UpdateStatement : Node, IStatement
    {
        public AliasExpression Table { get; set; }
        public SetClause SetClause { get; set; }
        public WhereClause WhereClause { get; set; }

        public UpdateStatement(AliasExpression table)
            : this(table, null, null)
        {
        }

        public UpdateStatement(AliasExpression table, SetClause setClause) : 
            this(table, setClause, null) 
        {
        }

        public UpdateStatement(AliasExpression table, SetClause setClause, WhereClause whereClause)
        {
            this.Table = table;
            this.SetClause = setClause;
            this.WhereClause = whereClause;
        }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);

            if (this.Table != null)
            {
                this.Table.Traverse(visitor);
            }

            if (this.SetClause != null)
            {
                this.SetClause.Traverse(visitor);
            }

            if (this.WhereClause != null)
            {
                this.WhereClause.Traverse(visitor);
            }

            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        public override string ToString()
        {
            var sv = new StringifierVisitor();
            this.Traverse(sv);
            return sv.ToString();
        }


    }
}
