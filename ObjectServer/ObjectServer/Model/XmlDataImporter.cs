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
        private IContext context;
        private dynamic modelDataModel;
        private string currentModule;

        public XmlDataImporter(IContext ctx, string currentModule)
        {
            this.context = ctx;
            this.modelDataModel = ctx.Database.Resources[ModelDataModel.ModelName];
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
                    this.ReadRecordElement(reader);
                }
            }
        }

        private void ReadRecordElement(XmlReader reader)
        {
            var modelName = reader["model"];
            dynamic model = this.context.Database.Resources[modelName];
            var key = reader["key"];

            if (model == null)
            {
                throw new InvalidDataException("We need a fucking 'model' attribute");
            }

            var record = new Dictionary<string, object>();
            this.ReadRecordFields(reader, model, record);

            //创建记录
            var id = (long)model.Create(this.context, record);

            if (!string.IsNullOrEmpty(key))
            {
                modelDataModel.Create(this.context, this.currentModule, model.Name, key, id);
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

        private void ReadFieldElement(XmlReader reader, dynamic model, Dictionary<string, object> record)
        {
            var refKey = reader["ref-key"];
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

                case FieldType.Money:
                case FieldType.Decimal:
                    fieldValue = reader.ReadElementContentAsDecimal();
                    break;

                case FieldType.Chars:
                case FieldType.Text:
                case FieldType.Enumeration:
                    fieldValue = reader.ReadElementContentAsString();
                    break;

                case FieldType.ManyToOne:
                    if (refKey == null)
                    {
                        throw new InvalidDataException("Many-to-one field must have a 'ref-key' attribute");
                    }
                    fieldValue = model.LookupId(this.context, metaField.Name, refKey);
                    break;

                default:
                    throw new NotSupportedException();
            }
            record[metaField.Name] = fieldValue;

        }

    }
}
