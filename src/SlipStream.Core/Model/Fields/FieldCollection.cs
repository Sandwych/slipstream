using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Model
{
    public class FieldCollection : Dictionary<string, IField>, IFieldCollection
    {
        private IModel model;

        public FieldCollection(IModel model)
        {
            this.model = model;
        }

        public IField Integer(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Integer);
            this.Add(name, field);
            return field;
        }

        public IField BigInteger(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.BigInteger);
            this.Add(name, field);
            return field;
        }

        public IField Double(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Double);
            this.Add(name, field);
            return field;
        }

        public IField Decimal(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Decimal);
            this.Add(name, field);
            return field;
        }

        public IField Boolean(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Boolean);
            this.Add(name, field);
            return field;
        }

        public IField Chars(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Chars);
            this.Add(name, field);
            return field;
        }

        public IField Text(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Text);
            this.Add(name, field);
            return field;
        }

        public IField Xml(string name)
        {
            var field = new XmlField(this.model, name);
            this.Add(name, field);
            return field;
        }

        public IField Binary(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Binary);
            this.Add(name, field);
            return field;
        }

        public IField DateTime(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.DateTime);
            this.Add(name, field);
            return field;
        }

        public IField Date(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Date);
            this.Add(name, field);
            return field;
        }

        public IField Time(string name)
        {
            var field = new ScalarField(this.model, name, FieldType.Time);
            this.Add(name, field);
            return field;
        }

        public IField ManyToOne(string name, string masterModel)
        {
            var field = new ManyToOneField(this.model, name, masterModel);
            this.Add(name, field);
            return field;
        }

        public IField OneToMany(string name, string childModel, string relatedField)
        {
            var field = new OneToManyField(this.model, name, childModel, relatedField);
            this.Add(name, field);
            return field;
        }


        public IField ManyToMany(string name, string refModel, string originField, string targetField)
        {
            var field = new ManyToManyField(this.model, name, refModel, originField, targetField);
            this.Add(name, field);
            return field;
        }

        public IField Enumeration(
            string name, IDictionary<string, string> options)
        {
            var field = new EnumerationField(this.model, name, options);
            this.Add(name, field);
            return field;
        }

        public IField Reference(
            string name)
        {
            var field = new ReferenceField(this.model, name);
            this.Add(name, field);
            return field;
        }

    }
}
