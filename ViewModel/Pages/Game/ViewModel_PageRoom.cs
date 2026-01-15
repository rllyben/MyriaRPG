using MyriaLib.Entities.Maps;
using MyriaLib.Services;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private Visibility _hasNpcs;
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
        public Visibility HasNpcs
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

        // Exit flags
        public bool HasNorth { get => _n; set { _n = value; OnPropertyChanged(); } }
        private bool _n;
        public bool HasEast { get => _e; set { _e = value; OnPropertyChanged(); } }
        private bool _e;
        public bool HasSouth { get => _s; set { _s = value; OnPropertyChanged(); } }
        private bool _s;
        public bool HasWest { get => _w; set { _w = value; OnPropertyChanged(); } }
        private bool _w;
        private Visibility btnFightVisibility;
        public Visibility BtnFightVisibility { get => btnFightVisibility; set { btnFightVisibility = value; OnPropertyChanged(); } }
        private bool _hasMonsters;
        public bool HasMonsters
        {
            get => _hasMonsters;
            set 
            { 
                _hasMonsters = value;
                if (value == true)
                    BtnFightVisibility = Visibility.Visible;
                else
                    BtnFightVisibility = Visibility.Hidden;

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
        // Commands
        public ICommand MoveCommand { get; }
        public ICommand StartFightCommand { get; }
        public ICommand OpenNpcsCommand { get; }

        public ViewModel_PageRoom()
        {
            Npcs = new ObservableCollection<string>();
            MoveCommand = new RelayCommand<string>(Move);
            StartFightCommand = new RelayCommand(StartFight);
            currentRoom = RoomService.GetRoomById(UserAccoundService.CurrentCharacter.CurrentRoom.Id);
            RoomName = MyriaLib.Systems.Localization.T(currentRoom.Name);
            RoomDescription = currentRoom.Description;
            ImageSource = $"Data/images/rooms/{currentRoom.Name}.jpg";
            if (currentRoom.Npcs != null && currentRoom.Npcs.Count > 0)
                HasNpcs = Visibility.Visible;
            else
                HasNpcs = Visibility.Hidden;
            foreach(string npc in currentRoom.Npcs)
            {
                Npcs.Add(npc);
            }
            RefreshRoomFlags();
            _instantce = this;
        }
        public static void RefreshLocalisation()
        {
            if (_instantce == null || _instantce.currentRoom == null)
                return;
            _instantce.RoomName = MyriaLib.Systems.Localization.T(_instantce.currentRoom.Name);
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
            ImageSource = $"Data/images/rooms/{currentRoom.Name}.jpg";
            GetDirections();
            RefreshRoomFlags();
        }

    }

}
