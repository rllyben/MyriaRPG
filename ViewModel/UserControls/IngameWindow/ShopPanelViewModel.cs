using MyriaLib.Entities.Items;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Services.Builder;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaLib.Systems.Events;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.UserControls.IngameWindow
{
    public enum ShopFilter { Weapons, Armor, Accessories, Utilities, Buyback }

    public class ShopPanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;
        private ShopFilter _activeFilter = ShopFilter.Weapons;

        public ShopFilter ActiveFilter
        {
            get => _activeFilter;
            set
            {
                _activeFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredStock));
                OnPropertyChanged(nameof(BtnBuy));
                OnPropertyChanged(nameof(IsQuantityControlVisible));
                OnPropertyChanged(nameof(CanBuy));
                SelectedStock = FilteredStock.FirstOrDefault();
            }
        }

        public IEnumerable<ShopItemVm> FilteredStock => _activeFilter switch
        {
            ShopFilter.Weapons     => Stock.OfType<ShopEquipmentItemVm>()
                                          .Where(i => !i.IsTool
                                                   && i.SlotType == EquipmentType.Weapon
                                                   && i.AllowedClasses.Contains(_player.Class)),
            ShopFilter.Armor       => Stock.OfType<ShopEquipmentItemVm>()
                                          .Where(i => !i.IsTool
                                                   && i.SlotType == EquipmentType.Armor
                                                   && i.AllowedClasses.Contains(_player.Class)),
            ShopFilter.Accessories => Stock.OfType<ShopEquipmentItemVm>()
                                          .Where(i => !i.IsTool
                                                   && i.SlotType == EquipmentType.Accessory
                                                   && i.AllowedClasses.Contains(_player.Class)),
            ShopFilter.Utilities   => Stock.Where(i => i is not ShopEquipmentItemVm
                                                    || (i is ShopEquipmentItemVm se && se.IsTool)),
            ShopFilter.Buyback     => BuybackStock.Cast<ShopItemVm>(),
            _                      => Stock
        };

        // Buyback list – filled when player sells via inventory context menu
        public ObservableCollection<BuybackItemVm> BuybackStock { get; } = new();

        public string Title    => Localization.T("npc.shop.title.equipment");
        public string BtnBack  => Localization.T("app.general.UI.back");
        public string BtnBuy   => _activeFilter == ShopFilter.Buyback
                                    ? Localization.T("npc.shop.rebuy")
                                    : Localization.T("npc.shop.buy");
        public string BtnSell  => Localization.T("npc.shop.sell");
        public string StockLabel         => Localization.T("npc.shop.stock");
        public string PlayerInventoryLabel => Localization.T("npc.shop.inventory");

        // Filter button labels (localized)
        public string LblWeapons     => Localization.T("npc.shop.filter.weapons");
        public string LblArmor       => Localization.T("npc.shop.filter.armor");
        public string LblAccessories => Localization.T("npc.shop.filter.accessories");
        public string LblUtilities   => Localization.T("npc.shop.filter.utilities");
        public string LblBuyback     => Localization.T("npc.shop.filter.buyback");

        public bool IsQuantityControlVisible => _activeFilter != ShopFilter.Buyback;

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
                OnPropertyChanged(nameof(BuyTotalPrice));
                OnPropertyChanged(nameof(CanBuy));
            }
        }

        public string SelectedStockName => SelectedStock?.Name ?? "";

        public string SelectedStockPrice
        {
            get
            {
                if (SelectedStock is BuybackItemVm bb)
                    return $"{StockPriceLabel}: {bb.TotalRebuyPrice}";
                if (SelectedStock != null)
                    return $"{StockPriceLabel}: {SelectedStock.BuyPrice}";
                return "";
            }
        }

        public string StockPriceLabel  => Localization.T("npc.shop.price");
        public string TotalLabel       => Localization.T("npc.shop.total");
        public string SellPriceLabel   => Localization.T("npc.shop.sellPrice");
        public string QuantityLabel    => Localization.T("npc.shop.quantity");
        public string MaxLabel         => Localization.T("app.general.UI.max");

        public int BuyQuantityMax => _selectedStock is BuybackItemVm ? 1
            : _selectedStock != null ? (int)(_player.Money.Balance.BronzeTotal / _selectedStock.BuyPrice) : 0;

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

        public int BuyTotalPrice => _selectedStock is BuybackItemVm bb
            ? bb.TotalRebuyPrice
            : _selectedStock != null ? _selectedStock.BuyPrice * _buyQuantity : 0;

        // Status
        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool CanBuy
        {
            get
            {
                if (_selectedStock == null) return false;
                if (_selectedStock is BuybackItemVm bb)
                    return _player.Money.Balance.BronzeTotal >= bb.TotalRebuyPrice;
                return _buyQuantity > 0 && _player.Money.Balance.BronzeTotal >= BuyTotalPrice;
            }
        }

        // Shop item tooltip
        private ItemTooltipViewModel _currentShopTooltip;
        private bool _isShopTooltipVisible;

        public ItemTooltipViewModel CurrentShopTooltip
        {
            get => _currentShopTooltip;
            set => SetProperty(ref _currentShopTooltip, value);
        }

        public bool IsShopTooltipVisible
        {
            get => _isShopTooltipVisible;
            set => SetProperty(ref _isShopTooltipVisible, value);
        }

        // Commands
        public ICommand FilterWeaponsCommand     { get; }
        public ICommand FilterArmorCommand       { get; }
        public ICommand FilterAccessoriesCommand { get; }
        public ICommand FilterUtilitiesCommand   { get; }
        public ICommand FilterBuybackCommand     { get; }
        public ICommand BackCommand              { get; }
        public ICommand BuyCommand               { get; }
        public ICommand IncreaseBuyQtyCommand    { get; }
        public ICommand DecreaseBuyQtyCommand    { get; }
        public ICommand MaxBuyQtyCommand         { get; }
        public ICommand ShowShopTooltipCommand   { get; }
        public ICommand HideShopTooltipCommand   { get; }

        public ShopPanelViewModel(Npc npc, Player player, Action goBack)
        {
            _npc    = npc;
            _player = player;
            _goBack = goBack;

            PlayerMoney = (int)_player.Money.Balance.BronzeTotal;

            _currentShopTooltip = new ItemTooltipViewModel();

            BackCommand           = new RelayCommand(_goBack);
            BuyCommand            = new RelayCommand(BuySelected);
            IncreaseBuyQtyCommand = new RelayCommand(() => BuyQuantity++);
            DecreaseBuyQtyCommand = new RelayCommand(() => BuyQuantity--);
            MaxBuyQtyCommand      = new RelayCommand(() => BuyQuantity = BuyQuantityMax);
            FilterWeaponsCommand     = new RelayCommand(() => ActiveFilter = ShopFilter.Weapons);
            FilterArmorCommand       = new RelayCommand(() => ActiveFilter = ShopFilter.Armor);
            FilterAccessoriesCommand = new RelayCommand(() => ActiveFilter = ShopFilter.Accessories);
            FilterUtilitiesCommand   = new RelayCommand(() => ActiveFilter = ShopFilter.Utilities);
            FilterBuybackCommand     = new RelayCommand(() => ActiveFilter = ShopFilter.Buyback);
            ShowShopTooltipCommand   = new RelayCommand<ShopItemVm>(ShowShopTooltip);
            HideShopTooltipCommand   = new RelayCommand(HideShopTooltip);

            _player.Inventory.ItemSold += OnItemSold;

            LoadShopStock();
        }

        private void OnItemSold(object? sender, ItemReceivedEventArgs e)
        {
            PlayerMoney = (int)_player.Money.Balance.BronzeTotal;

            // Add to buyback list so player can repurchase accidentally sold items
            BuybackStock.Add(new BuybackItemVm
            {
                Id       = e.Item.Id,
                Name     = Localization.T($"item.{e.Item.Id}"),
                BuyPrice = e.Item.BuyPrice,
                Quantity = e.Amount
            });

            if (_activeFilter == ShopFilter.Buyback)
                OnPropertyChanged(nameof(FilteredStock));
        }

        private void LoadShopStock()
        {
            Stock.Clear();

            foreach (var itemId in _npc.ItemNames)
            {
                if (!ItemFactory.TryCreateItem(itemId, out var item))
                    continue;

                if (item is EquipmentItem eq)
                {
                    // Tools always appear regardless of player class
                    if (eq.IsTool || eq.AllowedClasses.Contains(_player.Class))
                        Stock.Add(ShopEquipmentItemVm.FromEquipment(eq));
                }
                else
                {
                    if (item.AllowedClasses.Count == 0 || item.AllowedClasses.Contains(_player.Class))
                        Stock.Add(ShopItemVm.FromItem(item));
                }
            }

            SelectedStock = FilteredStock.FirstOrDefault();
            if (SelectedStock == null)
                StatusMessage = Localization.T("npc.shop.noStock");
        }

        private void BuySelected()
        {
            if (SelectedStock is BuybackItemVm buyback)
            {
                RebuySelected(buyback);
                return;
            }

            if (SelectedStock == null || BuyQuantity <= 0) return;

            int totalCost = SelectedStock.BuyPrice * BuyQuantity;

            if (_player.Money.Balance.BronzeTotal < totalCost)
            {
                StatusMessage = Localization.T("npc.shop.notEnoughMoney");
                return;
            }

            if (ItemFactory.TryCreateItem(SelectedStock.Id, out var item))
            {
                item.StackSize = BuyQuantity;

                if (_player.Inventory.AddItem(item, _player))
                {
                    _player.Money.TrySpend(totalCost);
                    PlayerMoney = (int)_player.Money.Balance.BronzeTotal;
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

        private void ShowShopTooltip(ShopItemVm shopItem)
        {
            if (shopItem == null) return;
            if (ItemFactory.TryCreateItem(shopItem.Id, out var item))
            {
                CurrentShopTooltip.SetItem(item);
                IsShopTooltipVisible = true;
            }
        }

        private void HideShopTooltip() => IsShopTooltipVisible = false;

        private void RebuySelected(BuybackItemVm buyback)
        {
            int totalCost = buyback.TotalRebuyPrice;

            if (_player.Money.Balance.BronzeTotal < totalCost)
            {
                StatusMessage = Localization.T("npc.shop.notEnoughMoney");
                return;
            }

            if (ItemFactory.TryCreateItem(buyback.Id, out var item))
            {
                item.StackSize = buyback.Quantity;

                if (_player.Inventory.AddItem(item, _player))
                {
                    _player.Money.TrySpend(totalCost);
                    PlayerMoney = (int)_player.Money.Balance.BronzeTotal;
                    BuybackStock.Remove(buyback);
                    StatusMessage = Localization.T("npc.shop.buySuccess", buyback.Quantity, buyback.Name);
                    OnPropertyChanged(nameof(FilteredStock));
                    SelectedStock = FilteredStock.FirstOrDefault();
                }
                else
                {
                    StatusMessage = Localization.T("npc.shop.inventoryFull");
                }
            }
        }
    }

    public class ShopItemVm : BaseViewModel
    {
        public string Id       { get; set; }
        public string Name     { get; set; }
        public int    BuyPrice { get; set; }

        public static ShopItemVm FromItem(Item item)
        {
            return new ShopItemVm
            {
                Id       = item.Id,
                Name     = Localization.T($"item.{item.Id}"),
                BuyPrice = item.BuyPrice
            };
        }

        public override string ToString() => Name;
    }

    public class BuybackItemVm : ShopItemVm
    {
        public int Quantity { get; set; }
        public int TotalRebuyPrice => BuyPrice * Quantity;
    }
}
