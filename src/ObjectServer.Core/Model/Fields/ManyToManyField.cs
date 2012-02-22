using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using NHibernate.SqlCommand;

using ObjectServer.Data;

namespace ObjectServer.Model
{
    internal sealed class ManyToManyField : AbstractField
    {
        public ManyToManyField(IModel model, string name,
            string refModel, string originField, string targetField)
            : base(model, name, FieldType.ManyToMany)
        {

            this.Relation = refModel;
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
            var relationModel = (IModel)this.Model.DbDomain.GetResource(this.Relation);
            var result = new Dictionary<long, object>();
            foreach (var rec in records)
            {
                var selfId = (long)rec[AbstractModel.IdFieldName];
                //中间表没有记录，返回空
                var sql = 
                    String.Format(CultureInfo.InvariantCulture, 
                        @"select ""{0}"" from ""{1}"" where ""{2}""=?",
                        this.RelatedField, relationModel.TableName, this.OriginField);
                var targetIds = this.Model.DbDomain.CurrentSession
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
                var id = (long)record[AbstractModel.IdFieldName];
                var fields = new string[] { this.Name };
                var newRecord = ((Dictionary<string, object>[])this.Model.ReadInternal(new long[] { id }, fields))[0];
                targetIds = (long[])newRecord[this.Name];
            }

            var relationModel = (IModel)this.Model.DbDomain.GetResource(this.Relation);
            var targetModelName = relationModel.Fields[this.RelatedField].Relation;
            var targetModel = (IModel)this.Model.DbDomain.GetResource(targetModelName);
            var targetRecords = targetModel.ReadInternal(targetIds, null);
            return targetRecords.Select(tr => new BrowsableRecord(targetModel, tr)).ToArray();
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
