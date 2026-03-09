using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using MyriaLib.Entities.Items;
using MyriaLib.Entities.Players;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel;

namespace MyriaRPG.ViewModels
{
    /// <summary>
    /// ViewModel for the Inventory Page
    /// Handles all inventory logic and data binding
    /// Uses XAML DataTemplate icons from Icons.xaml resource dictionary
    /// </summary>
    public class InventoryPageViewModel : BaseViewModel
    {
        private Player _player;
        private Dictionary<string, int> _gridPositions;
        private string _moneyDisplay;
        private string _inventoryTitle;
        private string _equipmentTitle;
        private string _backpackTitle;
        private string _moneyBagTitle;
        private string _weaponHint;
        private string _armorHint;
        private string _accessoryHint;

        private const string INVENTORY_LAYOUT_FILE = "Data/player_inventory_layout.json";
        private const int COLS = 7;
        private const int ROWS = 7;
        private const int TOTAL_SLOTS = COLS * ROWS;

        // Observable collections for binding
        private ObservableCollection<InventoryItemViewModel> _inventoryItems;
        private EquipmentSlotViewModel _weaponSlot;
        private EquipmentSlotViewModel _armorSlot;
        private EquipmentSlotViewModel _accessorySlot;
        private ItemTooltipViewModel _currentTooltip;
        private bool _isTooltipVisible;

        // Commands
        public ICommand EquipItemCommand { get; }
        public ICommand UnequipItemCommand { get; }
        public ICommand UseItemCommand { get; }
        public ICommand SellItemCommand { get; }
        public ICommand ShowTooltipCommand { get; }
        public ICommand HideTooltipCommand { get; }

        // Properties
         [LocalizedKey("app.general.UI.inventory")]
        public string InventoryTitle
        {
            get => _inventoryTitle;
            set => SetProperty(ref _inventoryTitle, value);
        }
        [LocalizedKey("pg.inventory.equipment.title")]
        public string EquipmentTitle
        {
            get => _equipmentTitle;
            set => SetProperty(ref _equipmentTitle, value);
        }
        [LocalizedKey("pg.inventory.backpack.title")]
        public string BackpackTitle
        {
            get => _backpackTitle;
            set => SetProperty(ref _backpackTitle, value);
        }
        [LocalizedKey("pg.inventory.moneybag.title")]
        public string MoneyBagTitle
        {
            get => _moneyBagTitle;
            set => SetProperty(ref _moneyBagTitle, value);
        }
        [LocalizedKey("pg.inventory.slot.weapon")]
        public string WeaponHint
        {
            get => _weaponHint;
            set => SetProperty(ref _weaponHint, value);
        }
        [LocalizedKey("pg.inventory.slot.armor")]
        public string ArmorHint
        {
            get => _armorHint;
            set => SetProperty(ref _armorHint, value);
        }
        [LocalizedKey("pg.inventory.slot.accessory")]
        public string AccessoryHint
        {
            get => _accessoryHint;
            set => SetProperty(ref _accessoryHint, value);
        }
        public Player Player
        {
            get => _player;
            set => SetProperty(ref _player, value);
        }

        public ObservableCollection<InventoryItemViewModel> InventoryItems
        {
            get => _inventoryItems;
            set => SetProperty(ref _inventoryItems, value);
        }

        public EquipmentSlotViewModel WeaponSlot
        {
            get => _weaponSlot;
            set => SetProperty(ref _weaponSlot, value);
        }

        public EquipmentSlotViewModel ArmorSlot
        {
            get => _armorSlot;
            set => SetProperty(ref _armorSlot, value);
        }

        public EquipmentSlotViewModel AccessorySlot
        {
            get => _accessorySlot;
            set => SetProperty(ref _accessorySlot, value);
        }

