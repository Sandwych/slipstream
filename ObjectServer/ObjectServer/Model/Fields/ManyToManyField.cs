using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
           IResourceScope ctx, ICollection<Dictionary<string, object>> records)
        {
            //中间表模型
            dynamic relationModel = ctx.DatabaseProfile.GetResource(this.Relation);
            var relationFields = new string[] { this.RelatedField };

            var domain = new object[][] { new object[] { this.OriginField, "=", (long)-1 } };
            var result = new Dictionary<long, object>();
            foreach (var rec in records)
            {
                var id = (long)rec["id"];
                domain[0][2] = id;
                var relIds = relationModel.SearchInternal(ctx, domain, 0, 0);

                //中间表没有记录，返回空
                if (relIds == null || relIds.Length <= 0)
                {
                    result[id] = new object[] { };
                }
                else
                {
                    //TODO 优化此处，或许应该用 SQL
                    var relationRecords = (Dictionary<string, object>[])
                        relationModel.ReadInternal(ctx, relIds, relationFields);
                    result[id] = relationRecords.Select(d => d[this.RelatedField]).ToArray();
                }
            }

            return result;
        }

        protected override object OnSetFieldValue(IResourceScope scope, object value)
        {
            throw new NotSupportedException();
        }

        public override object BrowseField(IResourceScope scope, IDictionary<string, object> record)
        {
            IEnumerable<long> targetIds = null;
            if (record.ContainsKey(this.Name))
            {
                var targetFields = (object[][])record[this.Name];
            }
            else //Lazy 的字段，我们重新读取
            {
                var id = (long)record["id"];
                var fields = new string[] { this.Name };
                var newRecord = ((Dictionary<string, object>[])this.Model.ReadInternal(scope, new long[] { id }, fields))[0];
                var m2mFields = (object[])newRecord[this.Name];
                targetIds = m2mFields.Select(tf => (long)((object[])tf)[0]);
            }

            var relationModel = (IMetaModel)scope.DatabaseProfile.GetResource(this.Relation);
            var targetModelName = relationModel.Fields[this.RelatedField].Relation;
            var targetModel = (IMetaModel)scope.DatabaseProfile.GetResource(targetModelName);
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
