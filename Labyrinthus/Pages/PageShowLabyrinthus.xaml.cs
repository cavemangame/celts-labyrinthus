using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Labyrinthus.Pages
{
  public partial class PageShowLabyrinthus
  {
    #region Ресурсы
    // TODO: добавить таску запихнуть все дефолтные кисти в ресурсы
    private Brush floorBrush;
    private Brush primitiveBorderBrush;
    private Pen primitiveBorderPen;

    #endregion

    #region Свойства

    /// <summary>
    /// Цвет пола. Задается в ColorPicker.
    /// </summary>
    public Color FloorColor
    {
      get { return (Color)GetValue(FloorColorProperty); }
      set { SetValue(FloorColorProperty, value); }
    }
    public static readonly DependencyProperty FloorColorProperty = DependencyProperty.Register(
      "FloorColor", typeof(Color), typeof(PageShowLabyrinthus),
      new FrameworkPropertyMetadata(Colors.Brown, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                    SelectedColorPropertyChanged));

    /// <summary>
    /// Цвет стен. Задается в ColorPicker.
    /// </summary>
    public Color BorderColor
    {
      get { return (Color)GetValue(BorderColorProperty); }
      set { SetValue(BorderColorProperty, value); }
    }
    public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(
      "BorderColor", typeof(Color), typeof(PageShowLabyrinthus),
      new FrameworkPropertyMetadata(Colors.Brown, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                    SelectedColorPropertyChanged));

    private static void SelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var page = (PageShowLabyrinthus)d;

      page.UpdatePicture();
    }
    #endregion

    #region Конструктор

    public PageShowLabyrinthus()
    {
      InitializeComponent();

      FloorColor = Colors.Brown;
      BorderColor = Colors.Black;

      DataContext = this;
    }

    #endregion

    #region Обработка событий

    private void LabyrinthusParams_Changed(object sender, RoutedEventArgs e)
    {
      UpdatePicture();
    }

    private void SaveLabyrinthus(object sender, RoutedEventArgs e)
    {
      OnSaveLabyrinthus();
    }

    #endregion

    #region private методы
    #region Отрисовка лабиринта

    private void UpdatePicture()
    {
      if (!IsInitialized)
      {
        return;
      }

      primitiveBorderBrush = new SolidColorBrush(BorderColor);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
      floorBrush = new SolidColorBrush(FloorColor);

      var wnd = (WindowMaster)Window.GetWindow(this);
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
        var hCount = (int)(picHeight / (zoom * wnd.Primitive.Height));
        var wCount = (int)(picWidth / (zoom * wnd.Primitive.Width));

        var drawingVisual = new DrawingVisual();

        DrawingContext dc = drawingVisual.RenderOpen();

        // фон
        dc.DrawRectangle(floorBrush, null, new Rect(0, 0, picWidth, picHeight));

        // рисуем стены
        for (int i = 0; i <= wCount; i++)
        {
          // на каждый примитив - медленней, на всё - медленней
          var geo = new StreamGeometry();
          using (var geoContext = geo.Open())
          {
            for (int j = 0; j <= hCount; j++)
            {
              DrawPrimitive(wnd, i, j, zoom, geoContext);
            }
          }
          dc.DrawGeometry(primitiveBorderBrush, primitiveBorderPen, geo);

        }
        dc.Close();

        var bmp = new RenderTargetBitmap(picWidth, picHeight, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(drawingVisual);
        LabyrinthusImage.Source = bmp;
      }
    }

    private static void DrawPrimitive(WindowMaster wnd, int i, int j, double zoom, StreamGeometryContext geoContext)
    {
      foreach (var lineInfo in wnd.Primitive.Lines)
      {
        geoContext.BeginFigure(
          new Point((lineInfo.X0 + wnd.Primitive.Width*i)*zoom, (lineInfo.Y0 + wnd.Primitive.Height*j)*zoom), false,
          false);
        geoContext.LineTo(
          new Point((lineInfo.X1 + wnd.Primitive.Width*i)*zoom, (lineInfo.Y1 + wnd.Primitive.Height*j)*zoom), true,
          false);
      }
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
    #endregion
  }
}
