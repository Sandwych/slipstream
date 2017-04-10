using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    public class FieldCollection : Dictionary<string, IField>, IFieldCollection
    {
        private IEntity _entity;

        public FieldCollection(IEntity entity)
        {
            this._entity = entity;
        }

        public IField Integer(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Integer);
            this.Add(name, field);
            return field;
        }

        public IField BigInteger(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.BigInteger);
            this.Add(name, field);
            return field;
        }

        public IField Double(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Double);
            this.Add(name, field);
            return field;
        }

        public IField Decimal(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Decimal);
            this.Add(name, field);
            return field;
        }

        public IField Boolean(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Boolean);
            this.Add(name, field);
            return field;
        }

        public IField Chars(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Chars);
            this.Add(name, field);
            return field;
        }

        public IField Text(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Text);
            this.Add(name, field);
            return field;
        }

        public IField Xml(string name)
        {
            var field = new XmlField(this._entity, name);
            this.Add(name, field);
            return field;
        }

        public IField Binary(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Binary);
            this.Add(name, field);
            return field;
        }

        public IField DateTime(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.DateTime);
            this.Add(name, field);
            return field;
        }

        public IField Date(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Date);
            this.Add(name, field);
            return field;
        }

        public IField Time(string name)
        {
            var field = new ScalarField(this._entity, name, FieldType.Time);
            this.Add(name, field);
            return field;
        }

        public IField ManyToOne(string name, string masterEntityName)
        {
            var field = new ManyToOneField(this._entity, name, masterEntityName);
            this.Add(name, field);
            return field;
        }

        public IField OneToMany(string name, string childEntityName, string relatedField)
        {
            var field = new OneToManyField(this._entity, name, childEntityName, relatedField);
            this.Add(name, field);
            return field;
        }


        public IField ManyToMany(string name, string refEntityName, string originField, string targetField)
        {
            var field = new ManyToManyField(this._entity, name, refEntityName, originField, targetField);
            this.Add(name, field);
            return field;
        }

        public IField Enumeration(
            string name, IDictionary<string, string> options)
        {
            var field = new EnumerationField(this._entity, name, options);
            this.Add(name, field);
            return field;
        }

        public IField Reference(
            string name)
        {
            var field = new ReferenceField(this._entity, name);
            this.Add(name, field);
            return field;
        }

    }
}
