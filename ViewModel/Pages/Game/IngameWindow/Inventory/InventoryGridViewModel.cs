using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using MyriaLib.Entities.Items;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory
{
    /// <summary>
    /// ViewModel for the inventory item grid (slots, drag/drop, tooltip).
    /// Reusable wherever the inventory grid is shown (standalone inventory page, shop sell panel, etc.)
    /// </summary>
    public class InventoryGridViewModel : BaseViewModel
    {
        private Player _player;
        private Dictionary<string, int> _gridPositions = new();
        private string _inventoryTitle;
        private ItemTooltipViewModel _currentTooltip;
        private bool _isTooltipVisible;

        private const string INVENTORY_LAYOUT_FILE = "Data/player_inventory_layout.json";

        public ObservableCollection<InventoryItemViewModel> InventoryItems { get; } = new();

        [LocalizedKey("app.general.UI.inventory")]
        public string InventoryTitle
        {
            get => _inventoryTitle;
            set => SetProperty(ref _inventoryTitle, value);
        }

        public ItemTooltipViewModel CurrentTooltip
        {
            get => _currentTooltip;
            set => SetProperty(ref _currentTooltip, value);
        }

        public bool IsTooltipVisible
        {
            get => _isTooltipVisible;
            set => SetProperty(ref _isTooltipVisible, value);
        }

        public string LblSellOne   => Localization.T("npc.shop.context.sell_one");
        public string LblSellStack => Localization.T("npc.shop.context.sell_stack");

        public ICommand EquipItemCommand { get; }
        public ICommand UseItemCommand { get; }
        public ICommand SellItemCommand { get; }
        public ICommand SellOneCommand { get; }
        public ICommand SellStackCommand { get; }
        public ICommand ShowTooltipCommand { get; }
        public ICommand HideTooltipCommand { get; }

        public InventoryGridViewModel(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _currentTooltip = new ItemTooltipViewModel();

            EquipItemCommand = new RelayCommand<InventoryItemViewModel>(EquipItem);
            UseItemCommand = new RelayCommand<InventoryItemViewModel>(UseItem);
            SellItemCommand = new RelayCommand<InventoryItemViewModel>(SellItem);
            SellOneCommand = new RelayCommand<InventoryItemViewModel>(vm => SellAmount(vm, 1));
            SellStackCommand = new RelayCommand<InventoryItemViewModel>(vm => SellAmount(vm, vm?.Item?.StackSize ?? 0));
            ShowTooltipCommand = new RelayCommand<InventoryItemViewModel>(ShowTooltip);
            HideTooltipCommand = new RelayCommand(HideTooltip);

            _player.Inventory.ItemReceived += (s, e) => RefreshInventory();
            _player.Inventory.ItemRemoved += (s, e) => RefreshInventory();

            LoadInventoryLayout();
            RefreshInventory();
        }

        public void RefreshInventory()
        {
            InventoryItems.Clear();
            for (int i = 0; i < _player.Inventory.Items.Count; i++)
                InventoryItems.Add(new InventoryItemViewModel(_player.Inventory.Items[i], i));
        }

        public void HandleItemDrop(InventoryItemViewModel draggedItem, int targetSlotIndex)
        {
            if (draggedItem?.Item == null) return;

            if (draggedItem.Item is EquipmentItem equipment)
            {
                if (_player.WeaponSlot == equipment) _player.WeaponSlot = null;
                else if (_player.ArmorSlot == equipment) _player.ArmorSlot = null;
                else if (_player.AccessorySlot == equipment) _player.AccessorySlot = null;
                _player.Inventory.AddItem(equipment, _player, "unequip");
                RefreshInventory();
                return;
            }

            _gridPositions[draggedItem.Item.Id] = targetSlotIndex;
            SaveInventoryLayout();
            RefreshInventory();
        }

        private void EquipItem(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item is not EquipmentItem equipment) return;
            if (!equipment.IsUsableBy(_player))
            {
                System.Windows.MessageBox.Show("This item cannot be equipped by your class!");
                return;
            }
            _player.Inventory.SwapEquipment(equipment.Name, _player);
            RefreshInventory();
        }

        private void UseItem(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item is not ConsumableItem consumable) return;
            _player.Inventory.UseItem(consumable.Name, _player);
            RefreshInventory();
        }

        private void SellItem(InventoryItemViewModel itemViewModel) =>
            SellAmount(itemViewModel, itemViewModel?.Item?.StackSize ?? 0);

        private void SellAmount(InventoryItemViewModel itemViewModel, int amount)
        {
            if (itemViewModel?.Item == null || amount <= 0) return;
            if (_player.Inventory.SellItem(itemViewModel.Item.Name, amount, ref _player))
                RefreshInventory();
        }

        private void ShowTooltip(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item == null) return;
            CurrentTooltip.SetItem(itemViewModel.Item);
            IsTooltipVisible = true;
        }

        private void HideTooltip() => IsTooltipVisible = false;

        private void LoadInventoryLayout()
        {
            try
            {
                if (File.Exists(INVENTORY_LAYOUT_FILE))
                    _gridPositions = JsonSerializer.Deserialize<Dictionary<string, int>>(
                        File.ReadAllText(INVENTORY_LAYOUT_FILE)) ?? new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load inventory layout: {ex.Message}");
            }
        }

        private void SaveInventoryLayout()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(INVENTORY_LAYOUT_FILE) ?? "Data");
                File.WriteAllText(INVENTORY_LAYOUT_FILE, JsonSerializer.Serialize(_gridPositions));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save inventory layout: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// ViewModel for a single inventory slot.
    /// </summary>
    public class InventoryItemViewModel : BaseViewModel
    {
        private Item _item;
        private int _index;
        private Brush _rarityBrush;

        public Item Item { get => _item; set => SetProperty(ref _item, value); }
        public int Index { get => _index; set => SetProperty(ref _index, value); }
        public Brush RarityBrush { get => _rarityBrush; set => SetProperty(ref _rarityBrush, value); }
        public bool IsStack => (_item?.StackSize ?? 0) > 1;
        public int GridColumn => _index % 7;
        public int GridRow => _index / 7;

        public InventoryItemViewModel(Item item, int index)
        {
            _item = item;
            _index = index;
            RarityBrush = GetRarityBrush(item.Rarity);
        }

        private static Brush GetRarityBrush(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common    => new SolidColorBrush(Color.FromRgb(160, 160, 160)),
            ItemRarity.Uncommon  => new SolidColorBrush(Color.FromRgb(30, 255, 0)),
            ItemRarity.Rare      => new SolidColorBrush(Color.FromRgb(0, 112, 221)),
            ItemRarity.Epic      => new SolidColorBrush(Color.FromRgb(163, 53, 238)),
            ItemRarity.Unique    => new SolidColorBrush(Color.FromRgb(170, 100, 100)),
            ItemRarity.Legendary => new SolidColorBrush(Color.FromRgb(255, 128, 0)),
            ItemRarity.Godly     => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            _                    => new SolidColorBrush(Color.FromRgb(160, 160, 160))
        };
    }

    /// <summary>
    /// ViewModel for the item hover tooltip. Shared by both InventoryGridViewModel and EquipmentViewModel.
    /// </summary>
    public class ItemTooltipViewModel : BaseViewModel
    {
        private string _itemNameKey;
        private string _itemNameSuffix = "";
        private string _itemType;
        private string _itemRarity;
        private Brush _rarityColor;
        private string _itemStats;

        // Lazy-translates the stored key so language switches work without re-calling SetItem.
        // The suffix (e.g. " +3") is appended after translation.
        public string ItemName => Localization.T(_itemNameKey) + _itemNameSuffix;
        public string ItemType { get => _itemType; set => SetProperty(ref _itemType, value); }
        public string ItemRarity { get => _itemRarity; set => SetProperty(ref _itemRarity, value); }
        public Brush RarityColor { get => _rarityColor; set => SetProperty(ref _rarityColor, value); }
        public string ItemStats { get => _itemStats; set => SetProperty(ref _itemStats, value); }

        public void SetItem(Item item)
        {
            _itemNameKey = item.Name;
            _itemNameSuffix = item is EquipmentItem eq && eq.UpgradeLevel >= 1
                ? $" +{eq.UpgradeLevel}"
                : "";
            OnPropertyChanged(nameof(ItemName));
            ItemType = $"{Localization.T("pg.inventory.tooltip.type")}: {item.GetType().Name}";
            ItemRarity = $"{Localization.T("pg.inventory.tooltip.rarity")}: {item.Rarity}";
            RarityColor = GetRarityBrush(item.Rarity);
            ItemStats = BuildStatsString(item);
        }

        private static string BuildStatsString(Item item)
        {
            if (item is EquipmentItem equip)
            {
                var lines = new List<string>();
                if (equip.BonusATK > 0)     lines.Add($"{Localization.T("pg.inventory.stat.atk")}: +{equip.BonusATK}");
                if (equip.BonusDEF > 0)     lines.Add($"{Localization.T("pg.inventory.stat.def")}: +{equip.BonusDEF}");
                if (equip.BonusMATK > 0)    lines.Add($"{Localization.T("pg.inventory.stat.matk")}: +{equip.BonusMATK}");
                if (equip.BonusMDEF > 0)    lines.Add($"{Localization.T("pg.inventory.stat.mdef")}: +{equip.BonusMDEF}");
                if (equip.BonusSTR > 0)     lines.Add($"{Localization.T("pg.inventory.stat.str")}: +{equip.BonusSTR}");
                if (equip.BonusDEX > 0)     lines.Add($"{Localization.T("pg.inventory.stat.dex")}: +{equip.BonusDEX}");
                if (equip.BonusEND > 0)     lines.Add($"{Localization.T("pg.inventory.stat.end")}: +{equip.BonusEND}");
                if (equip.BonusINT > 0)     lines.Add($"{Localization.T("pg.inventory.stat.int")}: +{equip.BonusINT}");
                if (equip.BonusSPR > 0)     lines.Add($"{Localization.T("pg.inventory.stat.spr")}: +{equip.BonusSPR}");
                if (equip.BonusHP > 0)      lines.Add($"{Localization.T("pg.inventory.stat.hp")}: +{equip.BonusHP}");
                if (equip.BonusMP > 0)      lines.Add($"{Localization.T("pg.inventory.stat.mp")}: +{equip.BonusMP}");
                if (equip.BonusAim > 0)     lines.Add($"{Localization.T("pg.inventory.stat.aim")}: +{equip.BonusAim}%");
                if (equip.BonusEvasion > 0) lines.Add($"{Localization.T("pg.inventory.stat.evasion")}: +{equip.BonusEvasion}%");
                if (equip.BonusCrit > 0)    lines.Add($"{Localization.T("pg.inventory.stat.crit")}: +{equip.BonusCrit}%");
                if (equip.BonusBlock > 0)   lines.Add($"{Localization.T("pg.inventory.stat.block")}: +{equip.BonusBlock}%");
                return lines.Count > 0 ? string.Join("\n", lines) : Localization.T("pg.inventory.tooltip.no_bonuses");
            }

            if (item is ConsumableItem consumable)
            {
                var lines = new List<string>();
                if (consumable.HealAmount > 0)  lines.Add($"{Localization.T("pg.inventory.stat.heal")}: {consumable.HealAmount}");
                if (consumable.ManaRestore > 0) lines.Add($"{Localization.T("pg.inventory.stat.mana")}: {consumable.ManaRestore}");
                return lines.Count > 0 ? string.Join("\n", lines) : Localization.T("pg.inventory.tooltip.no_effects");
            }

            return string.Empty;
        }

        private static Brush GetRarityBrush(ItemRarity rarity) => rarity switch
        {
            0    => new SolidColorBrush(Color.FromRgb(160, 160, 160)),
            MyriaLib.Systems.Enums.ItemRarity.Uncommon  => new SolidColorBrush(Color.FromRgb(30, 255, 0)),
            MyriaLib.Systems.Enums.ItemRarity.Rare => new SolidColorBrush(Color.FromRgb(0, 112, 221)),
            MyriaLib.Systems.Enums.ItemRarity.Epic      => new SolidColorBrush(Color.FromRgb(163, 53, 238)),
            MyriaLib.Systems.Enums.ItemRarity.Unique    => new SolidColorBrush(Color.FromRgb(170, 100, 100)),
            MyriaLib.Systems.Enums.ItemRarity.Legendary => new SolidColorBrush(Color.FromRgb(255, 128, 0)),
            MyriaLib.Systems.Enums.ItemRarity.Godly     => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            _                    => new SolidColorBrush(Color.FromRgb(160, 160, 160))
        };
    }
}
