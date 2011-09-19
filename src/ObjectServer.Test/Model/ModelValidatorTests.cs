using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using ObjectServer.Model;
using ObjectServer.Exceptions;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class ModelValidatorTests : LocalTestCase
    {

        [Test]
        public void Test_creation()
        {
            var model = (IModel)this.ServiceContext.GetResource("test.validator");
            dynamic record = new ExpandoObject();
            record.required_field = null;
            record.readonly_field = "hello!";

            var vex = Assert.Throws<ValidationException>(() =>
            {
                ModelValidator.ValidateRecordForCreation(model, record);
            });

            Assert.AreEqual(1, vex.Fields.Count);
        }
    }
}
