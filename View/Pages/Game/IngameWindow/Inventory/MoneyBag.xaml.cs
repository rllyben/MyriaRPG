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
    /// Interaktionslogik für MoneyBag.xaml
    /// </summary>
    public partial class MoneyBag : Page
    {
        private InventoryPageViewModel _viewModel;
        public MoneyBag()
        {
            InitializeComponent();
            _viewModel = new InventoryPageViewModel(UserAccoundService.CurrentCharacter);
            this.DataContext = _viewModel;
        }
    }
}
