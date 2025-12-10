using MyriaLib.Entities.Maps;
using MyriaRPG.Systems.MapNode.MapEdge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class ViewModel_PageLocalMap : BaseViewModel
    {
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }
        private string _windowTitle = "Map";

        public LocalMapSnapshot Map
        {
            get => _map;
            set { _map = value; OnPropertyChanged(); WindowTitle = _map.RoomName; }
        }
        private LocalMapSnapshot _map;

        public string? CurrentPoiId { get => _poi; set { _poi = value; OnPropertyChanged(); } }
        private string? _poi;

        public ICommand ClickPoiCommand { get; }
        public ICommand ClickExitCommand { get; }

        public ViewModel_PageLocalMap(Room currentRoom)
        {
            // Build from your real room data
            Map = BuildFromRoom(currentRoom);
        }

        private LocalMapSnapshot BuildFromRoom(Room r)
        {
            // Example: place smith at 25%/40%, healer at 65%/35%, exits at edges.
            var pois = new List<LocalPoi>();

            var exits = new List<LocalExit>();
            foreach (var kv in r.ExitIds) // Dictionary<string dir, string targetRoomId>
            {
                var (x, y, label) = kv.Key switch
                {
                    "north" => (0.50, 0.05, "North Exit"),
                    "south" => (0.50, 0.95, "South Exit"),
                    "west" => (0.05, 0.50, "West Exit"),
                    "east" => (0.95, 0.50, "East Exit"),
                    _ => (0.90, 0.90, kv.Key)
                };
                exits.Add(new LocalExit(kv.Value.ToString(), label, x, y));
            }

            return new LocalMapSnapshot(r.Id.ToString(), r.Name, pois, exits);
        }

    }

}