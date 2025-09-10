using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_CharacterSelectionPage : BaseViewModel
    {
        private string _btnJoin;
        private string _btnCreate;
        private string _btnDelete;
        private string _btnBack;
        private Visibility _visibility1 = Visibility.Hidden;
        private Visibility _visibility2 = Visibility.Hidden;
        private Visibility _visibility3 = Visibility.Hidden;
        private Visibility _visibility4 = Visibility.Hidden;
        private Visibility _visibility5 = Visibility.Hidden;

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
        public Visibility Visibility1 
        {
            get { return _visibility1; }
            set
            {
                _visibility1 = value; 
                OnPropertyChanged(nameof(Visibility1));
            }

        }
        public Visibility Visibility2 
        { 
            get { return _visibility2; }
            set
            {
                _visibility2 = value;
                OnPropertyChanged(nameof(Visibility2));
            }

        }
        public Visibility Visibility3 
        { 
            get { return _visibility3; }
            set
            {
                _visibility3 = value;
                OnPropertyChanged(nameof(Visibility3));
            }

        }
        public Visibility Visibility4 
        { 
            get { return _visibility4; }
            set
            {
                _visibility4 = value;
                OnPropertyChanged(nameof(Visibility4));
            }
            
        }
        public Visibility Visibility5 
        { 
            get { return _visibility5; }
            set
            {
                _visibility5 = value;
                OnPropertyChanged(nameof(Visibility5));
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
        public Player Character5 { get; set; }
        public string btnCharacter5 { get; set; }

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

            List<Player> list = CharacterService.LoadCharacters(UserAccoundService.CurrentUser);
            for (int count = 0; count < list.Count; count++)
            {
                switch (count)
                {
                    case 0:
                        Character1 = list[count];
                        btnCharacter1 = Character1.Name;
                        Visibility1 = Visibility.Visible; break;
                    case 1: 
                        Character2 = list[count];
                        btnCharacter2 = Character2.Name;
                        Visibility2 = Visibility.Visible; break;
                    case 2:
                        Character3 = list[count];
                        btnCharacter3 = Character3.Name;
                        Visibility3 = Visibility.Visible; break;
                    case 3:
                        Character4 = list[count];
                        btnCharacter4 = Character4.Name;
                        Visibility4 = Visibility.Visible; break;
                    case 4:
                        Character5 = list[count];
                        btnCharacter5 = Character5.Name;
                        Visibility5 = Visibility.Visible; break;
                }

            }
            
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
