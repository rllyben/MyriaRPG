using MyriaLib.Entities.Items;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
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
        // Commands
        public ICommand BeginDragItemCommand { get; }
        public ICommand EquipDropCommand { get; }

        public InventoryPageViewModel()
        {
            
            BeginDragItemCommand = new RelayCommand<ItemVm>(_ => { /* optional: highlight valid slots */ });
            EquipDropCommand = new RelayCommand<EquipDropArgs>(OnEquipDropped);


            Player character = UserAccoundService.CurrentCharacter;
            foreach (Item item in character.Inventory.Items)
            {
                ItemVm temp = new ItemVm();
                temp.Name = item.Name;
                string itemType = "";
                if (item is EquipmentItem eq)
                    itemType = $"{eq.SlotType}";
                else if (item is ConsumableItem con)
                    itemType = $"Consumable";
                else if (item is MaterialItem mat)
                    itemType = $"Material";
                temp.Type = itemType;
                temp.Rarity = item.Rarity;
                temp.Color = rarityColors[item.Rarity];

                Backpack.Add(temp);
            }
            if (character.WeaponSlot != null)
            {
                Item weapon = character.WeaponSlot;
                ItemVm temp = new ItemVm();
                temp.Name = weapon.Name;
                temp.Type = (weapon as EquipmentItem).SlotType.ToString();
                temp.Rarity = weapon.Rarity;
                temp.Color = rarityColors[weapon.Rarity];
                Swap(ref _ew, temp);
            }
            if (character.ArmorSlot != null)
            {
                Item armor = character.ArmorSlot;
                ItemVm temp = new ItemVm();
                temp.Name = armor.Name;
                temp.Type = (armor as EquipmentItem).SlotType.ToString();
                temp.Rarity = armor.Rarity;
                temp.Color = rarityColors[armor.Rarity];
                Swap(ref _ea, temp);
            }
            if (character.AccessorySlot != null)
            {
                Item accessory = character.AccessorySlot;
                ItemVm temp = new ItemVm();
                temp.Name = accessory.Name;
                temp.Type = (accessory as EquipmentItem).SlotType.ToString();
                temp.Rarity = accessory.Rarity;
                temp.Color = rarityColors[accessory.Rarity];
                Swap(ref _ex, temp);
            }
            Bronze = character.Money.Coins.TotalBronze;
        }

        private void OnEquipDropped(EquipDropArgs a)
        {
            if (a is null) return;
            var item = a.Item as ItemVm; // cast payload
            if (item is null) return;
            if (!IsCompatible(a.Slot, item)) return;

            switch (a.Slot)
            {
                case "Weapon": Swap(ref _ew, item); break;
                case "Armor": Swap(ref _ea, item); break;
                case "Accessory": Swap(ref _ex, item); break;
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
            // remove incoming from bag
            Backpack.Remove(incoming);
            // put previously equipped back to bag
            if (equipped != null) Backpack.Add(equipped);
            // equip new
            equipped = incoming;
            OnPropertyChanged(nameof(EquippedWeapon));
            OnPropertyChanged(nameof(EquippedArmor));
            OnPropertyChanged(nameof(EquippedAccessory));
        }

    }

}
