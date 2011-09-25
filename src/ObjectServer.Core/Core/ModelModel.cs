using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;

using ObjectServer.Utility;
using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public class ModelModel : AbstractTableModel
    {
        public const string ModelName = "core.model";

        public ModelModel()
            : base(ModelName)
        {
            this.AutoMigration = false;

            Fields.Chars("name").SetLabel("Name").SetSize(256).Required().Unique();
            Fields.Chars("label").SetLabel("Label").SetSize(256);
            Fields.Text("info").SetLabel("Information");
            Fields.Chars("module").SetLabel("Module").SetSize(128).Required();
            Fields.OneToMany("fields", "core.field", "model").SetLabel("Fields");
        }

        /// <summary>
        /// 提供一个方便读取指定模型所有字段的方法
        /// </summary>
        /// <param name="model"></param>
        /// <param name="scope"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        [ServiceMethod]
        public static Dictionary<string, object>[] GetFields(IModel model, IServiceContext scope, string modelName)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            var destModel = (AbstractModel)scope.GetResource(modelName);

            var modelDomain = new object[] { new object[] { "name", "=", modelName } };
            var modelIds = model.SearchInternal(scope, modelDomain);

            //TODO 检查 IDS 错误，好好想一下要用数据库的字段信息还是用内存的字段信息

            var fieldModel = (IModel)scope.GetResource("core.field");
            var fieldDomain = new object[] { new object[] { "model", "=", modelIds[0] } };
            var fieldIds = fieldModel.SearchInternal(scope, fieldDomain);
            var records = fieldModel.ReadInternal(scope, fieldIds);

            var enumFields =
                from r in records
                let type = (string)r["type"]
                where type == "Enumeration"
                select r;

            foreach (var f in enumFields)
            {
                var fieldName = (string)f["name"];
                f["options"] = destModel.Fields[fieldName].Options;
            }

            return records;
        }
    }
}
