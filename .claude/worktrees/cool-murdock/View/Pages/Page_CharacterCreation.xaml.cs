using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages
{
    /// <summary>
    /// Interaktionslogik für Page_CharacterCreation.xaml
    /// </summary>
    public partial class Page_CharacterCreation : Page
    {
        public Page_CharacterCreation()
        {
            InitializeComponent();
            this.DataContext = new ViewmModel_CaracterCreationPage();
        }
    }
}
