using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Labyrinthus.AidedCanvas
{
  class PrimitivePackCanvas : Canvas
  {
    #region Переменные

    /// <summary>
    /// Вижуалы канвы
    /// </summary>
    private readonly List<Visual> visuals = new List<Visual>();

    private DrawingVisual gridVisual;

    /// <summary>
    /// Расстояние от краев канвы до начала сетки примитива
    /// </summary>
    private const double SHIFT_FROM_BORDER = 10.0;

    /// <summary>
    /// Коэффициент - множитель относительно размера примитива 
    /// Используется для вычисления размеров сетки
    /// </summary>
    private const int CELLS_COUNT_KOEF = 3;

    #endregion

    #region Ресурсы
 
    private readonly Brush edgeBrush = new SolidColorBrush(Colors.LightGray);
    private readonly Pen edgePen;
    private readonly Brush primitiveBorderBrush = new SolidColorBrush(Colors.Black);
    private readonly Pen primitiveBorderPen;

    #endregion

    #region Constructor

    public PrimitivePackCanvas()
    {
      edgePen = new Pen(edgeBrush, 1.0);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
    }

    #endregion

    #region Overrides

    protected override int VisualChildrenCount
    {
      get { return visuals.Count; }
    }

    protected override Visual GetVisualChild(int index)
    {
      return visuals[index];
    }

    #endregion

    #region Private

    private void AddVisual(Visual visual)
    {
      visuals.Add(visual);
      AddVisualChild(visual);
      AddLogicalChild(visual);
    }

//    private void RemoveVisual(Visual visual)
//    {
//      visuals.Remove(visual);
//      RemoveVisualChild(visual);
//      RemoveLogicalChild(visual);
//    }

    private void ClearAll()
    {
      foreach (var visual in visuals)
      {
        RemoveVisualChild(visual);
        RemoveLogicalChild(visual);
      }
      visuals.Clear();
    }

    #endregion

    #region Public

    public void Refresh()
    {
      ClearAll();
      DrawEdges();
      DrawPrimitives();
    }

    #endregion

    #region Отрисовка

    /// <summary>
    /// Создание и отрисовка всех ребер
    /// </summary>
    private void DrawEdges()
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;
      var primitiveInfo = wnd.Primitive;
      double step = GetDrawStep(wnd);
      gridVisual = new DrawingVisual();

      using (DrawingContext dc = gridVisual.RenderOpen())
      {
        int width = primitiveInfo.Width*CELLS_COUNT_KOEF;
        int height = primitiveInfo.Height*CELLS_COUNT_KOEF;
        for (int i = 0; i <= width; i++)
        {
          for (int j = 0; j <= height; j++)
          {
            // вертикальные ребра
            if (j != height)
            {
              var startPoint = new Point(i*step + SHIFT_FROM_BORDER, j*step + SHIFT_FROM_BORDER);
              var endPoint = new Point(i*step + SHIFT_FROM_BORDER, (j + 1)*step + SHIFT_FROM_BORDER);
              dc.DrawLine(edgePen, startPoint, endPoint);
            }
            // горизонтальные ребра
            if (i != width)
            {
              var startPoint = new Point(i*step + SHIFT_FROM_BORDER, j*step + SHIFT_FROM_BORDER);
              var endPoint = new Point((i + 1)*step + SHIFT_FROM_BORDER, j*step + SHIFT_FROM_BORDER);
              dc.DrawLine(edgePen, startPoint, endPoint);
            }
          }
        }
      }
      AddVisual(gridVisual);
    }

    private void DrawPrimitives()
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;
      var primitiveInfo = wnd.Primitive;
      double step = GetDrawStep(wnd);
      var primitiveVisual = new DrawingVisual();

      // рисуем примитив в одном вижуале, чтоб работать с ним далее как с единым целым
      using (DrawingContext dc = primitiveVisual.RenderOpen())
      {
        foreach (var lineInfo in primitiveInfo.Lines)
        {
          dc.DrawLine(primitiveBorderPen,
                      new Point(lineInfo.X0*step + SHIFT_FROM_BORDER, lineInfo.Y0*step + SHIFT_FROM_BORDER),
                      new Point(lineInfo.X1*step + SHIFT_FROM_BORDER, lineInfo.Y1*step + SHIFT_FROM_BORDER));
        }
      }
      AddVisual(primitiveVisual);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Выдаем размер клетки для сетки. Здесь учитываем коэффициент (3) для свободной манипуляции примитивами
    /// </summary>
    private double GetDrawStep(WindowMaster wnd)
    {
      var primitiveInfo = wnd.Primitive;
      int cells = CELLS_COUNT_KOEF * Math.Max(primitiveInfo.Height, primitiveInfo.Width);
      double step = Math.Min((ActualHeight - 2 * SHIFT_FROM_BORDER) / cells,
                             (ActualWidth - 2 * SHIFT_FROM_BORDER) / cells);

      return step;
    }

    #endregion

    #region Манипуляции мышью

    /// <summary>
    /// тащим ли в данный момент примитив
    /// </summary>
    private bool isDragPrimitive;

    /// <summary>
    /// Позиция, за которую ухватили при перетаскивании примитив (относительно его края)
    /// </summary>
//    private Point primitiveHandlePoint = new Point(0, 0);

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {   
      if (!isDragPrimitive)
      {
        //CatchPrimitive(e.GetPosition(this));
        isDragPrimitive = true;
      }
      base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
      if (isDragPrimitive)
      {
        //PutPrimitive(e.GetPosition(this));
        isDragPrimitive = false;
      }
      base.OnMouseLeftButtonUp(e);
    }

    #endregion
  }
}
