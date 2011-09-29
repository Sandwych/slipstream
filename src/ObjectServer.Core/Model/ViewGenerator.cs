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
                where !AbstractTableModel.SystemReadonlyFields.Contains(f.Key)
                    && !f.Key.StartsWith("_")
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
                where !AbstractTableModel.SystemReadonlyFields.Contains(f.Key)
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

        public static string GenerateListView(IFieldCollection fields)
        {
            Debug.Assert(fields != null);

            var viewFields = fields.SkipWhile(
                f => AbstractTableModel.SystemReadonlyFields.Contains(f.Key)
                    || f.Key.StartsWith("_")
                    || f.Value.Type == FieldType.Text
                    || f.Value.Type == FieldType.ManyToMany
                    || f.Value.Type == FieldType.OneToMany);

            var vb = new ViewBuilder();
            vb.WriteListStart();
            foreach (var f in viewFields)
            {
                vb.WriteColumn(f.Value.Name);
            }
            vb.WriteListEnd();

            return vb.ToString();
        }
    }
}
