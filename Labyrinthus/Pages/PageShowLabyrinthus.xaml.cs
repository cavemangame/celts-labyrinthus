using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Labyrinthus.Pages
{
  public partial class PageShowLabyrinthus : Page
  {
    #region Ресурсы
    // TODO: добавить таску запихнуть все дефолтные кисти в ресурсы
    private readonly Brush primitiveBorderBrush;
    private readonly Pen primitiveBorderPen;
    private readonly Brush floorBrush;

    #endregion

    #region Конструктор

    public PageShowLabyrinthus()
    {
      InitializeComponent();
      primitiveBorderBrush = new SolidColorBrush(Colors.Black);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
      floorBrush = new SolidColorBrush(Colors.Brown);
    }

    #endregion

    #region Реакция на действия пользователя

    private void SaveLabyrinthus(object sender, RoutedEventArgs e)
    {
      OnSaveLabyrinthus();
    }

    private void PrimitiveZoom_TextChanged(object sender, TextChangedEventArgs e)
    {
      UpdatePicture();
    }

    private void PicHeight_TextChanged(object sender, TextChangedEventArgs e)
    {
      UpdatePicture();
    }

    private void PicWidth_TextChanged(object sender, TextChangedEventArgs e)
    {
      UpdatePicture();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      UpdatePicture();
    }

    private void FloorColorLabelClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      FloorColorPopup.IsOpen = true;
    }

    #endregion

    #region Отрисовка лабиринта

    private void UpdatePicture()
    {
      if (!IsInitialized)
      {
        return;
      }
      var wnd = (WindowMaster) Window.GetWindow(this);
      if (wnd != null)
      {
        if (wnd.Primitive.Width <= 0 || wnd.Primitive.Height <= 0)
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
        var hCount = (int) (picHeight / (zoom * wnd.Primitive.Height));
        var wCount = (int) (picWidth / (zoom * wnd.Primitive.Width));

        var drawingVisual = new DrawingVisual();

        DrawingContext dc = drawingVisual.RenderOpen();
        
        //using (DrawingContext dc = drawingVisual.RenderOpen())
        {
          dc.DrawRectangle(floorBrush, primitiveBorderPen, new Rect(0, 0, picWidth, picHeight));
          // рисуем
          for (int i = 0; i <= wCount; i++)
          {
            for (int j = 0; j <= hCount; j++)
            {
              DrawPrimitive(wnd, i, j, zoom, dc);
            }
          }
        }
        dc.Close();

        var bmp = new RenderTargetBitmap(picWidth, picHeight, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(drawingVisual);
        LabyrinthusImage.Source = bmp;
      }
    }

    private void DrawPrimitive(WindowMaster wnd, int i, int j, double zoom, DrawingContext dc)
    {
      var geo = new StreamGeometry();
      using (var geoContext = geo.Open())
      {
        foreach (var lineInfo in wnd.Primitive.Lines)
        {
          geoContext.BeginFigure(
             new Point((lineInfo.X0 + wnd.Primitive.Width * i) * zoom, (lineInfo.Y0 + wnd.Primitive.Height * j) * zoom), false, false);
          geoContext.LineTo(
            new Point((lineInfo.X1 + wnd.Primitive.Width * i) * zoom, (lineInfo.Y1 + wnd.Primitive.Height * j) * zoom), true, false);     
        }
      }

      dc.DrawGeometry(primitiveBorderBrush, primitiveBorderPen, geo);
    }

    #endregion

    #region Сохранение

    private void OnSaveLabyrinthus()
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

    #endregion
  }
}
