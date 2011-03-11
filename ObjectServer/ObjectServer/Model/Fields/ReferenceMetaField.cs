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
            throw new NotImplementedException();
            //从原始记录里把所有该字段的值取出
            var masterTables = new Dictionary<string, List<long>>(rawRecords.Count);
            var availableRecords = from r in rawRecords
                                   where r[this.Name] != null && !(r[this.Name] is DBNull)
                                   select r[this.Name];

            foreach (var r in availableRecords)
            {
                string model;
                long id;
                var parts = ((string)r).Split(':');
                model = parts[0];
                id = long.Parse(parts[1]);

                if (masterTables.ContainsKey(model))
                {
                    var ids = masterTables[model];
                    ids.Add(id);
                }
                else
                {
                    var ids = new List<long>() { id };
                    masterTables.Add(model, ids);
                }
            }

            if (masterTables.Count > 0)
            {
                foreach (var p in masterTables)
                {
                    dynamic masterModel = ctx.DatabaseProfile.GetResource(p.Key);
                    var masterNames = masterModel.NameGetter(ctx, p.Value);

                    foreach (var r in rawRecords)
                    {
                        var id = (long)r["id"];
                        var masterId = (long)r[this.Name];
                        result.Add(id, new object[] { masterId, masterNames[masterId] });
                    }
                }
            }
            else
            {
                foreach (var r in rawRecords)
                {
                    var id = (long)r["id"];
                    result.Add(id, DBNull.Value);
                }
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
