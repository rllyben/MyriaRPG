using MyriaLib.Entities.NPCs;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ICommand CloseCommand { get; }

        public GeneralNpcInteractionViewModel(Npc npc)
        {
            NpcName = MyriaLib.Systems.Localization.T(npc.NameKey);
            NpcTypeText = MyriaLib.Systems.Localization.T(npc.Type.ToString());
            DialogText = MyriaLib.Systems.Localization.T("npc." + npc.Id + "dialog");

            foreach(string service in npc.Services)
            {
                ServiceOption serviceOption = new ServiceOption() { Text = service };
                ServiceOptions.Add(serviceOption);

                switch (service)
                {
                    case "heal": serviceOption.Command = new RelayCommand(npc.HealingAction); break;
                }

            }
            CloseCommand = new RelayCommand(() => MainWindow.Instance.gameWindow.Visibility = Visibility.Hidden);
        }
        

    }
    public class ServiceOption : BaseViewModel
    {
        public string Text { get; set; }
        public ICommand Command { get; set; }
    }

}
