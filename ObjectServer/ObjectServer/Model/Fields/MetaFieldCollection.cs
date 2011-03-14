using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public class MetaFieldCollection : Dictionary<string, IMetaField>, IMetaFieldCollection
    {
        private IMetaModel model;

        public MetaFieldCollection(IMetaModel model)
        {
            this.model = model;
        }

        public IMetaField Integer(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Integer);
            this.Add(name, field);
            return field;
        }

        public IMetaField BigInteger(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.BigInteger);
            this.Add(name, field);
            return field;
        }

        public IMetaField Float(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Float);
            this.Add(name, field);
            return field;
        }

        public IMetaField Money(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Decimal);
            this.Add(name, field);
            return field;
        }

        public IMetaField Boolean(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Boolean);
            this.Add(name, field);
            return field;
        }

        public IMetaField Chars(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Chars);
            this.Add(name, field);
            return field;
        }

        public IMetaField Text(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Text);
            this.Add(name, field);
            return field;
        }

        public IMetaField Binary(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.Binary);
            this.Add(name, field);
            return field;
        }

        public IMetaField DateTime(string name)
        {
            var field = new ScalarMetaField(this.model, name, FieldType.DateTime);
            this.Add(name, field);
            return field;
        }

        public IMetaField ManyToOne(string name, string masterModel)
        {
            var field = new ManyToOneMetaField(this.model, name, masterModel);
            this.Add(name, field);
            return field;
        }

        public IMetaField OneToMany(string name, string childModel, string relatedField)
        {
            var field = new OneToManyMetaField(this.model, name, childModel, relatedField);
            this.Add(name, field);
            return field;
        }


        public IMetaField ManyToMany(string name, string refModel, string originField, string targetField)
        {
            var field = new ManyToManyMetaField(this.model, name, refModel, originField, targetField);
            this.Add(name, field);
            return field;
        }

        public IMetaField Enumeration(
            string name, IDictionary<string, string> options)
        {
            var field = new EnumerationMetaField(this.model, name, options);
            this.Add(name, field);
            return field;
        }

        public IMetaField Reference(
            string name)
        {
            var field = new ReferenceMetaField(this.model, name);
            this.Add(name, field);
            return field;
        }

        public IMetaField Version()
        {
            if (this.model.Fields.ContainsKey(AbstractModel.VersionFieldName))
            {
                throw new InvalidOperationException("You cannot to define more than one 'version' field");
            }

            var field = new ScalarMetaField(this.model, AbstractModel.VersionFieldName, FieldType.BigInteger);
            field.Required();
            this.Add(field.Name, field);
            return field;
        }
    }
}
