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

        public InventoryPage(Player player)
        {
            InitializeComponent();
        }
    }
}
