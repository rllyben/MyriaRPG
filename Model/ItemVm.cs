using System.Windows.Media;
using MyriaRPG.ViewModel;
using MyriaLib.Systems.Enums;

namespace MyriaRPG.Model
{
    public class ItemVm : BaseViewModel
    {
        public ItemVm() { }
        public ItemVm(string name, string type, ItemRarity rarity)
        { Name = name; Type = type; Rarity = rarity; IsEquipable = type is "Weapon" or "Armor" or "Accessory"; }
        public ItemVm(string name, string type, ItemRarity rarity, SolidColorBrush color)
        { Name = name; Type = type; Rarity = rarity; IsEquipable = type is "Weapon" or "Armor" or "Accessory"; Color = color; }


        public string Name { get; set; }
        public string Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public SolidColorBrush Color { get; set; }


        public string Icon { get => _icon; set { _icon = value; OnPropertyChanged(); } }
        private string _icon = "📦";
        public int Quantity { get => _qty; set { _qty = value; OnPropertyChanged(); } }
        private int _qty = 1;
        public string Description { get => _desc; set { _desc = value; OnPropertyChanged(); } }
        private string _desc = string.Empty;
        public string StatsLine { get => _stats; set { _stats = value; OnPropertyChanged(); } }
        private string _stats = string.Empty;


        public bool IsUsable { get => _usable; set { _usable = value; OnPropertyChanged(); } }
        private bool _usable;
        public bool IsEquipable { get => _equip; private set { _equip = value; OnPropertyChanged(); } }
        private bool _equip;
        public bool IsEquipped { get => _equipped; set { _equipped = value; OnPropertyChanged(); } }
        private bool _equipped;
    }
}
