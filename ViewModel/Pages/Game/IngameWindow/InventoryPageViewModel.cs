using MyriaLib.Entities.Items;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaLib.Systems.Events;
using MyriaRPG.Model;
using MyriaRPG.Model.Behaviors;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class InventoryPageViewModel : BaseViewModel
    {
        private Player _player;
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }
        private string _windowTitle = Localization.T("pg.inventory.title");
        private Dictionary<ItemRarity, SolidColorBrush> rarityColors = new Dictionary<ItemRarity, SolidColorBrush>() { 
            { ItemRarity.Common, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"))},
            { ItemRarity.Uncommon, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3CB371")) },
            { ItemRarity.Rare, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF"))},
            { ItemRarity.Epic, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF")) },
            { ItemRarity.Unique, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF59D")) },
            { ItemRarity.Legendary, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DAA520")) },
            { ItemRarity.Godly, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB6C1")) }
        };
        
        // Equipped
        public ItemVm? EquippedWeapon { get => _ew; set { _ew = value; OnPropertyChanged(); } }
        private ItemVm? _ew;
        public ItemVm? EquippedArmor { get => _ea; set { _ea = value; OnPropertyChanged(); } }
        private ItemVm? _ea;
        public ItemVm? EquippedAccessory { get => _ex; set { _ex = value; OnPropertyChanged(); } }
        private ItemVm? _ex;

        // Backpack
        public ObservableCollection<ItemVm> Backpack { get; } = new();
        public ItemVm? SelectedItem { get => _sel; set { _sel = value; OnPropertyChanged(); } }
        private ItemVm? _sel;

        // Money
        private Money money => new Money(Bronze);
        public long Bronze { get => _b; set { _b = value; OnPropertyChanged(); OnPropertyChanged(nameof(MoneyDisplay)); } }
        private long _b;
        public string MoneyDisplay => money.ToString();
        public string MoneyLabel => Localization.T("npc.shop.money");
        
        // Commands
        public ICommand BeginDragItemCommand { get; }
        public ICommand EquipDropCommand { get; }
        public ICommand UnequipCommand { get; }

        public InventoryPageViewModel()
        {
            BeginDragItemCommand = new RelayCommand<ItemVm>(_ => { /* optional: highlight valid slots */ });
            EquipDropCommand = new RelayCommand<EquipDropArgs>(OnEquipDropped);
            UnequipCommand = new RelayCommand<string>(UnequipItem);

            _player = UserAccoundService.CurrentCharacter;
            
            // Subscribe to inventory changes
            _player.Inventory.ItemReceived += OnInventoryChanged;
            
            RefreshBackpack();

            // Initialize equipped items
            if (_player.WeaponSlot != null)
            {
                EquipmentItem weapon = _player.WeaponSlot;
                ItemVm temp = new ItemVm(weapon);
                temp.Type = weapon.SlotType.ToString();
                temp.Color = rarityColors[weapon.Rarity];
                EquippedWeapon = temp;
            }
            if (_player.ArmorSlot != null)
            {
                EquipmentItem armor = _player.ArmorSlot;
                ItemVm temp = new ItemVm(armor);
                temp.Type = armor.SlotType.ToString();
                temp.Color = rarityColors[armor.Rarity];
                EquippedArmor = temp;
            }
            if (_player.AccessorySlot != null)
            {
                EquipmentItem accessory = _player.AccessorySlot;
                ItemVm temp = new ItemVm(accessory);
                temp.Type = accessory.SlotType.ToString();
                temp.Color = rarityColors[accessory.Rarity];
                EquippedAccessory = temp;
            }
            Bronze = _player.Money.Coins.TotalBronze;
        }

        private void OnInventoryChanged(object? sender, ItemReceivedEventArgs e)
        {
            // Refresh the whole backpack to ensure sync
            System.Windows.Application.Current.Dispatcher.Invoke(RefreshBackpack);
        }

        /// <summary>
        /// Refreshes the backpack display with actual items and empty slots.
        /// Always displays exactly 49 slots (7x7 grid).
        /// </summary>
        private void RefreshBackpack()
        {
            Backpack.Clear();
            
            int maxCapacity = _player.Inventory.Capacity; // Should be 49
            int itemCount = _player.Inventory.Items.Count;

            // Add all actual items from inventory
            foreach (Item item in _player.Inventory.Items)
            {
                ItemVm itemVm = new ItemVm(item);
                itemVm.Color = rarityColors[item.Rarity];
                
                string itemType = "";
                if (item is EquipmentItem eq)
                    itemType = $"{eq.SlotType}";
                else if (item is ConsumableItem con)
                    itemType = $"Consumable";
                else if (item is MaterialItem mat)
                    itemType = $"Material";
                
                itemVm.Type = itemType;
                Backpack.Add(itemVm);
            }
            
            // Fill remaining slots with empty placeholders to show full 7x7 grid
            int remainingSlots = maxCapacity - itemCount;
            for (int i = 0; i < remainingSlots; i++)
            {
                Backpack.Add(ItemVm.CreateEmptySlot());
            }
        }

        private void UnequipItem(string slot)
        {
            switch (slot)
            {
                case "Weapon":
                    if (EquippedWeapon != null)
                    {
                        if (_player.Inventory.UnequipItem(EquippedWeapon.Id, _player))
                        {
                            RefreshBackpack(); // Refresh to show unequipped item
                            EquippedWeapon = null;
                        }
                    }
                    break;
                case "Armor":
                    if (EquippedArmor != null)
                    {
                        if (_player.Inventory.UnequipItem(EquippedArmor.Id, _player))
                        {
                            RefreshBackpack();
                            EquippedArmor = null;
                        }
                    }
                    break;
                case "Accessory":
                    if (EquippedAccessory != null)
                    {
                        if (_player.Inventory.UnequipItem(EquippedAccessory.Id, _player))
                        {
                            RefreshBackpack();
                            EquippedAccessory = null;
                        }
                    }
                    break;
            }
        }

        private void OnEquipDropped(EquipDropArgs a)
        {
            if (a is null) return;
            var item = a.Item as ItemVm; // cast payload
            if (item is null || item.Id == "empty_slot") return; // Don't equip empty slots
            if (!IsCompatible(a.Slot, item)) return;

            switch (a.Slot)
            {
                case "Weapon": 
                    if (_player.Inventory.SwapEquipment(item.Id, _player))
                        Swap(ref _ew, item);
                    break;
                case "Armor": 
                    if (_player.Inventory.SwapEquipment(item.Id, _player)) 
                        Swap(ref _ea, item); 
                    break;
                case "Accessory": 
                    if (_player.Inventory.SwapEquipment(item.Id, _player))
                        Swap(ref _ex, item);
                    break;
            }
        }

        private bool IsCompatible(string slot, ItemVm item)
        {
            return slot switch
            {
                "Weapon" => item.Type == "Weapon",
                "Armor" => item.Type == "Armor",
                "Accessory" => item.Type == "Accessory",
                _ => false
            };
        }

        private void Swap(ref ItemVm? equipped, ItemVm incoming)
        {
            // Logic is now handled by RefreshBackpack mostly, but for the equipped property:
            equipped = incoming;
            
            OnPropertyChanged(nameof(EquippedWeapon));
            OnPropertyChanged(nameof(EquippedArmor));
            OnPropertyChanged(nameof(EquippedAccessory));
            
            RefreshBackpack(); // Re-sync backpack
        }
    }
}
