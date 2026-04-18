using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;

namespace MyriaRPG.View.Pages.Game.IngameWindow
{
    public partial class Page_SkillCombination : Page
    {
        public Page_SkillCombination()
        {
            InitializeComponent();
            DataContext = new SkillCombinationViewModel();
        }
    }
}
