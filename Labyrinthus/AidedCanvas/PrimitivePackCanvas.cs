using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Labyrinthus.Objects;

namespace Labyrinthus.AidedCanvas
{
  class PrimitivePackCanvas : Canvas
  {
    #region Переменные

    /// <summary>
    /// Вижуалы канвы
    /// </summary>
    private readonly List<Visual> visuals = new List<Visual>();

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

    #endregion

    #region Constructor

    public PrimitivePackCanvas()
    {
      edgePen = new Pen(edgeBrush, 1.0);
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

    private void RemoveVisual(Visual visual)
    {
      visuals.Remove(visual);
      RemoveVisualChild(visual);
      RemoveLogicalChild(visual);
    }

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
      //AddGrid();
    }

    #endregion

    #region Отрисовка
    /// <summary>
    /// Создание и отрисовка всех ребер
    /// </summary>
    private void DrawEdges()
    {
      var wnd = (WindowMaster) Window.GetWindow(this);

      if (null == wnd)
      {
        throw new InvalidOperationException(@"Не удалость найти главное окно приложения");
      }

      var primitiveInfo = wnd.Primitive;
      double step = GetDrawStep(wnd);
      var gridVisual = new DrawingVisual();

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

    private double GetDrawStep(WindowMaster wnd)
    {
      var primitiveInfo = wnd.Primitive;
      int cells = CELLS_COUNT_KOEF * Math.Max(primitiveInfo.Height, primitiveInfo.Width);
      double step = Math.Min((ActualHeight - 2 * SHIFT_FROM_BORDER) / cells,
                             (ActualWidth - 2 * SHIFT_FROM_BORDER) / cells) ;

      return step;
    }

    #endregion
  }
}
