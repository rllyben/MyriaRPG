using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Entities.Items;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaLib.Services.Builder;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class ShopPanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;

        // Localization
        public string Title => Localization.T("npc.shop.title");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnBuy => Localization.T("npc.shop.buy");
        public string StockHeader => Localization.T("npc.shop.stock");
        public string InventoryHeader => Localization.T("npc.shop.playeritems");
        public string BtnYes => Localization.T("app.general.UI.yes");
        public string BtnNo => Localization.T("app.general.UI.no");

        // Money
        private int _playerMoney;
        public int PlayerMoney
        {
            get => _playerMoney;
            set { _playerMoney = value; OnPropertyChanged(); OnPropertyChanged(nameof(MoneyText)); }
        }
        public string MoneyText => _player.Money?.ToString() ?? "0";

        // Shop stock
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
                OnPropertyChanged(nameof(SelectedPriceText));
                OnPropertyChanged(nameof(SelectedSellValueText));
                OnPropertyChanged(nameof(SelectedTypeText));
                OnPropertyChanged(nameof(CanBuy));
            }
        }

        public string SelectedName => SelectedStock?.Name ?? "";
        public string SelectedDescription => SelectedStock?.Description ?? "";
        public string SelectedPriceText => SelectedStock?.SourceItem != null
            ? $"{Localization.T("npc.shop.price.buy")}: {SelectedStock.SourceItem.BuyPrice}"
            : "";
        public string SelectedSellValueText => SelectedStock?.SourceItem != null
            ? $"{Localization.T("npc.shop.price.sell")}: {SelectedStock.SourceItem.SellValue}"
            : "";
        public string SelectedTypeText => SelectedStock?.TypeText ?? "";

        public bool CanBuy => SelectedStock != null;

        // Player inventory for selling
        public ObservableCollection<ItemVm> PlayerInventory { get; } = new();

        // Sell confirmation dialog
        private bool _showConfirmDialog;
        public bool ShowConfirmDialog
        {
            get => _showConfirmDialog;
            set { _showConfirmDialog = value; OnPropertyChanged(); }
        }

        private string _confirmMessage = "";
        public string ConfirmMessage
        {
            get => _confirmMessage;
            set { _confirmMessage = value; OnPropertyChanged(); }
        }

        private Item _pendingSellItem;

        // Commands
        public ICommand BackCommand { get; }
        public ICommand BuyCommand { get; }
        public ICommand SellItemCommand { get; }
        public ICommand ConfirmSellCommand { get; }
        public ICommand CancelSellCommand { get; }

        private static readonly Dictionary<ItemRarity, SolidColorBrush> RarityColors = new()
        {
            { ItemRarity.Common, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080")) },
            { ItemRarity.Uncommon, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CB371")) },
            { ItemRarity.Rare, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF")) },
            { ItemRarity.Epic, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF")) },
            { ItemRarity.Unique, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF59D")) },
            { ItemRarity.Legendary, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DAA520")) },
            { ItemRarity.Godly, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB6C1")) }
        };

        public ShopPanelViewModel(Npc npc, Player player, Action goBack)
        {
            _npc = npc;
            _player = player;
            _goBack = goBack;

            PlayerMoney = _player.Money.Coins.TotalBronze;

            BackCommand = new RelayCommand(_goBack);
            BuyCommand = new RelayCommand(BuySelected);
            SellItemCommand = new RelayCommand<ItemVm>(RequestSellItem);
            ConfirmSellCommand = new RelayCommand(ExecuteSell);
            CancelSellCommand = new RelayCommand(CancelSell);

            LoadStock();
            RefreshPlayerInventory();
        }

        private void LoadStock()
        {
            Stock.Clear();

            if (_npc.ItemRefs != null && _npc.ItemRefs.Count > 0)
            {
                foreach (var it in _npc.ItemRefs)
                    Stock.Add(ShopItemVm.FromItem(it));
            }
            else if (_npc.ItemNames != null && _npc.ItemNames.Count > 0)
            {
                foreach (string name in _npc.ItemNames)
                {
                    if (ItemFactory.TryCreateItem(name, out var item) && item != null)
                        Stock.Add(ShopItemVm.FromItem(item));
                }
            }

            SelectedStock = Stock.FirstOrDefault();
        }

        private void RefreshPlayerInventory()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PlayerInventory.Clear();
                foreach (Item item in _player.Inventory.Items)
                {
                    var vm = new ItemVm(item);
                    vm.Color = RarityColors.GetValueOrDefault(item.Rarity, Brushes.Gray);
                    string itemType = item switch
                    {
                        EquipmentItem eq => eq.SlotType.ToString(),
                        ConsumableItem => "Consumable",
                        MaterialItem => "Material",
                        _ => ""
                    };
                    vm.Type = itemType;
                    PlayerInventory.Add(vm);
                }
            });
        }

        private void BuySelected()
        {
            if (SelectedStock?.SourceItem == null) return;

            var result = _npc.BuyItem(_player, SelectedStock.SourceItem, 1);

            PlayerMoney = _player.Money.Coins.TotalBronze;
            OnPropertyChanged(nameof(MoneyText));
            RefreshPlayerInventory();
        }

        private void RequestSellItem(ItemVm itemVm)
        {
            if (itemVm == null || itemVm.IsEmpty) return;

            var invItem = _player.Inventory.Items.FirstOrDefault(i => i.Id == itemVm.Id);
            if (invItem == null) return;

            _pendingSellItem = invItem;
            ConfirmMessage = string.Format(
                Localization.T("npc.shop.sell.confirm"),
                Localization.T(invItem.Name),
                invItem.SellValue
            );
            ShowConfirmDialog = true;
        }

        private void ExecuteSell()
        {
            ShowConfirmDialog = false;
            if (_pendingSellItem == null) return;

            _npc.SellItem(_player, _pendingSellItem, 1);

            PlayerMoney = _player.Money.Coins.TotalBronze;
            OnPropertyChanged(nameof(MoneyText));
            RefreshPlayerInventory();
            _pendingSellItem = null;
        }

        private void CancelSell()
        {
            ShowConfirmDialog = false;
            _pendingSellItem = null;
        }
    }

    public class ShopItemVm : BaseViewModel
    {
        public Item SourceItem { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string BuyPriceText { get; private set; }
        public string TypeText { get; private set; }

        public static ShopItemVm FromItem(Item item)
        {
            string type = item switch
            {
                EquipmentItem eq => eq.SlotType.ToString(),
                ConsumableItem => "Consumable",
                MaterialItem => "Material",
                _ => ""
            };

            return new ShopItemVm
            {
                SourceItem = item,
                Name = Localization.T(item.Name),
                Description = Localization.T(item.Description + ".description"),
                BuyPriceText = item.BuyPrice.ToString(),
                TypeText = type
            };
        }
    }
}
