using MyriaLib.Entities.Players;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory;
using System.Windows.Controls;

namespace MyriaRPG.Pages
{
    public partial class InventoryPage : Page
    {
        public InventoryPage(Player player)
        {
            InitializeComponent();
            this.DataContext = new InventoryPageViewModel();
        }
    }
}
