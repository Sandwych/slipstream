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

        void AddColumn(IDbContext db, IFieldDescriptor field);
        void DeleteColumn(IDbContext db, string columnName);
        void AlterColumnNullable(IDbContext db, string columnName, bool nullable);
        void AlterColumnType(IDbContext db, string columnName, string sqlType);
        bool ColumnExists(string columnName);
        IColumnMetadata GetColumn(string columnName);
        IColumnMetadata[] GetAllColumns();

        bool TableExists(IDbContext db, string tableName);
        void CreateTable(IDbContext db, string tableName, string label);
        void CreateTable(IDbContext db, IModelDescriptor model, string label);

        void AddConstraint(IDbContext db, string constraintName, string constraint);
        void DeleteConstraint(IDbContext db, string constraintName);
        bool ConstraintExists(IDbContext db, string constraintName);

        void AddFK(IDbContext db, string columnName, string refTable, OnDeleteAction refAct);
        void DeleteFK(IDbContext db, string columnName);
        bool FKExists(IDbContext db, string columnName);
    }
}
