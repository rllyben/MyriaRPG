using MyriaLib.Entities.NPCs;
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
        private object _currentPanel;
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

        public object CurrentPanel
        {
            get => _currentPanel;
            set { _currentPanel = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ServiceOption> ServiceOptions { get; set; } = new();

        public ICommand BackToDialogCommand { get; }
        public ICommand CloseCommand { get; }

        public GeneralNpcInteractionViewModel(Npc npc)
        {
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
        private void ExecuteService(Npc npc, string serviceId)
        {
            var player = UserAccoundService.CurrentCharacter; // or however you access the active player

            // For now: services needing item/amount can open a popup first (shop UI)
            // Later you'll pass selected item + amount into Execute(...).
            var result = NpcInteractionService.Execute(player, npc, serviceId);

            // Use result to update the dialog line (and/or log)
            DialogText = MyriaLib.Systems.Localization.T(result.MessageKey, result.Args);

            // Optional: if result indicates opening a shop, you open your in-game window page here
            // if (result.MessageKey == "npc.action.open.shop_equipment") { ... }
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
