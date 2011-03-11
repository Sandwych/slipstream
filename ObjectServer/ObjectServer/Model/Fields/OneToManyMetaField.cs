using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class OneToManyMetaField : AbstractMetaField
    {
        public OneToManyMetaField(string name, string childModel, string relatedField)
            : base(name, FieldType.OneToMany)
        {
            this.Relation = childModel;
            this.RelatedField = relatedField;
            this.Lazy = true;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IResourceScope ctx, List<Dictionary<string, object>> records)
        {
            //查询字表
            dynamic childModel = ctx.DatabaseProfile.GetResource(this.Relation);
            //TODO 权限等处理

            var children = new Dictionary<long, object[]>();
            foreach (var master in records)
            {
                var masterId = (long)master["id"];
                var domain = new List<object[]>();
                domain.Add(new object[] { this.RelatedField, "=", masterId });
                var childIds = childModel.SearchInternal(ctx, domain.ToArray(), 0, 0xffff);
                children[masterId] = childIds;
            }


            var result = new Dictionary<long, object>();
            foreach (var p in records)
            {
                var masterId = (long)p["id"];
                result.Add(masterId, children[masterId]);
            }

            return result;
        }

        public override bool IsColumn()
        {
            return false;
        }

        public override bool IsReadonly
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException();
            }
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

        public override bool IsScalar
        {
            get { return false; }
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
