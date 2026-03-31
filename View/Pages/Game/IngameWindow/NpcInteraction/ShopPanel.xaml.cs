using MyriaLib.Entities.NPCs;
using MyriaLib.Services;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow.Inventory;
using MyriaRPG.ViewModel.UserControls.IngameWindow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyriaRPG.View.Pages.Game.IngameWindow.NpcInteraction
{
    public partial class ShopPanel : Page
    {
        private InventoryGridViewModel _inventoryViewModel;

        public ShopPanel(Npc npc)
        {
            InitializeComponent();
            var player = UserAccoundService.CurrentCharacter;
            this.DataContext = new ShopPanelViewModel(npc, player, () => NavigationService.GoBack());
            _inventoryViewModel = new InventoryGridViewModel(player);
            InventorySection.DataContext = _inventoryViewModel;
            MoneyBagSection.DataContext = new MoneyBagViewModel(player);
        }

        private void InventorySlot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.DataContext is InventoryItemViewModel itemViewModel)
                _inventoryViewModel.ShowTooltipCommand.Execute(itemViewModel);
        }

        private void InventorySlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _inventoryViewModel.HideTooltipCommand.Execute(null);
        }

        private void InventorySlot_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(InventoryItemViewModel)))
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void InventorySlot_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(InventoryItemViewModel)) is InventoryItemViewModel draggedItem
                && sender is Border targetBorder)
            {
                if (targetBorder.DataContext is InventoryItemViewModel targetItem)
                    _inventoryViewModel.HandleItemDrop(draggedItem, targetItem.Index);
                else
                    _inventoryViewModel.RefreshInventory();

                e.Handled = true;
            }
        }

        private void InventorySlot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is InventoryItemViewModel itemViewModel
                && itemViewModel.Item != null)
            {
                DataObject data = new DataObject(typeof(InventoryItemViewModel), itemViewModel);
                DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }
    }
}
