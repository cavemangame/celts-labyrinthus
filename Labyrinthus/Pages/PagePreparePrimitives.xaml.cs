using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Labyrinthus.Pages
{
  /// <summary>
  /// Interaction logic for PagePreparePrimitives.xaml
  /// </summary>
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
      MessageBox.Show("Тут будет загрузка примитива из файла");
    }

    private void SavePrimitive(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Тут будет сохранение примитива в файл");
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
