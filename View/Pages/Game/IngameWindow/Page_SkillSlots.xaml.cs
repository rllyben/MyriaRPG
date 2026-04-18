using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;

namespace MyriaRPG.View.Pages.Game.IngameWindow
{
    public partial class Page_SkillSlots : Page
    {
        public Page_SkillSlots()
        {
            InitializeComponent();
            DataContext = new SkillSlotViewModel();
        }
    }
}
