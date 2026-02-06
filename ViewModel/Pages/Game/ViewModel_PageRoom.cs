using MyriaLib.Entities.Maps;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Events;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game;
using MyriaRPG.View.Windows;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow;
using MyriaRPG.ViewModel.UserControls;
using MyriaRPG.View.Pages.Game.IngameWindow.NpcInteraction;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using MyriaLib.Services.Builder;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageRoom : BaseViewModel
    {
        private static ViewModel_PageRoom _instantce;

        private Player player;
        private Room currentRoom;

        private string btn_North;
        private bool _hasNorth;
        private bool _hasSouth;
        private bool _hasWest;
        private bool _hasEast;
        private string btn_South;
        private string btn_West;
        private string btn_East;

        private string _roomName;
        private string _roomDescription;
        private string _imageSource;

        private System.Windows.Visibility btnFightVisibility;
        private System.Windows.Visibility btnGatherVisibility;
        private System.Windows.Visibility _hasNpcsVisibility;

        private bool _hasMonsters;
        private bool _canGather;
        private bool _hasNpcs;
        private Npc _selectedNpc;

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
        [LocalizedKey("app.general.UI.fight")]
        public string BtnFight { get; set; }
        [LocalizedKey("pg.room.UI.gather")]
        public string BtnGather { get; set; }
        [LocalizedKey("app.general.UI.npcs")]
        public string BtnNpcs { get; set; }
        [LocalizedKey("pg.room.UI.talk")]
        public string BtnTalk { get; set; }

        public string RoomName 
        { 
            get { return _roomName; }
            private set
            {
                _roomName = value;
                OnPropertyChanged();
            }
        }
        public string RoomDescription 
        { 
            get { return _roomDescription; }
            private set
            {
                _roomDescription = value;
                OnPropertyChanged();
            }

        }
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }

        }


        // Exit flags
        public bool HasNorth { get => _hasNorth; set { _hasNorth = value; OnPropertyChanged(); } }
        public bool HasEast { get => _hasEast; set { _hasEast = value; OnPropertyChanged(); } }
        public bool HasSouth { get => _hasSouth; set { _hasSouth = value; OnPropertyChanged(); } }
        public bool HasWest { get => _hasWest; set { _hasWest = value; OnPropertyChanged(); } }

        public System.Windows.Visibility BtnFightVisibility { get => btnFightVisibility; set { btnFightVisibility = value; OnPropertyChanged(); } }
        public System.Windows.Visibility BtnGatherVisibility { get => btnGatherVisibility; set { btnGatherVisibility = value; OnPropertyChanged(); } }
        public System.Windows.Visibility HasNpcsVisibility { get => _hasNpcsVisibility; set { _hasNpcsVisibility = value; OnPropertyChanged(); } }
        public bool HasMonsters {
            get => _hasMonsters;
            set 
            { 
                _hasMonsters = value;
                if (value)
                    BtnFightVisibility = System.Windows.Visibility.Visible;
                else
                    BtnFightVisibility = System.Windows.Visibility.Hidden;

                OnPropertyChanged(); 
            }

        }
        public bool CanGather
        {
            get => _canGather;
            set 
            {
                _canGather = value;
                if (value)
                    BtnGatherVisibility = System.Windows.Visibility.Visible;
                else
                    BtnGatherVisibility = System.Windows.Visibility.Hidden;

                OnPropertyChanged();
            }

        }
        public bool HasNpcs
        {
            get => _hasNpcs;
            set
            {
                _hasNpcs = value;
                if (value)
                    HasNpcsVisibility = System.Windows.Visibility.Visible;
                else
                    HasNpcsVisibility = System.Windows.Visibility.Hidden;

                OnPropertyChanged();
            }

        }

        public Npc SelectedNpc { get => _selectedNpc; set { _selectedNpc = value; } }

        public ObservableCollection<string> Log { get; set; }
        public ObservableCollection<Npc> Npcs { get; set; }

        // Commands
        public ICommand MoveCommand { get; }
        public ICommand StartFightCommand { get; }
        public ICommand OpenNpcsCommand { get; }
        public ICommand TalkCommand { get; }
        public ICommand StartGatheringCommand { get; }

        public ViewModel_PageRoom()
        {
            player = UserAccoundService.CurrentCharacter;

            Npcs = new ObservableCollection<Npc>();
            Log = new ObservableCollection<string>();

            MoveCommand = new RelayCommand<string>(Move);
            StartFightCommand = new RelayCommand(StartFight);
            TalkCommand = new RelayCommand(TalkNpc);
            StartGatheringCommand = new RelayCommand(StartGathering);

            currentRoom = RoomService.GetRoomById(player.CurrentRoom.Id);
            RoomName = MyriaLib.Systems.Localization.T(currentRoom.Name);
            RoomDescription = currentRoom.Description;
            ImageSource = "/Data/images/rooms/" + currentRoom.Name + ".jpg";

            HasNpcs = currentRoom.Npcs != null && currentRoom.Npcs.Count > 0;
            Npcs.Clear();
            foreach (Npc npc in currentRoom.NpcRefs)
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
            WriteLog(Localization.T("log.item.received", e.Amount, Localization.T(e.Item.Name)));
        }
        private void OnPlayerSkillLearned(object? sender, SkillLearnedEventArgs e)
        {
            WriteLog(Localization.T("log.skill.learned", Localization.T(e.Skill.Name)));
        }
        public static void WriteLog(string msg)
        {
            _instantce.Log.Add(msg);
            if (_instantce.Log.Count > 5)
                _instantce.Log.Remove(_instantce.Log[0]);
        }
        private void TalkNpc()
        {
            if (SelectedNpc == null) return;
            MainWindow.Instance.gameWindow.Visibility = System.Windows.Visibility.Visible;
            Page page = new Page_GeneralNpcInteraction(SelectedNpc);
            Navigation.NavigateIngameWindow(page);
        }
        public static void RefreshLocalisation()
        {
            if (_instantce == null || _instantce.currentRoom == null)
                return;
            _instantce.RoomName = Localization.T(_instantce.currentRoom.Name);
        }
        public void StartGathering()
        {
            player.Inventory.AddItem(ItemFactory.CreateItem("iron_ore"), player);
        }
        private void RefreshRoomFlags()
        {
            GetDirections();
            HasMonsters = currentRoom.HasMonsters && !currentRoom.IsCleared
                         && (currentRoom.Monsters.Count > 0 || currentRoom.CurrentMonsters.Count > 0);
            CanGather = currentRoom.GathersRemaining > 0 && currentRoom.GatheringSpots != null && currentRoom.GatheringSpots.Count > 0;
            HasNpcs = currentRoom.Npcs != null && currentRoom.Npcs.Count > 0;
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
            if (!RoomService.CanEnterRoom(currentRoom.Exits[dir.ToLower()], player))
            {

                return;
            }
            currentRoom = currentRoom.Exits[dir.ToLower()];
            player.CurrentRoom = currentRoom;
            RoomName = MyriaLib.Systems.Localization.T(currentRoom.Name);
            RoomDescription = currentRoom.Description;
            ImageSource = "/Data/images/rooms/" + currentRoom.Name + ".jpg";
            if (currentRoom.Npcs != null && currentRoom.Npcs.Count > 0)
            {
                HasNpcs = currentRoom.Npcs.Count > 0;
                Npcs.Clear();
                foreach (Npc npc in currentRoom.NpcRefs)
                {
                    Npcs.Add(npc);
                }
            }
            GetDirections();
            RefreshRoomFlags();
        }

    }

}
