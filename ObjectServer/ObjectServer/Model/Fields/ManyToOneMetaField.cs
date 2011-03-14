using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ManyToOneMetaField : AbstractMetaField
    {
        OnDeleteAction onDelete;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="masterModel">主表对象</param>
        public ManyToOneMetaField(IMetaModel model, string name, string masterModel)
            : base(model, name, FieldType.ManyToOne)
        {
            this.Relation = masterModel;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IResourceScope ctx, ICollection<Dictionary<string, object>> rawRecords)
        {
            var fields = new string[] { "name" }; //TODO 改成静态变量
            var result = new Dictionary<long, object>(rawRecords.Count());
            dynamic masterModel = ctx.DatabaseProfile.GetResource(this.Relation);
            if (masterModel.ContainsField("name")) //如果有 name 字段
            {
                var manyToOneFieldValues = rawRecords.ToDictionary(_ => (long)_["id"]);

                var availableRecords = from r in rawRecords
                                       let mid = r[this.Name]
                                       where mid != null && mid != DBNull.Value
                                       select new { MasterId = (long)mid, SelfId = (long)r["id"] };
                if (availableRecords.Count() > 0)
                {
                    var masterRecords = masterModel.ReadInternal(
                        ctx, availableRecords.Select(ar => ar.MasterId), fields);
                    var masterNames = new Dictionary<long, string>(masterRecords.Length);
                    foreach (var mr in masterRecords)
                    {
                        masterNames.Add((long)mr["id"], (string)mr["name"]);
                    }

                    foreach (var mid in availableRecords)
                    {
                        result[mid.SelfId] = new object[] { mid.MasterId, masterNames[mid.MasterId] };
                    }
                }

                var nullRecords = from r in rawRecords
                                  let mid = r[this.Name]
                                  where mid == null || mid == DBNull.Value
                                  select (long)r["id"];
                foreach (var mid in nullRecords)
                {
                    result[mid] = DBNull.Value;
                }
            }
            else
            {
                foreach (var r in rawRecords)
                {
                    var id = (long)r["id"];
                    var masterId = (long)r[this.Name];
                    result.Add(id, new object[] { masterId, string.Empty });
                }
            }

            return result;
        }


        protected override object OnSetFieldValue(IResourceScope scope, object value)
        {
            return value;
        }

        public override object BrowseField(IResourceScope scope, IDictionary<string, object> record)
        {
            var destModelName = this.Relation;
            dynamic destMetaModel = scope.DatabaseProfile.GetResource(destModelName);
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
