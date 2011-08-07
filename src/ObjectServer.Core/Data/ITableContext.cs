using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Data
{
    public interface ITableContext
    {
        string Name { get; }

        void AddColumn(IDBContext db, IField field);
        void DeleteColumn(IDBContext db, string columnName);
        void AlterColumnNullable(IDBContext db, string columnName, bool nullable);
        void AlterColumnType(IDBContext db, string columnName, string sqlType);
        bool ColumnExists(string columnName);
        IColumnMetadata GetColumn(string columnName);
        IColumnMetadata[] GetAllColumns();

        bool TableExists(IDBContext db, string tableName);
        void CreateTable(IDBContext db, string tableName, string label);

        void AddConstraint(IDBContext db, string constraintName, string constraint);
        void DeleteConstraint(IDBContext db, string constraintName);
        bool ConstraintExists(IDBContext db, string constraintName);

        void AddFK(IDBContext db, string columnName, string refTable, OnDeleteAction refAct);
        void DeleteFK(IDBContext db, string columnName);
        bool FKExists(IDBContext db, string columnName);
    }
}
