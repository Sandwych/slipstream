using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using ObjectServer.Exceptions;
using ObjectServer.Core;

namespace ObjectServer.Model
{
    internal sealed class XmlDataImporter
    {
        private ITransactionContext context;
        private string currentModule;

        public XmlDataImporter(ITransactionContext ctx, string currentModule)
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
            var modelName = reader["model"];
            if (string.IsNullOrEmpty(modelName))
            {
                //TODO 英文异常,详细位置
                throw new DataException("记录必须指定“model”属性");
            }

            dynamic model = this.context.GetResource(modelName);
            var key = reader["key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new DataException("记录必须指定“key”属性");
            }

            var record = new Dictionary<string, object>();
            this.ReadRecordFields(reader, model, record);
            model.ImportRecord(this.context, noUpdate, record, key);
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
            if (refKey != null)
            {
                refKey.Trim();
            }

            var refModel = reader["ref-model"] as string;
            if (refModel != null)
            {
                refModel.Trim();
            }

            var fieldName = (string)reader["name"];
            fieldName.Trim();

            if (!model.Fields.ContainsKey(fieldName))
            {
                var msg = string.Format("Cannot found field: [{0}]", fieldName);
                throw new ArgumentOutOfRangeException(msg);
            }

            IField metaField = model.Fields[fieldName];
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
                case FieldType.Enumeration:
                    fieldValue = reader.ReadElementContentAsString().Trim();
                    break;

                case FieldType.Reference:
                    if (string.IsNullOrEmpty(refKey) || string.IsNullOrEmpty(refModel))
                    {
                        throw new DataException(
                            "Reference field must have 'ref-key' and 'ref-model' attributes");
                    }
                    var recordId = ModelDataModel.TryLookupResourceId(
                        this.context.DBContext, refModel, refKey);
                    if (recordId == null)
                    {
                        var msg = string.Format(
                            "Cannot found model for reference field: {0}:{1}", refModel, refKey);
                        throw new DataException(msg);
                    }
                    fieldValue = new object[] { refModel, recordId };
                    break;

                case FieldType.ManyToOne:
                    if (string.IsNullOrEmpty(refKey))
                    {
                        throw new DataException("Many-to-one field must have a 'ref-key' attribute");
                    }
                    fieldValue = ModelDataModel.TryLookupResourceId(
                        this.context.DBContext, metaField.Relation, refKey);
                    if (fieldValue == null)
                    {
                        throw new DataException("Cannot found model for ref-key: " + refKey);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
            record[metaField.Name] = fieldValue;

        }

    }
}
