using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public interface IVisitor
    {
        void VisitBefore(Identifier select);
        void VisitOn(Identifier select);
        void VisitAfter(Identifier select);

        void VisitBefore(SelectStatement select);
        void VisitOn(SelectStatement select);
        void VisitAfter(SelectStatement select);

        void VisitBefore(ColumnList columns);
        void VisitOn(ColumnList columns);
        void VisitAfter(ColumnList columns);

        void VisitBefore(FromClause from);
        void VisitOn(FromClause from);
        void VisitAfter(FromClause from);

        void VisitBefore(RawSql select);
        void VisitOn(RawSql select);
        void VisitAfter(RawSql select);
    }
}
