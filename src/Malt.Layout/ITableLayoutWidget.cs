using System;
using System.Collections.Generic;
using System.Text;

namespace Malt.Layout
{
    public interface ITableLayoutWidget
    {
        void Initialize();

        int ColumnCount { get; set; }

        void AddStarHeightRow(double weight);
        void AddAutoHeightRow();

        int RowCount { get; set; }

        void SetColumnSpan(int row, int col, int colspan);

        void SetRowSpan(int row, int col, int colspan);

        void SetCellWidget(object widget, int row, int col);

    }
}
