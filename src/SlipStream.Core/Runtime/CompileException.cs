using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.CodeDom;
using System.CodeDom.Compiler;

namespace SlipStream.Runtime
{
    [Serializable]
    public sealed class CompileException : Exception
    {
        public CompileException(string msg, CompilerErrorCollection errors)
            : base(msg)
        {
            this.Errors = errors;
        }

        public CompilerErrorCollection Errors { get; private set; }
    }
}