        public string MoneyDisplay
        {
            get => _moneyDisplay;
            set => SetProperty(ref _moneyDisplay, value);
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

        /// <summary>
        /// Constructor
        /// </summary>
        public InventoryPageViewModel(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _gridPositions = new Dictionary<string, int>();

            // Initialize collections
            _inventoryItems = new ObservableCollection<InventoryItemViewModel>();
            _weaponSlot = new EquipmentSlotViewModel(EquipmentType.Weapon);
            _armorSlot = new EquipmentSlotViewModel(EquipmentType.Armor);
            _accessorySlot = new EquipmentSlotViewModel(EquipmentType.Accessory);
            _currentTooltip = new ItemTooltipViewModel();
            _isTooltipVisible = false;

            // Initialize commands
            EquipItemCommand = new RelayCommand<InventoryItemViewModel>(EquipItem);
            UnequipItemCommand = new RelayCommand<EquipmentType>(UnequipItem);
            UseItemCommand = new RelayCommand<InventoryItemViewModel>(UseItem);
            SellItemCommand = new RelayCommand<InventoryItemViewModel>(SellItem);
            ShowTooltipCommand = new RelayCommand<InventoryItemViewModel>(ShowTooltip);
            HideTooltipCommand = new RelayCommand(HideTooltip);

            // Subscribe to player events
            _player.Inventory.ItemReceived += (s, e) => RefreshInventory();

            // Load initial state
            LoadInventoryLayout();
            RefreshInventory();
            UpdateMoneyDisplay();
        }

        /// <summary>
        /// Refresh entire inventory display
        /// </summary>
        public void RefreshInventory()
        {
            InventoryItems.Clear();

            // Create view models for each item
            for (int i = 0; i < _player.Inventory.Items.Count; i++)
            {
                var item = _player.Inventory.Items[i];
                var viewModel = new InventoryItemViewModel(item, i);
                InventoryItems.Add(viewModel);
            }

            // Refresh equipment slots
            RefreshEquipmentSlots();
            UpdateMoneyDisplay();
        }

        /// <summary>
        /// Refresh equipment slots
        /// </summary>
        public void RefreshEquipmentSlots()
        {
            WeaponSlot.Item = _player.WeaponSlot;
            ArmorSlot.Item = _player.ArmorSlot;
            AccessorySlot.Item = _player.AccessorySlot;
        }

        /// <summary>
        /// Equip an item from inventory
        /// </summary>
        private void EquipItem(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item is not EquipmentItem equipment)
                return;

            if (!equipment.IsUsableBy(_player))
            {
                System.Windows.MessageBox.Show("This item cannot be equipped by your class!");
                return;
            }

            _player.Inventory.SwapEquipment(equipment.Name, _player);
            RefreshInventory();
        }

        /// <summary>
        /// Unequip an item from equipment slot
        /// </summary>
        private void UnequipItem(EquipmentType slotType)
        {
            EquipmentItem equipped = slotType switch
            {
                EquipmentType.Weapon => _player.WeaponSlot,
                EquipmentType.Armor => _player.ArmorSlot,
                EquipmentType.Accessory => _player.AccessorySlot,
                _ => null
            };

            if (equipped == null)
                return;

            if (_player.Inventory.AddItem(equipped, _player))
            {
                switch (slotType)
                {
                    case EquipmentType.Weapon:
                        _player.WeaponSlot = null;
                        break;
                    case EquipmentType.Armor:
                        _player.ArmorSlot = null;
                        break;
                    case EquipmentType.Accessory:
                        _player.AccessorySlot = null;
                        break;
                }
                RefreshInventory();
            }
        }

        /// <summary>
        /// Use a consumable item
        /// </summary>
        private void UseItem(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item is not ConsumableItem consumable)
                return;

            _player.Inventory.UseItem(consumable.Name, _player);
            RefreshInventory();
        }

        /// <summary>
        /// Sell an item
        /// </summary>
        private void SellItem(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item == null)
                return;

