﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Labyrinthus.AidedCanvas;
using Labyrinthus.Objects;
using Microsoft.Win32;

namespace Labyrinthus.Pages
{
  public partial class PagePreparePrimitives
  {
    #region Свойства
    public int PrimitiveWidth { get; set; }
    public int PrimitiveHeight { get; set; }
    #endregion

    #region Конструктор
    public PagePreparePrimitives()
    {
      InitializeComponent();

      PrimitiveWidth = 5;
      PrimitiveHeight = 5;

      DataContext = this;
    }
    #endregion

    #region Обработка событий
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      RefreshCanvas();
    }

    private void Primitive_TextChanged(object sender, RoutedEventArgs e)
    {
      if (IsLoaded)
      {
        var primitive = ((WindowMaster)Application.Current.MainWindow).Primitive;

        PrimitiveCanvas.Children.Clear();
        primitive.Clear();

        RefreshCanvas();
      }
    }

    private void PrimitiveCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      var primitiveEdge = e.Source as PrimitiveEdgeControl;

      if (null == primitiveEdge)
      {
        return;
      }

      var primitiveLineInfo = GetPrimitiveLineInfo(primitiveEdge);
      var primitiveInfo = ((WindowMaster)Application.Current.MainWindow).Primitive;
      bool wasSelected = primitiveInfo.Lines.Any(lineInfo => primitiveLineInfo == lineInfo);

      if (wasSelected)
      {
        primitiveInfo.Lines.Remove(primitiveLineInfo);
      }
      else
      {
        primitiveInfo.Lines.Add(primitiveLineInfo);
      }
      primitiveEdge.ClickEdge(!wasSelected);
    }
    #endregion

    #region Загрузка и сохранение
    private void LoadPrimitive(object sender, RoutedEventArgs e)
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;
      var dlg = new OpenFileDialog
      {
        DefaultExt = ".pmv",
        Filter = "Примитивы (.pmv)|*.pmv"
      };

      var result = dlg.ShowDialog();

      if (result == true)
      {
        wnd.Primitive.Deserialize(dlg.FileName);
        PrimitiveWidth = wnd.Primitive.Width;
        PrimitiveHeight = wnd.Primitive.Height;
        PrimitiveCanvas.Children.Clear();
        RefreshCanvas();
      }
    }

    private void SavePrimitive(object sender, RoutedEventArgs e)
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;
      var dlg = new SaveFileDialog
      {
        FileName = "Примитив",
        DefaultExt = ".pmv",
        Filter = "Примитивы (.pmv)|*.pmv"
      };

      var result = dlg.ShowDialog();

      if (result == true)
      {
        wnd.Primitive.Serialize(dlg.FileName);
      }
    }
    #endregion

    #region private методы
    private void RefreshCanvas()
    {
      var primitive = ((WindowMaster)Application.Current.MainWindow).Primitive;

      primitive.Height = PrimitiveHeight;
      primitive.Width = PrimitiveWidth;

      int cells = Math.Max(PrimitiveHeight, PrimitiveWidth);
      double step = Math.Min((PrimitiveCanvas.Height) / cells,
                             (PrimitiveCanvas.Width) / cells);

      for (int i = 0; i <= PrimitiveWidth; i++)
      {
        for (int j = 0; j <= PrimitiveHeight; j++)
        {
          if (i != PrimitiveWidth)
          {
            DrawPrimitiveEdge(step, PrimitiveEdgeControl.EdgeTypes.Horizontal, i, j);
          }

          if (j != PrimitiveHeight)
          {
            DrawPrimitiveEdge(step, PrimitiveEdgeControl.EdgeTypes.Vertical, i, j);
          }
        }
      }
    }

    private void DrawPrimitiveEdge(double edgeSize,
      PrimitiveEdgeControl.EdgeTypes edgeType, int i, int j)
    {
      var edge = new PrimitiveEdgeControl
      {
        EdgeSize = edgeSize,
        EdgeType = edgeType,
        PrimitiveLineStart = new Point(i, j)
      };

      var primitiveLineInfo = GetPrimitiveLineInfo(edge);
      var primitiveInfo = ((WindowMaster)Application.Current.MainWindow).Primitive;
      bool wasSelected = primitiveInfo.Lines.Any(lineInfo => primitiveLineInfo == lineInfo);

      if (wasSelected)
      {
        edge.ClickEdge(true);
      }

      Canvas.SetTop(edge, j * edgeSize);
      Canvas.SetLeft(edge, i * edgeSize);
      PrimitiveCanvas.Children.Add(edge);
    }

    private static LineInfo GetPrimitiveLineInfo(PrimitiveEdgeControl primitiveEdge)
    {
      var primitiveLineInfo = new LineInfo(
        (int)primitiveEdge.PrimitiveLineStart.X,
        (int)primitiveEdge.PrimitiveLineStart.Y,
        (int)(PrimitiveEdgeControl.EdgeTypes.Horizontal == primitiveEdge.EdgeType ? primitiveEdge.PrimitiveLineStart.X + 1 : primitiveEdge.PrimitiveLineStart.X),
        (int)(PrimitiveEdgeControl.EdgeTypes.Vertical == primitiveEdge.EdgeType ? primitiveEdge.PrimitiveLineStart.Y + 1 : primitiveEdge.PrimitiveLineStart.Y));

      return primitiveLineInfo;
    }
    #endregion
  }
}
