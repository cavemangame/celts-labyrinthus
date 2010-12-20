using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Labyrinthus.AidedCanvas;
using Microsoft.Win32;

namespace Labyrinthus.Pages
{
  /// <summary>
  /// Interaction logic for PageShowLabyrinthus.xaml
  /// </summary>
  public partial class PageShowLabyrinthus : Page
  {
    private int width;
    private int height;
    private List<LineInfo> primitive = new List<LineInfo>();

    private readonly Brush primitiveBorderBrush;
    private readonly Pen primitiveBorderPen;
    private readonly Brush floorBrush;

    public PageShowLabyrinthus()
    {
      InitializeComponent();
      primitiveBorderBrush = new SolidColorBrush(Colors.Black);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
      floorBrush = new SolidColorBrush(Colors.Brown);
    }

    #region Реакция на действия пользователя

    private void SaveLabyrinthus(object sender, RoutedEventArgs e)
    {
      if (LabyrinthusImage.Source != null && LabyrinthusImage.Source is BitmapSource)
      {
        var dlg = new SaveFileDialog
                               {
                                 FileName = "Labyrinthus",
                                 DefaultExt = ".png",
                                 Filter = "Картинки (.png)|*.png"
                               };

        var result = dlg.ShowDialog();

        if (result == true)
        {
          string filename = dlg.FileName;
          using (var fileStream = new FileStream(filename, FileMode.Create))
          {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)LabyrinthusImage.Source));
            encoder.Save(fileStream);
          }
        }
      }
    }

    private void PrimitiveZoom_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (IsInitialized)
        UpdatePicture();
    }

    private void PicHeight_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (IsInitialized)
        UpdatePicture();
    }

    private void PicWidth_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (IsInitialized)
        UpdatePicture();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      var mainWindow = (WindowMaster)Window.GetWindow(this);
      if (mainWindow != null)
      {
        SetPrimitiveData(mainWindow.PrimitiveWidth, mainWindow.PrimitiveHeight, mainWindow.info);
      }
      UpdatePicture();
    }

    private void FloorColorLabelClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      FloorColorPopup.IsOpen = true;
    }

    #endregion

    #region Отрисовка лабиринта

    private void SetPrimitiveData(int w, int h, List<LineInfo> infos)
    {
      width = w;
      height = h;
      primitive = infos;
    }

    private void UpdatePicture()
    {

      if (width <= 0 || height <= 0)
      {
        return;
      }
      int picHeight, picWidth;
      double zoom;
      if (!Int32.TryParse(PicHeight.Text, out picHeight) || !Int32.TryParse(PicWidth.Text, out picWidth)
        || !Double.TryParse(PrimitiveZoom.Text, out zoom))
      {
        return;
      }
      LabyrinthusImage.Width = picWidth;
      LabyrinthusImage.Height = picHeight;

      // определяем количество примитивов по высоте и ширине рисунка
      var hCount = (int)(picHeight/ (zoom * height));
      var wCount = (int)(picWidth / (zoom * width));

      var drawingVisual = new DrawingVisual();
      
      using (DrawingContext dc = drawingVisual.RenderOpen())
      {
        dc.DrawRectangle(floorBrush, primitiveBorderPen, new Rect(0, 0, picWidth, picHeight));
        // рисуем
        for (int i = 0; i <= wCount; i++)
        {
          for (int j = 0; j <= hCount; j++)
          {
            DrawPrimitive(i, j, zoom, dc);
          }
        }
      }

      var bmp = new RenderTargetBitmap(picWidth, picHeight, 96, 96, PixelFormats.Pbgra32);
      bmp.Render(drawingVisual);
      LabyrinthusImage.Source = bmp;
    }

    private void DrawPrimitive(int i, int j, double zoom, DrawingContext dc)
    {
      foreach (var lineInfo in primitive)
      {
        dc.DrawLine(primitiveBorderPen,
                    new Point((lineInfo.X0 + width * i) * zoom, (lineInfo.Y0 + height * j) * zoom),
                    new Point((lineInfo.X1 + width * i) * zoom, (lineInfo.Y1 + height * j) * zoom));
      }
    }

    #endregion

  }
}
