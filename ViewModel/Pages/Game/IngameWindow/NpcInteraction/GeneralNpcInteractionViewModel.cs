using MyriaLib.Entities.NPCs;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.View.Windows;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class GeneralNpcInteractionViewModel : BaseViewModel
    {
        private string _npcName;
        private string _btnClose;

        public string NpcName
        {
            get => _npcName;
            set { _npcName = value; OnPropertyChanged(); }
        }

        [LocalizedKey("app.general.UI.close")]
        public string BtnClose
        {
            get => _btnClose;
            set { _btnClose = value; OnPropertyChanged(); }
        }

        public ICommand CloseCommand { get; }

        public GeneralNpcInteractionViewModel(Npc npc)
        {
            NpcName = MyriaLib.Systems.Localization.T(npc.NameKey);
            CloseCommand = new RelayCommand(() => MainWindow.Instance.gameWindow.Visibility = Visibility.Hidden);
        }
    }

    public class ServiceOption : BaseViewModel
    {
        public string Text { get; set; }
        public string Description { get; set; }
        public ICommand Command { get; set; }
    }
}
