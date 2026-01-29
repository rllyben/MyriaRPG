using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Entities.Items;
using MyriaLib.Systems;
using MyriaLib.Services.Builder;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class ShopPanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;

        public string Title => Localization.T("npc.shop.title");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnBuy => Localization.T("npc.shop.buy");
        public string BtnSell => Localization.T("npc.shop.sell");
        public string QtyLabel => Localization.T("npc.shop.qty");
        public string StockHeader => Localization.T("npc.shop.stock");

        public string MoneyText => _player.Money?.ToString() ?? "0";

        public ObservableCollection<ShopItemVm> Stock { get; } = new();

        private ShopItemVm _selectedStock;
        public ShopItemVm SelectedStock
        {
            get => _selectedStock;
            set
            {
                _selectedStock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedName));
                OnPropertyChanged(nameof(SelectedDescription));
                OnPropertyChanged(nameof(CanBuy));
            }

        }

        public string SelectedName => SelectedStock?.Name ?? "";
        public string SelectedDescription => SelectedStock?.Description ?? "";

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = Math.Max(1, value); OnPropertyChanged(); OnPropertyChanged(nameof(CanBuy)); }
        }

        public bool CanBuy => SelectedStock != null && Quantity > 0;

        public ICommand BackCommand { get; }
        public ICommand IncQtyCommand { get; }
        public ICommand DecQtyCommand { get; }
        public ICommand BuyCommand { get; }
        public ICommand SellCommand { get; }

        public bool CanSell => SelectedStock != null; // later: depends on inventory

        public ShopPanelViewModel(Npc npc, Player player, Action goBack)
        {
            _npc = npc;
            _player = player;
            _goBack = goBack;

            BackCommand = new RelayCommand(_goBack);
            IncQtyCommand = new RelayCommand(() => Quantity++);
            DecQtyCommand = new RelayCommand(() => Quantity--);

            BuyCommand = new RelayCommand(BuySelected);
            SellCommand = new RelayCommand(SellSelected);

            LoadStock();
        }

        private void LoadStock()
        {
            Stock.Clear();

            // If your NPC already has ItemRefs populated, use that.
            // Otherwise, use ItemNames + ItemService/ItemFactory.
            if (_npc.ItemRefs != null && _npc.ItemRefs.Count > 0)
            {
                foreach (var it in _npc.ItemRefs)
                    Stock.Add(ShopItemVm.FromItem(it));
            }
            else if (_npc.ItemNames != null && _npc.ItemNames.Count > 0)
            {
                foreach (string name in _npc.ItemNames)
                {
                    if (ItemFactory.TryCreateItem(name, out var item))

                    if (item != null)
                        Stock.Add(ShopItemVm.FromItem(item));
                }

            }

            SelectedStock = Stock.FirstOrDefault();
        }

        private void BuySelected()
        {
            if (SelectedStock?.SourceItem == null) return;

            // TODO: use Money.TrySpend if available
            // TODO: clone Quantity and add to inventory safely

            // Simple first version:
            var item = SelectedStock.SourceItem.CloneOne();
            item.StackSize = Quantity;

            _player.Inventory.AddItem(item, _player);
            OnPropertyChanged(nameof(MoneyText));
        }

        private void SellSelected()
        {
            // TODO: implement selling from player inventory
        }

    }
    public class ShopItemVm : BaseViewModel
    {
        public Item SourceItem { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string BuyPriceText { get; private set; }

        public static ShopItemVm FromItem(Item item)
        {
            return new ShopItemVm
            {
                SourceItem = item,
                Name = Localization.T(item.Name),
                Description = Localization.T(item.Description + ".description"),
                BuyPriceText = item.BuyPrice.ToString()
            };

        }

    }

}
