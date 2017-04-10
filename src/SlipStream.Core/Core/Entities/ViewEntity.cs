using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Entity;

namespace SlipStream.Core
{

    [Resource]
    public sealed class ViewEntity : AbstractSqlEntity
    {

        public ViewEntity() : base("core.view")
        {

            Fields.Chars("name").WithLabel("Name").WithSize(64).WithRequired();
            Fields.Chars("entity").WithLabel("Entity").WithSize(128).WithRequired();
            Fields.Enumeration("kind",
                new Dictionary<string, string>() {
                    { "form", "Form View" },
                    { "tree", "Tree View" },
                    { "chart", "Chart View" },
                })
                .WithLabel("View Kind").WithRequired();
            Fields.Xml("layout").WithLabel("Layout");
        }

        [ServiceMethod("GetView")]
        public static Dictionary<string, object> GetView(
            IEntity entity, string entityName, string viewKind, long? viewId)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException(nameof(entityName));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(viewKind))
            {
                throw new ArgumentNullException(nameof(viewKind));
            }


            /* 获取默认视图的策略如下：
             * 1. 先看是否指定了 viewId，如果有了，就处理一下视图继承然后返回
             * 2. 如果没有视图，就自动生成一个 Quick & Dirty 的
             * 
             * */
            Dictionary<string, object> result = null;
            var destEntity = entity.DbDomain.GetResource(entityName) as IEntity;

            if (viewId != null)
            {
                var viewRecords = entity.ReadInternal(new long[] { viewId.Value }, null);
                result = viewRecords[0];
            }
            else
            {
                var constraint = new object[][] {
                    new object[] { "kind", "=", viewKind },
                    new object[] { "entity", "=", entityName },
                };

                var viewIDs = entity.SearchInternal(constraint, null, 0, 0);
                if (viewIDs != null && viewIDs.Length > 0)
                {
                    result = entity.ReadInternal(viewIDs, null)[0];
                }
                else
                {
                    string layout;
                    layout = GenerateViewByType(viewKind, destEntity);
                    result = new Dictionary<string, object>()
                    {
                        { "name", "Auto-generated view" },
                        { "entity", entityName },
                        { "kind", viewKind },
                        { "layout", layout },
                    };
                }
            }

            return result;
        }

        private static string GenerateViewByType(string viewKind, IEntity destEntity)
        {
            Debug.Assert(!string.IsNullOrEmpty(viewKind));
            Debug.Assert(destEntity != null);

            string layout;
            switch (viewKind)
            {
                case "form":
                    layout = ViewGenerator.GenerateFormView(destEntity.Fields);
                    break;

                case "tree":
                    layout = ViewGenerator.GenerateTreeView(destEntity.Fields);
                    break;

                default:
                    var msg = string.Format("Not supported view type[{0}]", viewKind);
                    throw new NotSupportedException(msg);
            }

            return layout;
        }

    }
}
