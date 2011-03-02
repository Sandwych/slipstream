using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectServer
{
    public static class ServiceObjectExtensions
    {
        public static string GetModuleName(this IObjectService obj)
        {
            var nameParts = obj.Name.Split('.');
            return nameParts[0];
        }

        /// <summary>
        /// 验证服务对象名字是否合法
        /// </summary>
        /// <param name="obj"></param>
        public static void VerifyName(this IObjectService obj)
        {
            //检测名称是否合适
            if (!NamingRule.IsValidServiceName(obj.Name))
            {
                var msg = string.Format("Bad service object name '{0}'", obj.Name);
                throw new BadServiceObjectNameException(msg, obj.Name);
            }
        }


    }
}
