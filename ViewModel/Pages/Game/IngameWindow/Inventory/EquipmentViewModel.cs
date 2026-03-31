using System.Windows.Input;
using System.Windows.Media;
using MyriaLib.Entities.Items;
using MyriaLib.Entities.Players;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory
{
    /// <summary>
    /// ViewModel for the equipment slot panel (weapon, armor, accessory).
    /// Handles equip/unequip and drag-drop onto equipment slots.
    /// Subscribes to player.Inventory.ItemReceived so it stays in sync when
    /// InventoryGridViewModel makes changes to the shared player state.
    /// </summary>
    public class EquipmentViewModel : BaseViewModel
    {
        private readonly Player _player;
        private string _equipmentTitle;
        private string _weaponHint;
        private string _armorHint;
        private string _accessoryHint;
        private EquipmentSlotViewModel _weaponSlot;
        private EquipmentSlotViewModel _armorSlot;
        private EquipmentSlotViewModel _accessorySlot;
        private ItemTooltipViewModel _currentTooltip;
        private bool _isTooltipVisible;

        [LocalizedKey("pg.inventory.equipment.title")]
        public string EquipmentTitle { get => _equipmentTitle; set => SetProperty(ref _equipmentTitle, value); }

        [LocalizedKey("pg.inventory.slot.weapon")]
        public string WeaponHint { get => _weaponHint; set => SetProperty(ref _weaponHint, value); }

        [LocalizedKey("pg.inventory.slot.armor")]
        public string ArmorHint { get => _armorHint; set => SetProperty(ref _armorHint, value); }

        [LocalizedKey("pg.inventory.slot.accessory")]
        public string AccessoryHint { get => _accessoryHint; set => SetProperty(ref _accessoryHint, value); }

        public EquipmentSlotViewModel WeaponSlot { get => _weaponSlot; set => SetProperty(ref _weaponSlot, value); }
        public EquipmentSlotViewModel ArmorSlot { get => _armorSlot; set => SetProperty(ref _armorSlot, value); }
        public EquipmentSlotViewModel AccessorySlot { get => _accessorySlot; set => SetProperty(ref _accessorySlot, value); }

        public ItemTooltipViewModel CurrentTooltip { get => _currentTooltip; set => SetProperty(ref _currentTooltip, value); }
        public bool IsTooltipVisible { get => _isTooltipVisible; set => SetProperty(ref _isTooltipVisible, value); }

        public ICommand ShowTooltipCommand { get; }
        public ICommand HideTooltipCommand { get; }

        public EquipmentViewModel(Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _weaponSlot = new EquipmentSlotViewModel(EquipmentType.Weapon);
            _armorSlot = new EquipmentSlotViewModel(EquipmentType.Armor);
            _accessorySlot = new EquipmentSlotViewModel(EquipmentType.Accessory);
            _currentTooltip = new ItemTooltipViewModel();

            ShowTooltipCommand = new RelayCommand<InventoryItemViewModel>(ShowTooltip);
            HideTooltipCommand = new RelayCommand(HideTooltip);

            // Stay in sync with inventory changes triggered by InventoryGridViewModel
            _player.Inventory.ItemReceived += (s, e) => RefreshEquipmentSlots();

            RefreshEquipmentSlots();
        }

        public void RefreshEquipmentSlots()
        {
            WeaponSlot.Item = _player.WeaponSlot;
            ArmorSlot.Item = _player.ArmorSlot;
            AccessorySlot.Item = _player.AccessorySlot;
        }

        public void HandleEquipmentDrop(InventoryItemViewModel draggedItem, EquipmentType slotType)
        {
            if (draggedItem?.Item is not EquipmentItem equipment) return;
            if (!equipment.IsUsableBy(_player))
            {
                System.Windows.MessageBox.Show("This item cannot be equipped by your class!");
                return;
            }
            _player.Inventory.SwapEquipment(equipment.Id, _player);
            RefreshEquipmentSlots();
        }

        private void ShowTooltip(InventoryItemViewModel itemViewModel)
        {
            if (itemViewModel?.Item == null) return;
            CurrentTooltip.SetItem(itemViewModel.Item);
            IsTooltipVisible = true;
        }

        private void HideTooltip() => IsTooltipVisible = false;
    }

    /// <summary>
    /// ViewModel for a single equipment slot.
    /// </summary>
    public class EquipmentSlotViewModel : BaseViewModel
    {
        private EquipmentType _slotType;
        private EquipmentItem _item;
        private Brush _borderColor;

        public EquipmentType SlotType { get => _slotType; set => SetProperty(ref _slotType, value); }
        public EquipmentItem Item { get => _item; set => SetProperty(ref _item, value); }
        public Brush BorderColor { get => _borderColor; set => SetProperty(ref _borderColor, value); }

        public EquipmentSlotViewModel(EquipmentType slotType)
        {
            _slotType = slotType;
            BorderColor = slotType switch
            {
                EquipmentType.Weapon    => new SolidColorBrush(Color.FromRgb(255, 183, 0)),
                EquipmentType.Armor     => new SolidColorBrush(Color.FromRgb(30, 255, 0)),
                EquipmentType.Accessory => new SolidColorBrush(Color.FromRgb(0, 112, 221)),
                _                       => new SolidColorBrush(Colors.Gray)
            };
        }
    }
}
