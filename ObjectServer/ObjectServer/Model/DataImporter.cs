using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace ObjectServer.Model
{
    public class DataImporter
    {
        private IContext context;

        public DataImporter(IContext ctx)
        {
            this.context = ctx;
        }

        public void Import(string file)
        {
            throw new NotImplementedException();
            /*
            using (var fs = File.OpenRead(file))
            using(var reader = XmlReader.Create(fs))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
                    {
                        var noUpdate = false;
                        reader.ReadStartElement();

                        reader.ReadEndElement();
                    }
                }
                
            }
            */
        }

    }
}
