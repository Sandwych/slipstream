using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sandwych;

namespace SlipStream.Entity
{
    internal sealed class ManyToOneField : AbstractField
    {
        OnDeleteAction _onDeleteAction;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="masterEntity">主表对象</param>
        public ManyToOneField(IEntity entity, string name, string masterEntity)
            : base(entity, name, FieldType.ManyToOne)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(masterEntity))
            {
                throw new ArgumentNullException(nameof(masterEntity));
            }

            this.Relation = masterEntity;
            this.OnDeleteAction = OnDeleteAction.Cascade;
            this.WithRequired();
        }

        protected override Dictionary<long, object> OnGetFieldValues(
            ICollection<Dictionary<string, object>> rawRecords)
        {
            if (rawRecords == null)
            {
                throw new ArgumentNullException(nameof(rawRecords));
            }

            var fields = new string[] { "name" }; //TODO 改成静态变量
            var result = new Dictionary<long, object>(rawRecords.Count());
            dynamic masterEntity = this.Entity.DbDomain.GetResource(this.Relation);
            if (masterEntity.ContainsField("name")) //如果有 name 字段
            {
                var manyToOneFieldValues = rawRecords.ToDictionary(_ => (long)_[AbstractEntity.IdFieldName]);

                var availableRecords =
                    from r in rawRecords
                    let mid = r[this.Name]
                    where !mid.IsNull()
                    select new
                    {
                        MasterId = (long)mid,
                        SelfId = (long)r[AbstractEntity.IdFieldName]
                    };

                if (availableRecords.Any())
                {
                    var masterRecords = masterEntity.ReadInternal(availableRecords.Select(ar => ar.MasterId).ToArray(), fields);
                    var masterNames = new Dictionary<long, string>(masterRecords.Length);
                    foreach (var mr in masterRecords)
                    {
                        masterNames.Add((long)mr[AbstractEntity.IdFieldName], (string)mr["name"]);
                    }

                    foreach (var mid in availableRecords)
                    {
                        result[mid.SelfId] = new object[] { mid.MasterId, masterNames[mid.MasterId] };
                    }
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
            else
            {
                foreach (var r in rawRecords)
                {
                    var id = (long)r[AbstractEntity.IdFieldName];
                    var masterId = (long)r[this.Name];
                    result.Add(id, new object[] { masterId, string.Empty });
                }
            }

            return result;
        }


        protected override object OnSetFieldValue(object value)
        {
            return value;
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            object fieldValueObj;
            if (!record.TryGetValue(this.Name, out fieldValueObj) || fieldValueObj == null)
            {
                throw new Exceptions.ResourceException(
                    "Unable to get field value: " + this.Entity.Name + "." + this.Name);
            }

            long[] destIds = null;
            if (fieldValueObj is long || fieldValueObj is long?)
            {
                destIds = new long[] { (long)fieldValueObj };
            }
            else if (fieldValueObj is object[])
            {
                var destIdObj = ((object[])fieldValueObj).First();
                destIds = new long[] { (long)destIdObj };
            }
            else
            {
                throw new NotSupportedException();
            }

            var targetEntityName = this.Relation;
            dynamic targetMetaEntity = this.Entity.DbDomain.GetResource(targetEntityName);
            //查询 ManyToOne 字段
            var destRecord = targetMetaEntity.Read(destIds, null)[0];
            return new BrowsableRecord(targetMetaEntity, destRecord);
        }

        public override bool IsRequired
        {
            get
            {
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
                return this._onDeleteAction;
            }
            set
            {
                this._onDeleteAction = value;
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

            if (this.IsRequired && this.OnDeleteAction == SlipStream.Entity.OnDeleteAction.SetNull)
            {
                throw new Exceptions.ResourceException(
                    String.Format("Field [{0}] can not set to be OnDeleteAction.SetNull, because it's required.", this));
            }
        }
    }
}
