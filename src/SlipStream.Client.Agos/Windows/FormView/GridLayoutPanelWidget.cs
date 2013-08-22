using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;

using Sandwych.Layout;

namespace SlipStream.Client.Agos.Windows.FormView
{
    public class GridLayoutPanelWidget : Grid, ITableLayoutWidget
    {
        public GridLayoutPanelWidget()
            : base()
        {
        }

        public void Initialize()
        {
            //初始化列样式
            this.ColumnDefinitions.Clear();
            this.RowDefinitions.Clear();
            if (this.ColumnCount % 2 == 0) //列数是偶数的时候一般是标签和字段交替
            {
                float widthPercent = 1.0F / ((float)this.ColumnCount / 2.0F) * 100F;

                for (int i = 0; i < this.ColumnCount; i++)
                {
                    ColumnDefinition colDef;

                    if (i % 2 == 0) //偶数列一般是标签列，设成自动大小
                    {
                        colDef = new ColumnDefinition()
                        {
                            Width = GridLength.Auto,
                        };
                    }
                    else //奇数列一般是字段列，设成百分比自动扩展
                    {
                        colDef = new ColumnDefinition()
                        {
                            Width = new GridLength(widthPercent, GridUnitType.Star),
                        };
                    }

                    this.ColumnDefinitions.Add(colDef);
                }
            }
            else //奇数就全部设成自动大小
            {
                for (int i = 0; i < this.ColumnCount; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition()
                    {
                        Width = GridLength.Auto
                    };
                    this.ColumnDefinitions.Add(colDef);
                }
            }
        }

        public int ColumnCount
        {
            get;
            set;
        }

        public void AddStarHeightRow(double weight)
        {
            RowDefinition rowdef = new RowDefinition()
            {
                Height = new GridLength(weight, GridUnitType.Star)
            };

            base.RowDefinitions.Add(rowdef);
        }

        public void AddAutoHeightRow()
        {
            var rowdef = new RowDefinition()
            {
                Height = GridLength.Auto
            };
            base.RowDefinitions.Add(rowdef);
        }

        public int RowCount
        {
            get;
            set;
        }

        public void SetColumnSpan(int row, int col, int colspan)
        {
            FrameworkElement cell = null;

            foreach (FrameworkElement d in this.Children)
            {
                if (Grid.GetRow(d) == row && Grid.GetColumn(d) == col)
                {
                    cell = d;
                    break;
                }
            }

            if (cell == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            cell.SetValue(Grid.ColumnSpanProperty, colspan);
        }

        public void SetRowSpan(int row, int col, int colspan)
        {
            /*
            throw new NotImplementedException();
            */
        }

        public void SetCellWidget(object widget, int row, int col)
        {
            var uiWidget = (UIElement)widget;
            uiWidget.SetValue(Grid.RowProperty, row);
            uiWidget.SetValue(Grid.ColumnProperty, col);
            this.Children.Add(uiWidget);
        }
    }
}
