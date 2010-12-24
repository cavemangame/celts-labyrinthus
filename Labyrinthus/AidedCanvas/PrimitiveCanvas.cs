using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Labyrinthus.Objects;

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
      var wnd = (WindowMaster)Window.GetWindow(this);
      if (wnd != null)
      {
        wnd.Primitive.Height = h;
        wnd.Primitive.Width = w;
      }
      Refresh();
    }

    
    #endregion

    #region Обработка событий
    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
      TrySetUserPrimitiveAction(e.GetPosition(this));
    }
    #endregion

    #region Отрисовка
    /// <summary>
    /// Добавление сетки
    /// </summary>
    private void AddGrid()
    {
      var wnd = (WindowMaster)Window.GetWindow(this);
      if (wnd != null)
      {
        var visual = new DrawingVisual();
        int cells = Math.Max(wnd.Primitive.Height, wnd.Primitive.Width);
        double step = Math.Min((ActualHeight - 2 * shiftFromBorder) / cells,
                               (ActualWidth - 2 * shiftFromBorder) / cells);

        using (DrawingContext dc = visual.RenderOpen())
        {
          //рисуем сетку
          for (int i = 0; i <= wnd.Primitive.Width; i++)
          {
            dc.DrawLine(gridPen,
                        new Point(i * step + shiftFromBorder, shiftFromBorder),
                        new Point(i * step + shiftFromBorder, wnd.Primitive.Height * step + shiftFromBorder));
          }
          for (int i = 0; i <= wnd.Primitive.Height; i++)
          {
            dc.DrawLine(gridPen,
                        new Point(shiftFromBorder, i * step + shiftFromBorder),
                        new Point(wnd.Primitive.Width * step + shiftFromBorder, i * step + shiftFromBorder));
          }

          foreach (var lineInfo in wnd.Primitive.Lines)
          {
            dc.DrawLine(primitiveBorderPen,
                        new Point(lineInfo.X0 * step + shiftFromBorder, lineInfo.Y0 * step + shiftFromBorder),
                        new Point(lineInfo.X1 * step + shiftFromBorder, lineInfo.Y1 * step + shiftFromBorder));
          }
        }

        AddVisual(visual);
      }
    }

    private LineInfo GetPointedLine(Point pos)
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        return null;
      }

      var primitiveInfo = wnd.Primitive;
      int cells = Math.Max(primitiveInfo.Height, primitiveInfo.Width);
      double step = Math.Min((ActualHeight - 2 * shiftFromBorder) / cells,
                             (ActualWidth - 2 * shiftFromBorder) / cells);

      for (int i = 0; i <= primitiveInfo.Width; i++)
      {
        for (int j = 0; j < primitiveInfo.Height; j++)
        {
          var vertHitRect = new Rect(i * step + shiftFromBorder - 2, j * step + shiftFromBorder + 2, 4, step - 4);

          if (vertHitRect.Contains(pos))
          {
            return new LineInfo(i, j, i, j + 1);
          }

          var horHitRect = new Rect(i * step + shiftFromBorder + 2, j * step + shiftFromBorder - 2, step - 4, 4);

          if (horHitRect.Contains(pos))
          {
            return new LineInfo(i, j, i + 1, j);
          }
        }
      }
      return null;
    }

    private void TrySetUserPrimitiveAction(Point pos)
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        return;
      }

      var hitLine = GetPointedLine(pos);

      if (null == hitLine)
      {
        return;
      }

      var primitiveInfo = wnd.Primitive;
      bool hasThisLine = primitiveInfo.Lines.Any(lineInfo => hitLine == lineInfo);

      if (hasThisLine)
      {
        primitiveInfo.Lines.Remove(hitLine);
      }
      else
      {
        primitiveInfo.Lines.Add(hitLine);
      }

      Refresh();
    }
    #endregion
  }
}
