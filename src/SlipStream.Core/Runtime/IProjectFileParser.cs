using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SlipStream.Runtime
{
    public interface IProjectFileParser
    {
        ProjectFileDescriptor Parse(string csprojPath);
        ProjectFileDescriptor Parse(Stream stream);
    }
}
