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
using MyriaRPG.Services;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages
{
    /// <summary>
    /// Interaktionslogik für Page_StartupMenue.xaml
    /// </summary>
    public partial class Page_StartupMenue : Page
    {
        public Page_StartupMenue()
        {
            InitializeComponent();
            Navigation.SetNavigationFrame(Frame, 1);
            this.DataContext = new ViewModel_StartupMenuePage();
        }

    }

}
