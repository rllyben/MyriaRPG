using MyriaLib.Entities.Maps;
using MyriaLib.Services.Builder;
using MyriaLib.Services.Regestries;
using MyriaLib.Systems;
using MyriaRPG.Systems.MapNode;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class ViewModel_PageLocalMap : BaseViewModel
    {
        // Layout constants
        private const double STEP_X  = 160;
        private const double STEP_Y  = 100;
        private const double NODE_W  = 120;
        private const double NODE_H  = 40;
        private const double PADDING = 30;

        public string MapTitle { get; private set; } = "";

        // Used to size the canvas so the ScrollViewer knows how far to scroll
        public double CanvasWidth  { get; private set; }
        public double CanvasHeight { get; private set; }

        public IReadOnlyList<MapNodeVm> Nodes { get; private set; } = [];
        public IReadOnlyList<MapEdgeVm> Edges { get; private set; } = [];

        public ViewModel_PageLocalMap(Room currentRoom)
        {
            Build(currentRoom);
        }

        private void Build(Room currentRoom)
        {
            // Determine map title based on what region the player is in
            var city    = CityRegistry.GetCityByRoom(currentRoom);
            var dungeon = DungeonRegistry.GetDungeonByRoom(currentRoom);
            var cave    = CaveRegistry.GetCaveByRoom(currentRoom);
            var forest  = ForestRegistry.GetForestByRoom(currentRoom);

            MapTitle = city    != null ? Localization.T(city.Name)
                     : dungeon != null ? Localization.T(dungeon.Name)
                     : cave    != null ? Localization.T(cave.Name)
                     : forest  != null ? Localization.T(forest.Name)
                     : Localization.T("game.map.world.title");

            // BFS layout from MapBuilder — assigns (gridX, gridY) to every visible room
            var positions = MapBuilder.BuildRoomMap(currentRoom);

            if (positions.Count == 0) return;

            // Shift so min coords start at 0
            int minX = positions.Values.Min(p => p.x);
            int minY = positions.Values.Min(p => p.y);
            int maxX = positions.Values.Max(p => p.x) - minX;
            int maxY = positions.Values.Max(p => p.y) - minY;

            CanvasWidth  = (maxX + 1) * STEP_X + PADDING * 2;
            CanvasHeight = (maxY + 1) * STEP_Y + PADDING * 2;

            // Build node VMs
            var nodeMap = new Dictionary<int, MapNodeVm>(); // roomId -> node
            foreach (var (room, (gx, gy)) in positions)
            {
                double cx = (gx - minX) * STEP_X + STEP_X / 2 + PADDING;
                double cy = (gy - minY) * STEP_Y + STEP_Y / 2 + PADDING;

                var kind = room.IsBossRoom                          ? NodeKind.Boss
                         : room.IsDungeonRoom                       ? NodeKind.Dungeon
                         : room.IsCaveRoom                          ? NodeKind.Cave
                         : room.IsCity                              ? NodeKind.City
                         : forest != null                           ? NodeKind.Forest
                         : NodeKind.World;

                var node = new MapNodeVm
                {
                    RoomId   = room.Id,
                    Label    = Localization.T(room.Name),
                    X        = cx - NODE_W / 2,
                    Y        = cy - NODE_H / 2,
                    CenterX  = cx,
                    CenterY  = cy,
                    Width    = NODE_W,
                    Height   = NODE_H,
                    IsCurrent = room.Id == currentRoom.Id,
                    Kind     = kind
                };
                nodeMap[room.Id] = node;
            }

            // Build edge VMs (deduplicated — one line per room pair)
            var edges = new List<MapEdgeVm>();
            var seen  = new HashSet<(int, int)>();
            foreach (var (room, _) in positions)
            {
                if (!nodeMap.TryGetValue(room.Id, out var fromNode)) continue;

                foreach (var (_, targetId) in room.ExitIds)
                {
                    if (!nodeMap.TryGetValue(targetId, out var toNode)) continue;

                    var key = (Math.Min(room.Id, targetId), Math.Max(room.Id, targetId));
                    if (!seen.Add(key)) continue;

                    edges.Add(new MapEdgeVm
                    {
                        X1 = fromNode.CenterX, Y1 = fromNode.CenterY,
                        X2 = toNode.CenterX,   Y2 = toNode.CenterY
                    });
                }
            }

            Nodes = nodeMap.Values.ToList();
            Edges = edges;
        }
    }
}
