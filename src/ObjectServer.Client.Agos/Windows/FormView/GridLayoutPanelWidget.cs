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

using Malt.Layout;

namespace ObjectServer.Client.Agos.Windows.FormView
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

            //初始化行样式
            //默认策略是：最后一行设为 100% 占满剩余空间，其他的行固定高度 27
            for (int i = 0; i < this.RowCount - 1; i++)
            {
                var rowDef = new RowDefinition()
                {
                    Height = new GridLength(27, GridUnitType.Pixel)
                };

                this.RowDefinitions.Add(rowDef);
            }

            var lastRowDef = new RowDefinition()
            {
                Height = new GridLength(100, GridUnitType.Star)
            };
            this.RowDefinitions.Add(lastRowDef);
        }

        public int ColumnCount
        {
            get;
            set;
        }

        public void AddRow()
        {
            base.RowDefinitions.Add(new RowDefinition());
        }

        public int RowCount
        {
            get;
            set;
        }

        public void SetColumnSpan(int row, int col, int colspan)
        {
            /*
            throw new NotImplementedException();
            */
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
