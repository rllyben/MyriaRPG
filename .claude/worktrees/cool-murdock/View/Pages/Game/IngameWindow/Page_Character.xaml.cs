using MyriaLib.Services;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game.IngameWindow
{
    /// <summary>
    /// Interaktionslogik für Page_Character.xaml
    /// </summary>
    public partial class Page_Character : Page
    {
        public Page_Character()
        {
            InitializeComponent();
            this.DataContext = new CharacterPageViewModel(UserAccoundService.CurrentCharacter);
        }
    }
}
