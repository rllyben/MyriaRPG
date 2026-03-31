using MyriaLib.Services;
using MyriaLib.Systems.Enums;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyriaRPG.View.Pages.Game.IngameWindow.Inventory
{
    public partial class Equipment : Page
    {
        private EquipmentViewModel _viewModel;

        public Equipment()
        {
            InitializeComponent();
            _viewModel = new EquipmentViewModel(UserAccoundService.CurrentCharacter);
            this.DataContext = _viewModel;
        }

        private void EquipmentSlot_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(InventoryItemViewModel)))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void EquipmentSlot_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(InventoryItemViewModel)) is not InventoryItemViewModel itemViewModel) return;
            if (sender is not ContentControl control) return;

            EquipmentType slotType = control.Name switch
            {
                "WeaponSlot"    => EquipmentType.Weapon,
                "ArmorSlot"     => EquipmentType.Armor,
                "AccessorySlot" => EquipmentType.Accessory,
                _               => (EquipmentType)(-1)
            };

            if ((int)slotType == -1) return;

            _viewModel.HandleEquipmentDrop(itemViewModel, slotType);
            e.Handled = true;
        }

        private void EquipmentSlot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ContentControl control && control.DataContext is EquipmentSlotViewModel slotViewModel
                && slotViewModel.Item != null)
            {
                var tempItem = new InventoryItemViewModel(slotViewModel.Item, 0);
                _viewModel.ShowTooltipCommand.Execute(tempItem);
            }
        }

        private void EquipmentSlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.HideTooltipCommand.Execute(null);
        }

        private void EquipmentSlot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentControl control && control.DataContext is EquipmentSlotViewModel slotViewModel
                && slotViewModel.Item != null)
            {
                var tempItem = new InventoryItemViewModel(slotViewModel.Item, 0);
                DataObject data = new DataObject(typeof(InventoryItemViewModel), tempItem);
                DragDrop.DoDragDrop(control, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }
    }
}
