using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public static class ModelExtensions
    {
        public static bool ContainsField(this IModel model, string fieldName)
        {
            return model.DefinedFields.Count(f => f.Name == fieldName) > 0;
        }

        public static IEnumerable<IField> GetAllStorableFields(this IModel model)
        {
            return model.DefinedFields.Where(f => f.IsStorable() && f.Name != "id");
        }

        /// <summary>
        /// 获取此模型所依赖的其它模型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string[] GetDependedModels(this IModel model)
        {
            //TODO: 处理 many2many

            //现在只取主表
            var query = from f in model.DefinedFields
                        where f.Type == FieldType.ManyToOne
                        select f.Relation;

            //自己不能依赖自己
            query = from m in query
                    where m != model.Name
                    select m;

            return query.ToArray();
        }
    }
}
