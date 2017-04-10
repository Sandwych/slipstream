using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using SlipStream.Data;

namespace SlipStream.Sessions.Test
{
    [TestFixture(Category = "Security")]
    public class InMemoryUserSessionServiceTests
    {
        [SetUp]
        public void Init()
        {
            SlipstreamEnvironment.Initialize();
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void CheckPutAndGet()
        {
            var svc = new MemoryUserSessionStore();
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
            var svc = new MemoryUserSessionStore();
            var sess = new UserSession("session_test", 9999);
            svc.Put(sess);
            svc.Pulse(sess.Token);
        }
    }
}
