using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public interface IVisitor
    {
        void VisitBefore(IdentifierExpression node);
        void VisitOn(IdentifierExpression node);
        void VisitAfter(IdentifierExpression node);

        void VisitBefore(SelectStatement node);
        void VisitOn(SelectStatement node);
        void VisitAfter(SelectStatement node);

        void VisitBefore(AliasExpressionList node);
        void VisitOn(AliasExpressionList node);
        void VisitAfter(AliasExpressionList node);

        void VisitBefore(FromClause node);
        void VisitOn(FromClause node);
        void VisitAfter(FromClause node);


        void VisitBefore(WhereClause node);
        void VisitOn(WhereClause node);
        void VisitAfter(WhereClause node);

        void VisitBefore(JoinClause node);
        void VisitOn(JoinClause node);
        void VisitAfter(JoinClause node);

        void VisitBefore(OrderbyClause node);
        void VisitOn(OrderbyClause node);
        void VisitAfter(OrderbyClause node);

        void VisitBefore(OrderbyItem node);
        void VisitOn(OrderbyItem node);
        void VisitAfter(OrderbyItem node);

        void VisitBefore(OffsetClause node);
        void VisitOn(OffsetClause node);
        void VisitAfter(OffsetClause node);

        void VisitBefore(LimitClause node);
        void VisitOn(LimitClause node);
        void VisitAfter(LimitClause node);

        void VisitBefore(RawSql node);
        void VisitOn(RawSql node);
        void VisitAfter(RawSql node);

        void VisitBefore(UnaryExpression node);
        void VisitOn(UnaryExpression node);
        void VisitAfter(UnaryExpression node);

        void VisitBefore(BinaryExpression node);
        void VisitOn(BinaryExpression node);
        void VisitAfter(BinaryExpression node);

        void VisitBefore(ExpressionOperator node);
        void VisitOn(ExpressionOperator node);
        void VisitAfter(ExpressionOperator node);

        void VisitBefore(ValueExpression node);
        void VisitOn(ValueExpression node);
        void VisitAfter(ValueExpression node);


        void VisitBefore(AliasExpression node);
        void VisitOn(AliasExpression node);
        void VisitAfter(AliasExpression node);

        void VisitBefore(ExpressionGroup node);
        void VisitOn(ExpressionGroup node);
        void VisitAfter(ExpressionGroup node);


        void VisitBefore(BracketedExpression node);
        void VisitOn(BracketedExpression node);
        void VisitAfter(BracketedExpression node);

    }
}
