using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Exceptions
{
    [Serializable]
    public sealed class ModuleNotFoundException : ResourceException
    {
        public ModuleNotFoundException(string msg, string moduleName)
            : base(msg)
        {
            this.ModuleName = moduleName;
        }

        public string ModuleName { get; private set; }
    }
}
