using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaRPG.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyriaRPG.View.Pages.Game.IngameWindow.Inventory
{
    /// <summary>
    /// Interaktionslogik für Equipment.xaml
    /// </summary>
    public partial class Equipment : Page
    {
        private InventoryPageViewModel _viewModel;
        public Equipment()
        {
            InitializeComponent();
            // Load Icons.xaml resource dictionary
            try
            {
                var iconsUri = new Uri("pack://application:,,,/Resources/Icons.xaml", UriKind.Absolute);
                var iconsDictionary = new ResourceDictionary { Source = iconsUri };
                this.Resources.MergedDictionaries.Add(iconsDictionary);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Icons.xaml: {ex.Message}");
                // Continue without icons - they'll fall back to default or show errors
            }
            _viewModel = new InventoryPageViewModel(UserAccoundService.CurrentCharacter);
            this.DataContext = _viewModel;
        }

        /// <summary>
        /// Handle drag over equipment slot
        /// </summary>
        private void EquipmentSlot_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(InventoryItemViewModel)))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle drop on equipment slot
        /// </summary>
        private void EquipmentSlot_Drop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"EquipmentSlot_Drop called, sender: {sender?.GetType().Name}");

            if (e.Data.GetData(typeof(InventoryItemViewModel)) is InventoryItemViewModel itemViewModel)
            {
                System.Diagnostics.Debug.WriteLine($"Got InventoryItemViewModel: {itemViewModel.Item?.Name}");

                // Get the equipment slot from the sender's DataContext
                if (sender is ContentControl control)
                {
                    // The Tag contains the slot type hint, but we need to determine from context
                    // Try to get the actual slot type from the control's name or position
                    string slotName = control.Name;
                    System.Diagnostics.Debug.WriteLine($"Slot name: {slotName}");

                    MyriaLib.Systems.Enums.EquipmentType slotType;

                    if (slotName == "WeaponSlot")
                        slotType = MyriaLib.Systems.Enums.EquipmentType.Weapon;
                    else if (slotName == "ArmorSlot")
                        slotType = MyriaLib.Systems.Enums.EquipmentType.Armor;
                    else if (slotName == "AccessorySlot")
                        slotType = MyriaLib.Systems.Enums.EquipmentType.Accessory;
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Unknown slot: {slotName}");
                        return; // Unknown slot
                    }

                    System.Diagnostics.Debug.WriteLine($"Calling HandleEquipmentDrop with slotType: {slotType}");
                    _viewModel.HandleEquipmentDrop(itemViewModel, slotType);
                    e.Handled = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Sender is not ContentControl, it's {sender?.GetType().Name}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not get InventoryItemViewModel from drop data");
            }
        }
        /// <summary>
        /// Handle mouse enter on equipment slot - show tooltip
        /// </summary>
        private void EquipmentSlot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ContentControl control && control.DataContext is EquipmentSlotViewModel slotViewModel)
            {
                if (slotViewModel.Item != null)
                {
                    // Create a temporary InventoryItemViewModel just to pass the item to the command
                    var tempItemViewModel = new InventoryItemViewModel(slotViewModel.Item, 0);
                    _viewModel.ShowTooltipCommand.Execute(tempItemViewModel);
                }
            }
        }

        /// <summary>
        /// Handle mouse leave on equipment slot - hide tooltip
        /// </summary>
        private void EquipmentSlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.HideTooltipCommand.Execute(null);
        }
        /// <summary>
        /// Initiate drag from equipment slot
        /// </summary>
        private void EquipmentSlot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentControl control && control.DataContext is EquipmentSlotViewModel slotViewModel)
            {
                if (slotViewModel.Item != null)
                {
                    // Create a temporary InventoryItemViewModel to pass the equipped item
                    var tempItemViewModel = new InventoryItemViewModel(slotViewModel.Item, 0);
                    DataObject data = new DataObject(typeof(InventoryItemViewModel), tempItemViewModel);
                    DragDrop.DoDragDrop(control, data, DragDropEffects.Move);
                    e.Handled = true;
                }
            }
        }
    }
}
