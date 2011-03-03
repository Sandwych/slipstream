using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    public class BrowsableModel : DynamicObject
    {
        private Dictionary<string, object> record;
        private IModel metaModel;
        private IContext context;

        public BrowsableModel(IContext ctx, IModel metaModel, Dictionary<string, object> record)
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
            if (metaField.IsColumn() && metaField.Type != FieldType.ManyToOne)
            {
                result = record[binder.Name];
            }
            else if (metaField.Type == FieldType.ManyToOne)
            {
                result = this.GetManyToOneField(metaField);
            }
            else if (metaField.Type == FieldType.OneToMany)
            {
                result = this.GetOneToManyOrManyToManyField(metaField);
            }
            else if (metaField.Type == FieldType.ManyToMany)
            {
                result = this.GetOneToManyOrManyToManyField(metaField);
            }

            return true;
        }

        private object GetManyToOneField(IMetaField metaField)
        {
            var destModelName = metaField.Relation;
            var destMetaModel = (IModel)this.context.Database.ServiceObjects.Resolve(destModelName);
            var fieldValue = (object[])this.record[metaField.Name];
            var destIds = new object[] { fieldValue[0] };
            //查询 ManyToOne 字段
            var destRecord = destMetaModel.Read(this.context, destIds, null)[0];
            return new BrowsableModel(this.context, destMetaModel, destRecord);
        }

        private object GetOneToManyOrManyToManyField(IMetaField metaField)
        {
            var targetModelName = metaField.Relation;
            var targetModel = (IModel)this.context.Database.ServiceObjects.Resolve(targetModelName);
            var selfId = this.record["id"];
            //TODO: 下面的条件还不够，差 active 等等
            var domain = new object[][] { new object[] { metaField.RelatedField, "=", selfId } };
            var destIds = targetModel.Search(this.context, domain, 0, 0);
            var records = targetModel.Read(this.context, destIds, null);
            return records.Select(r => new BrowsableModel(this.context, targetModel, r)).ToArray();
        }
    }
}
