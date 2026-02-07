using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.View.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class GeneralNpcInteractionViewModel : BaseViewModel
    {
        // Because of Bug to not show ServiceOption Text property without
        public string Text { get; set; }

        private string _npcName;
        private string _npcTypeText;
        private string _dialogText;
        private string _btnClose;
        private BaseViewModel _currentPanel;
        public BaseViewModel CurrentPanel
        {
            get => _currentPanel;
            set { _currentPanel = value; OnPropertyChanged(); }
        }
        public string NpcName 
        {
            get => _npcName;
            set 
            { 
                _npcName = value; 
                OnPropertyChanged();
            } 

        }
        public string NpcTypeText
        {
            get => _npcTypeText;
            set
            {
                _npcTypeText = value;
                OnPropertyChanged();
            }

        }
        public string DialogText
        {
            get => _dialogText;
            set
            {
                _dialogText = value;
                OnPropertyChanged();
            }

        }

        [LocalizedKey("app.general.UI.close")]
        public string BtnClose
        {
            get => _btnClose;
            set
            {
                _btnClose = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<ServiceOption> ServiceOptions { get; set; } = new();

        public ICommand BackToDialogCommand { get; }
        public ICommand CloseCommand { get; }

        public GeneralNpcInteractionViewModel(Npc npc)
        {
            Player player = UserAccoundService.CurrentCharacter;

            var dialogPanel = new DialogPanelViewModel(
                npc,
                player,
                onNavigate: panel => CurrentPanel = panel
            );

            CurrentPanel = dialogPanel;

            NpcName = MyriaLib.Systems.Localization.T(npc.NameKey);
            NpcTypeText = MyriaLib.Systems.Localization.T(npc.Type.ToString());
            DialogText = MyriaLib.Systems.Localization.T("npc." + npc.Id + "dialog");

            foreach (string service in npc.Services)
            {
                var serviceOption = new ServiceOption
                {
                    // Better text than raw "heal"/"buy_items"
                    Text = MyriaLib.Systems.Localization.T($"npc.service.{service}.title"),

                    Command = new RelayCommand(() => ExecuteService(npc, service))
                };

                ServiceOptions.Add(serviceOption);
            }
            CloseCommand = new RelayCommand(() => MainWindow.Instance.gameWindow.Visibility = Visibility.Hidden);
        }

    }
    public class ServiceOption : BaseViewModel
    {
        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public ICommand Command { get; set; }
    }

}
