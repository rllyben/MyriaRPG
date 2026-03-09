using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyriaLib.Entities.Items;
using MyriaLib.Entities.Players;
using MyriaRPG.ViewModels;

namespace MyriaRPG.Pages
{
    /// <summary>
    /// Interaction logic for InventoryPage.xaml
    /// Uses MVVM pattern with InventoryPageViewModel
    /// </summary>
    public partial class InventoryPage : Page
    {
        private InventoryPageViewModel _viewModel;

        public InventoryPage(Player player)
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
            
            // Create and set the ViewModel
            _viewModel = new InventoryPageViewModel(player);
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
        /// Handle mouse enter on inventory slot - show tooltip
        /// </summary>
        private void InventorySlot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.DataContext is InventoryItemViewModel itemViewModel)
            {
                _viewModel.ShowTooltipCommand.Execute(itemViewModel);
            }
        }

        /// <summary>
        /// Handle mouse leave on inventory slot - hide tooltip
        /// </summary>
        private void InventorySlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.HideTooltipCommand.Execute(null);
        }

        /// <summary>
        /// Handle drag over inventory slot
        /// </summary>
        private void InventorySlot_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(InventoryItemViewModel)))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle drop on inventory slot
        /// </summary>
        private void InventorySlot_Drop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"InventorySlot_Drop called, sender: {sender?.GetType().Name}");
            
            if (e.Data.GetData(typeof(InventoryItemViewModel)) is InventoryItemViewModel draggedItem)
            {
                System.Diagnostics.Debug.WriteLine($"Got dragged item: {draggedItem.Item?.Name}");
                
                if (sender is Border targetBorder)
                {
                    // Try to get the target item from DataContext
                    if (targetBorder.DataContext is InventoryItemViewModel targetItem)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dropping on item: {targetItem.Item?.Name}, Index: {targetItem.Index}");
                        _viewModel.HandleItemDrop(draggedItem, targetItem.Index);
                        e.Handled = true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Target DataContext is empty slot, refreshing inventory");
                        // If no target item, it might be an empty slot - still allow the drop
                        // Just refresh inventory to update positions
                        _viewModel.RefreshInventory();
                        e.Handled = true;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Sender is not Border, it's {sender?.GetType().Name}");
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
        /// Initiate drag from inventory slot
        /// </summary>
        private void InventorySlot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is InventoryItemViewModel itemViewModel)
            {
                if (itemViewModel.Item != null)
                {
                    DataObject data = new DataObject(typeof(InventoryItemViewModel), itemViewModel);
                    DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                    e.Handled = true;
                }
            }
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
