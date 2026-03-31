using MyriaLib.Services;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game.IngameWindow.Inventory
{
    public partial class MoneyBag : Page
    {
        public MoneyBag()
        {
            InitializeComponent();
            this.DataContext = new MoneyBagViewModel(UserAccoundService.CurrentCharacter);
        }
    }
}
