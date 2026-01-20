using MyriaLib.Entities.Maps;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Events;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageRoom : BaseViewModel
    {
        private static ViewModel_PageRoom _instantce;
        private string btn_North;
        private string btn_South;
        private string btn_West;
        private string btn_East; 
        private System.Windows.Visibility _hasNpcs;
        [LocalizedKey("game.exit.north")]
        public string BtnNorth
        {
            get { return btn_North; }
            set
            {
                btn_North = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("game.exit.south")]
        public string BtnSouth
        {
            get { return btn_South; }
            set
            {
                btn_South = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("game.exit.west")]
        public string BtnWest
        {
            get { return btn_West; }
            set
            {
                btn_West = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("game.exit.east")]
        public string BtnEast
        {
            get { return btn_East; }
            set
            {
                btn_East = value;
                OnPropertyChanged();
            }

        }
        public System.Windows.Visibility HasNpcs
        {
            get => _hasNpcs;
            set { _hasNpcs = value; OnPropertyChanged(); }
        }
        private Room currentRoom;
        private string _roomName;
        public string RoomName 
        { 
            get { return _roomName; }
            private set
            {
                _roomName = value;
                OnPropertyChanged();
            }
        }
        private string _roomDescription = "Sunlit plaza with cobblestones and a gentle fountain.";
        public string RoomDescription 
        { 
            get { return _roomDescription; }
            private set
            {
                _roomDescription = value;
                OnPropertyChanged();
            }

        }

        [LocalizedKey("app.general.UI.fight")]
        public string BtnFight { get; set; }
        [LocalizedKey("app.general.UI.npcs")]
        public string BtnNpcs { get; set; }
        [LocalizedKey("pg.room.UI.talk")]
        public string BtnTalk { get; set; }

        // Exit flags
        public bool HasNorth { get => _n; set { _n = value; OnPropertyChanged(); } }
        private bool _n;
        public bool HasEast { get => _e; set { _e = value; OnPropertyChanged(); } }
        private bool _e;
        public bool HasSouth { get => _s; set { _s = value; OnPropertyChanged(); } }
        private bool _s;
        public bool HasWest { get => _w; set { _w = value; OnPropertyChanged(); } }
        private bool _w;
        public ObservableCollection<string> Log { get; set; }
        private System.Windows.Visibility btnFightVisibility;
        public System.Windows.Visibility BtnFightVisibility { get => btnFightVisibility; set { btnFightVisibility = value; OnPropertyChanged(); } }
        private bool _hasMonsters;
        public bool HasMonsters
        {
            get => _hasMonsters;
            set 
            { 
                _hasMonsters = value;
                if (value == true)
                    BtnFightVisibility = System.Windows.Visibility.Visible;
                else
                    BtnFightVisibility = System.Windows.Visibility.Hidden;

                OnPropertyChanged(); 
            }

        }
        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<string> Npcs { get; set; }
        public string SelectedNpc { get; set; }
        // Commands
        public ICommand MoveCommand { get; }
        public ICommand StartFightCommand { get; }
        public ICommand OpenNpcsCommand { get; }
        public ICommand TalkCommand { get; }
        public ViewModel_PageRoom()
        {
            Player player = UserAccoundService.CurrentCharacter;
            Npcs = new ObservableCollection<string>();
            MoveCommand = new RelayCommand<string>(Move);
            StartFightCommand = new RelayCommand(StartFight);
            TalkCommand = new RelayCommand(TalkNpc);
            currentRoom = RoomService.GetRoomById(player.CurrentRoom.Id);
            RoomName = Localization.T(currentRoom.Name);
            RoomDescription = currentRoom.Description;
            ImageSource = "/Data/images/rooms/" + currentRoom.Name + ".jpg";
            Log = new ObservableCollection<string>();
            if (currentRoom.Npcs != null && currentRoom.Npcs.Count > 0)
                HasNpcs = System.Windows.Visibility.Visible;
            else
                HasNpcs = System.Windows.Visibility.Hidden;
            foreach(string npc in currentRoom.Npcs)
            {
                Npcs.Add(npc);
            }
            RefreshRoomFlags();
            _instantce = this;

            player.SkillLearned += OnPlayerSkillLearned;
            player.Inventory.ItemReceived += OnItemReceived;
            player.XpGained += OnXpGained;
            player.LeveledUp += OnLeveledUp;
        }
        private void OnXpGained(object? sender, XpGainedEventArgs e)
        {
            WriteLog(Localization.T("log.xp.gained", e.Amount));
        }
        private void OnLeveledUp(object? sender, LevelUpEventArgs e)
        {
            WriteLog(Localization.T("log.level.up", e.NewLevel));
        }
        private void OnItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            WriteLog(Localization.T("log.item.received", e.Amount, e.Item.Name));
        }
        private void OnPlayerSkillLearned(object? sender, SkillLearnedEventArgs e)
        {
            WriteLog(Localization.T("log.skill.learned", e.Skill.Name));
        }
        public static void WriteLog(string msg)
        {
            _instantce.Log.Add(msg);
            if (_instantce.Log.Count > 5)
                _instantce.Log.Remove(_instantce.Log[0]);
        }
        private void TalkNpc()
        {
            if (SelectedNpc == "Healer")
            {
                UserAccoundService.CurrentCharacter.Heal(UserAccoundService.CurrentCharacter.MaxHealth);
                UserAccoundService.CurrentCharacter.RestoreMana(UserAccoundService.CurrentCharacter.MaxMana);
                WriteLog(Localization.T("msg.healer.fullheal"));
            }
        }
        public static void RefreshLocalisation()
        {
            if (_instantce == null || _instantce.currentRoom == null)
                return;
            _instantce.RoomName = Localization.T(_instantce.currentRoom.Name);
        }
        private void RefreshRoomFlags()
        {
            GetDirections();
            HasMonsters = currentRoom.HasMonsters && !currentRoom.IsCleared
                         && (currentRoom.Monsters.Count > 0 || currentRoom.CurrentMonsters.Count > 0);
        }
        private void GetDirections()
        {
            HasNorth = false; HasEast = false; HasSouth = false; HasWest = false;
            foreach (string direction in currentRoom.ExitIds.Keys)
            {
                switch (direction)
                {
                    case "north": HasNorth = true; break;
                    case "east": HasEast = true; break;
                    case "south": HasSouth = true; break;
                    case "west": HasWest = true; break;
                }

            }

        }
        private void StartFight()
        {
            Navigation.NavigateGamePage(new Page_Fight());
        }
        private void Move(string? dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return;
            // use shared game lib to TryMove and update:
            RoomName = $"Moved {dir}";
            RoomDescription = $"You travel {dir}.";
            // Then set exit flags for new room accordingly

            currentRoom = RoomService.GetRoomById(currentRoom.Exits[dir.ToLower()].Id);
            UserAccoundService.CurrentCharacter.CurrentRoom = currentRoom;
            RoomName = MyriaLib.Systems.Localization.T(currentRoom.Name);
            RoomDescription = currentRoom.Description;
            ImageSource = "/Data/images/rooms/" + currentRoom.Name + ".jpg";
            if (currentRoom.Npcs != null && currentRoom.Npcs.Count > 0)
            {
                HasNpcs = System.Windows.Visibility.Visible;
                Npcs.Clear();
                foreach (string npc in currentRoom.Npcs)
                {
                    Npcs.Add(npc);
                }
            }
            else
                HasNpcs = System.Windows.Visibility.Hidden;
            GetDirections();
            RefreshRoomFlags();
        }

    }

}
