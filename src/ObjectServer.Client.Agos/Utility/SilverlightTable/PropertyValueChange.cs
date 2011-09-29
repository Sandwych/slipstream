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

namespace SilverlightTable
{
  /// <summary>
  /// A simple class used to communicate property value changes to a Row
  /// </summary>
  public class PropertyValueChange
  {
    private string _propertyName;

    private object _value;

    public object Value
    {
      get { return _value; }
    }

    public string PropertyName
    {
      get { return _propertyName; }
    }

    public PropertyValueChange(string propertyName, object value)
    {
      _propertyName = propertyName;
      _value = value;
    }
  }
}
