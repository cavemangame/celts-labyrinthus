using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Labyrinthus.Objects;
using Labyrinthus.Pages;

namespace Labyrinthus
{
  /// <summary>
  /// Interaction logic for WindowMaster.xaml
  /// </summary>
  public partial class WindowMaster : Window
  {
    private int CurrentPageNumber = 0;
    public PrimitiveInfo Primitive = new PrimitiveInfo();

    public WindowMaster()
    {
      InitializeComponent();
    }

    #region События и команды

    private void CloseCommand(object sender, ExecutedRoutedEventArgs e)
    {
      Close();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
      switch (CurrentPageNumber)
      {
        case 1:
          CurrentPageNumber = 0;
          if (FramePages.CanGoBack)
          {
            FramePages.GoBack();
          }
          else
          {
            FramePages.Navigate(new Uri(@"Pages\PagePreparePrimitives.xaml", UriKind.RelativeOrAbsolute));          
          }
          BackButton.Visibility = Visibility.Hidden;
          break;
        default:
          throw new InvalidOperationException("Как-то умудрились нажать кнопку, хотя она невидима");
      }
      ForwardButton.Visibility = Visibility.Visible;
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
      switch (CurrentPageNumber)
      {
        case 0:
          CurrentPageNumber = 1;
          if (FramePages.CanGoForward)
          {
            FramePages.GoForward();
          }
          else
          {
            FramePages.Navigate(new Uri(@"Pages\PageShowLabyrinthus.xaml", UriKind.RelativeOrAbsolute));         
          }

          ForwardButton.Visibility = Visibility.Collapsed;
          break;
        default:
          throw new InvalidOperationException("Как-то умудрились нажать кнопку, хотя она невидима");
      }
      BackButton.Visibility = Visibility.Visible;
    }

    #endregion
  }
}
