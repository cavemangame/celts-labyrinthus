using System.Windows;
using System.Windows.Controls;
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
      DataContext = this;
      PrimitiveWidth = 5;
      PrimitiveHeight = 5;
    }
    #endregion

    #region Обработка событий
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      RefreshCanvas();
    }

    private void Primitive_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (IsLoaded)
      {
        RefreshCanvas();
      }
    }
    #endregion

    #region Загрузка и сохранение
    private void LoadPrimitive(object sender, RoutedEventArgs e)
    {
      var wnd = (WindowMaster)Window.GetWindow(this);
      if (wnd != null)
      {
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
          PrimitiveCanvas.Refresh();
        }
      }
    }

    private void SavePrimitive(object sender, RoutedEventArgs e)
    {
      var wnd = (WindowMaster)Window.GetWindow(this);
      if (wnd != null)
      {
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
    }
    #endregion

    #region private методы
    private void RefreshCanvas()
    {
      PrimitiveCanvas.SetNewSizes(PrimitiveWidth, PrimitiveHeight);
    }
    #endregion
  }
}
