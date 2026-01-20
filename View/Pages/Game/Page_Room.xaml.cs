using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages.Game;

namespace MyriaRPG.View.Pages.Game
{
    /// <summary>
    /// Interaktionslogik für Page_Room.xaml
    /// </summary>
    public partial class Page_Room : Page
    {
        public Page_Room()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_PageRoom();
        }
    }
}
