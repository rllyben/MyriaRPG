using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;

namespace MyriaRPG.View.Pages.Game.IngameWindow
{
    /// <summary>
    /// Interaktionslogik für Page_Inventory.xaml
    /// </summary>
    public partial class Page_Inventory : Page
    {
        public Page_Inventory()
        {
            InitializeComponent();
            this.DataContext = new InventoryPageViewModel();
        }

    }

}
