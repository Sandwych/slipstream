using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("placeholder")]
    public class PlaceHolder : Placable
    {
        public PlaceHolder()
        {
            this.ColumnSpan = 0;
            this.RowSpan = 1;
        }        
    }
}
