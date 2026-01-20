using MyriaLib.Entities.Maps;
using MyriaLib.Entities.Players;
using MyriaLib.Entities.Skills;
using MyriaLib.Services;
using MyriaLib.Systems.Events;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game;
using MyriaRPG.View.Pages.Game.IngameWindow;
using MyriaRPG.View.Windows;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;
using MyriaRPG.ViewModel.UserControls;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageGame : BaseViewModel
    {
        private string btn_Inventory;
        private string btn_Character;
        private string btn_Skills;
        private string btn_Quests;
        private string btn_Map;
        private string btn_Settings;
        [LocalizedKey("app.general.UI.settings")]
        public string BtnSettings
        {
            get { return btn_Settings; }
            set 
            { 
                btn_Settings = value; 
                OnPropertyChanged();
            }
        }
        [LocalizedKey("pg.inventory.title")]
        public string BtnInventory
        {
            get { return btn_Inventory; }
            set
            {
                btn_Inventory = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.title")]
        public string BtnCharacter
        {
            get { return btn_Character; }
            set
            {
                btn_Character = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.skills.title")]
        public string BtnSkills
        {
            get { return btn_Skills; }
            set
            {
                btn_Skills = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.title")]
        public string BtnQuests
        {
            get { return btn_Quests; }
            set
            {
                btn_Quests = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("game.map.title")]
        public string BtnMap
        {
            get { return btn_Map; }
            set
            {
                btn_Map = value;
                OnPropertyChanged();
            }

        }
        // Character (name-only for header)
        public CharacterHeaderVm Char { get; } = new();

        // Commands
        public ICommand MapCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand OpenInventoryCommand { get; }
        public ICommand OpenCharacterCommand { get; }
        public ICommand OpenSkillsCommand { get; }
        public ICommand OpenQuestsCommand { get; }


        public ViewModel_PageGame()
        {
            Page_Room roomPage = new();
            Navigation.RegisterGamePage(roomPage, 0);
            MapCommand = new RelayCommand(OpenMap);
            SettingsCommand = new RelayCommand(OpenSettings);
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
            ((MainWindow.Instance.gameWindow.DataContext) as ViewModel_GameWindow).Title = ((character.DataContext) as CharacterPageViewModel).WindowTitle;
            Navigation.NavigateIngameWindow(character);
        }
        private void OpenSkills()
        {
            var player = UserAccoundService.CurrentCharacter;                 // or wherever your current player lives
            var skills = player.Skills;

            var page = new Page_Skills()
            {
                DataContext = new SkillPageViewModel()
            };
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible;/* open skills popup */
            ((MainWindow.Instance.gameWindow.DataContext) as ViewModel_GameWindow).Title = ((page.DataContext) as SkillPageViewModel).WindowTitle;
            Navigation.NavigateIngameWindow(page);
        }
        private void OpenQuests()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open quests popup */
            Page_QuestList page = new Page_QuestList();
            ((MainWindow.Instance.gameWindow.DataContext) as ViewModel_GameWindow).Title = ((page.DataContext) as QuestListPageViewModel).WindowTitle;
            Navigation.NavigateIngameWindow(page);
        }
        private void OpenSettings()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open quests popup */
            Page_Settings page = new Page_Settings();
            ((MainWindow.Instance.gameWindow.DataContext) as ViewModel_GameWindow).Title = ((page.DataContext) as IngameWindow.ViewModel_SettingsPage).WindowTitle;
            Navigation.NavigateIngameWindow(page);
        }

    }


    public class CharacterHeaderVm : BaseViewModel
    {
        private static CharacterHeaderVm instance;
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set 
            { 
                _name = UserAccoundService.CurrentCharacter.Name;
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
                _level = UserAccoundService.CurrentCharacter.Level;
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
                _xp = UserAccoundService.CurrentCharacter.Experience;
                OnPropertyChanged();
                XpPercent++;
            } 

        }

        private long _xpToNext = 1;
        public long XpToNext
        {
            get { return _xpToNext; }
            set 
            { 
                _xpToNext = UserAccoundService.CurrentCharacter.ExpForNextLvl;
                OnPropertyChanged();
                XpPercent++;
            }
        
        }

        private int _hp;
        public int Hp
        {
            get { return _hp; }
            set 
            { 
                _hp = UserAccoundService.CurrentCharacter.CurrentHealth; OnPropertyChanged();
                HpDisplay = "";
            }
        
        }

        private int _hpMax = 1;
        public int MaxHp
        {
            get { return _hpMax; }
            set 
            {
                _hpMax = UserAccoundService.CurrentCharacter.MaxHealth;
                OnPropertyChanged();
                HpDisplay = "";
            }
        
        }

        private int _mp;
        public int Mana
        {
            get { return _mp; }
            set 
            { 
                _mp = UserAccoundService.CurrentCharacter.CurrentMana;
                OnPropertyChanged();
                ManaDisplay = "";
            }
        
        }

        private int _mpMax = 1;
        public int MaxMana
        {
            get { return _mpMax; }
            set 
            { 
                _mpMax = UserAccoundService.CurrentCharacter.MaxMana;
                OnPropertyChanged();
                ManaDisplay = "";
            }
        
        }
        public string NameAndLevel { get; set; }
        private int xpPercent;
        public int XpPercent 
        {
            get => xpPercent;
            set
            {
                xpPercent = (int)Math.Round(100.0 * CurrentXp / Math.Max(1, XpToNext));
                OnPropertyChanged();
            }

        }
        private string hpDisplay;
        public string HpDisplay 
        {
            get => hpDisplay;
            set
            {
                hpDisplay = $"{Hp}/{MaxHp}";
                OnPropertyChanged();
            }

        }
        private string manaDisplay;
        public string ManaDisplay 
        {
            get => manaDisplay;
            set
            {
                manaDisplay = $"{Mana}/{MaxMana}";
                OnPropertyChanged();
            }

        }

        public CharacterHeaderVm()
        {
            Player character = UserAccoundService.CurrentCharacter;
            Set(character.Name, character.Level, character.Experience, character.ExpForNextLvl, character.CurrentHealth, character.MaxHealth, character.CurrentMana, character.MaxMana);

            NameAndLevel = string.IsNullOrWhiteSpace(Name) ? string.Empty : $"{Name} • Lv {Level}";
            XpPercent = (int)Math.Round(100.0 * CurrentXp / Math.Max(1, XpToNext));
            HpDisplay = $"{Hp}/{MaxHp}";
            ManaDisplay = $"{Mana}/{MaxMana}";
            instance = this;
            character.XpGained += OnXpUpdateEvent;
            character.LeveledUp += OnXpUpdateEvent;
            character.HealthChanged += (s, e) => { Refresh(); };
            character.ManaChanged += (s, e) => { Refresh(); };
        }
        private void OnXpUpdateEvent(object? sender, EventArgs e)
        {
            Refresh();
        }
        private void Refresh()
        {
            instance.Hp++;
            instance.MaxHp++;
            instance.Mana++;
            instance.MaxMana++;
            instance.CurrentXp++;
            instance.XpToNext++;
            instance.XpPercent++;
            instance.Level++;
        }
        public void Set(string name, int level, long currentXp, long xpToNext, int hp, int maxHp, int mana, int maxMana)
        {
            Name = name; Level = level; CurrentXp = currentXp; XpToNext = xpToNext;
            Hp = hp; MaxHp = maxHp; Mana = mana; MaxMana = maxMana;
        }

    }

}