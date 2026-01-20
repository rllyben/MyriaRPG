using MyriaRPG.Services;
using MyriaRPG.ViewModel.Pages.Game;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game
{
    /// <summary>
    /// Interaktionslogik für Page_Room.xaml
    /// </summary>
    public partial class Page_Game : Page
    {
        public Page_Game()
        {
            InitializeComponent();
            Navigation.SetNavigationFrame(frm_Navigation, 4);
            this.DataContext = new ViewModel_PageGame();
        }

    }

}
