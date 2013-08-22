using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Runtime
{
    public enum ReferenceType
    {
        Library,
        Project
    }

    public class ReferenceDescriptor
    {
        public string SimpleName { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public ReferenceType ReferenceType { get; set; }
    }

    public class ProjectFileDescriptor
    {
        public string AssemblyName { get; set; }
        public IEnumerable<string> SourceFilenames { get; set; }
        public IEnumerable<ReferenceDescriptor> References { get; set; }
    }

}
