using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using NHibernate.SqlCommand;

using SlipStream.Data;

namespace SlipStream.Entity
{
    internal sealed class ManyToManyField : AbstractField
    {
        public ManyToManyField(IEntity entity, string name,
            string refEntityName, string originField, string targetField)
            : base(entity, name, FieldType.ManyToMany)
        {

            this.Relation = refEntityName;
            this.OriginField = originField;
            this.RelatedField = targetField;
            this.Lazy = true;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           ICollection<Dictionary<string, object>> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("records");
            }

            //中间表模型
            var relationEntity = (IEntity)this.Entity.DbDomain.GetResource(this.Relation);
            var result = new Dictionary<long, object>();
            foreach (var rec in records)
            {
                var selfId = (long)rec[AbstractEntity.IdFieldName];
                //中间表没有记录，返回空
                var sql = 
                    String.Format(CultureInfo.InvariantCulture, 
                        @"select ""{0}"" from ""{1}"" where ""{2}""=?",
                        this.RelatedField, relationEntity.TableName, this.OriginField);
                var targetIds = this.Entity.DbDomain.CurrentSession
                    .DataContext.QueryAsArray<object>(sql, selfId);
                result[selfId] = targetIds.Select(o => (long)o).ToArray();
            }

            return result;
        }

        protected override object OnSetFieldValue(object value)
        {
            if (!(value is long[]))
            {
                throw new ArgumentException("'value' must be long[]", "value");
            }

            return value;
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            long[] targetIds = null;
            if (record.ContainsKey(this.Name))
            {
                targetIds = (long[])record[this.Name];
            }
            else //Lazy 的字段，我们重新读取
            {
                var id = (long)record[AbstractEntity.IdFieldName];
                var fields = new string[] { this.Name };
                var newRecord = ((Dictionary<string, object>[])this.Entity.ReadInternal(new long[] { id }, fields))[0];
                targetIds = (long[])newRecord[this.Name];
            }

            var relationEntity = (IEntity)this.Entity.DbDomain.GetResource(this.Relation);
            var targetEntityName = relationEntity.Fields[this.RelatedField].Relation;
            var targetEntity = (IEntity)this.Entity.DbDomain.GetResource(targetEntityName);
            var targetRecords = targetEntity.ReadInternal(targetIds, null);
            return targetRecords.Select(tr => new BrowsableRecord(targetEntity, tr)).ToArray();
        }

        public override bool IsColumn { get { return false; } }

        public override bool IsReadonly
        {
            get { return false; }
            set { throw new NotSupportedException(); }
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

    }
}
