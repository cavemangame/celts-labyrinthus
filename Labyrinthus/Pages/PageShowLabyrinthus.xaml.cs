using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Labyrinthus.Pages
{
  /// <summary>
  /// Страница содержит финальное представление лабиринта.
  /// Рисунок можно перемещать и поворачивать.
  /// </summary>
  public partial class PageShowLabyrinthus
  {
    #region Поля
    private Brush floorBrush;
    private Brush primitiveBorderBrush;
    private Pen primitiveBorderPen;

    /// <summary>
    /// Сдвиг рисунка лабиринта по горизонати
    /// </summary>
    private int xShift;

    /// <summary>
    /// Сдвиг рисунка лабиринта по вертикали
    /// </summary>
    private int yShift;

    /// <summary>
    /// Начальная точка перемещения
    /// </summary>
    private Point moveStartPoint;

    /// <summary>
    /// Битмам лабиринта. Сохраняется для ускорения перемещения.
    /// </summary>
    private RenderTargetBitmap labirinthusBitmap;
    #endregion

    #region Свойства

    /// <summary>
    /// Масштаб в пикселях
    /// </summary>
    public int Zoom { get; set; }

    /// <summary>
    /// Угол поворота
    /// </summary>
    public int RotateAngle { get; set; }

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

      page.DrawLabyrinthus();
    }
    #endregion

    #region Конструктор

    public PageShowLabyrinthus()
    {
      InitializeComponent();

      Zoom = 10;
      RotateAngle = 0;
      FloorColor = Colors.Brown;
      BorderColor = Colors.Black;

      DataContext = this;
    }

    #endregion

    #region Обработка событий

    private void LabyrinthusImagePanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
      // начало перетаскивания рисунка
      moveStartPoint = e.MouseDevice.GetPosition(LabyrinthusImagePanel);
    }

    private void LabyrinthusImagePanel_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
      {
        return;
      }

      var moveEndPoint = e.MouseDevice.GetPosition(LabyrinthusImagePanel);
      var wnd = (WindowMaster)Application.Current.MainWindow;

      // TODO: остаток от деления должен учитывать поворот
      xShift = (int)(xShift + moveEndPoint.X - moveStartPoint.X) % (wnd.Primitive.Width * Zoom);
      yShift = (int)(yShift + moveEndPoint.Y - moveStartPoint.Y) % (wnd.Primitive.Height * Zoom);

      moveStartPoint = moveEndPoint;
      DrawBitmap();
    }

    private void LabyrinthusParams_Changed(object sender, RoutedEventArgs e)
    {
      xShift = yShift = 0;
      DrawLabyrinthus();
    }

    private void SaveLabyrinthus(object sender, RoutedEventArgs e)
    {
      OnSaveLabyrinthus();
    }

    #endregion

    #region private методы
    #region Отрисовка лабиринта

    private void DrawLabyrinthus()
    {
      if (!IsInitialized)
      {
        return;
      }

      var wnd = (WindowMaster)Application.Current.MainWindow;

      if (wnd.Primitive.Width <= 0 || wnd.Primitive.Height <= 0)
      {
        return;
      }

      UpdateBitmap();
      DrawBitmap();
    }


    /// <summary>
    /// Пересоздает полную картинку для ускорения перемещения
    /// </summary>
    private void UpdateBitmap()
    {
      var wnd = (WindowMaster)Application.Current.MainWindow;

      if (wnd.Primitive.Width <= 0 || wnd.Primitive.Height <= 0)
      {
        return;
      }

      primitiveBorderBrush = new SolidColorBrush(BorderColor);
      primitiveBorderPen = new Pen(primitiveBorderBrush, 1.0);
      floorBrush = new SolidColorBrush(FloorColor);

      // видимая часть рисунка
      var visiblePicHeight = (int)LabyrinthusImagePanel.Height;
      var visiblePicWidth = (int)LabyrinthusImagePanel.Width;

      // полный рисунок - нужно для поворота и движения картинки
      var totalPicHeight = visiblePicHeight * 3;
      var totalPicWidth = visiblePicWidth * 3;

      // определяем количество примитивов по высоте и ширине рисунка
      var hCount = totalPicHeight / (Zoom * wnd.Primitive.Height);
      var wCount = totalPicWidth / (Zoom * wnd.Primitive.Width);

      // начинаем рисовать
      var drawingVisual = new DrawingVisual();
      DrawingContext dc = drawingVisual.RenderOpen();

      dc.DrawRectangle(floorBrush, null, new Rect(0, 0, totalPicWidth, totalPicHeight));
      for (int i = 0; i < wCount; i++)
      {
        // на каждый примитив - медленней, на всё - медленней
        var geo = new StreamGeometry();

        using (var geoContext = geo.Open())
        {
          for (int j = 0; j < hCount; j++)
          {
            DrawPrimitive(wnd, i, j, Zoom, geoContext);
          }
        }
        dc.DrawGeometry(primitiveBorderBrush, primitiveBorderPen, geo);
      }
      dc.Close();

      drawingVisual.Transform = new RotateTransform(
        RotateAngle, totalPicWidth / 2d, totalPicHeight / 2d);

      labirinthusBitmap = new RenderTargetBitmap(totalPicWidth, totalPicHeight, 96, 96, PixelFormats.Pbgra32);
      labirinthusBitmap.Render(drawingVisual);
    }


    /// <summary>
    /// Рисует готовый битмап лабиринта
    /// </summary>
    private void DrawBitmap()
    {
      // видимая часть рисунка
      var visiblePicHeight = (int)LabyrinthusImagePanel.Height;
      var visiblePicWidth = (int)LabyrinthusImagePanel.Width;

      LabyrinthusImage.Width = visiblePicWidth;
      LabyrinthusImage.Height = visiblePicHeight;

      // полный рисунок - нужно для поворота и движения картинки
      var totalPicHeight = visiblePicHeight * 3;
      var totalPicWidth = visiblePicWidth * 3;

      // показываем только центральную часть
      var visibleRect = new Int32Rect(totalPicWidth / 2 - visiblePicWidth / 2 - xShift,
                                      totalPicHeight / 2 - visiblePicHeight / 2 - yShift,
                                      visiblePicWidth, visiblePicHeight);
      var visibleImage = new CroppedBitmap(labirinthusBitmap, visibleRect);

      LabyrinthusImage.Source = visibleImage;
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
