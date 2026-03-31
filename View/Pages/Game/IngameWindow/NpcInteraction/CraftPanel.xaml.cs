using MyriaLib.Entities.NPCs;
using MyriaLib.Services;
using MyriaRPG.ViewModel.UserControls.IngameWindow;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game.IngameWindow.NpcInteraction
{
    public partial class CraftPanel : Page
    {
        public CraftPanel(Npc npc)
        {
            InitializeComponent();
            DataContext = new CraftPanelViewModel(npc, UserAccoundService.CurrentCharacter, () => NavigationService.GoBack());
        }
    }
}
