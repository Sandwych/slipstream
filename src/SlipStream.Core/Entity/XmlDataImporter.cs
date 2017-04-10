using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using SlipStream.Exceptions;
using SlipStream.Core;

namespace SlipStream.Entity
{
    internal sealed class XmlDataImporter
    {
        private IServiceContext context;
        private string currentModule;

        public XmlDataImporter(IServiceContext ctx, string currentModule)
        {
            this.context = ctx;
            this.currentModule = currentModule;
        }

        public void Import(string inputFile)
        {
            using (var fs = File.OpenRead(inputFile))
            using (var bs = new BufferedStream(fs))
            using (var reader = XmlReader.Create(bs))
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
            var entityName = reader["entity"];
            if (string.IsNullOrEmpty(entityName))
            {
                //TODO 英文异常,详细位置
                throw new DataException("记录必须指定 entity 属性");
            }

            dynamic entity = this.context.GetResource(entityName);
            var key = reader["key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new DataException("记录必须指定“key”属性");
            }

            var record = new Dictionary<string, object>();
            this.ReadRecordFields(reader, entity, record);
            entity.ImportRecord(noUpdate, record, key);
        }

        private void ReadRecordFields(XmlReader reader, dynamic entity, Dictionary<string, object> record)
        {
            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "field")
                {
                    this.ReadFieldElement(reader, entity, record);
                }
            }
        }

        private void ReadFieldElement(
            XmlReader reader, dynamic entity, Dictionary<string, object> record)
        {
            var refKey = reader["ref-key"] as string;
            if (refKey != null)
            {
                refKey.Trim();
            }

            var refEntity = reader["ref-entity"] as string;
            if (refEntity != null)
            {
                refEntity.Trim();
            }

            var fieldName = (string)reader["name"];
            fieldName.Trim();

            if (!entity.Fields.ContainsKey(fieldName))
            {
                var msg = string.Format("Cannot found field: [{0}]", fieldName);
                throw new ArgumentOutOfRangeException(msg);
            }

            IField metaField = entity.Fields[fieldName];
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

                case FieldType.Double:
                    fieldValue = reader.ReadElementContentAsDouble();
                    break;

                case FieldType.DateTime:
                case FieldType.Date:
                case FieldType.Time:
                    fieldValue = reader.ReadElementContentAsDateTime();
                    break;

                case FieldType.Decimal:
                    fieldValue = reader.ReadElementContentAsDecimal();
                    break;

                case FieldType.Chars:
                case FieldType.Text:
                case FieldType.Xml:
                case FieldType.Enumeration:
                    fieldValue = reader.ReadElementContentAsString().Trim();
                    break;

                case FieldType.Reference:
                    if (string.IsNullOrEmpty(refKey) || string.IsNullOrEmpty(refEntity))
                    {
                        throw new DataException(
                            "Reference field must have 'ref-key' and 'ref-entity' attributes");
                    }
                    var recordId = EntityDataEntity.TryLookupResourceId(
                        this.context.DataContext, refEntity, refKey);
                    if (recordId == null)
                    {
                        var msg = string.Format(
                            "Cannot found entity for reference field: {0}:{1}", refEntity, refKey);
                        throw new DataException(msg);
                    }
                    fieldValue = new object[] { refEntity, recordId };
                    break;

                case FieldType.ManyToOne:
                    if (string.IsNullOrEmpty(refKey))
                    {
                        throw new DataException("Many-to-one field must have a 'ref-key' attribute");
                    }
                    fieldValue = EntityDataEntity.TryLookupResourceId(
                        this.context.DataContext, metaField.Relation, refKey);
                    if (fieldValue == null)
                    {
                        throw new DataException("Cannot found entity for ref-key: " + refKey);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
            record[metaField.Name] = fieldValue;

        }

    }
}
