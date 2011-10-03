using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer.Model
{
    internal sealed class ViewBuilder
    {
        private StringBuilder sbView = new StringBuilder();

        public ViewBuilder()
        {
            sbView.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        }

        public void WriteFormStart()
        {
            sbView.AppendLine("<form col=\"4\" >");
        }

        public void WriteFormEnd()
        {
            sbView.AppendLine("</form>");
        }

        public void WriteTreeStart()
        {
            sbView.AppendLine("<tree>");
        }

        public void WriteTreeEnd()
        {
            sbView.AppendLine("</tree>");
        }

        public void WriteColumn(string field, string where = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(field));

            if (string.IsNullOrEmpty(where))
            {
                sbView.AppendFormat("<field name=\"{0}\" />\n", field);
            }
            else
            {
                sbView.AppendFormat("<field name=\"{0}\" where=\"{1}\" />\n", field, where);
            }
        }

        public void WriteField(string field, int colspan = 1, bool fill = false)
        {
            Debug.Assert(!string.IsNullOrEmpty(field));

            sbView.AppendFormat("<field name=\"{0}\" colspan=\"{1}\" fill=\"{2}\" />\n", 
                field, colspan, fill.ToString().ToLowerInvariant());
        }

        public void WriteFieldLabel(string field)
        {
            Debug.Assert(!string.IsNullOrEmpty(field));
            sbView.AppendFormat("<label field=\"{0}\" />\n", field);
        }

        public void WriteLabel(string text, int colspan = 1)
        {
            Debug.Assert(!string.IsNullOrEmpty(text));
            sbView.AppendFormat("<label text=\"{0}\" colspan=\"{1}\" />\n", text, colspan);
        }

        public void WriteGridStart(int cols = 2)
        {
            sbView.AppendFormat("<grid cols=\"{0}\" >\n", cols);
        }

        public void WriteGridEnd()
        {
            sbView.AppendLine("</grid>");
        }

        public void WriteNewLine()
        {
            sbView.AppendLine("<br/>");
        }

        public void WriteHLine(string str = null, int colspan = 4)
        {
            if (str != null)
            {
                sbView.AppendFormat("<hr text=\"{0}\" colspan=\"{1}\" />", str, colspan);
            }
            else
            {
                sbView.AppendFormat("<hr colspan=\"{0}\"/>", colspan);
            }
        }

        public override string ToString()
        {
            return sbView.ToString();
        }
    }
}
