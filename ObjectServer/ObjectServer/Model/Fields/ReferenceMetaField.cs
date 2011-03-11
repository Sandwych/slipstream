using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ReferenceMetaField : AbstractMetaField
    {
        OnDeleteAction refAct;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="masterModel">主表对象</param>
        public ReferenceMetaField(string name, string masterModel)
            : base(name, FieldType.ManyToOne)
        {
            this.Relation = masterModel;
        }


        protected override Dictionary<long, object> OnGetFieldValues(
           IResourceScope ctx, List<Dictionary<string, object>> rawRecords)
        {
            var result = new Dictionary<long, object>(rawRecords.Count());
            this.LoadAllNames(ctx, rawRecords, result);

            return result;
        }

        private void LoadAllNames(IResourceScope ctx,
            List<Dictionary<string, object>> rawRecords,
            Dictionary<long, object> result)
        {
            var refFieldValues = rawRecords.ToDictionary(_ => (long)_["id"]);

            //从原始记录里把所有该字段的值取出
            var availableRecords =
                from r in rawRecords
                where r[this.Name] != null && r[this.Name] != DBNull.Value
                let parts = ((string)r[this.Name]).Split(':')
                select new
                {
                    SelfId = (long)r["id"],
                    Model = parts[0],
                    RefId = long.Parse(parts[1])
                };

            foreach (var r in availableRecords)
            {
                //dynamic masterModel = ctx.DatabaseProfile.GetResource(r.Model);
                refFieldValues[r.SelfId][this.Name] = new object[] { r.Model, r.RefId };
            }
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
    }
}
