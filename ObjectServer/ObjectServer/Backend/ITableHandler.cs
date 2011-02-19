using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    public interface ITableHandler
    {
        string Name { get; }

        void AddColumn(IField field);

        void UpgradeColumn(IField field);

         bool TableExists(string tableName);

         void CreateTable(string tableName, string label);         
    }
}
