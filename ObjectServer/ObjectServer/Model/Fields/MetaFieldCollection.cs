using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public class MetaFieldCollection : Dictionary<string, IMetaField>, IMetaFieldCollection
    {
        public IMetaField Integer(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Integer);
            this.Add(name, field);
            return field;
        }

        public IMetaField BigInteger(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Integer);
            this.Add(name, field);
            return field;
        }

        public IMetaField Float(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Float);
            this.Add(name, field);
            return field;
        }

        public IMetaField Money(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Money);
            this.Add(name, field);
            return field;
        }

        public IMetaField Boolean(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Boolean);
            this.Add(name, field);
            return field;
        }

        public IMetaField Chars(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Chars);
            this.Add(name, field);
            return field;
        }

        public IMetaField Text(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Text);
            this.Add(name, field);
            return field;
        }

        public IMetaField Binary(string name)
        {
            var field = new ScalarMetaField(name, FieldType.Binary);
            this.Add(name, field);
            return field;
        }

        public IMetaField DateTime(string name)
        {
            var field = new ScalarMetaField(name, FieldType.DateTime);
            this.Add(name, field);
            return field;
        }

        public IMetaField ManyToOne(string name, string masterModel)
        {
            var field = new ManyToOneMetaField(name, masterModel);
            this.Add(name, field);
            return field;
        }

        public IMetaField OneToMany(string name, string childModel, string relatedField)
        {
            var field = new OneToManyMetaField(name, childModel, relatedField);
            this.Add(name, field);
            return field;
        }


        public IMetaField ManyToMany(string name, string refModel, string originField, string targetField)
        {
            var field = new ManyToManyMetaField(name, refModel, originField, targetField);
            this.Add(name, field);
            return field;
        }

        public IMetaField Enumeration(
            string name, IEnumerable<KeyValuePair<string, string>> options)
        {
            var field = new EnumerationMetaField(name, options);
            this.Add(name, field);
            return field;
        }
    }
}
