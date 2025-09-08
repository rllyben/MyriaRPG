using MyriaLib.Entities.Players;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_CharacterSelectionPage : BaseViewModel
    {
        private string _btnJoin;
        private string _btnCreate;
        private string _btnDelete;
        private string _btnBack;

        [LocalizedKey("pg.character.select.btn.join")]
        public string btnJoin 
        {
            get { return _btnJoin; }
            set
            {
                _btnJoin = value;
                OnPropertyChanged(nameof(btnJoin));
            }

        }

        [LocalizedKey("pg.character.select.btn.create")]
        public string btnCreate 
        {
            get { return _btnCreate; }
            set
            {
                _btnCreate = value;
                OnPropertyChanged(nameof(btnCreate));
            }
            
        }

        [LocalizedKey("pg.character.select.btn.delete")]
        public string btnDelete 
        {
            get { return _btnDelete; }
            set
            {
                _btnDelete = value;
                OnPropertyChanged(nameof(btnDelete));
            }

        }

            [LocalizedKey("app.general.UI.back")]
        public string btnBack 
        { 
            get { return _btnBack; }
            set
            {
                _btnBack = value;
                OnPropertyChanged(nameof(btnBack));
            }

        }

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
            LocalizationAutoWire.Wire(this);
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
