using MyriaLib.Entities.Maps;
using MyriaLib.Services;
using MyriaLib.Services.Builder;
using MyriaLib.Services.Regestries;
using MyriaLib.Systems;
using MyriaRPG.Systems.MapNode;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class ViewModel_PageLocalMap : BaseViewModel
    {
        // Layout constants
        private const double STEP_X   = 160;
        private const double STEP_Y   = 100;
        private const double NODE_W   = 120;
        private const double NODE_H   = 40;
        private const double GROUP_W  = 150;
        private const double GROUP_H  = 50;
        private const double PADDING  = 30;

        // A zone with ≥ this many rooms is shown collapsed on the world map
        private const int GROUP_THRESHOLD = 4;

        public string MapTitle { get; private set; } = "";
        public double CanvasWidth  { get; private set; }
        public double CanvasHeight { get; private set; }

        public IReadOnlyList<MapNodeVm> Nodes { get; private set; } = [];
        public IReadOnlyList<MapEdgeVm> Edges { get; private set; } = [];

        public ViewModel_PageLocalMap(Room currentRoom)
        {
            Build(currentRoom);
        }

        // ── Entry point ──────────────────────────────────────────────────────────

        private void Build(Room currentRoom)
        {
            var bigZones = GetBigZones();
            var currentZone = bigZones.FirstOrDefault(z => z.RoomIds.Contains(currentRoom.Id));

            if (currentZone != null)
                BuildGroupView(currentRoom, currentZone, bigZones);
            else
                BuildWorldView(currentRoom, bigZones);
        }

        // ── Zone list ────────────────────────────────────────────────────────────

        private record ZoneInfo(string Id, string DisplayName, List<int> RoomIds, NodeKind Kind,
                                int AnchorRoomId = 0, bool IsDungeon = false);

        private static IReadOnlyList<ZoneInfo> GetBigZones()
        {
            var zones = new List<ZoneInfo>();

            foreach (var c in CityRegistry.GetAll().Where(c => c.RoomIds.Count >= GROUP_THRESHOLD))
                zones.Add(new ZoneInfo(c.Id, Localization.T(c.Name), c.RoomIds, NodeKind.City, c.AnchorRoomId));

            foreach (var c in CaveRegistry.GetAll().Where(c => c.RoomIds.Count >= GROUP_THRESHOLD))
                zones.Add(new ZoneInfo(c.Id, Localization.T(c.Name), c.RoomIds, NodeKind.Cave, c.AnchorRoomId));

            foreach (var f in ForestRegistry.GetAll().Where(f => f.RoomIds.Count >= GROUP_THRESHOLD))
                zones.Add(new ZoneInfo(f.Id, Localization.T(f.Name), f.RoomIds, NodeKind.Forest, f.AnchorRoomId));

            foreach (var d in DungeonRegistry.GetAll())
                zones.Add(new ZoneInfo(d.Id, Localization.T(d.Name), d.RoomIds, NodeKind.Dungeon,
                                       d.AnchorRoomId, IsDungeon: true));

            return zones;
        }

        // ── Group view — show only rooms inside the zone the player is currently in ──

        private void BuildGroupView(Room currentRoom, ZoneInfo zone, IReadOnlyList<ZoneInfo> allZones)
        {
            MapTitle = zone.DisplayName;

            var zoneRoomIds = new HashSet<int>(zone.RoomIds);
            var positions = BuildZoneBfs(currentRoom, zoneRoomIds);
            if (positions.Count == 0) return;

            int minX = positions.Values.Min(p => p.x);
            int minY = positions.Values.Min(p => p.y);
            int maxX = positions.Values.Max(p => p.x);
            int maxY = positions.Values.Max(p => p.y);

            // Build regular zone nodes
            var nodeMap = new Dictionary<int, MapNodeVm>();
            foreach (var (room, (gx, gy)) in positions)
            {
                double cx = (gx - minX) * STEP_X + STEP_X / 2 + PADDING;
                double cy = (gy - minY) * STEP_Y + STEP_Y / 2 + PADDING;

                var kind = room.IsBossRoom ? NodeKind.Boss : zone.Kind;

                nodeMap[room.Id] = new MapNodeVm
                {
                    RoomId    = room.Id,
                    Label     = Localization.T(room.Name),
                    X         = cx - NODE_W / 2,
                    Y         = cy - NODE_H / 2,
                    CenterX   = cx,
                    CenterY   = cy,
                    Width     = NODE_W,
                    Height    = NODE_H,
                    IsCurrent = room.Id == currentRoom.Id,
                    Kind      = kind
                };
            }

            // Build edges between zone rooms
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

            // Add adjacent zone group nodes (e.g., dungeon group node visible from cave view)
            if (!zone.IsDungeon)
            {
                var adjacentZoneSeen = new HashSet<string>();
                foreach (var (room, _) in positions)
                {
                    if (!nodeMap.TryGetValue(room.Id, out var fromNode)) continue;
                    foreach (var (dir, targetId) in room.ExitIds)
                    {
                        if (zoneRoomIds.Contains(targetId)) continue;
                        var adjZone = allZones.FirstOrDefault(z => z.RoomIds.Contains(targetId));
                        if (adjZone == null || !adjacentZoneSeen.Add(adjZone.Id)) continue;

                        int dx = dir.ToLower() switch { "east" => 1, "west" => -1, _ => 0 };
                        int dy = dir.ToLower() switch { "south" => 1, "north" => -1, _ => 0 };

                        double cx = fromNode.CenterX + dx * STEP_X;
                        double cy = fromNode.CenterY + dy * STEP_Y;

                        var groupNode = new MapNodeVm
                        {
                            RoomId      = -1,
                            Label       = adjZone.DisplayName,
                            X           = cx - GROUP_W / 2,
                            Y           = cy - GROUP_H / 2,
                            CenterX     = cx,
                            CenterY     = cy,
                            Width       = GROUP_W,
                            Height      = GROUP_H,
                            IsCurrent   = false,
                            Kind        = adjZone.Kind,
                            IsGroupNode = true
                        };

                        // Use a synthetic int key to store in nodeMap (negative to avoid collision)
                        int syntheticKey = -(adjZone.GetHashCode() & 0x7FFFFFFF) - 1;
                        nodeMap[syntheticKey] = groupNode;

                        edges.Add(new MapEdgeVm
                        {
                            X1 = fromNode.CenterX, Y1 = fromNode.CenterY,
                            X2 = cx,               Y2 = cy
                        });
                    }
                }
            }

            // Canvas bounds from all nodes (including group nodes that may extend beyond grid)
            double maxRight  = nodeMap.Values.Max(n => n.X + n.Width);
            double maxBottom = nodeMap.Values.Max(n => n.Y + n.Height);
            CanvasWidth  = maxRight  + PADDING;
            CanvasHeight = maxBottom + PADDING;

            Nodes = nodeMap.Values.ToList();
            Edges = edges;
        }

        // ── World view — show everything; collapse big zones to single nodes ──────

        private void BuildWorldView(Room currentRoom, IReadOnlyList<ZoneInfo> bigZones)
        {
            MapTitle = Localization.T("game.map.world.title");

            var positions = MapBuilder.BuildRoomMap(currentRoom);
            if (positions.Count == 0) return;

            // Build room-to-zone lookup (non-dungeon zones; dungeon rooms are excluded by MapBuilder)
            var nonDungeonZones = bigZones.Where(z => !z.IsDungeon).ToList();
            var roomToZone = new Dictionary<int, ZoneInfo>();
            foreach (var zone in nonDungeonZones)
                foreach (var id in zone.RoomIds)
                    roomToZone[id] = zone;

            // Zone rooms are traversed early in the MapBuilder BFS, causing rooms adjacent to
            // zone interiors (e.g. Room 20 / Room 19 reached via WW zone) to be placed at wrong
            // positions before the "correct" non-zone chain (Ceralith chain) can claim them.
            // Fix: run a second BFS that never enters zone rooms so every non-zone room is
            // placed purely by its non-zone neighbours, then override MapBuilder positions.
            var rPos = new Dictionary<int, (int x, int y)>();
            {
                var rOccupied = new HashSet<(int, int)> { (0, 0) };
                var rQueue    = new Queue<(Room room, int x, int y)>();
                var rVisited  = new HashSet<int>();

                rPos[currentRoom.Id] = (0, 0);
                rQueue.Enqueue((currentRoom, 0, 0));

                while (rQueue.Count > 0)
                {
                    var (room, x, y) = rQueue.Dequeue();
                    if (!rVisited.Add(room.Id)) continue;

                    foreach (var (dir, targetId) in room.ExitIds)
                    {
                        if (roomToZone.ContainsKey(targetId)) continue; // never enter zone rooms

                        var target = RoomService.GetRoomById(targetId);
                        if (target == null || target.IsDungeonRoom || rPos.ContainsKey(targetId)) continue;

                        int dx = dir.ToLower() switch { "east" => 1, "west" => -1, _ => 0 };
                        int dy = dir.ToLower() switch { "south" => 1, "north" => -1, _ => 0 };

                        int extend = 1;
                        (int cx, int cy) cand = (x + dx, y + dy);
                        while (rOccupied.Contains(cand)) { extend++; cand = (x + dx * extend, y + dy * extend); }

                        rOccupied.Add(cand);
                        rPos[targetId] = cand;
                        rQueue.Enqueue((target, cand.cx, cand.cy));
                    }
                }

                // Apply corrected positions: match by room.Id so reference inequality is not an issue
                foreach (var (room, _) in positions.ToList())
                {
                    if (rPos.TryGetValue(room.Id, out var corrected))
                        positions[room] = corrected;
                }
            }

            // Anchor each zone.
            // For zones with AnchorRoomId: compute from the gateway non-zone room's
            // (now-corrected) position + direction to anchor room, so the zone node
            // always appears directly adjacent to the correct non-zone neighbour.
            // Fallback / zones without AnchorRoomId: BFS-first.
            var zoneGridPos = new Dictionary<string, (double gx, double gy)>();

            foreach (var zone in nonDungeonZones.Where(z => z.AnchorRoomId != 0))
            {
                bool found = false;
                foreach (var (roomId, pos) in rPos)
                {
                    if (found) break;
                    if (zone.RoomIds.Contains(roomId)) continue;
                    var room = RoomService.GetRoomById(roomId);
                    if (room == null) continue;
                    foreach (var (dir, targetId) in room.ExitIds)
                    {
                        if (targetId != zone.AnchorRoomId) continue;
                        int dx = dir.ToLower() switch { "east" => 1, "west" => -1, _ => 0 };
                        int dy = dir.ToLower() switch { "south" => 1, "north" => -1, _ => 0 };
                        zoneGridPos[zone.Id] = (pos.x + dx, pos.y + dy);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // Anchor room not reachable from a non-zone gateway — use anchor room's BFS pos
                    foreach (var (room, pos) in positions)
                    {
                        if (room.Id == zone.AnchorRoomId) { zoneGridPos[zone.Id] = (pos.x, pos.y); break; }
                    }
                }
            }

            // BFS-first for zones without AnchorRoomId (or as final fallback)
            foreach (var (room, pos) in positions)
            {
                if (!roomToZone.TryGetValue(room.Id, out var zone)) continue;
                if (!zoneGridPos.ContainsKey(zone.Id))
                    zoneGridPos[zone.Id] = (pos.x, pos.y);
            }

            // Cascade-correct correctable bridge chains between two different big zones
            var cascadedPaths = new HashSet<(string, int)>();
            foreach (var zone in nonDungeonZones)
            {
                if (!zoneGridPos.TryGetValue(zone.Id, out _)) continue;
                var zoneRoomSet = new HashSet<int>(zone.RoomIds);

                foreach (var (room, _) in positions)
                {
                    if (!zoneRoomSet.Contains(room.Id)) continue;
                    foreach (var (dir, targetId) in room.ExitIds)
                    {
                        if (zoneRoomSet.Contains(targetId)) continue;       // within zone
                        if (roomToZone.ContainsKey(targetId)) continue;     // another zone room
                        if (!cascadedPaths.Add((zone.Id, targetId))) continue;

                        if (IsBridge(targetId, room.Id, zone, roomToZone))
                            CascadeCorrect(zone, dir, targetId, room.Id, positions, zoneGridPos, roomToZone);
                    }
                }
            }

            // Position dungeon group nodes relative to their connecting non-dungeon room
            var dungeonZones = bigZones.Where(z => z.IsDungeon).ToList();
            foreach (var dungeon in dungeonZones)
            {
                var dungeonRoomSet = new HashSet<int>(dungeon.RoomIds);
                bool found = false;
                foreach (var (room, pos) in positions)
                {
                    if (found) break;
                    foreach (var (dir, targetId) in room.ExitIds)
                    {
                        if (!dungeonRoomSet.Contains(targetId)) continue;

                        // Effective position: zone anchor if room is in a zone, else own position
                        double effectiveGx, effectiveGy;
                        if (roomToZone.TryGetValue(room.Id, out var connectingZone) &&
                            zoneGridPos.TryGetValue(connectingZone.Id, out var zPos))
                            (effectiveGx, effectiveGy) = zPos;
                        else
                            (effectiveGx, effectiveGy) = (pos.x, pos.y);

                        int dx = dir.ToLower() switch { "east" => 1, "west" => -1, _ => 0 };
                        int dy = dir.ToLower() switch { "south" => 1, "north" => -1, _ => 0 };

                        zoneGridPos[dungeon.Id] = (effectiveGx + dx, effectiveGy + dy);
                        found = true;
                        break;
                    }
                }
            }

            // Gather all effective grid coords (non-zone rooms + zone centroids)
            var allGx = new List<double>();
            var allGy = new List<double>();

            foreach (var (room, (gx, gy)) in positions)
            {
                if (roomToZone.ContainsKey(room.Id)) continue;
                allGx.Add(gx); allGy.Add(gy);
            }
            foreach (var (_, (gx, gy)) in zoneGridPos)
            {
                allGx.Add(gx); allGy.Add(gy);
            }

            if (allGx.Count == 0) return;

            double minGx = allGx.Min(), maxGx = allGx.Max();
            double minGy = allGy.Min(), maxGy = allGy.Max();

            CanvasWidth  = (maxGx - minGx + 1) * STEP_X + PADDING * 2;
            CanvasHeight = (maxGy - minGy + 1) * STEP_Y + PADDING * 2;

            // String-keyed node map: room ID string or "zone_<zoneId>"
            var nodeMap = new Dictionary<string, MapNodeVm>();

            // Non-zone rooms
            foreach (var (room, (gx, gy)) in positions)
            {
                if (roomToZone.ContainsKey(room.Id)) continue;

                double cx = (gx - minGx) * STEP_X + STEP_X / 2 + PADDING;
                double cy = (gy - minGy) * STEP_Y + STEP_Y / 2 + PADDING;

                var kind = GetRoomKind(room);

                nodeMap[room.Id.ToString()] = new MapNodeVm
                {
                    RoomId    = room.Id,
                    Label     = Localization.T(room.Name),
                    X         = cx - NODE_W / 2,
                    Y         = cy - NODE_H / 2,
                    CenterX   = cx,
                    CenterY   = cy,
                    Width     = NODE_W,
                    Height    = NODE_H,
                    IsCurrent = room.Id == currentRoom.Id,
                    Kind      = kind
                };
            }

            // All zone group nodes (non-dungeon and dungeon)
            foreach (var (zoneId, (gx, gy)) in zoneGridPos)
            {
                var zone = bigZones.First(z => z.Id == zoneId);
                double cx = (gx - minGx) * STEP_X + STEP_X / 2 + PADDING;
                double cy = (gy - minGy) * STEP_Y + STEP_Y / 2 + PADDING;

                nodeMap["zone_" + zoneId] = new MapNodeVm
                {
                    RoomId      = -1,
                    Label       = zone.DisplayName,
                    X           = cx - GROUP_W / 2,
                    Y           = cy - GROUP_H / 2,
                    CenterX     = cx,
                    CenterY     = cy,
                    Width       = GROUP_W,
                    Height      = GROUP_H,
                    IsCurrent   = false,
                    Kind        = zone.Kind,
                    IsGroupNode = true
                };
            }

            // Edges: replace zone-room endpoints with the zone node key
            // Also handle dungeon zone edges (from connecting non-dungeon room)
            var edges = new List<MapEdgeVm>();
            var seenEdges = new HashSet<(string, string)>();

            // Edges from MapBuilder BFS (non-dungeon rooms)
            foreach (var (room, _) in positions)
            {
                string fromKey = roomToZone.TryGetValue(room.Id, out var fz)
                    ? "zone_" + fz.Id
                    : room.Id.ToString();

                if (!nodeMap.ContainsKey(fromKey)) continue;

                foreach (var (_, targetId) in room.ExitIds)
                {
                    string toKey = roomToZone.TryGetValue(targetId, out var tz)
                        ? "zone_" + tz.Id
                        : targetId.ToString();

                    if (!nodeMap.ContainsKey(toKey)) continue;
                    if (fromKey == toKey) continue;

                    var edgeKey = string.Compare(fromKey, toKey, StringComparison.Ordinal) < 0
                        ? (fromKey, toKey) : (toKey, fromKey);
                    if (!seenEdges.Add(edgeKey)) continue;

                    var fromNode = nodeMap[fromKey];
                    var toNode   = nodeMap[toKey];
                    edges.Add(new MapEdgeVm
                    {
                        X1 = fromNode.CenterX, Y1 = fromNode.CenterY,
                        X2 = toNode.CenterX,   Y2 = toNode.CenterY
                    });
                }

                // Dungeon edges: exits from non-dungeon rooms into dungeon rooms
                foreach (var (_, targetId) in room.ExitIds)
                {
                    var dungeonZone = dungeonZones.FirstOrDefault(d => d.RoomIds.Contains(targetId));
                    if (dungeonZone == null) continue;

                    string dungeonKey = "zone_" + dungeonZone.Id;
                    if (!nodeMap.ContainsKey(dungeonKey)) continue;

                    string roomKey = roomToZone.TryGetValue(room.Id, out var rz)
                        ? "zone_" + rz.Id
                        : room.Id.ToString();
                    if (!nodeMap.ContainsKey(roomKey)) continue;
                    if (roomKey == dungeonKey) continue;

                    var edgeKey = string.Compare(roomKey, dungeonKey, StringComparison.Ordinal) < 0
                        ? (roomKey, dungeonKey) : (dungeonKey, roomKey);
                    if (!seenEdges.Add(edgeKey)) continue;

                    var fromNode = nodeMap[roomKey];
                    var toNode   = nodeMap[dungeonKey];
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

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static NodeKind GetRoomKind(Room room)
        {
            if (room.IsBossRoom)    return NodeKind.Boss;
            if (room.IsDungeonRoom) return NodeKind.Dungeon;
            if (room.IsCaveRoom)    return NodeKind.Cave;
            if (room.IsCity)        return NodeKind.City;
            if (ForestRegistry.GetForestByRoom(room) != null) return NodeKind.Forest;
            return NodeKind.World;
        }

        /// <summary>
        /// Returns true if the non-zone room chain starting at <paramref name="startId"/> (coming from
        /// <paramref name="fromId"/>) leads to exactly one other big zone without any junction.
        /// </summary>
        private static bool IsBridge(int startId, int fromId, ZoneInfo originZone,
                                      Dictionary<int, ZoneInfo> roomToZone)
        {
            var visited = new HashSet<int> { fromId };
            int current = startId;

            while (true)
            {
                if (roomToZone.TryGetValue(current, out var zone))
                {
                    // Hit a zone: bridge only if it's a different zone
                    return zone.Id != originZone.Id;
                }

                var room = RoomService.GetRoomById(current);
                if (room == null) return false;

                visited.Add(current);

                var outward = room.ExitIds
                    .Where(e => !visited.Contains(e.Value))
                    .ToList();

                if (outward.Count == 0) return false; // dead end — not a bridge
                if (outward.Count > 1) return false;  // junction — not correctable

                current = outward[0].Value;
            }
        }

        /// <summary>
        /// Overrides grid positions of rooms in a bridge corridor so they sit directly in line
        /// with <paramref name="originZone"/>'s anchor, then updates the terminating zone's anchor.
        /// </summary>
        private static void CascadeCorrect(ZoneInfo originZone, string direction, int startId, int fromId,
                                           Dictionary<Room, (int x, int y)> positions,
                                           Dictionary<string, (double gx, double gy)> zoneGridPos,
                                           Dictionary<int, ZoneInfo> roomToZone)
        {
            int dx = direction.ToLower() switch { "east" => 1, "west" => -1, _ => 0 };
            int dy = direction.ToLower() switch { "south" => 1, "north" => -1, _ => 0 };

            var (anchorGx, anchorGy) = zoneGridPos[originZone.Id];
            var visited = new HashSet<int> { fromId };
            int current = startId;
            int step = 1;

            while (true)
            {
                if (roomToZone.TryGetValue(current, out var endZone))
                {
                    // Update the terminating zone's anchor to the cascaded position
                    zoneGridPos[endZone.Id] = (anchorGx + dx * step, anchorGy + dy * step);
                    break;
                }

                var room = RoomService.GetRoomById(current);
                if (room == null) break;

                visited.Add(current);

                if (positions.ContainsKey(room))
                    positions[room] = ((int)(anchorGx + dx * step), (int)(anchorGy + dy * step));

                step++;

                var next = room.ExitIds
                    .Where(e => !visited.Contains(e.Value))
                    .ToList();

                if (next.Count != 1) break;
                current = next[0].Value;
            }
        }

        /// <summary>BFS restricted to a specific set of room IDs (for group view).</summary>
        private static Dictionary<Room, (int x, int y)> BuildZoneBfs(Room startRoom, HashSet<int> zoneRoomIds)
        {
            var positions = new Dictionary<Room, (int x, int y)>();
            var occupied  = new HashSet<(int, int)> { (0, 0) };
            var queue     = new Queue<(Room room, int x, int y)>();
            var visited   = new HashSet<int>();

            queue.Enqueue((startRoom, 0, 0));
            positions[startRoom] = (0, 0);

            while (queue.Count > 0)
            {
                var (room, x, y) = queue.Dequeue();
                if (!visited.Add(room.Id)) continue;

                foreach (var (dir, targetId) in room.ExitIds)
                {
                    if (!zoneRoomIds.Contains(targetId)) continue;
                    var target = RoomService.GetRoomById(targetId);
                    if (target == null || positions.ContainsKey(target)) continue;

                    int ddx = dir.ToLower() switch { "east" => 1, "west" => -1, _ => 0 };
                    int ddy = dir.ToLower() switch { "north" => -1, "south" => 1, _ => 0 };

                    int extend = 1;
                    (int cx, int cy) candidate = (x + ddx * extend, y + ddy * extend);
                    while (occupied.Contains(candidate))
                    {
                        extend++;
                        candidate = (x + ddx * extend, y + ddy * extend);
                    }

                    occupied.Add(candidate);
                    positions[target] = candidate;
                    queue.Enqueue((target, candidate.cx, candidate.cy));
                }
            }

            return positions;
        }
    }
}
