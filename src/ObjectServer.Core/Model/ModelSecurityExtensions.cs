using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Core;

namespace ObjectServer.Model
{
    internal static class ModelSecurityExtensions
    {

        public static bool CanCreateModel(this ITransactionContext scope, long userId, string model)
        {
            return scope.IsDeniedModelServiceMethod(userId, model, "allow_create");
        }

        public static bool CanReadModel(this ITransactionContext scope, long userId, string model)
        {
            return scope.IsDeniedModelServiceMethod(userId, model, "allow_read");
        }

        public static bool CanWriteModel(this ITransactionContext scope, long userId, string model)
        {
            return scope.IsDeniedModelServiceMethod(userId, model, "allow_write");
        }

        public static bool CanDeleteModel(this ITransactionContext scope, long userId, string model)
        {
            return scope.IsDeniedModelServiceMethod(userId, model, "allow_delete");
        }

        private static bool IsDeniedModelServiceMethod(this ITransactionContext scope, long userId, string model, string action)
        {
            Debug.Assert(scope != null);
            Debug.Assert(userId >= 0);
            Debug.Assert(!string.IsNullOrEmpty(model));
            Debug.Assert(!string.IsNullOrEmpty(action));

            if (!scope.Session.IsSystemUser)
            {
                var records = ModelAccessModel.FindByModelAndUserId(scope, model, userId);
                var denyCount = records.Count(r => !(bool)r[action]);
                var result = denyCount == 0;
                return result;
            }
            else
            {
                return true;
            }
        }
    }
}
