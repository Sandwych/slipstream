using System;
using System.Collections.Generic;
using System.Text;

namespace Malt.Layout
{
    internal struct CellPosition
    {
        public CellPosition(int row, int col)
            : this()
        {
            this.Row = row;
            this.Column = col;
        }

        public int Row { get; set; }
        public int Column { get; set; }
    }

}
