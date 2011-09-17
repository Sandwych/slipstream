using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer.Model
{
    internal sealed class ReferenceField : AbstractField
    {
        OnDeleteAction refAct;
        IDictionary<string, string> options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="masterModel">主表对象</param>
        public ReferenceField(IModel model, string name)
            : base(model, name, FieldType.Reference)
        {
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IServiceContext scope, ICollection<Dictionary<string, object>> rawRecords)
        {
            var result = new Dictionary<long, object>(rawRecords.Count());
            this.LoadAllNames(scope, rawRecords, result);

            return result;
        }

        protected override object OnSetFieldValue(IServiceContext scope, object value)
        {
            var fieldValue = (object[])value;
            if (fieldValue.Length != 2)
            {
                throw new ArgumentException("value");
            }

            var modelName = (string)fieldValue[0];
            this.VerifyModelName(modelName);

            var columnValue = fieldValue[0].ToString() + ':' + fieldValue[1].ToString();
            return columnValue;
        }

        private void VerifyModelName(string modelName)
        {
            if (this.options != null && !this.options.ContainsKey(modelName))
            {
                throw new ArgumentOutOfRangeException("value", "Bad model name");
            }
        }

        private void LoadAllNames(IServiceContext ctx,
            ICollection<Dictionary<string, object>> rawRecords,
            IDictionary<long, object> result)
        {
            //从原始记录里把所有该字段的值取出
            var availableRecords =
                from r in rawRecords
                where !r[this.Name].IsNull()
                let parts = ((string)r[this.Name]).Split(':')
                select new
                {
                    SelfId = (long)r[AbstractModel.IDFieldName],
                    Model = parts[0],
                    RefId = long.Parse(parts[1])
                };

            var ids = new long[1];
            var fields = new string[] { "name" };
            foreach (var r in availableRecords)
            {
                this.VerifyModelName(r.Model);

                var masterModel = (IModel)ctx.GetResource(r.Model);
                ids[0] = r.RefId;
                string name = null;
                if (masterModel.Fields.ContainsKey("name"))
                {
                    var record = masterModel.ReadInternal(ctx, ids, fields)[0];
                    name = (string)record["name"];
                }

                result[r.SelfId] = new object[] { r.Model, r.RefId, name };
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

        public override object BrowseField(IServiceContext scope, IDictionary<string, object> record)
        {
            var fieldValue = (object[])record[this.Name];
            var destModelName = (string)fieldValue[0];
            var destMetaModel = (IModel)scope.GetResource(destModelName);
            var destIds = new long[] { (long)fieldValue[1] };
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
                return this.refAct;
            }
            set
            {
                if (this.IsRequired && OnDeleteAction == OnDeleteAction.SetNull)
                {
                    throw new ArgumentException("不能同时设置为必填字段和可空");
                }
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

    }
}
