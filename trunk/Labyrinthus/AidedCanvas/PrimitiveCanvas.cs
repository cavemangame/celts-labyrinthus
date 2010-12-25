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
    #region Константы
    /// <summary>
    /// Расстояние от краев канвы до начала сетки примитива
    /// </summary>
    private const double SHIFT_FROM_BORDER = 10.0;
    #endregion

    #region Переменные
    /// <summary>
    /// Ребра и прилегающая область (http://code.google.com/p/celts-labyrinthus/issues/detail?id=7)
    /// </summary>
    private readonly List<Visual> edgesVisuals = new List<Visual>();

    /// <summary>
    /// Мап ребер и примитивов
    /// </summary>
    private readonly Dictionary<Visual, LineInfo> primitivesMap = new Dictionary<Visual, LineInfo>();
    #endregion

    #region Ресурсы
    private readonly Brush gridBrush;
    private readonly Brush primitiveBorderBrush;
    private readonly Pen gridPen;
    private readonly Pen primitiveBorderPen;
    #endregion

    #region Конструктор
    public PrimitiveCanvas()
    {
      gridBrush = new SolidColorBrush(Colors.LightGray) { Opacity = 0.8 };
      gridPen = new Pen(gridBrush, 1.0);
      primitiveBorderBrush = new SolidColorBrush(Colors.Black);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
    }
    #endregion

    #region Overrides
    protected override int VisualChildrenCount
    {
      get { return edgesVisuals.Count; }
    }

    protected override Visual GetVisualChild(int index)
    {
      return edgesVisuals[index];
    }

    #endregion

    #region public методы
    public void Refresh()
    {
      ClearAll();
      DrawEdges();
      DrawPrimitives();
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

    #region private методы
    public void AddVisual(Visual visual)
    {
      edgesVisuals.Add(visual);
      AddVisualChild(visual);
      AddLogicalChild(visual);
    }

    public void RemoveVisual(Visual visual)
    {
      edgesVisuals.Remove(visual);
      RemoveVisualChild(visual);
      RemoveLogicalChild(visual);
    }

    public void ClearAll()
    {
      foreach (var visual in edgesVisuals)
      {
        RemoveVisualChild(visual);
        RemoveLogicalChild(visual);
      }
      edgesVisuals.Clear();
    }
    #endregion

    #region Обработка событий
    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
      ClickBorder(e.GetPosition(this));
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
//      HighlightBorder(e.GetPosition(this));
    }
    #endregion

    #region Отрисовка
    /// <summary>
    /// Создание и отрисовка всех ребер
    /// </summary>
    private void DrawEdges()
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        return;
      }

      var primitiveInfo = wnd.Primitive;
      double step = GetDrawStep();

      for (int i = 0; i <= primitiveInfo.Width; i++)
      {
        for (int j = 0; j <= primitiveInfo.Height; j++)
        {
          if (j != primitiveInfo.Height)
          {
            var vertEdgeVisual = new DrawingVisual();

            using (DrawingContext dc = vertEdgeVisual.RenderOpen())
            {
              dc.DrawLine(gridPen,
                          new Point(i * step + SHIFT_FROM_BORDER, j *       step + SHIFT_FROM_BORDER),
                          new Point(i * step + SHIFT_FROM_BORDER, (j + 1) * step + SHIFT_FROM_BORDER));
            }
            primitivesMap.Add(vertEdgeVisual, new LineInfo(i, j, i, j + 1));
            AddVisual(vertEdgeVisual);
          }

          if (i != primitiveInfo.Width)
          {
            var horEdgeVisual = new DrawingVisual();

            using (DrawingContext dc = horEdgeVisual.RenderOpen())
            {
              dc.DrawLine(gridPen,
                          new Point(i *       step + SHIFT_FROM_BORDER, j * step + SHIFT_FROM_BORDER),
                          new Point((i + 1) * step + SHIFT_FROM_BORDER, j * step + SHIFT_FROM_BORDER));
            }
            primitivesMap.Add(horEdgeVisual, new LineInfo(i, j, i + 1, j));
            AddVisual(horEdgeVisual);
          }
        }
      }
    }


    /// <summary>
    /// Создание и отрисовка всех примитивов
    /// </summary>
    private void DrawPrimitives()
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        return;
      }

      var primitiveInfo = wnd.Primitive;
      double step = GetDrawStep();

      foreach (var lineInfo in primitiveInfo.Lines)
      {
          var primitiveVisual = new DrawingVisual();

          using (DrawingContext dc = primitiveVisual.RenderOpen())
          {
            dc.DrawLine(primitiveBorderPen,
                        new Point(lineInfo.X0 * step + SHIFT_FROM_BORDER, lineInfo.Y0 * step + SHIFT_FROM_BORDER),
                        new Point(lineInfo.X1 * step + SHIFT_FROM_BORDER, lineInfo.Y1 * step + SHIFT_FROM_BORDER));
          }

          AddVisual(primitiveVisual);
      }
    }


