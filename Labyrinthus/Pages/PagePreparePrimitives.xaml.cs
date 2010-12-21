using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Labyrinthus.Pages
{
  public partial class PagePreparePrimitives
  {
    #region Конструктор

    public PagePreparePrimitives()
    {
      InitializeComponent();
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
          PrimitiveWidth.Text = wnd.Primitive.Width.ToString();
          PrimitiveHeight.Text = wnd.Primitive.Height.ToString();
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

    protected void PrimitivePropertiesChanged()
    {
      int w, h;
      if (Int32.TryParse(PrimitiveWidth.Text, out w) && Int32.TryParse(PrimitiveHeight.Text, out h))
      {
        if (h > 0 && w > 0)
        {
          PrimitiveCanvas.SetNewSizes(h, w);
        }
      }
    }

    private void Primitive_TextChanged(object sender, TextChangedEventArgs e)
    {
      PrimitivePropertiesChanged();
    }
  }
}
