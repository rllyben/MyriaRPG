using MyriaLib.Entities.Items;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Services.Builder;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Pages;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game.IngameWindow;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.UserControls.IngameWindow
{
    public enum ShopFilter { Weapons, Armor, Accessories, Utilities }
    public class ShopPanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;
        private Page _inventoryPage;
        private ShopFilter _activeFilter = ShopFilter.Weapons;
        public ShopFilter ActiveFilter
        {
            get => _activeFilter;
            set { _activeFilter = value; OnPropertyChanged(); OnPropertyChanged(nameof(FilteredStock)); }
        }

        public IEnumerable<ShopItemVm> FilteredStock => ActiveFilter switch
        {
            ShopFilter.Weapons => Stock.OfType<ShopEquipmentItemVm>()
                                           .Where(i => i.SlotType == EquipmentType.Weapon
                                                    && i.AllowedClasses.Contains(_player.Class)),
            ShopFilter.Armor => Stock.OfType<ShopEquipmentItemVm>()
                                           .Where(i => i.SlotType == EquipmentType.Armor
                                                    && i.AllowedClasses.Contains(_player.Class)),
            ShopFilter.Accessories => Stock.OfType<ShopEquipmentItemVm>()
                                           .Where(i => i.SlotType == EquipmentType.Accessory
                                                    && i.AllowedClasses.Contains(_player.Class)),
            ShopFilter.Utilities => Stock.Where(i => i is not ShopEquipmentItemVm),
            _ => Stock
        };
        public Page InventoryPage 
        { 
            get => _inventoryPage;
            private set
            {
                _inventoryPage = value;
                OnPropertyChanged(nameof(InventoryPage));
            }
        }
        public string Title => Localization.T("npc.shop.title.equipment");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnBuy => Localization.T("npc.shop.buy");
        public string BtnSell => Localization.T("npc.shop.sell");
        public string StockLabel => Localization.T("npc.shop.stock");
        public string PlayerInventoryLabel => Localization.T("npc.shop.inventory");

        private int _playerMoney;
        public int PlayerMoney
        {
            get => _playerMoney;
            set { _playerMoney = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayerMoneyText)); }
        }

        public string PlayerMoneyText => $"💰 {PlayerMoney}";

        // Shop Stock
        public ObservableCollection<ShopItemVm> Stock { get; } = new();

        private ShopItemVm _selectedStock;
        public ShopItemVm SelectedStock
        {
            get => _selectedStock;
            set
            {
                _selectedStock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedStockName));
                OnPropertyChanged(nameof(SelectedStockPrice));
                OnPropertyChanged(nameof(BuyQuantityMax));
                OnPropertyChanged(nameof(CanBuy));
            }
        }

        public string SelectedStockName => SelectedStock?.Name ?? "";
        public string SelectedStockPrice => SelectedStock != null ? $"{StockPriceLabel}: {SelectedStock.BuyPrice}" : "";
        public string StockPriceLabel => Localization.T("npc.shop.price");
        public int BuyQuantityMax => SelectedStock != null ? _player.Money.Coins.Bronze / SelectedStock.BuyPrice : 0;

        // Player Inventory (Equipment only)
        public ObservableCollection<PlayerEquipmentVm> PlayerEquipment { get; } = new();

        private PlayerEquipmentVm _selectedPlayerEquipment;
        public PlayerEquipmentVm SelectedPlayerEquipment
        {
            get => _selectedPlayerEquipment;
            set
            {
                _selectedPlayerEquipment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPlayerEquipName));
                OnPropertyChanged(nameof(SelectedPlayerEquipSellPrice));
                OnPropertyChanged(nameof(CanSell));
            }
        }

        public string SelectedPlayerEquipName => SelectedPlayerEquipment?.Name ?? "";
        public string SelectedPlayerEquipSellPrice => SelectedPlayerEquipment != null 
            ? $"{SellPriceLabel}: {SelectedPlayerEquipment.SellPrice}" 
            : "";
        public string SellPriceLabel => Localization.T("npc.shop.sellPrice");

        // Quantity Control
        private int _buyQuantity = 1;
        public int BuyQuantity
        {
            get => _buyQuantity;
            set
            {
                if (value < 1) value = 1;
                if (value > BuyQuantityMax && BuyQuantityMax > 0) value = BuyQuantityMax;
                
                _buyQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanBuy));
                OnPropertyChanged(nameof(BuyTotalPrice));
            }
        }

        public int BuyTotalPrice => SelectedStock != null ? SelectedStock.BuyPrice * BuyQuantity : 0;

        // Status
        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool CanBuy => SelectedStock != null && BuyQuantity > 0 && _player.Money.Coins.TotalBronze >= BuyTotalPrice;
        public bool CanSell => SelectedPlayerEquipment != null;

        // Commands

        public ICommand FilterWeaponsCommand { get; }
        public ICommand FilterArmorCommand { get; }
        public ICommand FilterAccessoriesCommand { get; }
        public ICommand FilterUtilitiesCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand BuyCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand IncreaseBuyQtyCommand { get; }
        public ICommand DecreaseBuyQtyCommand { get; }
        public ICommand MaxBuyQtyCommand { get; }

        public ShopPanelViewModel(Npc npc, Player player, Action goBack)
        {
            _npc = npc;
            _player = player;
            _goBack = goBack;

            PlayerMoney = _player.Money.Coins.TotalBronze;
            //InventoryPage = new InventoryPage(UserAccoundService.CurrentCharacter);

            // Commands
            BackCommand = new RelayCommand(_goBack);
            BuyCommand = new RelayCommand(BuySelected);
            SellCommand = new RelayCommand(SellSelected);
            IncreaseBuyQtyCommand = new RelayCommand(() => BuyQuantity++);
            DecreaseBuyQtyCommand = new RelayCommand(() => BuyQuantity--);
            MaxBuyQtyCommand = new RelayCommand(() => BuyQuantity = BuyQuantityMax);
            FilterWeaponsCommand = new RelayCommand(() => ActiveFilter = ShopFilter.Weapons);
            FilterArmorCommand = new RelayCommand(() => ActiveFilter = ShopFilter.Armor);
            FilterAccessoriesCommand = new RelayCommand(() => ActiveFilter = ShopFilter.Accessories);
            FilterUtilitiesCommand = new RelayCommand(() => ActiveFilter = ShopFilter.Utilities);

            LoadShopStock();
            LoadPlayerEquipment();
        }

        private void LoadShopStock()
        {
            Stock.Clear();

            // Smith sells equipment items
            var equipmentItems = _npc.ItemNames;

            foreach (var itemId in equipmentItems)
            {
                if (ItemFactory.TryCreateItem(itemId, out var item))
                {
                    Console.WriteLine(item is EquipmentItem);
                    if (item is EquipmentItem eq && eq.AllowedClasses.Contains(_player.Class))
                        Stock.Add(ShopEquipmentItemVm.FromEquipment(eq));
                    else if (item.AllowedClasses.Contains(_player.Class))
                        Stock.Add(ShopItemVm.FromItem(item));
                }
            }

            SelectedStock = Stock.FirstOrDefault();
            if (SelectedStock == null)
            {
                StatusMessage = Localization.T("npc.shop.noStock");
            }
        }

        private void LoadPlayerEquipment()
        {
            PlayerEquipment.Clear();

            foreach (var item in _player.Inventory.Items.OfType<EquipmentItem>())
            {
                PlayerEquipment.Add(new PlayerEquipmentVm(item));
            }

            SelectedPlayerEquipment = PlayerEquipment.FirstOrDefault();
            if (PlayerEquipment.Count == 0)
            {
                StatusMessage = Localization.T("npc.shop.noEquipmentToSell");
            }
        }

        private void BuySelected()
        {
            if (SelectedStock == null || BuyQuantity <= 0) return;

            int totalCost = SelectedStock.BuyPrice * BuyQuantity;

            if (_player.Money.Coins.TotalBronze < totalCost)
            {
                StatusMessage = Localization.T("npc.shop.notEnoughMoney");
                return;
            }

            // Create item with quantity
            if (ItemFactory.TryCreateItem(SelectedStock.Id, out var item))
            {
                item.StackSize = BuyQuantity;

                if (_player.Inventory.AddItem(item, _player))
                {
                    // Deduct money
                    _player.Money.Coins.TrySpend(totalCost);
                    PlayerMoney = _player.Money.Coins.TotalBronze;

                    StatusMessage = Localization.T("npc.shop.buySuccess", BuyQuantity, SelectedStock.Name);
                    BuyQuantity = 1;
                    OnPropertyChanged(nameof(CanBuy));
                }
                else
                {
                    StatusMessage = Localization.T("npc.shop.inventoryFull");
                }
            }
            else
            {
                StatusMessage = Localization.T("npc.shop.buyFailed");
            }
        }

        private void SellSelected()
        {
            if (SelectedPlayerEquipment == null) return;

            // Smith pays 80% of buy price
            int sellPrice = (int)(SelectedPlayerEquipment.BuyPrice * 0.8f);

            // Find the actual item in inventory and remove it
            var itemToRemove = _player.Inventory.Items.FirstOrDefault(i => i.Id == SelectedPlayerEquipment.Id);
            if (itemToRemove == null) return;

            if (_player.Inventory.RemoveItem(itemToRemove))
            {
                // Add money
                _player.Money.TryAdd(sellPrice);
                PlayerMoney = _player.Money.Coins.TotalBronze;

                StatusMessage = Localization.T("npc.shop.sellSuccess", SelectedPlayerEquipment.Name, sellPrice);
                LoadPlayerEquipment();
                OnPropertyChanged(nameof(CanSell));
            }
            else
            {
                StatusMessage = Localization.T("npc.shop.sellFailed");
            }
        }
    }

    public class ShopItemVm : BaseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BuyPrice { get; set; }

        public static ShopItemVm FromItem(Item item)
        {
            return new ShopItemVm
            {
                Id = item.Id,
                Name = Localization.T($"item.{item.Id}"),
                BuyPrice = item.BuyPrice
            };
        }

        public override string ToString() => Name;
    }

    public class PlayerEquipmentVm : BaseViewModel
    {
        private EquipmentItem _item;

        public string Id => _item.Id;
        public string Name => Localization.T($"item.{_item.Id}");
        public int BuyPrice => _item.BuyPrice;
        public int SellPrice => (int)(_item.BuyPrice * 0.8f);

        public PlayerEquipmentVm(EquipmentItem item)
        {
            _item = item;
        }

        public override string ToString() => Name;
    }
}
