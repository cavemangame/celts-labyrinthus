using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    /// Отдельный вижуал сетки
    /// </summary>
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

    private readonly Dictionary<PrimitiveInfo, GridPoint> primitives = new Dictionary<PrimitiveInfo, GridPoint>();

    #endregion

    #region Ресурсы
 
    private readonly Brush edgeBrush = new SolidColorBrush(Colors.LightGray);
    private readonly Pen edgePen;
    private readonly Brush primitiveSlideBrush = new SolidColorBrush(Colors.AntiqueWhite);
    private readonly Pen primitiveSlidePen;
    private readonly Brush primitiveBorderBrush = new SolidColorBrush(Colors.Black);
    private readonly Pen primitiveBorderPen;

    #endregion

    #region Constructor

    public PrimitivePackCanvas()
    {
      edgePen = new Pen(edgeBrush, 1.0);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);

      primitiveSlideBrush.Opacity = 0.25;
      primitiveSlidePen = new Pen(primitiveBorderBrush, 0.3) { DashStyle = DashStyles.Dash };

      var wnd = (WindowMaster) Application.Current.MainWindow;
      if (wnd != null)
      {
        primitives.Add(wnd.Primitive, new GridPoint(0, 0));
      }
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
      DrawPrimitives();
    }

    #endregion

    #region Отрисовка

    /// <summary>
    /// Создание и отрисовка всех ребер
    /// </summary>
    private void DrawEdges()
    {
      double step = GetDrawStep();
      gridVisual = new DrawingVisual();

      using (DrawingContext dc = gridVisual.RenderOpen())
      {
        int width = GetMaxWidth()*CELLS_COUNT_KOEF;
        int height = GetMaxHeigth()*CELLS_COUNT_KOEF;
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
      foreach (KeyValuePair<PrimitiveInfo, GridPoint> primitive in primitives)
      {
        DrawPrimitive(primitive.Value, primitive.Key, GetDrawStep());
      }
    }


    private void DrawPrimitive(GridPoint corner, PrimitiveInfo primitiveInfo, double step)
    {
      var primitiveVisual = new DrawingVisual();

      // рисуем примитив в одном вижуале, чтоб работать с ним далее как с единым целым
      using (DrawingContext dc = primitiveVisual.RenderOpen())
      {
        dc.DrawRectangle(primitiveSlideBrush, primitiveSlidePen,
                         new Rect(corner.X*step + SHIFT_FROM_BORDER, corner.Y*step + SHIFT_FROM_BORDER,
                                  primitiveInfo.Width*step, primitiveInfo.Height*step));

        foreach (var lineInfo in primitiveInfo.Lines)
        {
          dc.DrawLine(primitiveBorderPen,
                      new Point((lineInfo.X0 + corner.X)*step + SHIFT_FROM_BORDER,
                                (lineInfo.Y0 + corner.Y)*step + SHIFT_FROM_BORDER),
                      new Point((lineInfo.X1 + corner.X)*step + SHIFT_FROM_BORDER,
                                (lineInfo.Y1 + corner.Y)*step + SHIFT_FROM_BORDER));
        }
      }
      AddVisual(primitiveVisual);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Выдаем размер клетки для сетки. Здесь учитываем коэффициент (3) для свободной манипуляции примитивами
    /// </summary>
    private double GetDrawStep()
    {
      double step = double.MaxValue;
      foreach (PrimitiveInfo primitiveInfo in primitives.Keys)
      {
        int cells = CELLS_COUNT_KOEF * Math.Max(primitiveInfo.Height, primitiveInfo.Width);
        step = Math.Min(step, Math.Min((ActualHeight - 2 * SHIFT_FROM_BORDER) / cells,
                             (ActualWidth - 2 * SHIFT_FROM_BORDER) / cells));

      }
      return step;
    }

    private int GetMaxWidth()
    {
      int width = 5;
      foreach (PrimitiveInfo primitiveInfo in primitives.Keys)
      {
        width = Math.Max(width, primitiveInfo.Width);
      }
      return width; 
    }

     private int GetMaxHeigth()
     {
       int height = 5;
       foreach (PrimitiveInfo primitiveInfo in primitives.Keys)
       {
         height = Math.Max(height, primitiveInfo.Height);
       }
       return height;
     }

    private GridPoint ConvertToGridPoint(Point pt)
    {
      double step = GetDrawStep();
      var x = (int)((pt.X - SHIFT_FROM_BORDER) / step);
      var y = (int)((pt.Y - SHIFT_FROM_BORDER) / step);
      // пока левоверх
      return new GridPoint(x, y);
    }

    #endregion

    #region Манипуляции мышью

    /// <summary>
    /// тащим ли в данный момент примитив
    /// </summary>
    private bool isDraggingPrimitive;
    /// <summary>
    /// Позиция, за которую ухватили при перетаскивании примитив (относительно его края)
    /// </summary>
    private Vector clickOffset = new Vector(0, 0);

    private Point draggedPoint = new Point(0, 0);
    /// <summary>
    /// Текущий двигающийся вижуал
    /// </summary>
    private DrawingVisual selectedVisual = null;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {   
      if (!isDraggingPrimitive)
      {
        CatchPrimitive(e.GetPosition(this));
      }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
      if (isDraggingPrimitive)
      {
        PutPrimitive(e.GetPosition(this));
      }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
      {
        SetCursorState(e.GetPosition(this));
      }
      if (isDraggingPrimitive)
      {
        MovePrimitive(e.GetPosition(this));
      }
    }

    private void MovePrimitive(Point pt)
    {
      draggedPoint = pt + clickOffset;
      var move = new TranslateTransform(draggedPoint.X, draggedPoint.Y);
      selectedVisual.Transform = move;
    }

    private void CatchPrimitive(Point pointClicked)
    {
      var visual = GetVisual(pointClicked);
      if (visual != null && visual != gridVisual)
      {
        var topLeftCorner = new Point(
          visual.ContentBounds.TopLeft.X + edgePen.Thickness/2,
          visual.ContentBounds.TopLeft.Y + edgePen.Thickness/2);

        clickOffset = topLeftCorner - pointClicked;

        AddDraggingVisual(visual);
        Cursor = Cursors.Hand;
        isDraggingPrimitive = true;
      }
    }

    private void PutPrimitive(Point pt)
    {
      if (selectedVisual != null)
      {
        RemoveVisual(selectedVisual);
        //GetPrimitivePair(selectedVisual) = ConvertToGridPoint(pt);
      }
      selectedVisual = null;
      isDraggingPrimitive = false;
      Cursor = Cursors.Arrow;
    }


    private void SetCursorState(Point pt)
    {
      var drawingVisual = GetVisual(pt);
      if (drawingVisual != null && drawingVisual != gridVisual)
      {
        Cursor = Cursors.Hand;
      }
      else
      {
        Cursor = Cursors.Arrow;
      }
    }

    private void AddDraggingVisual(DrawingVisual visual)
    {
      selectedVisual = new DrawingVisual();
      using (DrawingContext dc = selectedVisual.RenderOpen())
      {
        dc.DrawDrawing(visual.Drawing);
      }
      selectedVisual.Opacity = 0.3;
      AddVisual(selectedVisual);
    }

    private DrawingVisual GetVisual(Point pt)
    {
      HitTestResult hitResult = VisualTreeHelper.HitTest(this, pt);
      return hitResult.VisualHit as DrawingVisual;
    }

    private GridPoint GetPrimitivePair(DrawingVisual visual)
    {
      return new GridPoint(0,0);
    }

    #endregion
  }
}
