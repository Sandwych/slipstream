using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    public interface ITable
    {
        string Name { get; }

        void AddColumn(IDatabase db, IField field);
        void DeleteColumn(IDatabase db, string columnName);
        void UpgradeColumn(IDatabase db, IField field);

        bool TableExists(IDatabase db, string tableName);
        void CreateTable(IDatabase db, string tableName, string label);
    }
}
