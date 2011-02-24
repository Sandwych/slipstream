using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectServer
{
    public static class ServiceObjectExtensions
    {
        private static readonly Regex nameRegex =
            new Regex(@"\w+\.(\w+\.?)+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool IsValidName(this IServiceObject obj)
        {
            return nameRegex.IsMatch(obj.Name);
        }

        public static string GetModuleName(this IServiceObject obj)
        {
            var nameParts = obj.Name.Split('.');
            return nameParts[0];
        }

        /// <summary>
        /// 验证服务对象名字是否合法
        /// </summary>
        /// <param name="obj"></param>
        public static void VerifyName(this IServiceObject obj)
        {
            //检测名称是否合适
            if (!obj.IsValidName())
            {
                var msg = string.Format("Bad service object name '{0}'", obj.Name);
                throw new BadServiceObjectNameException(msg, obj.Name);
            }

            //检测模块名是否存在
            /*var moduleName = obj.GetModuleName();
            if (!ObjectServerStarter.ModulePool.Contains(moduleName))
            {
                var msg = string.Format("Cannot found moudule '{0}'", moduleName);
                throw new ModuleNotFoundException(msg, moduleName);
            }*/
        }


    }
}
