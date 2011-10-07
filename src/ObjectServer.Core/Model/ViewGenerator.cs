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

            Func<FieldType, bool> isBigFieldDlg = (FieldType ft) =>
                ft == FieldType.OneToMany || ft == FieldType.ManyToMany || ft == FieldType.Text;

            var smallFields =
                from f in fields
                where !AbstractSqlModel.SystemReadonlyFields.Contains(f.Key)
                    && !f.Value.Name.StartsWith("_")
                    && !isBigFieldDlg(f.Value.Type)
                select f;

            //生成单行字段
            var vb = new ViewBuilder();
            vb.WriteFormStart();
            foreach (var f in smallFields)
            {
                vb.WriteFieldLabel(f.Value.Name);
                vb.WriteField(f.Value.Name);
            }

            var bigFields =
                from f in fields
                where !AbstractSqlModel.SystemReadonlyFields.Contains(f.Key)
                    && !f.Key.StartsWith("_")
                    && isBigFieldDlg(f.Value.Type)
                select f;


            //生成比较占空间的字段
            foreach (var f in bigFields)
            {
                vb.WriteNewLine();
                vb.WriteHLine(f.Value.Label);
                vb.WriteNewLine();
                vb.WriteField(f.Value.Name, 4, true);
            }

            vb.WriteFormEnd();
            return vb.ToString();
        }

        public static string GenerateTreeView(IFieldCollection fields)
        {
            Debug.Assert(fields != null);

            var viewFields =
                from f in fields
                where !AbstractSqlModel.SystemReadonlyFields.Contains(f.Key)
                    && !f.Value.Name.StartsWith("_")
                    && f.Value.Type != FieldType.Text
                    && f.Value.Type != FieldType.ManyToMany
                    && f.Value.Type != FieldType.OneToMany
                select f.Value;

            var vb = new ViewBuilder();
            vb.WriteTreeStart();
            foreach (var f in viewFields)
            {
                if (f.Name == "name")
                {
                    vb.WriteColumn(f.Name, "basic");
                }
                else
                {
                    vb.WriteColumn(f.Name, "advanced");
                }
            }
            vb.WriteTreeEnd();

            return vb.ToString();
        }
    }
}
