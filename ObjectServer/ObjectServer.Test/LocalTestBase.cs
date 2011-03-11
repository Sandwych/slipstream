using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class LocalTestCase
    {
        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            if (!ObjectServerStarter.Initialized)
            {
                ObjectServerStarter.Initialize();
            }

            var service = ObjectServerStarter.ExportedService;
            this.Service = service;

            this.SessionId = this.Service.LogOn("objectserver", "root", "root");
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = ObjectServerStarter.ExportedService;
            service.LogOff(this.SessionId);
        }

        public string SessionId { get; private set; }

        public IExportedService Service { get; private set; }


        protected void ClearModelDataTable(ResourceScope context)
        {
            dynamic modelDataModel = context.DatabaseProfile.GetResource("core.model_data");
            var ids = modelDataModel.SearchInternal(context, null, 0, 0);
            if (ids.Length > 0)
            {
                modelDataModel.DeleteInternal(context, ids);
            }
        }


        protected void ClearTestModelTable(ResourceScope context, dynamic testObjectModel)
        {
            testObjectModel = context.DatabaseProfile.GetResource("test.test_model");
            var ids = testObjectModel.SearchInternal(context, null, 0, 0);
            if (ids.Length > 0)
            {
                testObjectModel.DeleteInternal(context, ids);
            }
        }


        protected void ClearMasterAndChildTable(ResourceScope context)
        {
            dynamic childModel = context.DatabaseProfile.GetResource("test.child");
            var ids = childModel.SearchInternal(context, null, 0, 0);
            if (ids.Length > 0)
            {
                childModel.DeleteInternal(context, ids);
            }
            dynamic masterModel = context.DatabaseProfile.GetResource("test.master");
            ids = masterModel.SearchInternal(context, null, 0, 0);
            if (ids.Length > 0)
            {
                masterModel.DeleteInternal(context, ids);
            }
        }

    }
}
