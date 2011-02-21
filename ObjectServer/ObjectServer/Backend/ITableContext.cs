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

        void AddColumn(IDatabaseContext db, IMetaField field);
        void DeleteColumn(IDatabaseContext db, string columnName);
        void UpgradeColumn(IDatabaseContext db, IMetaField field);

        bool TableExists(IDatabaseContext db, string tableName);
        void CreateTable(IDatabaseContext db, string tableName, string label);

        void AddConstraint(IDatabaseContext db, string constraintName, string constraint);
        void DeleteConstraint(IDatabaseContext db, string constraintName);

        void AddFk(IDatabaseContext db, string columnName, string refTable, ReferentialAction refAct);
        void DeleteFk(IDatabaseContext db, string columnName);
        bool FkExists(IDatabaseContext db, string columnName);
    }
}
