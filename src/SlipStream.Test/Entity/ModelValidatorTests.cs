using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using SlipStream.Entity;
using SlipStream.Exceptions;

namespace SlipStream.Entity.Test
{
    [TestFixture(Category = "ORM")]
    public class ModelValidatorTests : ServiceContextTestCaseBase
    {

        [Test]
        public void CanValidateForCreation()
        {
            dynamic model = this.GetResource("test.validator");
            dynamic record = new ExpandoObject();
            record.required_field = null;
            record.readonly_field = "hello!";

            var vex = Assert.Throws<ValidationException>(() =>
            {
                EntityValidator.ValidateRecordForCreation(model, record);
            });

            Assert.AreEqual(1, vex.Fields.Count);
        }
    }
}
