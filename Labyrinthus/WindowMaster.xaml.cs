using System;
using System.Windows;
using System.Windows.Input;
using Labyrinthus.Objects;

namespace Labyrinthus
{
  /// <summary>
  /// Interaction logic for WindowMaster.xaml
  /// </summary>
  public partial class WindowMaster
  {
    private int currentPageNumber;
    public PrimitiveInfo Primitive { get; private set; }

    public WindowMaster()
    {
      Primitive = new PrimitiveInfo();

      InitializeComponent();
    }

    #region События и команды

    private void CloseCommand(object sender, ExecutedRoutedEventArgs e)
    {
      Close();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
      switch (currentPageNumber)
      {
        case 1:
          currentPageNumber = 0;
          NavigateBack(@"Pages\PagePreparePrimitives.xaml");
          BackButton.Visibility = Visibility.Hidden;
          break;
        case 2:
          currentPageNumber = 1;
          NavigateBack(@"Pages\PagePackPrimitives.xaml");
          break;
        case 3:
          currentPageNumber = 2;
          NavigateBack(@"Pages\PageShowLabyrinthus.xaml");
          break;
        default:
          throw new InvalidOperationException("Как-то умудрились нажать кнопку, хотя она невидима");
      }
      ForwardButton.Visibility = Visibility.Visible;
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
      switch (currentPageNumber)
      {
        case 0:
          currentPageNumber = 1;
          NavigateForward(@"Pages\PagePackPrimitives.xaml");
          break;
        case 1:
          currentPageNumber = 2;
          NavigateForward(@"Pages\PageShowLabyrinthus.xaml");
          break;
        case 2:
          currentPageNumber = 3;
          NavigateForward(@"Pages\PageShow3DLabyrinthus.xaml");
          ForwardButton.Visibility = Visibility.Collapsed;
          break;
        default:
          throw new InvalidOperationException("Как-то умудрились нажать кнопку, хотя она невидима");
      }
      BackButton.Visibility = Visibility.Visible;
    }

    #endregion

    #region Методы для навигации

    private void NavigateForward(string uriPath)
    {
      if (FramePages.CanGoForward)
      {
        FramePages.GoForward();
      }
      else
      {
        FramePages.Navigate(new Uri(uriPath, UriKind.RelativeOrAbsolute));         
      }
    }

    private void NavigateBack(string uriPath)
    {
      if (FramePages.CanGoBack)
      {
        FramePages.GoBack();
      }
      else
      {
        FramePages.Navigate(new Uri(uriPath, UriKind.RelativeOrAbsolute));
      }
    }

    #endregion
  }
}
