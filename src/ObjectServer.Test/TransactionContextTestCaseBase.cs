using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class TransactionContextTestCaseBase : TransactionTestCaseBase
    {
        private void ClearTestUsers()
        {
            dynamic userModel = this.GetResource("core.user");

            var constraints = new object[] { new object[] { "login", "=", "test" } };
            var ids = userModel.Search(this.TransactionContext, constraints, null, 0, 0);
            if (ids.Length > 0)
            {
                userModel.Delete(this.TransactionContext, ids);
            }
        }

        public IExportedService Service { get; private set; }

        public ITransactionContext TransactionContext { get; private set; }

        protected dynamic GetResource(string resName)
        {
            return this.TransactionContext.GetResource(resName);
        }

        [SetUp]
        public void BeforeTest()
        {
            Debug.Assert(this.TransactionContext == null);
            Debug.Assert(!string.IsNullOrEmpty(this.SessionId));

            this.TransactionContext = new TransactionContext(TestingDatabaseName, this.SessionId);
        }

        [TearDown]
        public void AfterTest()
        {
            Debug.Assert(this.TransactionContext != null);
            this.TransactionContext.Dispose();
            this.TransactionContext = null;
        }

        protected void ClearTestModelTable()
        {
            Debug.Assert(this.TransactionContext != null);
            this.ClearModel("test.test_model");
        }


        protected void ClearMasterAndChildTable()
        {
            Debug.Assert(this.TransactionContext != null);
            this.ClearModel("test.child");
            this.ClearModel("test.master");
        }

        protected void ClearModel(string modelName)
        {
            dynamic model = this.GetResource(modelName);
            var ids = model.Search(this.TransactionContext, null, null, 0, 0);
            if (ids.Length > 0)
            {
                model.Delete(this.TransactionContext, ids);
            }
        }

    }
}
