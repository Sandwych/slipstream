using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Utility;

namespace ObjectServer.Model
{
    internal sealed class ManyToManyField : AbstractField
    {
        public ManyToManyField(IMetaModel model, string name,
            string refModel, string originField, string targetField)
            : base(model, name, FieldType.ManyToMany)
        {

            this.Relation = refModel;
            this.OriginField = originField;
            this.RelatedField = targetField;
            this.Lazy = true;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IServiceScope ctx, ICollection<Dictionary<string, object>> records)
        {
            //中间表模型
            var relationModel = (IMetaModel)ctx.GetResource(this.Relation);
            var result = new Dictionary<long, object>();
            foreach (var rec in records)
            {
                var selfId = (long)rec["id"];

                //中间表没有记录，返回空
                var sql = string.Format(
                    "SELECT \"{0}\" FROM \"{1}\" WHERE \"{2}\" = @0",
                    this.RelatedField, relationModel.TableName, this.OriginField);
                var targetIds = ctx.DatabaseProfile.Connection.QueryAsArray(sql, selfId);
                result[selfId] = targetIds.Select(o => (long)o).ToArray();
            }

            return result;
        }

        protected override object OnSetFieldValue(IServiceScope scope, object value)
        {
            if (!(value is long[]))
            {
                throw new ArgumentException("'value' must be long[]", "value");
            }

            return value;
        }

        public override object BrowseField(IServiceScope scope, IDictionary<string, object> record)
        {
            long[] targetIds = null;
            if (record.ContainsKey(this.Name))
            {
                var targetFields = (object[][])record[this.Name];
            }
            else //Lazy 的字段，我们重新读取
            {
                var id = (long)record["id"];
                var fields = new string[] { this.Name };
                var newRecord = ((Dictionary<string, object>[])this.Model.ReadInternal(scope, new long[] { id }, fields))[0];
                targetIds = (long[])newRecord[this.Name];
            }

            var relationModel = (IMetaModel)scope.GetResource(this.Relation);
            var targetModelName = relationModel.Fields[this.RelatedField].Relation;
            var targetModel = (IMetaModel)scope.GetResource(targetModelName);
            var targetRecords = targetModel.ReadInternal(scope, targetIds);
            return targetRecords.Select(tr => new BrowsableRecord(scope, targetModel, tr)).ToArray();
        }

        public override bool IsColumn()
        {
            return false;
        }

        public override bool IsReadonly
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public override bool IsScalar
        {
            get { return false; }
        }

        public override int Size
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override ObjectServer.Model.OnDeleteAction OnDeleteAction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override IDictionary<string, string> Options
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

    }
}
