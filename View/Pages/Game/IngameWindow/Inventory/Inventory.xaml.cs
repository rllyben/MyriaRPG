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
    /// Interaktionslogik für Inventory.xaml
    /// </summary>
    public partial class Inventory : Page
    {
        private InventoryPageViewModel _viewModel;
        public Inventory()
        {
            InitializeComponent();
            _viewModel = new InventoryPageViewModel(UserAccoundService.CurrentCharacter);
            this.DataContext = _viewModel;
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
    }
}
