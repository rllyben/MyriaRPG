using MyriaLib.Entities.Players;
using MyriaRPG.Model;
using MyriaRPG.ViewModel;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory
{
    /// <summary>
    /// ViewModel for the money bag display.
    /// Subscribes to player.Inventory.ItemReceived to refresh after buy/sell operations.
    /// </summary>
    public class MoneyBagViewModel : BaseViewModel
    {
        private readonly Player _player;
        private string _moneyBagTitle;
        private string _moneyDisplay;

        [LocalizedKey("pg.inventory.moneybag.title")]
        public string MoneyBagTitle { get => _moneyBagTitle; set => SetProperty(ref _moneyBagTitle, value); }

        public string MoneyDisplay { get => _moneyDisplay; set => SetProperty(ref _moneyDisplay, value); }

        public MoneyBagViewModel(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _player.Inventory.ItemReceived += (s, e) => UpdateMoneyDisplay();
            UpdateMoneyDisplay();
        }

        public void UpdateMoneyDisplay()
        {
            MoneyDisplay = "0 Gold";
        }
    }
}
