using System;
using System.Collections.Generic;
using System.Text;

using Malt.Layout.Widgets;
using Malt.Layout.Models;

namespace Malt.Layout
{
    public interface IWidgetFactory
    {
        /// <summary>
        /// 创建自定义控件
        /// </summary>
        object CreateFieldWidget(Field field);

        /// <summary>
        /// 创建表格布局控件
        /// </summary>
        /// <returns></returns>
        ITableLayoutWidget CreateTableLayoutWidget();

        /// <summary>
        /// 创建标签控件
        /// </summary>
        /// <returns></returns>
        ILabelWidget CreateLabelWidget(Models.Label label);

        /// <summary>
        /// 创建水平线控件
        /// </summary>
        /// <returns></returns>
        IHorizontalLineWidget CreateHorizontalLineWidget();
        
    }
}
