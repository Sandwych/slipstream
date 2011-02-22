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
    public abstract class ModelBase : StaticServiceObjectBase
    {
        private readonly IDictionary<string, IMetaField> declaredFields =
            new SortedList<string, IMetaField>();


        public const string IdFieldName = "id";
        public const string ActiveFieldName = "_field";
        public const string VersionFieldName = "_version";

        public override string Label { get; protected set; }
        public string Module { get; protected set; }

        protected ModelBase()
            : base()
        {
            this.AddInternalFields();
        }

        public override void Initialize(IDatabaseContext db, ObjectPool pool)
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
        }


        private void CreateModel(IDatabaseContext db)
        {
            var rowCount = db.Execute(
                "INSERT INTO core_model(name, module, label) VALUES(@0, @1, @2);",
                this.Name, this.Module, this.Label);

            if (rowCount != 1)
            {
                throw new DataException("Failed to insert record of table core_model");
            }

        }

        public override string[] ReferencedObjects
        {
            get
            {
                var query = from f in this.DefinedFields.Values
                            where f.Type == FieldType.ManyToOne
                            select f.Relation;

                //自己不能依赖自己
                query = from m in query
                        where m != this.Name
                        select m;

                return query.Distinct().ToArray();
            }
        }

        #region Field Methods

        public IDictionary<string, IMetaField> DefinedFields { get { return this.declaredFields; } }

        protected void IntegerField(string name, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;
            if (getter == null)
            {
                field = new ScalarMetaField(name, FieldType.Integer);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.Integer, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;

            declaredFields.Add(field.Name, field);
        }

        protected void BitIntegerField(string name, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;
            if (getter == null)
            {
                field = new ScalarMetaField(name, FieldType.BigInteger);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.BigInteger, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void BooleanField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;
            if (getter == null)
            {
                field = new ScalarMetaField(name, FieldType.Boolean);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.Boolean, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void TextField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;

            if (getter == null)
            {
                field = new ScalarMetaField(name, FieldType.Text);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.Text, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void CharsField(
            string name, string label, int size, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;

            if (getter == null)
            {
                field = new ScalarMetaField(name, FieldType.Chars);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.Chars, getter);
            }

            field.Label = label;
            field.Size = size;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void DateTimeField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;

            if (getter == null)
            {
                field = new ScalarMetaField(name, FieldType.DateTime);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.DateTime, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void ManyToOneField(
            string name, string masterModel, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;

            if (getter == null)
            {
                field = new ManyToOneMetaField(name);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.ManyToOne, getter);
            }
            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;
            field.Relation = masterModel;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void OneToManyField(
            string name, string childModel, string relatedField,
            string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            MetaField field;

            if (getter == null)
            {
                field = new OneToManyMetaField(name);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.OneToMany, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;
            field.Relation = childModel;
            field.RelatedField = relatedField;

            field.Validate();
            declaredFields.Add(field.Name, field);
        }

        protected void ManyToManyField(
            string name,
            string refModel, string originField, string targetField,
            string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {

            MetaField field;

            if (getter == null)
            {
                field = new ManyToManyMetaField(name, refModel, originField, targetField);
            }
            else
            {
                field = new FunctionMetaField(name, FieldType.ManyToMany, getter);
            }

            field.Label = label;
            field.Required = required;
            field.Getter = getter;
            field.DefaultProc = defaultProc;


            field.Validate();
            declaredFields.Add(field.Name, field);

        }


        #endregion


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


    }
}
