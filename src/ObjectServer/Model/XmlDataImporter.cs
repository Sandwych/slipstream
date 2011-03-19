using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using ObjectServer.Core;

namespace ObjectServer.Model
{
    public sealed class XmlDataImporter
    {
        private IResourceScope context;
        private string currentModule;

        public XmlDataImporter(IResourceScope ctx, string currentModule)
        {
            this.context = ctx;
            this.currentModule = currentModule;
        }

        public void Import(string inputFile)
        {
            using (var fs = File.OpenRead(inputFile))
            using (var reader = XmlReader.Create(fs))
            {
                this.ImportInternal(reader);
            }
        }

        public void Import(Stream input)
        {
            using (var reader = XmlReader.Create(input))
            {
                this.ImportInternal(reader);
            }
        }

        public void Import(TextReader input)
        {
            using (var reader = XmlReader.Create(input))
            {
                this.ImportInternal(reader);
            }
        }

        public void Import(XmlReader input)
        {
            this.ImportInternal(input);
        }

        private void ImportInternal(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
                {
                    this.ReadDataElement(reader);
                }
            }
        }

        private void ReadDataElement(XmlReader reader)
        {
            bool noUpdate = false;
            if (!string.IsNullOrEmpty(reader["noupdate"]))
            {
                noUpdate = bool.Parse(reader["noupdate"]);
            }

            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "record")
                {
                    this.ReadRecordElement(reader, noUpdate);
                }
            }
        }

        private void ReadRecordElement(XmlReader reader, bool noUpdate)
        {
            var modelName = reader["model"];
            var model = (IMetaModel)this.context.GetResource(modelName);
            var key = reader["key"];

            if (model == null)
            {
                throw new InvalidDataException("We need a fucking 'model' attribute");
            }

            var record = new Dictionary<string, object>();
            this.ReadRecordFields(reader, model, record);

            this.ImportRecord(noUpdate, model, record, key);
        }

        private void ImportRecord(
            bool noUpdate, IMetaModel model, Dictionary<string, object> record, string key = null)
        {
            //查找 key 指定的记录看是否存在
            long? existedId = null;
            if (!string.IsNullOrEmpty(key))
            {
                existedId = ModelDataModel.TryLookupResourceId(
                    this.context.DatabaseProfile.DataContext, model.Name, key);
            }

            if (existedId == null) // Create
            {
                existedId = (long)model.CreateInternal(this.context, record);
                if (!string.IsNullOrEmpty(key))
                {
                    ModelDataModel.Create(
                        this.context.DatabaseProfile.DataContext, this.currentModule, model.Name, key, existedId.Value);
                }
            }
            else if (existedId != null && !noUpdate) //Update 
            {
                if (model.Fields.ContainsKey(AbstractModel.VersionFieldName)) //处理版本
                {
                    var fields = new string[] { AbstractModel.VersionFieldName };
                    var read = model.ReadInternal(this.context, new long[] { existedId.Value }, fields)[0];
                    var version = (long)read[AbstractModel.VersionFieldName];
                    record[AbstractModel.VersionFieldName] = version + 1;
                }

                model.WriteInternal(this.context, existedId.Value, record);
                ModelDataModel.UpdateResourceId(
                    this.context.DatabaseProfile.DataContext, model.Name, key, existedId.Value);
            }
            else
            {
                //忽略此记录
            }
        }

        private void ReadRecordFields(XmlReader reader, dynamic model, Dictionary<string, object> record)
        {
            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "field")
                {
                    this.ReadFieldElement(reader, model, record);
                }
            }
        }

        private void ReadFieldElement(
            XmlReader reader, dynamic model, Dictionary<string, object> record)
        {
            var refKey = reader["ref-key"] as string;
            var fieldName = reader["name"];

            IMetaField metaField = model.Fields[fieldName];
            object fieldValue = null;
            switch (metaField.Type)
            {
                case FieldType.BigInteger:
                    fieldValue = reader.ReadElementContentAsLong();
                    break;

                case FieldType.Integer:
                    fieldValue = reader.ReadElementContentAsInt();
                    break;

                case FieldType.Boolean:
                    fieldValue = reader.ReadElementContentAsBoolean();
                    break;

                case FieldType.Float:
                    fieldValue = reader.ReadElementContentAsDouble();
                    break;

                case FieldType.DateTime:
                    fieldValue = reader.ReadElementContentAsDateTime();
                    break;

                case FieldType.Decimal:
                    fieldValue = reader.ReadElementContentAsDecimal();
                    break;

                case FieldType.Chars:
                case FieldType.Text:
                case FieldType.Enumeration:
                    fieldValue = reader.ReadElementContentAsString();
                    break;

                case FieldType.ManyToOne:
                    if (string.IsNullOrEmpty(refKey))
                    {
                        throw new InvalidDataException("Many-to-one field must have a 'ref-key' attribute");
                    }
                    fieldValue = ModelDataModel.TryLookupResourceId(
                        this.context.DatabaseProfile.DataContext, metaField.Relation, refKey);
                    if (fieldValue == null)
                    {
                        throw new InvalidDataException("Cannot found model for ref-key: " + refKey);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
            record[metaField.Name] = fieldValue;

        }

    }
}
