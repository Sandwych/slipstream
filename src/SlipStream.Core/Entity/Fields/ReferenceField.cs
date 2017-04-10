using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sandwych;

namespace SlipStream.Entity
{
    internal sealed class ReferenceField : AbstractField
    {
        OnDeleteAction refAct;
        IDictionary<string, string> options;

        public ReferenceField(IEntity entity, string name)
            : base(entity, name, FieldType.Reference)
        {
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           ICollection<Dictionary<string, object>> rawRecords)
        {
            var result = new Dictionary<long, object>(rawRecords.Count());
            this.LoadAllNames(rawRecords, result);

            return result;
        }

        protected override object OnSetFieldValue(object value)
        {
            var fieldValue = (object[])value as object[];

            if (this.IsRequired && fieldValue == null)
            {
                var msg = string.Format("Field [{0}] cannot be null", this.Name);
                throw new ArgumentOutOfRangeException("value", msg);
            }

            if (fieldValue == null)
            {
                return null;
            }

            if (fieldValue.Length != 2)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            var entityName = (string)fieldValue[0];
            this.VerifyEntityName(entityName);

            var columnValue = fieldValue[0].ToString() + ':' + fieldValue[1].ToString();
            return columnValue;
        }

        private void VerifyEntityName(string entity)
        {
            if (this.options != null && !this.options.ContainsKey(entity))
            {
                throw new ArgumentOutOfRangeException(nameof(entity), "Bad entity name");
            }
        }

        private void LoadAllNames(ICollection<Dictionary<string, object>> rawRecords,
            IDictionary<long, object> result)
        {
            //从原始记录里把所有该字段的值取出
            var availableRecords =
                from r in rawRecords
                where !r[this.Name].IsNull()
                let parts = ((string)r[this.Name]).Split(':')
                select new
                {
                    SelfId = (long)r[AbstractEntity.IdFieldName],
                    Entity = parts[0],
                    RefId = long.Parse(parts[1])
                };

            var ids = new long[1];
            var fields = new string[] { "name" };
            foreach (var r in availableRecords)
            {
                this.VerifyEntityName(r.Entity);

                var masterEntity = (IEntity)this.Entity.DbDomain.GetResource(r.Entity);
                ids[0] = r.RefId;
                string name = null;
                if (masterEntity.Fields.ContainsKey("name"))
                {
                    var record = masterEntity.ReadInternal(ids, fields)[0];
                    name = (string)record["name"];
                }

                result[r.SelfId] = new object[] { r.Entity, r.RefId, name };
            }


            var nullRecords = from r in rawRecords
                              let mid = r[this.Name]
                              where mid.IsNull()
                              select (long)r[AbstractEntity.IdFieldName];
            foreach (var mid in nullRecords)
            {
                result[mid] = null;
            }
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            if (record == null || record.Count == 0)
            {
                throw new ArgumentNullException("record");
            }

            var fieldValue = (object[])record[this.Name];
            var destMetaEntityName = (string)fieldValue[0];
            var destMetaEntity = (IEntity)this.Entity.DbDomain.GetResource(destMetaEntityName);
            var destIds = new long[] { (long)fieldValue[1] };
            var destRecord = destMetaEntity.ReadInternal(destIds, null)[0];
            return new BrowsableRecord(destMetaEntity, destRecord);
        }

        public override bool IsRequired
        {
            get
            {
                if (this.OnDeleteAction == OnDeleteAction.SetNull)
                {
                    this.OnDeleteAction = OnDeleteAction.Restrict;
                }
                return base.IsRequired;
            }
        }

        public override bool IsColumn { get { return !this.IsFunctional; } }

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

        public override OnDeleteAction OnDeleteAction
        {
            get
            {
                return this.refAct;
            }
            set
            {
                this.refAct = value;
            }
        }

        public override IDictionary<string, string> Options
        {
            get
            {
                Debug.Assert(this.options != null);
                return this.options;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.options = new Dictionary<string, string>(value);
            }
        }

        public override void VerifyDefinition()
        {
            base.VerifyDefinition();

            if (this.IsRequired && OnDeleteAction == OnDeleteAction.SetNull)
            {
                throw new ArgumentException("不能同时设置为必填字段和可空");
            }
        }

    }
}
