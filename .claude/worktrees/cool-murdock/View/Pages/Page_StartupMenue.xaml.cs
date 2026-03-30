using System.Windows.Controls;
using MyriaRPG.Services;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages
{
    /// <summary>
    /// Interaktionslogik für Page_StartupMenue.xaml
    /// </summary>
    public partial class Page_StartupMenue : Page
    {
        public Page_StartupMenue()
        {
            InitializeComponent();
            Navigation.SetNavigationFrame(Frame, NavigationFrameType.Startup);
            this.DataContext = new ViewModel_StartupMenuePage();
        }

    }

}
