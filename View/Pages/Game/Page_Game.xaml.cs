using MyriaRPG.Services;
using MyriaRPG.ViewModel.Pages.Game;
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

namespace MyriaRPG.View.Pages.Game
{
    /// <summary>
    /// Interaktionslogik für Page_Room.xaml
    /// </summary>
    public partial class Page_Game : Page
    {
        public Page_Game()
        {
            InitializeComponent();
            Navigation.SetNavigationFrame(frm_Navigation, 4);
            this.DataContext = new ViewModel_PageGame();
        }

    }

}
