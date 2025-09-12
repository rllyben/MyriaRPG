using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaRPG.View.Windows;
using MyriaRPG.Utils;
using MyriaRPG.View.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageRoom : BaseViewModel
    {
        public string RoomName { get; private set; } = "Lumina's Rise";
        public string RoomDescription { get; private set; } = "Sunlit plaza with cobblestones and a gentle fountain.";


        // Character (name-only for header)
        public CharacterHeaderVm Char { get; } = new();


        // Exit flags
        public bool HasNorth { get => _n; set { _n = value; OnPropertyChanged(); } }
        private bool _n;
        public bool HasSouth { get => _s; set { _s = value; OnPropertyChanged(); } }
        private bool _s;
        public bool HasWest { get => _w; set { _w = value; OnPropertyChanged(); } }
        private bool _w;
        public bool HasEast { get => _e; set { _e = value; OnPropertyChanged(); } }
        private bool _e;


        // Commands
        public ICommand MoveCommand { get; }
        public ICommand MapCommand { get; }
        public ICommand OpenInventoryCommand { get; }
        public ICommand OpenCharacterCommand { get; }
        public ICommand OpenSkillsCommand { get; }
        public ICommand OpenQuestsCommand { get; }


        public ViewModel_PageRoom()
        {
            MoveCommand = new RelayCommand<string>(Move);
            MapCommand = new RelayCommand(OpenMap);
            OpenInventoryCommand = new RelayCommand(OpenInventory);
            OpenCharacterCommand = new RelayCommand(OpenCharacter);
            OpenSkillsCommand = new RelayCommand(OpenSkills);
            OpenQuestsCommand = new RelayCommand(OpenQuests);

            HasNorth = true; HasEast = true; HasWest = false; HasSouth = false;
        }


        private void Move(string? dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return;
            // use shared game lib to TryMove and update:
            RoomName = $"Moved {dir}"; OnPropertyChanged(nameof(RoomName));
            RoomDescription = $"You travel {dir}."; OnPropertyChanged(nameof(RoomDescription));
            // Then set exit flags for new room accordingly
        }


        private void OpenMap()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open text-map overlay */
        }
        private void OpenInventory()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open inventory popup */
        }
        private void OpenCharacter()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open character popup */
        }
        private void OpenSkills()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible;/* open skills popup */
        }
        private void OpenQuests()
        {
            MainWindow.Instance.gameWindow.Visibility = Visibility.Visible; /* open quests popup */
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
