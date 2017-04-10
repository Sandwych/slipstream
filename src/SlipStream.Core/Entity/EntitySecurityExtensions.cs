using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Core;

namespace SlipStream.Entity
{
    internal static class EntitySecurityExtensions
    {
        public static bool CanCreateEntity(this IServiceContext scope, string entityName)
        {
            return scope.IsDeniedEntityServiceMethod(entityName, "create");
        }

        public static bool CanReadEntity(this IServiceContext scope, string entityName)
        {
            return scope.IsDeniedEntityServiceMethod(entityName, "read");
        }

        public static bool CanWriteEntity(this IServiceContext scope, string entityName)
        {
            return scope.IsDeniedEntityServiceMethod(entityName, "write");
        }

        public static bool CanDeleteEntity(this IServiceContext scope, string entityName)
        {
            return scope.IsDeniedEntityServiceMethod(entityName, "delete");
        }

        private static bool IsDeniedEntityServiceMethod(this IServiceContext scope, string entityName, string action)
        {
            Debug.Assert(scope != null);
            Debug.Assert(!string.IsNullOrEmpty(entityName));
            Debug.Assert(!string.IsNullOrEmpty(action));

            if (!scope.UserSession.IsSystemUser)
            {
                return EntityAccessEntity.CheckForCurrentUser(scope, entityName, action);
            }
            else
            {
                return true;
            }
        }
    }
}
