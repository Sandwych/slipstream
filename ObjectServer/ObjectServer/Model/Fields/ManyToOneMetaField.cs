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
        public ManyToOneMetaField(string name, string masterModel)
            : base(name, FieldType.ManyToOne)
        {
            this.Relation = masterModel;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IResourceScope ctx, List<Dictionary<string, object>> childRecords)
        {
            var result = new Dictionary<long, object>(childRecords.Count());
            dynamic masterModel = ctx.DatabaseProfile.GetResource(this.Relation);
            if (masterModel.ContainsField("name")) //如果有 name 字段
            {
                //从原始记录里把所有该字段的值取出
                var masterTableIds = (
                    from r in childRecords
                    where r[this.Name] != null && r[this.Name] != DBNull.Value
                    select (long)r[this.Name]);

                if (masterTableIds.Count() > 0)
                {
                    var masterNames = masterModel.NameGetter(ctx, masterTableIds);

                    foreach (var r in childRecords)
                    {
                        var id = (long)r["id"];
                        var masterField = r[this.Name];
                        if (masterField != DBNull.Value && masterField != null)
                        {
                            var masterId = (long)masterField;
                            result.Add(id, new object[2] { masterId, masterNames[masterId] });
                        }
                        else
                        {
                            result.Add(id, DBNull.Value);
                        }
                    }
                }
                else
                {
                    foreach (var r in childRecords)
                    {
                        var id = (long)r["id"];
                        result.Add(id, DBNull.Value);
                    }
                }
            }
            else
            {
                foreach (var r in childRecords)
                {
                    var id = (long)r["id"];
                    var masterId = (long)r[this.Name];
                    result.Add(id, new object[] { masterId, string.Empty });
                }
            }

            return result;
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
    }
}
