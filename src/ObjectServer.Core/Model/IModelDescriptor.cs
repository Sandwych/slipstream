using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IModelDescriptor
    {
        NameGetter NameGetter { get; }
        ICollection<InheritanceInfo> Inheritances { get; }
        IFieldCollection Fields { get; }
        string TableName { get; }

        bool AutoMigration { get; }

        bool Hierarchy { get; }
        bool CanCreate { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        bool CanDelete { get; }
        bool IsVersioned { get; }

        bool LogCreation { get; }
        bool LogWriting { get; }
    }
}
