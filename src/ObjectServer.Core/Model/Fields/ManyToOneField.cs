using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ManyToOneField : AbstractField
    {
        OnDeleteAction onDelete;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="masterModel">主表对象</param>
        public ManyToOneField(IModel model, string name, string masterModel)
            : base(model, name, FieldType.ManyToOne)
        {
            this.Relation = masterModel;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IServiceContext ctx, ICollection<Dictionary<string, object>> rawRecords)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (rawRecords == null)
            {
                throw new ArgumentNullException("rawRecords");
            }

            var fields = new string[] { "name" }; //TODO 改成静态变量
            var result = new Dictionary<long, object>(rawRecords.Count());
            dynamic masterModel = ctx.GetResource(this.Relation);
            if (masterModel.ContainsField("name")) //如果有 name 字段
            {
                var manyToOneFieldValues = rawRecords.ToDictionary(_ => (long)_[AbstractModel.IDFieldName]);

                var availableRecords = from r in rawRecords
                                       let mid = r[this.Name]
                                       where !mid.IsNull()
                                       select new
                                       {
                                           MasterId = (long)mid,
                                           SelfId = (long)r[AbstractModel.IDFieldName]
                                       };

                if (availableRecords.Count() > 0)
                {
                    var masterRecords = masterModel.ReadInternal(
                        ctx, availableRecords.Select(ar => ar.MasterId).ToArray(), fields);
                    var masterNames = new Dictionary<long, string>(masterRecords.Length);
                    foreach (var mr in masterRecords)
                    {
                        masterNames.Add((long)mr[AbstractModel.IDFieldName], (string)mr["name"]);
                    }

                    foreach (var mid in availableRecords)
                    {
                        result[mid.SelfId] = new object[] { mid.MasterId, masterNames[mid.MasterId] };
                    }
                }

                var nullRecords = from r in rawRecords
                                  let mid = r[this.Name]
                                  where mid.IsNull()
                                  select (long)r[AbstractModel.IDFieldName];
                foreach (var mid in nullRecords)
                {
                    result[mid] = null;
                }
            }
            else
            {
                foreach (var r in rawRecords)
                {
                    var id = (long)r[AbstractModel.IDFieldName];
                    var masterId = (long)r[this.Name];
                    result.Add(id, new object[] { masterId, string.Empty });
                }
            }

            return result;
        }


        protected override object OnSetFieldValue(IServiceContext scope, object value)
        {
            return value;
        }

        public override object BrowseField(IServiceContext scope, IDictionary<string, object> record)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            var destModelName = this.Relation;
            dynamic destMetaModel = scope.GetResource(destModelName);
            var fieldValue = (object[])record[this.Name];
            var destIds = new long[] { (long)fieldValue[0] };
            //查询 ManyToOne 字段
            var destRecord = destMetaModel.ReadInternal(scope, destIds, null)[0];
            return new BrowsableRecord(scope, destMetaModel, destRecord);
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

        public override bool IsColumn()
        {
            return !this.IsFunctional;
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

        public override OnDeleteAction OnDeleteAction
        {
            get
            {
                return this.onDelete;
            }
            set
            {
                if (this.IsRequired && OnDeleteAction == OnDeleteAction.SetNull)
                {
                    throw new ArgumentException("不能同时设置为必填字段和可空");
                }
                this.onDelete = value;
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
