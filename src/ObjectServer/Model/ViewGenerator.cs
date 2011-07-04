using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer.Model
{
    internal class ViewGenerator
    {
        public static string GenerateFormView(IFieldCollection fields)
        {
            Debug.Assert(fields != null);
            
            var vb = new ViewBuilder();
            vb.WriteFormStart();
            foreach (var f in fields)
            {
                vb.WriteField(f.Value.Name);
            }
            vb.WriteFormEnd();

            return vb.ToString();
        }

        public static string GenerateListView(IFieldCollection fields)
        {
            Debug.Assert(fields != null);

            var vb = new ViewBuilder();
            vb.WriteListStart();
            foreach (var f in fields)
            {
                vb.WriteField(f.Value.Name);
            }
            vb.WriteListEnd();

            return vb.ToString();
        }
    }
}
