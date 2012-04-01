﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace ObjectServer.Runtime
{
    public class DefaultProjectFileParser : IProjectFileParser
    {
        public ProjectFileDescriptor Parse(string csprojPath)
        {
            string content = File.ReadAllText(csprojPath);
            using (var reader = new StringReader(content))
            {
                return this.Parse(reader);
            }
        }

        public ProjectFileDescriptor Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return this.Parse(reader);
            }
        }

        public ProjectFileDescriptor Parse(TextReader reader)
        {
            var document = XDocument.Load(XmlReader.Create(reader));
            return new ProjectFileDescriptor
            {
                AssemblyName = GetAssemblyName(document),
                SourceFilenames = GetSourceFilenames(document).ToArray(),
                References = GetReferences(document).ToArray()
            };
        }

        private static string GetAssemblyName(XDocument document)
        {
            return document
                .Elements(ns("Project"))
                .Elements(ns("PropertyGroup"))
                .Elements(ns("AssemblyName"))
                .Single()
                .Value;
        }

        private static IEnumerable<string> GetSourceFilenames(XDocument document)
        {
            return document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("Compile"))
                .Attributes("Include")
                .Select(c => c.Value);
        }

        private static IEnumerable<ReferenceDescriptor> GetReferences(XDocument document)
        {
            var assemblyReferences = document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("Reference"))
                .Where(c => c.Attribute("Include") != null)
                .Select(c =>
                {
                    string path = null;
                    XElement attribute = c.Elements(ns("HintPath")).FirstOrDefault();
                    if (attribute != null)
                    {
                        path = attribute.Value;
                    }

                    return new ReferenceDescriptor
                    {
                        SimpleName = ExtractAssemblyName(c.Attribute("Include").Value),
                        FullName = c.Attribute("Include").Value,
                        Path = path,
                        ReferenceType = ReferenceType.Library
                    };
                });

            var projectReferences = document
                .Elements(ns("Project"))
                .Elements(ns("ItemGroup"))
                .Elements(ns("ProjectReference"))
                .Attributes("Include")
                .Select(c => new ReferenceDescriptor
                {
                    SimpleName = Path.GetFileNameWithoutExtension(c.Value),
                    FullName = Path.GetFileNameWithoutExtension(c.Value),
                    Path = c.Value,
                    ReferenceType = ReferenceType.Project
                });

            return assemblyReferences.Union(projectReferences);
        }

        private static string ExtractAssemblyName(string value)
        {
            int index = value.IndexOf(',');
            return index < 0 ? value : value.Substring(0, index);
        }

        private static XName ns(string name)
        {
            return XName.Get(name, "http://schemas.microsoft.com/developer/msbuild/2003");
        }
    }
}
