using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages.Game;

namespace MyriaRPG.View.Pages.Game
{
    /// <summary>
    /// Interaktionslogik für Page_Fight.xaml
    /// </summary>
    public partial class Page_Fight : Page
    {
        public Page_Fight()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_PageFight();
        }
    }
}
