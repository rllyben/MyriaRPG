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
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages.Settings
{
    /// <summary>
    /// Interaktionslogik für Page_SettingsVisuals.xaml
    /// </summary>
    public partial class Page_SettingsVisuals : Page
    {
        public Page_SettingsVisuals()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_SettingsVisuals();
        }

    }

}
