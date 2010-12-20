using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Labyrinthus.AidedCanvas
{
  public class PrimitiveCanvas : Canvas
  {
    #region Переменные

    /// <summary>
    /// Вижуалы канвы
    /// </summary>
    private readonly List<Visual> visuals = new List<Visual>();

    /// <summary>
    /// Информация о примитиве в виде набора линий (линия == сторона клетки)
    /// </summary>
    public List<LineInfo> Primitive { get; set; }

    public int PrimitiveWidth { get; private set; }
    public int PrimitiveHeight { get; private set; }

    /// <summary>
    /// Расстояние от краев канвы до начала сетки примитива
    /// </summary>
    private const double shiftFromBorder = 10.0;

    #endregion

    #region Ресурсы

    private readonly Brush gridBrush;
    private readonly Brush primitiveBorderBrush;
    private readonly Pen gridPen;
    private readonly Pen primitiveBorderPen;

    #endregion

    #region Overrides

    public PrimitiveCanvas()
    {
      PrimitiveWidth = PrimitiveHeight = 0;
      Primitive = new List<LineInfo>();
      gridBrush = new SolidColorBrush(Colors.LightGray) { Opacity = 0.5 };
      gridPen = new Pen(gridBrush, 1.0);
      primitiveBorderBrush = new SolidColorBrush(Colors.Black);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
    }

    protected override int VisualChildrenCount
    {
      get { return visuals.Count; }
    }

    protected override Visual GetVisualChild(int index)
    {
      return visuals[index];
    }

    #endregion

    #region Public

    public void AddVisual(Visual visual)
    {
      visuals.Add(visual);
      AddVisualChild(visual);
      AddLogicalChild(visual);
    }

    public void RemoveVisual(Visual visual)
    {
      visuals.Remove(visual);
      RemoveVisualChild(visual);
      RemoveLogicalChild(visual);
    }

    public void ClearAll()
    {
      foreach (var visual in visuals)
      {
        RemoveVisualChild(visual);
        RemoveLogicalChild(visual);
      }
      visuals.Clear();
    }

    public void Refresh()
    {
      ClearAll();
      AddGrid();
    }

    public void SetNewSizes(int h, int w)
    {
      PrimitiveWidth = w;
      PrimitiveHeight = h;
      Refresh();
    }

    /// <summary>
    /// Добавление сетки
    /// </summary>
    private void AddGrid()
    {
      var visual = new DrawingVisual();
      int cells = Math.Max(PrimitiveHeight, PrimitiveWidth);
      double step = Math.Min((ActualHeight - 2 * shiftFromBorder) / cells,
        (ActualWidth - 2 * shiftFromBorder) / cells);

      using (DrawingContext dc = visual.RenderOpen())
      {
        //рисуем сетку
        for (int i = 0; i <= PrimitiveWidth; i++)
        {
          dc.DrawLine(gridPen,
            new Point(i * step + shiftFromBorder, shiftFromBorder),
            new Point(i * step + shiftFromBorder, PrimitiveHeight * step + shiftFromBorder));
        }
        for (int i = 0; i <= PrimitiveHeight; i++)
        {
          dc.DrawLine(gridPen,
            new Point(shiftFromBorder, i * step + shiftFromBorder),
            new Point(PrimitiveWidth * step + shiftFromBorder, i * step + shiftFromBorder));
        }

        foreach (var lineInfo in Primitive)
        {
          dc.DrawLine(primitiveBorderPen, 
            new Point(lineInfo.X0 * step + shiftFromBorder, lineInfo.Y0 * step + shiftFromBorder),
            new Point(lineInfo.X1 * step + shiftFromBorder, lineInfo.Y1 * step + shiftFromBorder));
        }
      }
      
      AddVisual(visual);
    }
    #endregion

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
      Point pos = e.GetPosition(this);
      int cells = Math.Max(PrimitiveHeight, PrimitiveWidth);
      double step = Math.Min((ActualHeight - 2 * shiftFromBorder) / cells,
        (ActualWidth - 2 * shiftFromBorder) / cells);

      // вертикальные
      for (int i = 0; i <= PrimitiveWidth; i++)
      {
        for (int j = 0; j < PrimitiveHeight; j++)
        {
          var hitRect = new Rect(i * step + shiftFromBorder - 2, j * step + shiftFromBorder + 2, 4, step - 4);
          if (hitRect.Contains(pos))
          {
            var hitLine = new LineInfo(i, j, i, j+1);
            bool hasThisLine = false;
            foreach (var lineInfo in Primitive)
            {
              if (hitLine.Equals(lineInfo))
              {
                hasThisLine = true;
                break;
              }
            }

            if (hasThisLine)
            {
              Primitive.Remove(hitLine);
            }
            else
            {
              Primitive.Add(hitLine);
            }
          }
        }
      }

      // горизонтальные
      for (int i = 0; i < PrimitiveWidth; i++)
      {
        for (int j = 0; j <= PrimitiveHeight; j++)
        {
          var hitRect = new Rect(i * step + shiftFromBorder + 2, j * step + shiftFromBorder - 2, step - 4, 4);
          if (hitRect.Contains(pos))
          {
            var hitLine = new LineInfo(i, j, i + 1, j);
            bool hasThisLine = false;
            foreach (var lineInfo in Primitive)
            {
              if (hitLine.Equals(lineInfo))
              {
                hasThisLine = true;
                break;
              }
            }

            if (hasThisLine)
            {
              Primitive.Remove(hitLine);
            }
            else
            {
              Primitive.Add(hitLine);
            }
          }
        }
      }
      Refresh();
    }
  }
}