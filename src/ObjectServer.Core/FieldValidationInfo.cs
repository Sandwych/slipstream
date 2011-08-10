using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class FieldValidationInfo
    {
        public FieldValidationInfo(string field, string errorMessage)
        {
            this.Field = field;
            this.ErrorMessage = errorMessage;
        }

        public string Field { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}
