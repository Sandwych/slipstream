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
    public abstract class AbstractModel : AbstractResource, IMetaModel
    {
        private readonly IMetaFieldCollection declaredFields =
            new MetaFieldCollection();

        public const string IdFieldName = "id";
        public const string ActiveFieldName = "_active";
        public const string VersionFieldName = "_version";

        protected AbstractModel(string name)
            : base(name)
        {
        }

        public override void Load(IDatabaseProfile db)
        {
            base.Load(db);

            //检测此模型是否存在于数据库 core_model 表
            var sql = "SELECT DISTINCT COUNT(\"id\") FROM core_model WHERE name=@0";
            var count = (long)db.DataContext.QueryValue(sql, this.Name);
            if (count <= 0)
            {
                this.CreateModel(db);
            }
        }

        private void CreateModel(IDatabaseProfile db)
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

            var model = res as IMetaModel;
            if (model != null)
            {
                //这里的字段合并策略也是添加，如果存在就直接替换
                foreach (var field in model.Fields)
                {
                    this.Fields[field.Key] = field.Value;
                }
            }
        }


        #region IModel Members

        public abstract string TableName { get; protected set; }
        public abstract bool Hierarchy { get; protected set; }
        public abstract bool CanCreate { get; protected set; }
        public abstract bool CanRead { get; protected set; }
        public abstract bool CanWrite { get; protected set; }
        public abstract bool CanDelete { get; protected set; }
        public abstract bool LogCreation { get; protected set; }
        public abstract bool LogWriting { get; protected set; }
        public abstract NameGetter NameGetter { get; protected set; }
        public abstract object[] SearchInternal(
            IResourceScope ctx, object[] domain = null, long offset = 0, long limit = 0);
        public abstract long CreateInternal(IResourceScope ctx, IDictionary<string, object> propertyBag);
        public abstract void WriteInternal(IResourceScope ctx, long id, IDictionary<string, object> record);
        public abstract Dictionary<string, object>[] ReadInternal(
            IResourceScope ctx, object[] ids, IEnumerable<string> fields = null);
        public abstract void DeleteInternal(IResourceScope ctx, IEnumerable<long> ids);
        public abstract dynamic Browse(IResourceScope ctx, long id);

        #endregion
    }
}