            if (_player.Inventory.SellItem(itemViewModel.Item.Name, itemViewModel.Item.StackSize, ref _player))
            {
                RefreshInventory();
            }
        }

        /// <summary>
        /// Show tooltip for an item
        /// </summary>
        private void ShowTooltip(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item == null)
                return;

            CurrentTooltip.SetItem(itemViewModel.Item);
            IsTooltipVisible = true;
        }

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        private void HideTooltip()
        {
            IsTooltipVisible = false;
        }

        /// <summary>
        /// Handle item drag-drop
        /// </summary>
        public void HandleItemDrop(InventoryItemViewModel draggedItem, int targetSlotIndex)
        {
            System.Diagnostics.Debug.WriteLine($"HandleItemDrop called with item: {draggedItem?.Item?.Name}, targetSlotIndex: {targetSlotIndex}");
            
            if (draggedItem?.Item == null)
            {
                System.Diagnostics.Debug.WriteLine("Dragged item is null");
                return;
            }

            // If the item being dropped is equipped equipment, unequip it first
            if (draggedItem.Item is EquipmentItem equipment)
            {
                System.Diagnostics.Debug.WriteLine($"Item is equipped equipment, attempting to unequip: {equipment.Name}");
                
                // Check which slot it's currently in and unequip it
                if (_player.WeaponSlot == equipment)
                {
                    System.Diagnostics.Debug.WriteLine("Unequipping from weapon slot");
                    _player.WeaponSlot = null;
                    System.Diagnostics.Debug.WriteLine($"Adding unequipped item back to inventory: {equipment.Name}");
                    _player.Inventory.AddItem(equipment, _player, "unequip");
                }
                else if (_player.ArmorSlot == equipment)
                {
                    System.Diagnostics.Debug.WriteLine("Unequipping from armor slot");
                    _player.ArmorSlot = null;
                    System.Diagnostics.Debug.WriteLine($"Adding unequipped item back to inventory: {equipment.Name}");
                    _player.Inventory.AddItem(equipment, _player, "unequip");
                }
                else if (_player.AccessorySlot == equipment)
                {
                    System.Diagnostics.Debug.WriteLine("Unequipping from accessory slot");
                    _player.AccessorySlot = null;
                    System.Diagnostics.Debug.WriteLine($"Adding unequipped item back to inventory: {equipment.Name}");
                    _player.Inventory.AddItem(equipment, _player, "unequip");
                }
                
                System.Diagnostics.Debug.WriteLine($"Equipment unequipped, refreshing inventory");
                RefreshInventory();
                return;
            }

            // For regular inventory items, update position tracking
            System.Diagnostics.Debug.WriteLine($"Updating grid position for {draggedItem.Item.Id} to {targetSlotIndex}");
            _gridPositions[draggedItem.Item.Id] = targetSlotIndex;
            SaveInventoryLayout();
            System.Diagnostics.Debug.WriteLine($"Refreshing inventory");
            RefreshInventory();
            System.Diagnostics.Debug.WriteLine($"Inventory refresh complete");
        }

        /// <summary>
        /// Handle equipment slot drop
        /// </summary>
        public void HandleEquipmentDrop(InventoryItemViewModel draggedItem, EquipmentType slotType)
        {
            System.Diagnostics.Debug.WriteLine($"HandleEquipmentDrop called with item: {draggedItem?.Item?.Name}, slotType: {slotType}");
            
            if (draggedItem?.Item == null)
            {
                System.Diagnostics.Debug.WriteLine("Dragged item is null");
                return;
            }

            if (draggedItem.Item is not EquipmentItem equipment)
            {
                System.Diagnostics.Debug.WriteLine($"Item is not EquipmentItem, it's {draggedItem.Item.GetType().Name}");
                System.Windows.MessageBox.Show("You can only equip equipment items!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Item is EquipmentItem: {equipment.Name}");

            if (!equipment.IsUsableBy(_player))
            {
                System.Diagnostics.Debug.WriteLine($"Item not usable by player class");
                System.Windows.MessageBox.Show("This item cannot be equipped by your class!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Calling SwapEquipment with itemId: {equipment.Id}");
            _player.Inventory.SwapEquipment(equipment.Id, _player);
            System.Diagnostics.Debug.WriteLine($"SwapEquipment completed, refreshing inventory");
            RefreshInventory();
            System.Diagnostics.Debug.WriteLine($"Inventory refreshed");
        }

        /// <summary>
        /// Update money display
        /// </summary>
        public void UpdateMoneyDisplay()
        {
            // User cleared this method - keeping stub
            MoneyDisplay = "0 Gold";
        }

        /// <summary>
        /// Load inventory layout from file
        /// </summary>
        private void LoadInventoryLayout()
        {
            try
            {
                if (File.Exists(INVENTORY_LAYOUT_FILE))
                {
                    string json = File.ReadAllText(INVENTORY_LAYOUT_FILE);
                    _gridPositions = JsonSerializer.Deserialize<Dictionary<string, int>>(json) 
                                   ?? new Dictionary<string, int>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load inventory layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Save inventory layout to file
        /// </summary>
        private void SaveInventoryLayout()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(INVENTORY_LAYOUT_FILE) ?? "Data");
                string json = JsonSerializer.Serialize(_gridPositions);
                File.WriteAllText(INVENTORY_LAYOUT_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save inventory layout: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// ViewModel for an inventory item
    /// </summary>
    public class InventoryItemViewModel : BaseViewModel
    {
        private Item _item;
        private int _index;
        private string _rarityColor;
        private Brush _rarityBrush;

        public Item Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public string RarityColor
        {
            get => _rarityColor;
            set => SetProperty(ref _rarityColor, value);
        }

        public Brush RarityBrush
        {
            get => _rarityBrush;
            set => SetProperty(ref _rarityBrush, value);
        }

        public InventoryItemViewModel(Item item, int index)
        {
            _item = item;
            _index = index;
            UpdateRarityColor();
        }

        private void UpdateRarityColor()
        {
            RarityColor = GetRarityColor(_item.Rarity);
            RarityBrush = GetRarityBrush(_item.Rarity);
        }

        private string GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "#A0A0A0",
                ItemRarity.Uncommon => "#1EFF00",
                ItemRarity.Rare => "#0070DD",
                ItemRarity.Epic => "#A335EE",
                ItemRarity.Unique => "#FF8000",
                ItemRarity.Legendary => "#FF8000",
                ItemRarity.Godly => "#FF0000",
                _ => "#A0A0A0"
            };
        }

        private Brush GetRarityBrush(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                ItemRarity.Uncommon => new SolidColorBrush(Color.FromRgb(30, 255, 0)),
                ItemRarity.Rare => new SolidColorBrush(Color.FromRgb(0, 112, 221)),
                ItemRarity.Epic => new SolidColorBrush(Color.FromRgb(163, 53, 238)),
                ItemRarity.Unique => new SolidColorBrush(Color.FromRgb(170, 100, 100)),
                ItemRarity.Legendary => new SolidColorBrush(Color.FromRgb(255, 128, 0)),
                ItemRarity.Godly => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                _ => new SolidColorBrush(Color.FromRgb(160, 160, 160))
            };
        }
    }

    /// <summary>
    /// ViewModel for equipment slot
    /// </summary>
    public class EquipmentSlotViewModel : BaseViewModel
    {
        private EquipmentType _slotType;
        private EquipmentItem _item;
        private string _slotLabel;
        private Brush _borderColor;

        public EquipmentType SlotType
        {
            get => _slotType;
            set => SetProperty(ref _slotType, value);
        }

        public EquipmentItem Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public string SlotLabel
        {
            get => _slotLabel;
            set => SetProperty(ref _slotLabel, value);
        }

        public Brush BorderColor
        {
            get => _borderColor;
            set => SetProperty(ref _borderColor, value);
        }

        public EquipmentSlotViewModel(EquipmentType slotType)
        {
            _slotType = slotType;
            UpdateSlotProperties();
        }

        private void UpdateSlotProperties()
        {
            SlotLabel = _slotType.ToString().ToUpper();
            BorderColor = _slotType switch
            {
                EquipmentType.Weapon => new SolidColorBrush(Color.FromRgb(255, 183, 0)),
                EquipmentType.Armor => new SolidColorBrush(Color.FromRgb(30, 255, 0)),
                EquipmentType.Accessory => new SolidColorBrush(Color.FromRgb(0, 112, 221)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }

    /// <summary>
    /// ViewModel for item tooltip
    /// </summary>
    public class ItemTooltipViewModel : BaseViewModel
    {
        private string _itemName;
        private string _itemType;
        private string _itemRarity;
        private Brush _rarityColor;
        private string _itemStats;

        public string ItemName
        {
            get => Localization.T(_itemName);
            set => SetProperty(ref _itemName, value);
        }

        public string ItemType
        {
            get => _itemType;
            set => SetProperty(ref _itemType, value);
        }

        public string ItemRarity
        {
            get => _itemRarity;
            set => SetProperty(ref _itemRarity, value);
        }

        public Brush RarityColor
        {
            get => _rarityColor;
            set => SetProperty(ref _rarityColor, value);
        }

        public string ItemStats
        {
            get => _itemStats;
            set => SetProperty(ref _itemStats, value);
        }

        public void SetItem(Item item)
        {
            ItemName = item.Name;
            ItemType = $"{Localization.T("pg.inventory.tooltip.type")}: {item.GetType().Name}";
            ItemRarity = $"{Localization.T("pg.inventory.tooltip.rarity")}: {item.Rarity}";
            RarityColor = GetRarityBrush(item.Rarity);

            string stats = BuildStatsString(item);
            ItemStats = stats;
        }

        /// <summary>
        /// Builds a stats string showing only non-zero bonuses using localized stat names
        /// </summary>
        private string BuildStatsString(Item item)
        {
            var statLines = new List<string>();

            if (item is EquipmentItem equip)
            {
                // Combat Stats
                if (equip.BonusATK > 0) statLines.Add($"{Localization.T("pg.inventory.stat.atk")}: +{equip.BonusATK}");
                if (equip.BonusDEF > 0) statLines.Add($"{Localization.T("pg.inventory.stat.def")}: +{equip.BonusDEF}");
                if (equip.BonusMATK > 0) statLines.Add($"{Localization.T("pg.inventory.stat.matk")}: +{equip.BonusMATK}");
                if (equip.BonusMDEF > 0) statLines.Add($"{Localization.T("pg.inventory.stat.mdef")}: +{equip.BonusMDEF}");

                // Attribute Stats
                if (equip.BonusSTR > 0) statLines.Add($"{Localization.T("pg.inventory.stat.str")}: +{equip.BonusSTR}");
                if (equip.BonusDEX > 0) statLines.Add($"{Localization.T("pg.inventory.stat.dex")}: +{equip.BonusDEX}");
                if (equip.BonusEND > 0) statLines.Add($"{Localization.T("pg.inventory.stat.end")}: +{equip.BonusEND}");
                if (equip.BonusINT > 0) statLines.Add($"{Localization.T("pg.inventory.stat.int")}: +{equip.BonusINT}");
                if (equip.BonusSPR > 0) statLines.Add($"{Localization.T("pg.inventory.stat.spr")}: +{equip.BonusSPR}");

                // Resource Stats
                if (equip.BonusHP > 0) statLines.Add($"{Localization.T("pg.inventory.stat.hp")}: +{equip.BonusHP}");
                if (equip.BonusMP > 0) statLines.Add($"{Localization.T("pg.inventory.stat.mp")}: +{equip.BonusMP}");

                // Percent Stats
                if (equip.BonusAim > 0) statLines.Add($"{Localization.T("pg.inventory.stat.aim")}: +{equip.BonusAim}%");
                if (equip.BonusEvasion > 0) statLines.Add($"{Localization.T("pg.inventory.stat.evasion")}: +{equip.BonusEvasion}%");
                if (equip.BonusCrit > 0) statLines.Add($"{Localization.T("pg.inventory.stat.crit")}: +{equip.BonusCrit}%");
                if (equip.BonusBlock > 0) statLines.Add($"{Localization.T("pg.inventory.stat.block")}: +{equip.BonusBlock}%");

                if (statLines.Count == 0)
                    return Localization.T("pg.inventory.tooltip.no_bonuses");

                return string.Join("\n", statLines);
            }
            else if (item is ConsumableItem consumable)
            {
                var consumableStats = new List<string>();
                
                if (consumable.HealAmount > 0) consumableStats.Add($"{Localization.T("pg.inventory.stat.heal")}: {consumable.HealAmount}");
                if (consumable.ManaRestore > 0) consumableStats.Add($"{Localization.T("pg.inventory.stat.mana")}: {consumable.ManaRestore}");

                if (consumableStats.Count == 0)
                    return Localization.T("pg.inventory.tooltip.no_effects");

                return string.Join("\n", consumableStats);
            }

            return "";
        }

        private Brush GetRarityBrush(ItemRarity rarity)
        {
            return rarity switch
            {
                MyriaLib.Systems.Enums.ItemRarity.Common => new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                MyriaLib.Systems.Enums.ItemRarity.Uncommon => new SolidColorBrush(Color.FromRgb(30, 255, 0)),
                MyriaLib.Systems.Enums.ItemRarity.Rare => new SolidColorBrush(Color.FromRgb(0, 112, 221)),
                MyriaLib.Systems.Enums.ItemRarity.Epic => new SolidColorBrush(Color.FromRgb(163, 53, 238)),
                MyriaLib.Systems.Enums.ItemRarity.Unique => new SolidColorBrush(Color.FromRgb(170, 100, 100)),
                MyriaLib.Systems.Enums.ItemRarity.Legendary => new SolidColorBrush(Color.FromRgb(255, 128, 0)),
                MyriaLib.Systems.Enums.ItemRarity.Godly => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                _ => new SolidColorBrush(Color.FromRgb(160, 160, 160))
            };
        }
    }
}
