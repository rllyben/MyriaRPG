using System.Windows.Controls;
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
