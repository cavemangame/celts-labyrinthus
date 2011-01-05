using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace Labyrinthus
{
  /// <summary>
  /// Класс для преобразования Point3D в string
  /// </summary>
  [ValueConversion(typeof(Point3D), typeof(string))]
  public class Point3DDebugConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var position = (Point3D)value;

      return string.Format("{0:0.0}, {1:0.0}, {2:0.0}",
                           position.X, position.Y, position.Z);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  /// <summary>
  /// Класс для преобразования Vector3D в string
  /// </summary>
  [ValueConversion(typeof(Vector3D), typeof(string))]
  public class Vector3DDebugConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var position = (Vector3D)value;

      return string.Format("{0:0.0}, {1:0.0}, {2:0.0}",
                           position.X, position.Y, position.Z);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
