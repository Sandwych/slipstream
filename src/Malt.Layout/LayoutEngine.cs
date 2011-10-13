using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

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

        public ITableLayoutWidget CreateLayout(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var pos = new CellPosition();
            var tablePanel = this.widgetFactory.CreateTableLayoutWidget(container);
            tablePanel.ColumnCount = container.ColumnCount;
            tablePanel.RowCount = ComputeRowCount(container, tablePanel.ColumnCount);

            tablePanel.Initialize();

            foreach (IPlacable placable in container.Children)
            {
                if (pos.Column == 0)
                {
                    if (placable.Fill)
                    {
                        tablePanel.AddStarHeightRow(1);
                    }
                    else
                    {
                        tablePanel.AddAutoHeightRow();
                    }
                }

                var colSpan = ComputeColumnSpan(tablePanel.ColumnCount, placable, pos);
                object widget = this.CreateWidget(placable);

                if (widget != null)
                {
                    tablePanel.SetCellWidget(widget, pos.Row, pos.Column);
                    tablePanel.SetColumnSpan(pos.Row, pos.Column, colSpan);
                }

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
            Debug.Assert(this.widgetFactory != null);
            Debug.Assert(container != null);

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
            Debug.Assert(placable != null);

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
                var hl = placable as HorizontalLine;
                widget = this.widgetFactory.CreateHorizontalLineWidget(hl);
            }
            else if (placable is Notebook)
            {
                var nb = placable as Notebook;
                widget = this.widgetFactory.CreateNotebookWidget(nb);
                foreach (Page page in nb.Pages)
                {
                    var le = new LayoutEngine(this.widgetFactory);
                    var pageContent = le.CreateLayout(page);
                    var pageWidget = this.widgetFactory.CreatePageWidget(page, widget, pageContent);
                }
            }
            else if (placable is Button)
            {
                var button = placable as Button;
                widget = this.widgetFactory.CreateButtonWidget(button);
            }
            else if (placable is Input)
            {
                var field = placable as Input;
                widget = this.widgetFactory.CreateInputWidget(field);
            }
            else
            {
                throw new NotSupportedException("不支持的控件类型：" + placable.GetType().FullName);
            }
            return widget;
        }

    }
}
