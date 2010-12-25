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
    private const double SHIFT_FROM_BORDER = 10;
    #endregion

    #region Переменные
    /// <summary>
    /// Набор вижуалов канвы
    /// </summary>
    private readonly List<Visual> visuals = new List<Visual>();

    /// <summary>
    /// Мап (любой вижуал ребра) - (линия примитива).
    /// Используется для определения, над какой линей примитива расположена мышь.
    /// Вижуал ребра - ребро и прилегающая к нему область,
    ///                выделение ребра,
    ///                подсветка при наведении.
    /// </summary>
    private readonly Dictionary<Visual, LineInfo> edgeVisualPrimitiveMap = new Dictionary<Visual, LineInfo>();

    /// <summary>
    /// Мап (линия примитива) - (вижуал для отрисовки ее выделения)
    /// </summary>
    private readonly Dictionary<LineInfo, DrawingVisual> primitiveVisualMap = new Dictionary<LineInfo, DrawingVisual>();
    #endregion

    #region Ресурсы
    private readonly Brush edgeBrush = new SolidColorBrush(Colors.LightGray);
    private readonly Brush edgeAreaBrush = new SolidColorBrush(Colors.LightGray) { Opacity = 0 };
    private readonly Brush primitiveBorderBrush = new SolidColorBrush(Colors.Black);
    private readonly Pen edgePen;
    private readonly Pen primitiveBorderPen;
    #endregion

    #region Конструктор
    public PrimitiveCanvas()
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

      if (null == wnd)
      {
        return;
      }

      wnd.Primitive.Height = h;
      wnd.Primitive.Width = w;
      Refresh();
    }

    #endregion

    #region private методы
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
      edgeVisualPrimitiveMap.Clear();
      primitiveVisualMap.Clear();
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
          // вертикальные ребра
          if (j != primitiveInfo.Height)
          {
            var vertEdgeVisual = new DrawingVisual();
            var lineInfo = new LineInfo(i, j, i, j + 1);

            using (DrawingContext dc = vertEdgeVisual.RenderOpen())
            {
              var startPoint = new Point(i * step + SHIFT_FROM_BORDER, j * step + SHIFT_FROM_BORDER);
              var endPoint = new Point(i * step + SHIFT_FROM_BORDER, (j + 1) * step + SHIFT_FROM_BORDER);

              // само ребро
              dc.DrawLine(edgePen, startPoint, endPoint);

              // прилегающая область
              var segments = new PathSegmentCollection();

              if (i != 0)
              {
                segments.Add(new LineSegment(
                  new Point(startPoint.X - step / 2, (startPoint.Y + endPoint.Y) / 2), false));
              }
              segments.Add(new LineSegment(endPoint, false));
              if (i != primitiveInfo.Width)
              {
                segments.Add(new LineSegment(
                  new Point(startPoint.X + step / 2, (startPoint.Y + endPoint.Y) / 2), false));
              }

              var pathFigure = new PathFigure(startPoint, segments, true);
              var figures = new PathFigureCollection {pathFigure};
              var geometry = new PathGeometry(figures);

              dc.DrawGeometry(edgeAreaBrush, null, geometry);
            }

            edgeVisualPrimitiveMap.Add(vertEdgeVisual, lineInfo);
            AddVisual(vertEdgeVisual);
          }

          // горизонтальные ребра
          if (i != primitiveInfo.Width)
          {
            var horEdgeVisual = new DrawingVisual();
            var lineInfo = new LineInfo(i, j, i + 1, j);

            using (DrawingContext dc = horEdgeVisual.RenderOpen())
            {
              var startPoint = new Point(i * step + SHIFT_FROM_BORDER, j * step + SHIFT_FROM_BORDER);
              var endPoint = new Point((i + 1) * step + SHIFT_FROM_BORDER, j * step + SHIFT_FROM_BORDER);

              // само ребро
              dc.DrawLine(edgePen, startPoint, endPoint);

              // прилегающая область
              var segments = new PathSegmentCollection();

              if (j != 0)
              {
                segments.Add(new LineSegment(
                  new Point((startPoint.X +endPoint.X) / 2, startPoint.Y - step / 2), false));
              }
              segments.Add(new LineSegment(endPoint, false));
              if (j != primitiveInfo.Height)
              {
                segments.Add(new LineSegment(
                  new Point((startPoint.X + endPoint.X) / 2, startPoint.Y + step / 2), false));
              }

              var pathFigure = new PathFigure(startPoint, segments, true);
              var figures = new PathFigureCollection { pathFigure };
              var geometry = new PathGeometry(figures);

              dc.DrawGeometry(edgeAreaBrush, null, geometry);
            }

            edgeVisualPrimitiveMap.Add(horEdgeVisual, lineInfo);
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

        edgeVisualPrimitiveMap.Add(primitiveVisual, lineInfo);
        primitiveVisualMap.Add(lineInfo, primitiveVisual);
        AddVisual(primitiveVisual);
      }
    }


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
      var primitiveLineInfo = edgeVisualPrimitiveMap[edgeVisual];

      if (null == primitiveLineInfo)
      {
        return;
      }

      var primitiveInfo = wnd.Primitive;
      bool wasSelected = primitiveInfo.Lines.Any(lineInfo => primitiveLineInfo == lineInfo);

      if (wasSelected)
      {
        primitiveInfo.Lines.Remove(primitiveLineInfo);

        var primitiveVisual = primitiveVisualMap[primitiveLineInfo];

        edgeVisualPrimitiveMap.Remove(primitiveVisual);
        primitiveVisualMap.Remove(primitiveLineInfo);
        RemoveVisual(primitiveVisual);
      }
      else
      {
        primitiveInfo.Lines.Add(primitiveLineInfo);

        var primitiveVisual = new DrawingVisual();

        using (DrawingContext dc = primitiveVisual.RenderOpen())
        {
          var step = GetDrawStep();

          dc.DrawLine(primitiveBorderPen,
                      new Point(primitiveLineInfo.X0 * step + SHIFT_FROM_BORDER, primitiveLineInfo.Y0 * step + SHIFT_FROM_BORDER),
                      new Point(primitiveLineInfo.X1 * step + SHIFT_FROM_BORDER, primitiveLineInfo.Y1 * step + SHIFT_FROM_BORDER));
        }

        edgeVisualPrimitiveMap.Add(primitiveVisual, primitiveLineInfo);
        primitiveVisualMap.Add(primitiveLineInfo, primitiveVisual);
        AddVisual(primitiveVisual);
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
