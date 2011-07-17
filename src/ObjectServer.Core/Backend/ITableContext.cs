using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    public interface ITableContext
    {
        string Name { get; }

        void AddColumn(IDBConnection db, IField field);
        void DeleteColumn(IDBConnection db, string columnName);
        void AlterColumnNullable(IDBConnection db, string columnName, bool nullable);
        void AlterColumnType(IDBConnection db, string columnName, string sqlType);
        bool ColumnExists(string columnName);
        IColumnMetadata GetColumn(string columnName);
        IColumnMetadata[] GetAllColumns();

        bool TableExists(IDBConnection db, string tableName);
        void CreateTable(IDBConnection db, string tableName, string label);

        void AddConstraint(IDBConnection db, string constraintName, string constraint);
        void DeleteConstraint(IDBConnection db, string constraintName);
        bool ConstraintExists(IDBConnection db, string constraintName);

        void AddFK(IDBConnection db, string columnName, string refTable, OnDeleteAction refAct);
        void DeleteFK(IDBConnection db, string columnName);
        bool FKExists(IDBConnection db, string columnName);
    }
}
