using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    public sealed class BrowsableRecord : DynamicObject
    {
        private Dictionary<string, object> record;
        private dynamic metaModel;
        private IResourceScope context;

        public BrowsableRecord(IResourceScope ctx, dynamic metaModel, Dictionary<string, object> record)
        {
            this.metaModel = metaModel;
            this.record = record;
            this.context = ctx;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotSupportedException();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (!metaModel.Fields.ContainsKey(binder.Name))
            {
                return false;
            }

            var metaField = metaModel.Fields[binder.Name];

            switch ((FieldType)metaField.Type)
            {
                case FieldType.Integer:
                case FieldType.BigInteger:
                case FieldType.Boolean:
                case FieldType.Chars:
                case FieldType.DateTime:
                case FieldType.Decimal:
                case FieldType.Text:
                case FieldType.Binary:
                case FieldType.Enumeration:
                case FieldType.Float:
                    result = this.GetScalarField(metaField);
                    break;

                case FieldType.OneToMany:
                case FieldType.ManyToMany:
                    result = this.GetOneToManyOrManyToManyField(metaField);
                    break;

                case FieldType.ManyToOne:
                    result = this.GetManyToOneField(metaField);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return true;
        }

        private object GetScalarField(IMetaField metaField)
        {
            return this.record[metaField.Name];
        }

        private object GetManyToOneField(IMetaField metaField)
        {
            var destModelName = metaField.Relation;
            dynamic destMetaModel = this.context.Database.GetResource(destModelName);
            var fieldValue = (object[])this.record[metaField.Name];
            var destIds = new object[] { fieldValue[0] };
            //查询 ManyToOne 字段
            var destRecord = destMetaModel.ReadInternal(this.context, destIds, null)[0];
            return new BrowsableRecord(this.context, destMetaModel, destRecord);
        }

        private object GetOneToManyOrManyToManyField(IMetaField metaField)
        {
            var targetModelName = metaField.Relation;
            dynamic targetModel = this.context.Database.GetResource(targetModelName);
            var thisId = this.record["id"];
            //TODO: 下面的条件可能还不够，差 active 等等
            var domain = new object[][] { new object[] { metaField.RelatedField, "=", thisId } };
            var destIds = targetModel.SearchInternal(this.context, domain, 0, 0);
            var records = (Dictionary<string, object>[])targetModel.ReadInternal(this.context, destIds, null);
            return records.Select(r => new BrowsableRecord(this.context, targetModel, r)).ToArray();
        }
    }
}
