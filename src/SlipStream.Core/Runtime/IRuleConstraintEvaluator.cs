using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Runtime
{
    public interface IRuleConstraintEvaluator
    {
        void SetVariable(string varName, object value);
        dynamic Evaluate(string exp);
    }
}
