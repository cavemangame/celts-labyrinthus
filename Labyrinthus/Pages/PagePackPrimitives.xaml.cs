using System.Windows.Controls;
using System.Windows.Input;

namespace Labyrinthus.Pages
{
  /// <summary>
  /// Interaction logic for PagePackPrimitives.xaml
  /// </summary>
  public partial class PagePackPrimitives : Page
  {
    public PagePackPrimitives()
    {
      InitializeComponent();
    }

    private void PrimitivePackCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {

    }

    private void PrimitivePackCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {

    }

    private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
      PrimitivePackCanvas.Refresh();
    }
  }
}
