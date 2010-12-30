using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Labyrinthus.AidedCanvas
{
  /// <summary>
  /// Кастомный контрол для ребра сетки создания примитива
  /// </summary>
  public class PrimitiveEdgeControl : Control
  {
    #region Внутренние типы
    /// <summary>
    /// Типы ребер
    /// </summary>
    public enum EdgeTypes
    {
      Horizontal,
      Vertical
    }
    #endregion

    #region Свойства
    private EdgeTypes edgeType;

    /// <summary>
    /// Тип ребра
    /// </summary>
    public EdgeTypes EdgeType
    {
      get { return edgeType; }
      set
      {
        edgeType = value;
        EdgeTransform = new RotateTransform(
          EdgeTypes.Horizontal == edgeType ? -45 : 45);
      }
    }

    /// <summary>
    /// Длина ребра
    /// </summary>
    public double EdgeSize
    {
      set
      {
        SetValue(EdgeSizeProperty, value);
        EdgeAreaSize = Math.Sqrt(Math.Pow(value, 2) / 2);
      }
    }

    public static DependencyProperty EdgeSizeProperty = DependencyProperty.Register(
      "EdgeSize", typeof(double), typeof(PrimitiveEdgeControl));


    /// <summary>
    /// Линия примитива данного ребра
    /// </summary>
    public Point PrimitiveLineStart { get; set; }


    /// <summary>
    /// Кисть для отрисовки ребра
    /// </summary>
    private Brush EdgeBrush
    {
      set
      {
        SetValue(EdgeBrushProperty, value);
      }
    }

    public static DependencyProperty EdgeBrushProperty = DependencyProperty.Register(
      "EdgeBrush", typeof(Brush), typeof(PrimitiveEdgeControl));


    /// <summary>
    /// Длина стороны квадрата прилегающей области ребра
    /// </summary>
    private double EdgeAreaSize
    {
      set
      {
        SetValue(EdgeAreaSizeProperty, value);
      }
    }

    public static DependencyProperty EdgeAreaSizeProperty = DependencyProperty.Register(
      "EdgeAreaSize", typeof(double), typeof(PrimitiveEdgeControl));


    /// <summary>
    /// Трансформация для ребра
    /// </summary>
    private Transform EdgeTransform
    {
      set
      {
        SetValue(EdgeTransformProperty, value);
      }
    }

    public static DependencyProperty EdgeTransformProperty = DependencyProperty.Register(
      "EdgeTransform", typeof(Transform), typeof(PrimitiveEdgeControl));
    #endregion

    #region Конструктор
    static PrimitiveEdgeControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(PrimitiveEdgeControl), new FrameworkPropertyMetadata(typeof(PrimitiveEdgeControl)));
    }

    public PrimitiveEdgeControl()
    {
      EdgeType = EdgeTypes.Horizontal;
      EdgeBrush = new SolidColorBrush(Colors.LightGray);

      DataContext = this;
    }
    #endregion

    #region public методы
    public void ClickEdge(bool selected)
    {
      EdgeBrush = new SolidColorBrush(selected ? Colors.Black : Colors.LightGray);
    }
    #endregion
  }
}
