using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer.Model
{
    public abstract class ModelBase : ResourceBase
    {
        private readonly IMetaFieldCollection declaredFields =
            new MetaFieldCollection();


        public const string IdFieldName = "id";
        public const string ActiveFieldName = "_field";
        public const string VersionFieldName = "_version";

        protected ModelBase(string name)
            : base(name)
        {
            this.AddInternalFields();
        }

        public override void Load(IDatabase db)
        {
            //检测此模型是否存在于数据库 core_model 表
            var sql = "SELECT DISTINCT COUNT(\"id\") FROM core_model WHERE name=@0";
            var count = (long)db.DataContext.QueryValue(sql, this.Name);
            if (count <= 0)
            {
                this.CreateModel(db);
            }
        }



        /// <summary>
        /// 注册内部字段
        /// </summary>
        private void AddInternalFields()
        {
            Fields.BigInteger("id").SetLabel("ID").Required();
        }


        private void CreateModel(IDatabase db)
        {
            var rowCount = db.DataContext.Execute(
                "INSERT INTO \"core_model\"(\"name\", \"module\", \"label\") VALUES(@0, @1, @2);",
                this.Name, this.Module, this.Label);

            if (rowCount != 1)
            {
                throw new DataException("Failed to insert record of table core_model");
            }

        }

        /// <summary>
        /// 获取此对象以来的所有其他对象名称
        /// 这里处理的很简单，就是直接检测 many-to-one 的对象
        /// </summary>
        /// <returns></returns>
        public override string[] GetReferencedObjects()
        {
            var query =
                from f in this.Fields.Values
                where f.Type == FieldType.ManyToOne
                select f.Relation;

            //自己不能依赖自己
            query = from m in query
                    where m != this.Name
                    select m;

            return query.Distinct().ToArray();
        }

        public IMetaFieldCollection Fields { get { return this.declaredFields; } }

        protected void VerifyFields(IEnumerable<string> fields)
        {
            Debug.Assert(fields != null);
            var notExistedFields =
                fields.Count(fn => !this.declaredFields.ContainsKey(fn));
            if (notExistedFields > 0)
            {
                throw new ArgumentException("Bad field name", "fields");
            }
        }

        public bool ContainsField(string fieldName)
        {
            return this.Fields.ContainsKey(fieldName);
        }

        public IEnumerable<IMetaField> GetAllStorableFields()
        {
            return this.Fields.Values.Where(f => f.IsColumn() && f.Name != "id");
        }

        public override void MergeFrom(IResource res)
        {
            base.MergeFrom(res);

            var model = res as IModel;
            if (model != null)
            {
                //这里的字段合并策略也是添加，如果存在就直接替换
                foreach (var field in model.Fields)
                {
                    this.Fields[field.Key] = field.Value;
                }
            }
        }

    }
}
