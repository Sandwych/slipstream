using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    //TODO 处理 lazy 的字段
    public sealed class BrowsableRecord : DynamicObject
    {
        private IDictionary<string, object> record;
        private IMetaModel metaModel;
        private IResourceScope context;

        public BrowsableRecord(IResourceScope ctx, dynamic metaModel, long id)
        {
            this.metaModel = (IMetaModel)metaModel;
            this.context = ctx;
            this.record = metaModel.ReadInternal(ctx, new long[] { id }, null)[0];
        }

        public BrowsableRecord(IResourceScope ctx, dynamic metaModel, IDictionary<string, object> record)
        {
            this.metaModel = (IMetaModel)metaModel;
            this.context = ctx;
            this.record = record;
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
                    result = this.GetOneToManyField(metaField);
                    break;

                case FieldType.ManyToMany:
                    result = this.GetManyToManyField(metaField);
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
            dynamic destMetaModel = this.context.DatabaseProfile.GetResource(destModelName);
            var fieldValue = (object[])this.record[metaField.Name];
            var destIds = new long[] { (long)fieldValue[0] };
            //查询 ManyToOne 字段
            var destRecord = destMetaModel.ReadInternal(this.context, destIds, null)[0];
            return new BrowsableRecord(this.context, destMetaModel, destRecord);
        }

        private object GetOneToManyField(IMetaField metaField)
        {
            //TODO 重构成跟Many-to-many 一样的
            var targetModelName = metaField.Relation;
            dynamic targetModel = this.context.DatabaseProfile.GetResource(targetModelName);
            var thisId = this.record["id"];
            //TODO: 下面的条件可能还不够，差 active 等等
            var domain = new object[][] { new object[] { metaField.RelatedField, "=", thisId } };
            var destIds = targetModel.SearchInternal(this.context, domain, 0, 0);
            var records = (Dictionary<string, object>[])targetModel.ReadInternal(this.context, destIds, null);
            return records.Select(r => new BrowsableRecord(this.context, targetModel, r)).ToArray();
        }

        private object GetManyToManyField(IMetaField metaField)
        {
            IEnumerable<long> targetIds = null;
            if (this.record.ContainsKey(metaField.Name))
            {
                var targetFields = (object[][])record[metaField.Name];
            }
            else //Lazy 的字段，我们重新读取
            {
                var id = (long)this.record["id"];
                var fields = new string[] { metaField.Name };
                var newRecord = ((Dictionary<string, object>[])this.metaModel.ReadInternal(this.context, new long[] { id }, fields))[0];
                var m2mFields = (object[])newRecord[metaField.Name];
                targetIds = m2mFields.Select(tf => (long)((object[])tf)[0]);
            }

            var relationModel = (IMetaModel)this.context.DatabaseProfile.GetResource(metaField.Relation);
            var targetModelName = relationModel.Fields[metaField.RelatedField].Relation;
            var targetModel = (IMetaModel)this.context.DatabaseProfile.GetResource(targetModelName);
            var targetRecords = targetModel.ReadInternal(this.context, targetIds);
            return targetRecords.Select(tr => new BrowsableRecord(this.context, targetModel, tr)).ToArray();
        }
    }
}
