using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronRuby;

using ObjectServer.Core;

namespace ObjectServer.Runtime
{
    internal class RubyRuleConstraintEvaluator : IRuleConstraintEvaluator
    {
        private readonly static Type _rubyHashOpsType = typeof(IronRuby.Builtins.HashOps);
        private readonly ScriptRuntime _runtime;
        private readonly ScriptEngine _engine;
        private readonly ScriptScope _scope;

        public RubyRuleConstraintEvaluator()
        {
            this._runtime = Ruby.CreateRuntime();
            this._engine = Ruby.GetEngine(this._runtime);
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
