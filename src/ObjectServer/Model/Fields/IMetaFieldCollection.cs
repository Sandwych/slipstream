using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IMetaFieldCollection : IDictionary<string, IMetaField>
    {
        IMetaField Integer(string name);
        IMetaField BigInteger(string name);
        IMetaField Float(string name);
        IMetaField Money(string name);
        IMetaField Boolean(string name);
        IMetaField DateTime(string name);
        IMetaField Chars(string name);
        IMetaField Text(string name);
        IMetaField Binary(string name);


        IMetaField ManyToOne(string name, string masterModel);
        IMetaField OneToMany(string name, string childModel, string relatedField);
        IMetaField ManyToMany(string name, string refModel, string originField, string targetField);

        IMetaField Enumeration(string name, IDictionary<string, string> options);
        IMetaField Reference(string name);

        IMetaField Version();
    }
}
