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
            sbView.AppendLine("<form>");
        }

        public void WriteFormEnd()
        {
            sbView.AppendLine("</form>");
        }

        public void WriteListStart()
        {
            sbView.AppendLine("<list>");
        }

        public void WriteListEnd()
        {
            sbView.AppendLine("</list>");
        }

        public void WriteField(string field)
        {
            Debug.Assert(!string.IsNullOrEmpty(field));
            
            sbView.AppendFormat("<field name=\"{0}\" />\n", field);
        }

        public void WriteLabel(string text)
        {
            Debug.Assert(!string.IsNullOrEmpty(text));
            sbView.AppendFormat("<label text=\"{0}\" />\n", text);
        }

        public void WriteFieldLabel(string field)
        {
            Debug.Assert(!string.IsNullOrEmpty(field));
            sbView.AppendFormat("<label field=\"{0}\" />\n", field);
        }

        public void WriteGridStart(int cols = 4)
        {
            sbView.AppendFormat("<grid cols=\"{0}\" >\n", cols);
        }

        public void WriteGridEnd()
        {
            sbView.AppendLine("</grid>");
        }

        public void WriteNewLine()
        {
            sbView.AppendLine("<newline/>");
        }

        public override string ToString()
        {
            return sbView.ToString();
        }
    }
}
