using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    public interface IEntityDescriptor
    {
        NameGetter NameGetter { get; }
        ICollection<InheritanceDescriptor> Inheritances { get; }
        IFieldCollection Fields { get; }
        string TableName { get; }
        IEnumerable<OrderExpression> Order { get; }
        IEntityDescriptor OrderBy(IEnumerable<OrderExpression> order);

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
