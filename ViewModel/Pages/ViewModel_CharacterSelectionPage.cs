using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyriaLib.Entities.Players;
using MyriaLib.Systems;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_CharacterSelectionPage
    {
        public string btnJoin { get; set; } = Localization.T("pg.character.select.btn.join");
        public string btnCreate { get; set; } = Localization.T("pg.character.select.btn.create");
        public string btnDelete { get; set; } = Localization.T("pg.character.select.btn.delete");
        public string btnBack { get; set; } = Localization.T("app.general.UI.back");

        public Player Character1 { get; set; }
        public string btnCharacter1 { get; set; }
        public Player Character2 { get; set; }
        public string btnCharacter2 { get; set; }
        public Player Character3 { get; set; }
        public string btnCharacter3 { get; set; }
        public Player Character4 { get; set; }
        public string btnCharacter4 { get; set; }

        public ICommand Join { get; }
        public ICommand Create { get; }
        public ICommand Delete { get; }
        public ICommand Back { get; }

        public Player SelectedPlayer { get; set; }

        public ViewModel_CharacterSelectionPage() 
        {
            Join = new RelayCommand(JoinAction, JoinCanExecute);
            Create = new RelayCommand(CreateAction);
            Delete = new RelayCommand(DeleteAction, DeleteCanExecute);
            Back = new RelayCommand(BackAction);
        }
        private void JoinAction()
        {

        }
        private bool JoinCanExecute()
        {
            return SelectedPlayer != null;
        }
        private void CreateAction()
        {

        }
        private void DeleteAction()
        {
            SelectedPlayer = null;
        }
        private bool DeleteCanExecute()
        {
            return SelectedPlayer != null;
        }
        private void BackAction()
        {
            Navigation.NavigateMain(new Page_StartupMenue());
        }

    }

}
