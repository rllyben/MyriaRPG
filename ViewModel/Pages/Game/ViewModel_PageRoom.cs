using MyriaLib.Entities.Maps;
using MyriaLib.Services;
using MyriaRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageRoom : BaseViewModel
    {
        private Room currentRoom;
        private string _roomName = "Lumina's Rise";
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

        // Exit flags
        public bool HasNorth { get => _n; set { _n = value; OnPropertyChanged(); } }
        private bool _n;
        public bool HasEast { get => _e; set { _e = value; OnPropertyChanged(); } }
        private bool _e;
        public bool HasSouth { get => _s; set { _s = value; OnPropertyChanged(); } }
        private bool _s;
        public bool HasWest { get => _w; set { _w = value; OnPropertyChanged(); } }
        private bool _w;


        // Commands
        public ICommand MoveCommand { get; }

        public ViewModel_PageRoom()
        {
            MoveCommand = new RelayCommand<string>(Move);
            currentRoom = RoomService.GetRoomById(UserAccoundService.CurrentCharacter.CurrentRoom.Id);
            RoomName = currentRoom.Name;
            RoomDescription = currentRoom.Description;
            GetDirections();
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
        private void Move(string? dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return;
            // use shared game lib to TryMove and update:
            RoomName = $"Moved {dir}";
            RoomDescription = $"You travel {dir}.";
            // Then set exit flags for new room accordingly

            currentRoom = RoomService.GetRoomById(currentRoom.Exits[dir.ToLower()].Id);
            UserAccoundService.CurrentCharacter.CurrentRoom = currentRoom;
            RoomName = currentRoom.Name;
            RoomDescription = currentRoom.Description;
            GetDirections();
        }

    }

}
