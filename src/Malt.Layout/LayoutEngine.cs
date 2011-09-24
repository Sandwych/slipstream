using System;
using System.Collections.Generic;
using System.Text;

using Malt.Layout.Models;

namespace Malt.Layout
{
    public class LayoutEngine
    {
        private IWidgetFactory widgetFactory;

        public LayoutEngine(IWidgetFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            this.widgetFactory = factory;
        }

        public ITableLayoutWidget CreateLayoutTable(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var pos = new CellPosition();
            var tablePanel = this.widgetFactory.CreateTableLayoutWidget();
            tablePanel.ColumnCount = container.ColumnCount;
            tablePanel.RowCount = ComputeRowCount(container, tablePanel.ColumnCount);

            tablePanel.Initialize();

            foreach (IPlacable placable in container.Children)
            {
                if (pos.Column == 0)
                {
                    tablePanel.AddRow();
                }

                var colSpan = ComputeColumnSpan(tablePanel.ColumnCount, placable, pos);

                object widget = this.CreateWidget(placable);

                tablePanel.SetCellWidget(widget, pos.Row, pos.Column);
                tablePanel.SetColumnSpan(pos.Row, pos.Column, colSpan);

                pos.Column += colSpan;
                if (pos.Column >= tablePanel.ColumnCount) //该转换下一行了
                {
                    pos.Row++;
                    pos.Column = 0;
                    continue;
                }
            }

            return tablePanel;
        }

        private int ComputeRowCount(IContainer container, int columnCount)
        {
            var pos = new CellPosition();
            int rowCount = 0;

            foreach (IPlacable placable in container.Children)
            {
                if (pos.Column == 0)
                {
                    rowCount++;
                }

                var colSpan = ComputeColumnSpan(columnCount, placable, pos);

                pos.Column += colSpan;
                if (pos.Column >= columnCount) //该转换下一行了
                {
                    pos.Row++;
                    pos.Column = 0;
                    continue;
                }
            }

            return rowCount + 1;
        }

        private static int ComputeColumnSpan(int columnCount, IPlacable placable, CellPosition pos)
        {
            //先看整个格子够不够放
            if (placable.ColumnSpan <= 0) // <= 0 表示从当前位置起占满整行
            {
                return columnCount - pos.Column;
            }
            else
            {
                return Math.Min(columnCount - pos.Column, placable.ColumnSpan);
            }
        }


        private object CreateWidget(IPlacable placable)
        {
            object widget = null;

            if (placable is Label)
            {
                var label = placable as Label;
                var labelWidget = this.widgetFactory.CreateLabelWidget(label);
                widget = labelWidget;
            }
            else if (placable is NewLine)
            {
                widget = null;
            }
            else if (placable is HorizontalLine)
            {
                widget = this.widgetFactory.CreateHorizontalLineWidget();
            }
            else if (placable is Field)
            {
                var field = placable as Field;
                widget = this.widgetFactory.CreateFieldWidget(field);
            }
            else
            {
                throw new NotSupportedException("不支持的控件类型：" + placable.GetType().FullName);
            }
            return widget;
        }

    }
}
