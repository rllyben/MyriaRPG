using MyriaLib.Entities.NPCs;
using MyriaLib.Services;
using MyriaRPG.ViewModel.UserControls.IngameWindow;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game.IngameWindow.NpcInteraction
{
    public partial class UpgradePanel : Page
    {
        public UpgradePanel(Npc npc)
        {
            InitializeComponent();
            DataContext = new UpgradePanelViewModel(npc, UserAccoundService.CurrentCharacter, () => NavigationService.GoBack());
        }
    }
}
