using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaRPG.View.Windows;
using MyriaRPG.Utils;
using MyriaRPG.Services;
using MyriaRPG.View.Pages.Game.IngameWindow;
using System.Windows;
using System.Windows.Input;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;
using MyriaLib.Entities.Maps;
using MyriaRPG.ViewModel.UserControls;
using MyriaLib.Entities.Skills;
using System.Windows.Controls;
using MyriaRPG.View.Pages.Game;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageGame : BaseViewModel
    {
        // Character (name-only for header)
        public CharacterHeaderVm Char { get; } = new();

        // Commands
        public ICommand MapCommand { get; }
        public ICommand OpenInventoryCommand { get; }
        public ICommand OpenCharacterCommand { get; }
        public ICommand OpenSkillsCommand { get; }
        public ICommand OpenQuestsCommand { get; }


        public ViewModel_PageGame()
        {
            Page_Room roomPage = new();
            Page_Fight combatPage = new();
            Navigation.RegisterGamePage(roomPage, 0);
            Navigation.RegisterGamePage(combatPage, 1);
            MapCommand = new RelayCommand(OpenMap);
            OpenInventoryCommand = new RelayCommand(OpenInventory);
            OpenCharacterCommand = new RelayCommand(OpenCharacter);
            OpenSkillsCommand = new RelayCommand(OpenSkills);
            OpenQuestsCommand = new RelayCommand(OpenQuests);
            if (UserAccoundService.CurrentCharacter.CurrentRoom == null)
                UserAccoundService.CurrentCharacter.CurrentRoom = RoomService.GetRoomById(UserAccoundService.CurrentCharacter.CurrentRoomId);
            Navigation.NavigateGamePageToRegister(0);
        }

        private void OpenMap()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open inventory popup */
            var room = RoomService.GetRoomById(UserAccoundService.CurrentCharacter.CurrentRoom.Id);
            var page = new Page_LocalMap { DataContext = new ViewModel_PageLocalMap(room) };
            ((MainWindow.Instance.gameWindow.DataContext) as ViewModel_GameWindow).Title = ((page.DataContext) as ViewModel_PageLocalMap).WindowTitle;
            Navigation.NavigateIngameWindow(page);
        }
        private void OpenInventory()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open inventory popup */
            Page_Inventory inv = new Page_Inventory();
            ((MainWindow.Instance.gameWindow.DataContext) as ViewModel_GameWindow).Title = ((inv.DataContext) as InventoryPageViewModel).WindowTitle;
            Navigation.NavigateIngameWindow(inv);
        }
        private void OpenCharacter()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open character popup */
            Page_Character character = new Page_Character();
            Navigation.NavigateIngameWindow(character);
        }
        private void OpenSkills()
        {
            var player = UserAccoundService.CurrentCharacter;                 // or wherever your current player lives
            var skills = player.Skills;

            var page = new Page_Skills()
            {
                DataContext = new SkillPageViewModel(player, skills)
            };
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible;/* open skills popup */
            Navigation.NavigateIngameWindow(page);
        }
        private void OpenQuests()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open quests popup */
            Page_QuestList list = new Page_QuestList();
            Navigation.NavigateIngameWindow(list);
        }

    }


    public class CharacterHeaderVm : BaseViewModel
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set 
            { 
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NameAndLevel));
            }
        
        }

        private int _level;
        public int Level
        { 
            get { return _level; }
            set 
            { 
                _level = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NameAndLevel));
            } 

        }

        private long _xp;
        public long CurrentXp
        {
            get { return _xp; }
            set 
            { 
                _xp = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(XpPercent));
            } 

        }

        private long _xpToNext = 1;
        public long XpToNext
        {
            get { return _xpToNext; }
            set 
            { 
                _xpToNext = value <= 0 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(XpPercent));
            }
        
        }

        private int _hp;
        public int Hp
        {
            get { return _hp; }
            set 
            { 
                _hp = value; OnPropertyChanged();
                OnPropertyChanged(nameof(HpDisplay));
            }
        
        }

        private int _hpMax = 1;
        public int MaxHp
        {
            get { return _hpMax; }
            set 
            {
                _hpMax = value <= 0 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HpDisplay));
            }
        
        }

        private int _mp;
        public int Mana
        {
            get { return _mp; }
            set 
            { 
                _mp = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ManaDisplay));
            }
        
        }

        private int _mpMax = 1;
        public int MaxMana
        {
            get { return _mpMax; }
            set 
            { 
                _mpMax = value <= 0 ? 1 : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ManaDisplay));
            }
        
        }

        public string NameAndLevel { get; set; }
        public int XpPercent { get; set; }
        public string HpDisplay { get; set; }
        public string ManaDisplay { get; set; }

        public CharacterHeaderVm()
        {
            Player character = UserAccoundService.CurrentCharacter;
            Set(character.Name, character.Level, character.Experience, character.ExpForNextLvl, character.CurrentHealth, character.MaxHealth, character.CurrentMana, character.MaxMana);

            NameAndLevel = string.IsNullOrWhiteSpace(Name) ? string.Empty : $"{Name} • Lv {Level}";
            XpPercent = (int)Math.Round(100.0 * CurrentXp / Math.Max(1, XpToNext));
            HpDisplay = $"{Hp}/{MaxHp}";
            ManaDisplay = $"{Mana}/{MaxMana}";
        }

        public void Set(string name, int level, long currentXp, long xpToNext, int hp, int maxHp, int mana, int maxMana)
        {
            Name = name; Level = level; CurrentXp = currentXp; XpToNext = xpToNext;
            Hp = hp; MaxHp = maxHp; Mana = mana; MaxMana = maxMana;
        }

    }

}