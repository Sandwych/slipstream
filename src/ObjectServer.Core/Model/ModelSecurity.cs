using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Core;

namespace ObjectServer.Model
{
    internal static class ModelSecurity
    {

        public static bool CanCreateModel(IServiceScope scope, long userId, string model)
        {
            return IsDeniedModelServiceMethod(scope, userId, model, "allow_create");
        }

        public static bool CanReadModel(IServiceScope scope, long userId, string model)
        {
            return IsDeniedModelServiceMethod(scope, userId, model, "allow_read");
        }

        public static bool CanWriteModel(IServiceScope scope, long userId, string model)
        {
            return IsDeniedModelServiceMethod(scope, userId, model, "allow_write");
        }

        public static bool CanDeleteModel(IServiceScope scope, long userId, string model)
        {
            return IsDeniedModelServiceMethod(scope, userId, model, "allow_delete");
        }

        private static bool IsDeniedModelServiceMethod(IServiceScope scope, long userId, string model, string action)
        {
            Debug.Assert(scope != null);
            Debug.Assert(userId >= 0);
            Debug.Assert(!string.IsNullOrEmpty(model));
            Debug.Assert(!string.IsNullOrEmpty(action));

            var accessModel = (ModelAccessModel)scope.GetResource(ModelAccessModel.ModelName);
            var records = accessModel.FindByModelAndUserId(scope, model, userId);
            var denyCount = records.Count(r => !(bool)r[action]);
            var result = denyCount == 0;
            return result;
        }
    }
}
