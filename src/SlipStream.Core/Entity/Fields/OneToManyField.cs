using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    internal sealed class OneToManyField : AbstractField
    {
        public OneToManyField(IEntity entity, string name, string childEntityName, string relatedField)
            : base(entity, name, FieldType.OneToMany)
        {
            this.Relation = childEntityName;
            this.RelatedField = relatedField;
            this.Lazy = true;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           ICollection<Dictionary<string, object>> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("records");
            }

            //查询字表
            IEntity childEntity = (IEntity)this.Entity.DbDomain.GetResource(this.Relation);

            var children = new Dictionary<long, long[]>();
            foreach (var master in records)
            {
                var masterID = (long)master[AbstractEntity.IdFieldName];
                var constraint = new List<object[]>();
                constraint.Add(new object[] { this.RelatedField, "=", masterID });
                var childIDs = childEntity.SearchInternal(constraint.ToArray(), null, 0, 0);
                children[masterID] = childIDs;
            }

            var result = new Dictionary<long, object>();
            foreach (var p in records)
            {
                var masterId = (long)p[AbstractEntity.IdFieldName];
                result.Add(masterId, children[masterId]);
            }

            return result;
        }

        protected override object OnSetFieldValue(object value)
        {
            throw new NotSupportedException();
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            //TODO 重构成跟Many-to-many 一样的
            var targetEntityName = this.Relation;
            IEntity targetEntity = (IEntity)this.Entity.DbDomain.GetResource(targetEntityName);
            var thisId = record[AbstractEntity.IdFieldName];
            //TODO: 下面的条件可能还不够，差 active 等等
            var constraint = new object[][] { new object[] { this.RelatedField, "=", thisId } };
            var destIds = targetEntity.SearchInternal(constraint, null, 0, 0);
            var records = (Dictionary<string, object>[])targetEntity.ReadInternal(destIds, null);
            return records.Select(r => new BrowsableRecord(targetEntity, r)).ToArray();
        }

        public override bool IsColumn { get { return false; } }

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

        public override SlipStream.Entity.OnDeleteAction OnDeleteAction
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

        public override void VerifyDefinition()
        {
            base.VerifyDefinition();
        }
    }
}
