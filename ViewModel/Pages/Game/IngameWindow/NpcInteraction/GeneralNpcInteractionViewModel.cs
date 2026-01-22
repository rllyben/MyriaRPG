using MyriaLib.Entities.NPCs;
using MyriaLib.Systems;
using MyriaRPG.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class GeneralNpcInteractionViewModel : BaseViewModel
    {
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
        public ObservableCollection<ServiceOption> ServiceOptions = new();

        public GeneralNpcInteractionViewModel(Npc npc)
        {
            NpcName = Localization.T(npc.NameKey);
            NpcTypeText = Localization.T(npc.Type.ToString());
            DialogText = Localization.T("npc." + npc.Id + "dialog");

            foreach(string service in npc.Services)
            {
                ServiceOption serviceOption = new ServiceOption();
                serviceOption.Text = service;
                ServiceOptions.Add(serviceOption);
            }

        }

    }
    public class ServiceOption
    {
        public string Text { get; set; }
    }

}
