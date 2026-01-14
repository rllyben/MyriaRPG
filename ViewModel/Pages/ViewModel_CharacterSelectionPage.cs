using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Services.Builder;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;
using MyriaRPG.View.Pages.Game;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages
{
    public class ViewModel_CharacterSelectionPage : BaseViewModel
    {
        private List<Player> characters = new List<Player>();
        private Player _selectedPlayer;
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

        [LocalizedKey("app.general.UI.create")]
        public string btnCreate 
        {
            get { return _btnCreate; }
            set
            {
                _btnCreate = value;
                OnPropertyChanged(nameof(btnCreate));
            }
            
        }

        [LocalizedKey("app.general.UI.delete")]
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
        public string btnCharacter1 { get; set; }
        public string btnCharacter2 { get; set; }
        public string btnCharacter3 { get; set; }
        public string btnCharacter4 { get; set; }
        public string btnCharacter5 { get; set; }

        public RelayCommand Join { get; }
        public ICommand Create { get; }
        public RelayCommand Delete { get; }
        public ICommand Back { get; }
        public ICommand SelectFirst { get; }
        public ICommand SelectSecond { get; }
        public ICommand SelectThird { get; }
        public ICommand SelectFourth { get; }
        public ICommand SelectFifth { get; }

        public Player SelectedPlayer 
        { 
            get { return _selectedPlayer; } 
            set
            {
                _selectedPlayer = value;
                OnPropertyChanged(nameof(SelectedPlayer));
                Join.RaiseCanExecuteChanged();
                Delete.RaiseCanExecuteChanged();
                UserAccoundService.CurrentCharacter = value;
            }

        }

        public ViewModel_CharacterSelectionPage() 
        {
            Join = new RelayCommand(JoinAction, IsSelected);
            Create = new RelayCommand(CreateAction);
            Delete = new RelayCommand(DeleteAction, IsSelected);
            Back = new RelayCommand(BackAction);
            SelectFirst = new RelayCommand(SelectFirstAction);
            SelectSecond = new RelayCommand(SelectSecondAction);
            SelectThird = new RelayCommand(SelectThirdAction);
            SelectFourth = new RelayCommand(SelectFourthAction);
            SelectFifth = new RelayCommand(SelectFifthAction);

            LocalizationAutoWire.Wire(this);

            characters = CharacterService.LoadCharacters(UserAccoundService.CurrentUser);
            for (int count = 0; count < characters.Count; count++)
            {
                switch (count)
                {
                    case 0:
                        btnCharacter1 = characters[count].Name;
                        Visibility1 = Visibility.Visible; break;
                    case 1: 
                        btnCharacter2 = characters[count].Name;
                        Visibility2 = Visibility.Visible; break;
                    case 2:
                        btnCharacter3 = characters[count].Name;
                        Visibility3 = Visibility.Visible; break;
                    case 3:
                        btnCharacter4 = characters[count].Name;
                        Visibility4 = Visibility.Visible; break;
                    case 4:
                        btnCharacter5 = characters[count].Name;
                        Visibility5 = Visibility.Visible; break;
                }

            }
            
        }
        private void SelectFirstAction()
        {
            SelectedPlayer = characters[0];
        }
        private void SelectSecondAction()
        {
            SelectedPlayer = characters[1];
        }
        private void SelectThirdAction()
        {
            SelectedPlayer = characters[2];
        }
        private void SelectFourthAction()
        {
            SelectedPlayer = characters[3];
        }
        private void SelectFifthAction()
        {
            SelectedPlayer = characters[4];
        }
        private void JoinAction()
        {
            var skills = SkillFactory.GetSkillsFor(UserAccoundService.CurrentCharacter);
            SkillFactory.UpdateSkills(UserAccoundService.CurrentCharacter);
            Navigation.NavigateMain(new Page_Game());
        }
        private bool IsSelected()
        {
            return SelectedPlayer != null;
        }
        private void CreateAction()
        {
            Navigation.NavigateMain(new Page_CharacterCreation());
        }
        private void DeleteAction()
        {
            SelectedPlayer = null;
            UserAccoundService.CurrentCharacter = null;
        }
        private void BackAction()
        {
            Navigation.NavigateMain(new Page_StartupMenue());
        }

    }

}
