using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython;
using IronPython.Hosting;

using ObjectServer.Core;

namespace ObjectServer.Runtime
{
    internal class PythonRuleConstraintEvaluator : IRuleConstraintEvaluator
    {
        private readonly static Type _IronPythonBultin = typeof(IronPython.Modules.Builtin);
        private readonly static ScriptRuntime s_runtime = Python.CreateRuntime();
        private readonly ScriptEngine _engine;
        private readonly ScriptScope _scope;

        public PythonRuleConstraintEvaluator()
        {
            this._engine = Python.GetEngine(s_runtime);
            this._scope = this._engine.CreateScope();
        }

        public void SetVariable(string varName, object value)
        {
            this._scope.SetVariable(varName, value);
        }

        public dynamic Evaluate(string exp)
        {
            var scriptSource = this._engine.CreateScriptSourceFromString(exp, SourceCodeKind.Expression);
            var compiledCode = scriptSource.Compile();
            var dynObj = compiledCode.Execute(this._scope);
            return dynObj;
        }
    }
}
