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
    public abstract class ModelBase : ServiceObject
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<IField> declaredFields =
            new List<IField>();

        public override string Label { get; protected set; }
        public string Module { get; protected set; }

        protected ModelBase()
            : base()
        {
            this.AddInternalFields();
        }

        public override void Initialize(IDatabase db, ObjectPool pool)
        {
            base.Initialize(db, pool);

            //检测此模型是否存在于数据库 core_model 表
            var sql = "SELECT DISTINCT COUNT(id) FROM core_model WHERE name=@0";
            var count = (long)db.QueryValue(sql, this.Name);
            if (count <= 0)
            {
                this.CreateModel(db);
            }
        }


        /// <summary>
        /// 注册内部字段
        /// TODO: 这里必须修改，改成让用户来添加，系统自动按字段名的约定来识别
        /// </summary>
        private void AddInternalFields()
        {
            BitIntegerField("id", "ID", true, null, null);

            //DefineField("_creator", "Creation User", "BIGINT", 1);
            //DefineField("_updator", "Last Modifiation User", "BIGINT", 1);
        }


        private void CreateModel(IDatabase db)
        {
            var rowCount = db.Execute(
                "INSERT INTO core_model(name, module, label) VALUES(@0, @1, @2);",
                this.Name, this.Module, this.Label);

            if (rowCount != 1)
            {
                throw new DataException("Failed to insert record of table core_model");
            }

        }

        #region Field Methods

        public IList<IField> DefinedFields { get { return this.declaredFields; } }

        protected void IntegerField(string name, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Integer)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            declaredFields.Add(field);
        }

        protected void BitIntegerField(string name, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.BigInteger)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void BooleanField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Boolean)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void TextField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Text)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void CharsField(
            string name, string label, int size, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Chars)
            {
                Label = label,
                Size = size,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void DateTimeField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.DateTime)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void ManyToOneField(
            string name, string masterModel, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.ManyToOne)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
                Relation = masterModel,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void OneToManyField(
            string name, string childModel, string relatedField,
            string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.OneToMany)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
                Relation = childModel,
                RelatedField = relatedField,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void ManyToManyField(
            string name, string relatedModel,
            string refTableName, string originField, string targetField,
            string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.ManyToMany)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
                Relation = refTableName,
                OriginField = targetField,
                RelatedField = originField,
            };

            field.Validate();
            declaredFields.Add(field);
        }


        #endregion


        protected void VerifyFields(IEnumerable<string> fields)
        {
            Debug.Assert(fields != null);
            var notExistedFields =
                fields.Count(fn => !this.declaredFields.Exists(f => f.Name == (string)fn));
            if (notExistedFields > 0)
            {
                throw new ArgumentException("fields");
            }

            var internalFields =
                 fields.Count(fn => this.declaredFields.Exists(f => f.Name == (string)fn && f.Internal));
            if (internalFields > 0)
            {
                throw new ArgumentException("fields");
            }
        }

    }
}
