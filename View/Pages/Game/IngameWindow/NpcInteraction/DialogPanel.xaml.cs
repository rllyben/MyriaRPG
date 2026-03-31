using MyriaLib.Entities.NPCs;
using MyriaLib.Services;
using MyriaRPG.ViewModel.UserControls.IngameWindow;
using System.Windows.Controls;

namespace MyriaRPG.View.Pages.Game.IngameWindow.NpcInteraction
{
    public partial class DialogPanel : Page
    {
        private readonly Npc _npc;

        public DialogPanel(Npc npc)
        {
            _npc = npc;
            InitializeComponent();
            DataContext = new DialogPanelViewModel(npc, UserAccoundService.CurrentCharacter, NavigateToService);
        }

        private void NavigateToService(string serviceId)
        {
            Page page = serviceId switch
            {
                "shop" or "buy_items" or "shop_equipment" => new ShopPanel(_npc),
                "upgrade"                                 => new UpgradePanel(_npc),
                "craft"                                   => new CraftPanel(_npc),
                _                                         => null
            };

            if (page != null)
                NavigationService.Navigate(page);
        }
    }
}