/*
    /// <summary>
    /// Возвращает линию, над областью которой находится точка
    /// </summary>
    /// <param name="pos">точка</param>
    /// <returns></returns>
    private LineInfo GetPointedLine(Point pos)
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        return null;
      }

      var primitiveInfo = wnd.Primitive;
      int cells = Math.Max(primitiveInfo.Height, primitiveInfo.Width);
      double step = Math.Min((ActualHeight - 2 * SHIFT_FROM_BORDER) / cells,
                             (ActualWidth - 2 * SHIFT_FROM_BORDER) / cells);

      for (int i = 0; i <= primitiveInfo.Width; i++)
      {
        for (int j = 0; j <= primitiveInfo.Height; j++)
        {
          if (j != primitiveInfo.Height)
          {
            var vertHitRect = new Rect(i * step + SHIFT_FROM_BORDER - 2, j * step + SHIFT_FROM_BORDER + 2, 4, step - 4);

            if (vertHitRect.Contains(pos))
            {
              return new LineInfo(i, j, i, j + 1);
            }
          }

          if (i != primitiveInfo.Width)
          {
            var horHitRect = new Rect(i * step + SHIFT_FROM_BORDER + 2, j * step + SHIFT_FROM_BORDER - 2, step - 4, 4);

            if (horHitRect.Contains(pos))
            {
              return new LineInfo(i, j, i + 1, j);
            }
          }
        }
      }
      return null;
    }
*/

/*
    /// <summary>
    /// Подсветить область над границей
    /// </summary>
    /// <param name="pos"></param>
    private void HighlightBorder(Point pos)
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

      if (highlightVisual != null)
      {
        RemoveVisual(highlightVisual);
      }
      highlightVisual = new DrawingVisual();

      var primitiveInfo = wnd.Primitive;
      int cells = Math.Max(primitiveInfo.Height, primitiveInfo.Width);
      double step = Math.Min((ActualHeight - 2 * SHIFT_FROM_BORDER) / cells,
                             (ActualWidth - 2 * SHIFT_FROM_BORDER) / cells);

      using (DrawingContext dc = highlightVisual.RenderOpen())
      {
        var brush = new SolidColorBrush(Colors.Red);
        bool isHorLine = hitLine.Y0 == hitLine.Y1;

        brush.Opacity = 0.1;
        dc.DrawEllipse(brush, null,
                       new Point((hitLine.X0 + hitLine.X1) / 2d * step + SHIFT_FROM_BORDER,
                                 (hitLine.Y0 + hitLine.Y1) / 2d * step + SHIFT_FROM_BORDER),
                       isHorLine ? 18 : 6, !isHorLine ? 18 : 6);

        AddVisual(highlightVisual);
      }
    }
*/


    private void ClickBorder(Point pos)
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        return;
      }

      var hitTest =  VisualTreeHelper.HitTest(this, pos);
      var edgeVisual= (DrawingVisual)hitTest.VisualHit;
      var primitiveLineInfo = primitivesMap[edgeVisual];

      if (null == primitiveLineInfo)
      {
        return;
      }

      var primitiveInfo = wnd.Primitive;
      bool wasSelected = primitiveInfo.Lines.Any(lineInfo => primitiveLineInfo == lineInfo);

      if (wasSelected)
      {
        primitiveInfo.Lines.Remove(primitiveLineInfo);
      }
      else
      {
        primitiveInfo.Lines.Add(primitiveLineInfo);
      }

      using (DrawingContext dc = edgeVisual.RenderOpen())
      {
        var step = GetDrawStep();

        dc.DrawLine(wasSelected ? gridPen : primitiveBorderPen,
                    new Point(primitiveLineInfo.X0 * step + SHIFT_FROM_BORDER, primitiveLineInfo.Y0 * step + SHIFT_FROM_BORDER),
                    new Point(primitiveLineInfo.X1 * step + SHIFT_FROM_BORDER, primitiveLineInfo.Y1 * step + SHIFT_FROM_BORDER));
      }
    }


    private double GetDrawStep()
    {
      var wnd = (WindowMaster)Window.GetWindow(this);

      if (null == wnd)
      {
        throw new InvalidOperationException(@"Не удалость найти Window");
      }

      var primitiveInfo = wnd.Primitive;
      int cells = Math.Max(primitiveInfo.Height, primitiveInfo.Width);
      double step = Math.Min((ActualHeight - 2 * SHIFT_FROM_BORDER) / cells,
                             (ActualWidth - 2 * SHIFT_FROM_BORDER) / cells);

      return step;
    }
    #endregion
  }
}
