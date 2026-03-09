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
            if (e.Data.GetData(typeof(InventoryItemViewModel)) is InventoryItemViewModel itemViewModel)
            {
                if (sender is ContentControl control && control.Tag is string slotTypeStr)
                {
                    if (Enum.TryParse<MyriaLib.Systems.Enums.EquipmentType>(slotTypeStr, out var slotType))
                    {
                        _viewModel.HandleEquipmentDrop(itemViewModel, slotType);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
