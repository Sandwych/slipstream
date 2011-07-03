using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer.Model
{
    public abstract class AbstractModel : AbstractResource, IModel
    {
        public const string IDFieldName = "id";
        public const string VersionFieldName = "_version";
        public const string CreatedTimeFieldName = "_created_time";
        public const string ModifiedTimeFieldName = "_modified_time";
        public const string CreatedUserFieldName = "_created_user";
        public const string ModifiedUserFieldName = "_modified_user";
        public const string ActiveFieldName = "_active";

        private readonly IFieldCollection fields;


        protected AbstractModel(string name)
            : base(name)
        {
            this.AutoMigration = true;
            this.fields = new FieldCollection(this);
            this.Inheritances = new InheritanceCollection();

            this.RegisterInternalServiceMethods();
            this.AddInternalFields();
        }

        /// <summary>
        /// 此函数要允许多次调用
        /// </summary>
        /// <param name="db"></param>
        public override void Load(IDBProfile db)
        {            
            base.Load(db);

            this.InitializeInheritances(db);

            this.SyncModel(db);
        }


        /// <summary>
        /// 注册内部服务，每个模型都有
        /// </summary>
        private void RegisterInternalServiceMethods()
        {
            var selfType = typeof(AbstractModel);
            this.RegisterServiceMethod(selfType.GetMethod("Search"));
            this.RegisterServiceMethod(selfType.GetMethod("Create"));
            this.RegisterServiceMethod(selfType.GetMethod("Read"));
            this.RegisterServiceMethod(selfType.GetMethod("Write"));
            this.RegisterServiceMethod(selfType.GetMethod("Delete"));
        }

        private void AddInternalFields()
        {
            Debug.Assert(!this.Fields.ContainsKey(IDFieldName));

            Fields.BigInteger(IDFieldName).SetLabel("ID").Required();
        }

        /// <summary>
        /// 初始化继承设置
        /// </summary>
        /// <param name="db"></param>
        private void InitializeInheritances(IDBProfile db)
        {
            //验证继承声明
            //这里可以安全地访问 many-to-one 指向的 ResourceContainer 里的对象，因为依赖排序的原因
            //被指向的对象肯定已经更早注册了
            foreach (var ii in this.Inheritances)
            {
                if (!db.ContainsResource(ii.BaseModel))
                {
                    var msg = string.Format(
                        "Cannot found the base model '{0}' in inheritances", ii.BaseModel);
                    throw new ResourceNotFoundException(msg, ii.BaseModel);
                }

                if (!this.Fields.ContainsKey(ii.RelatedField))
                {
                    throw new FieldAccessException();
                }

                //把“基类”模型的字段引用复制过来
                var baseModel = (IModel)db.GetResource(ii.BaseModel);
                foreach (var baseField in baseModel.Fields)
                {
                    if (!this.Fields.ContainsKey(baseField.Key))
                    {
                        var imf = new InheritedField(this, baseField.Value);
                        this.Fields.Add(baseField.Key, imf);
                    }
                }

            }
        }

        #region Inheritance staff

        public ICollection<InheritanceInfo> Inheritances { get; private set; }

        protected AbstractModel Inherit(string modelName, string relatedField)
        {
            var ii = new InheritanceInfo(modelName, relatedField);
            if (this.Inheritances.Select(i => i.BaseModel).Contains(modelName))
            {
                var msg = string.Format("Duplicated inheritance: '{0}'", modelName);
                throw new ArgumentException(msg, modelName);
            }

            this.Inheritances.Add(ii);

            return this;
        }

        #endregion

        /// <summary>
        /// 同步代码定义的模型到数据库
        /// </summary>
        /// <param name="db"></param>
        private void SyncModel(IDBProfile db)
        {

            //检测此模型是否存在于数据库 core_model 表
            var modelId = this.FindExistedModelInDb(db);

            if (modelId == null)
            {
                this.CreateModel(db);
                modelId = this.FindExistedModelInDb(db);
            }

            this.SyncFields(db, modelId);
        }

        /// <summary>
        /// 同步代码定义的字段到数据库
        /// </summary>
        /// <param name="db"></param>
        /// <param name="modelId"></param>
        private void SyncFields(IDBProfile db, long? modelId)
        {
            //同步代码定义的字段与数据库 core_model_field 表里记录的字段信息
            var sql = @"
SELECT * FROM ""core_field""  WHERE ""module""=@0 AND ""model""=@1
";
            var dbFields = db.Connection.QueryAsDictionary(sql, this.Module, modelId.Value);
            var dbFieldsNames = (from f in dbFields select (string)f["name"]).ToArray();

            //先插入代码定义了，但数据库不存在的            
            sql = @"
INSERT INTO ""core_field""(""module"", ""model"", ""name"", ""relation"", ""label"", ""type"", ""help"") 
    VALUES(@0, @1, @2, @3, @4, @5, @6)";
            var fieldsToAppend = this.Fields.Keys.Except(dbFieldsNames);
            foreach (var fieldName in fieldsToAppend)
            {
                var field = this.Fields[fieldName];
                db.Connection.Execute(sql,
                    this.Module, modelId.Value, fieldName, field.Relation, field.Label, field.Type.ToString(), "");
            }

            //删除数据库存在，但代码未定义的
            var fieldsToDelete = dbFieldsNames.Except(this.Fields.Keys);
            sql = @"DELETE FROM ""core_field"" WHERE ""name""=@0 AND ""module""=@1 AND ""model""=@2";
            foreach (var f in fieldsToDelete)
            {
                db.Connection.Execute(sql, f, this.Module, modelId.Value);
            }

            //更新现存的（交集）
            var fieldsToUpdate = dbFieldsNames.Intersect(this.Fields.Keys);
            foreach (var dbField in dbFields)
            {
                var fieldName = (string)dbField["name"];

                if (fieldsToUpdate.Contains(fieldName))
                {
                    SyncSingleField(db, dbField, fieldName);
                }
            }
        }

        /// <summary>
        /// 同步单个字段
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="dbField"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private void SyncSingleField(IDBProfile db, Dictionary<string, object> dbField, string fieldName)
        {
            var sql = @"UPDATE ""core_field"" SET ""type""=@0, ""relation""=@1, ""label""=@2, ""help""=@3  WHERE ""id""=@4";

            var fieldLabel = dbField["label"] is DBNull ? null : (string)dbField["label"];
            var fieldRelation = dbField["relation"] is DBNull ? null : (string)dbField["relation"];
            var fieldHelp = dbField["help"] is DBNull ? null : (string)dbField["help"];
            var fieldType = (string)dbField["type"];
            var fieldId = (long)dbField["id"];

            var metaField = this.Fields[fieldName];
            var metaFieldType = metaField.Type.ToString();
            if (fieldLabel != metaField.Label ||
                fieldRelation != metaField.Relation ||
                fieldType != metaFieldType ||
                fieldHelp != metaField.Help)
            {
                db.Connection.Execute(sql, metaFieldType, metaField.Relation, metaField.Label, metaField.Help, fieldId);

            }
        }

        private long? FindExistedModelInDb(IDBProfile db)
        {
            var sql = "SELECT MAX(\"id\") FROM core_model WHERE name=@0";
            var o = db.Connection.QueryValue(sql, this.Name);
            if (o is DBNull)
            {
                return null;
            }
            else
            {
                return (long)o;
            }
        }

        private void CreateModel(IDBProfile db)
        {
            var rowCount = db.Connection.Execute(
                "INSERT INTO \"core_model\"(\"name\", \"module\", \"label\") VALUES(@0, @1, @2);",
                this.Name, this.Module, this.Label);

            if (rowCount != 1)
            {
                throw new DataException("Failed to insert record of table core_model");
            }

            var modelId = (long)db.Connection.QueryValue(
                "SELECT MAX(id) FROM core_model WHERE name = @0 AND module = @1", this.Name, this.Module);

            //插入一笔到 core_model_data 方便以后导入时引用
            var key = "model_" + this.Name.Replace('.', '_');
            Core.ModelDataModel.Create(db.Connection, this.Module, Core.ModelModel.ModelName, key, modelId);
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

        public IFieldCollection Fields { get { return this.fields; } }

        protected void VerifyFields(IEnumerable<string> fields)
        {
            Debug.Assert(fields != null);
            var notExistedFields =
                fields.Count(fn => !this.fields.ContainsKey(fn));
            if (notExistedFields > 0)
            {
                throw new ArgumentException("Bad field name", "fields");
            }
        }

        public bool ContainsField(string fieldName)
        {
            return this.Fields.ContainsKey(fieldName);
        }

        public IField[] GetAllStorableFields()
        {
            return this.Fields.Values.Where(f => f.IsColumn() && f.Name != "id").ToArray();
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



        #region Service Methods

        [ServiceMethod]
        public static long[] Search(
            IModel model, IServiceScope ctx, object[] domain = null, object[] order = null, long offset = 0, long limit = 0)
        {
            OrderExpression[] orderInfos = OrderExpression.GetDefaultOrders();

            if (order != null)
            {
                orderInfos = new OrderExpression[order.Length];
                for (int i = 0; i < orderInfos.Length; i++)
                {
                    var orderTuple = (object[])order[i];
                    var so = SortDirectionParser.Parser((string)orderTuple[1]);
                    orderInfos[i] = new OrderExpression((string)orderTuple[0], so);
                }
            }

            return model.SearchInternal(ctx, domain, orderInfos, offset, limit);
        }

        [ServiceMethod]
        public static Dictionary<string, object>[] Read(
            IModel model, IServiceScope ctx, object[] clientIds, object[] fields = null)
        {
            string[] strFields = null;
            if (fields != null)
            {
                strFields = fields.Select(f => (string)f).ToArray();
            }
            var ids = clientIds.Select(id => (long)id).ToArray();
            return model.ReadInternal(ctx, ids, strFields);
        }

        [ServiceMethod]
        public static long Create(
            IModel model, IServiceScope ctx, IDictionary<string, object> propertyBag)
        {
            return model.CreateInternal(ctx, propertyBag);
        }

        [ServiceMethod]
        public static void Write(
           IModel model, IServiceScope ctx, object id, IDictionary<string, object> userRecord)
        {
            model.WriteInternal(ctx, (long)id, userRecord);
        }

        [ServiceMethod]
        public static void Delete(
            IModel model, IServiceScope ctx, object[] ids)
        {
            var longIds = ids.Select(id => (long)id).ToArray();
            model.DeleteInternal(ctx, longIds);
        }

        [ServiceMethod]
        public static Dictionary<string, object> GetDefaultView(IModel self, string viewType = "form")
        {
            throw new NotImplementedException();
        }


        #endregion


        #region IModel Members

        public abstract string TableName { get; protected set; }
        public abstract bool Hierarchy { get; protected set; }
        public abstract bool CanCreate { get; protected set; }
        public abstract bool CanRead { get; protected set; }
        public abstract bool CanWrite { get; protected set; }
        public abstract bool CanDelete { get; protected set; }

        public bool AutoMigration { get; protected set; }

        public abstract bool LogCreation { get; protected set; }
        public abstract bool LogWriting { get; protected set; }


        public abstract NameGetter NameGetter { get; protected set; }
        public abstract long[] SearchInternal(
            IServiceScope scope, object[] domain = null, OrderExpression[] orders = null, long offset = 0, long limit = 0);
        public abstract long CreateInternal(
            IServiceScope scope, IDictionary<string, object> propertyBag);
        public abstract void WriteInternal(
            IServiceScope scope, long id, IDictionary<string, object> record);
        public abstract Dictionary<string, object>[] ReadInternal(
            IServiceScope scope, long[] ids, string[] requiredFields = null);
        public abstract void DeleteInternal(IServiceScope ctx, long[] ids);
        public abstract dynamic Browse(IServiceScope ctx, long id);

        #endregion
    }
}
