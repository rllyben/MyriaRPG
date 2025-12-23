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
using MyriaRPG.ViewModel.Pages.Game;

namespace MyriaRPG.View.Pages.Game
{
    /// <summary>
    /// Interaktionslogik für Page_Fight.xaml
    /// </summary>
    public partial class Page_Fight : Page
    {
        public Page_Fight()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_PageFight();
        }
    }
}
