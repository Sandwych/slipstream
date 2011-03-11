using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ManyToManyMetaField : AbstractMetaField
    {
        public ManyToManyMetaField(string name,
            string refModel, string originField, string targetField)
            : base(name, FieldType.ManyToMany)
        {

            this.Relation = refModel;
            this.OriginField = originField;
            this.RelatedField = targetField;
            this.Lazy = true;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IResourceScope ctx, List<Dictionary<string, object>> records)
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

    }
}
