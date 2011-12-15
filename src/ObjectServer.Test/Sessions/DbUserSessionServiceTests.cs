using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Autofac;

using ObjectServer.Data;

namespace ObjectServer.Sessions.Test
{
    [TestFixture]
    public class DbUserSessionServiceTests
    {
        private IDataContext _dataContext;

        [SetUp]
        public void Init()
        {
            SlipstreamEnvironment.Initialize();

            this._dataContext = ObjectServer.Data.DataProvider
                .OpenDataContext(ServiceContextTestCaseBase.TestingDatabaseName);
        }

        [TearDown]
        public void Cleanup()
        {
            if (this._dataContext != null)
            {
                this._dataContext.Close();
                this._dataContext = null;
            }
        }

        [Test]
        public void CheckPutAndGet()
        {
            var svc = new DbUserSessionService(this._dataContext);
            var sess = new UserSession("session_test", 9999);
            svc.Put(sess);

            var newSess = svc.GetByToken(sess.Token);
            Assert.AreEqual(sess.Token, newSess.Token);
            Assert.AreEqual(9999, newSess.UserId);
            Assert.AreEqual("session_test", newSess.Login);
            Assert.That(!newSess.IsSystemUser);
            Assert.That(newSess.IsActive);
        }

        [Test]
        public void PulseShouldBeSuccessfully()
        {
            var svc = new DbUserSessionService(this._dataContext);
            var sess = new UserSession("session_test", 9999);
            svc.Put(sess);
            svc.Pulse(sess.Token);
        }
    }
}
