using AmigaNet.Amos.Screens;

namespace AmigaNet.Legion.Pathfinding
{
    /// <summary>
    /// Rasterizes a screen's static-terrain <see cref="Zone"/> rectangles (id &gt;= 20)
    /// into a walkability grid, so a pathfinder can query cells in O(1) instead of
    /// linear-scanning the zone list per lookup. Rebuilt lazily whenever
    /// <see cref="ScreensManager.ZoneEpoch"/> changes.
    ///
    /// Walkability is army-dependent, mirroring A_RUCH's original check
    /// (`ST == 0 || ((ST>100 && ST<120 || ST>30 && ST<41) && A == ARM)`): shop/trap
    /// zones (30-40, 100-119) are only passable for the player army.
    ///
    /// Dynamic unit blockers (id 1-20) can be set via <see cref="SetDynamicBlockers"/>
    /// before each pathfinding query so A* plans around other units instead of
    /// relying solely on the post-hoc sidestep heuristic.
    /// </summary>
    public class NavGrid
    {
        public const int CellSize = 8;
        private const int MinStaticZoneId = 21;

        private int builtForEpoch = -1;
        private bool[,] blockedForPlayer = new bool[0, 0];
        private bool[,] blockedForOther = new bool[0, 0];

        // Dynamic blockers: unit positions collected fresh before each repath.
        // These cells are blocked for everyone except the unit being planned for.
        private readonly HashSet<(int X, int Y)> dynamicBlocked = new();

        public int Cols { get; private set; }
        public int Rows { get; private set; }

        /// <summary>
        /// Sets dynamic blocker cells from unit zone rectangles. Call before each
        /// pathfinding query so A* plans around other units. Each rect is
        /// (left, top, width, height) in pixel coordinates, matching SetZone bounds.
        /// </summary>
        public void SetDynamicBlockers(List<(int Left, int Top, int Width, int Height)> unitRects)
        {
            dynamicBlocked.Clear();
            foreach (var (left, top, width, height) in unitRects)
            {
                var (cx1, cy1) = ToCell(Math.Max(0, left), Math.Max(0, top));
                var (cx2, cy2) = ToCell(Math.Max(0, left + width), Math.Max(0, top + height));
                for (var cy = cy1; cy <= cy2; cy++)
                    for (var cx = cx1; cx <= cx2; cx++)
                        if (InBounds(cx, cy))
                            dynamicBlocked.Add((cx, cy));
            }
        }

        /// <summary>
        /// Clears dynamic blockers. Call after pathfinding to avoid stale data.
        /// </summary>
        public void ClearDynamicBlockers() => dynamicBlocked.Clear();

        public void RebuildIfNeeded(ScreensManager screens)
        {
            if (builtForEpoch == screens.ZoneEpoch && Cols > 0) return;

            var (width, height) = screens.GetScreenSize();
            Cols = Math.Max(1, (width + CellSize - 1) / CellSize);
            Rows = Math.Max(1, (height + CellSize - 1) / CellSize);
            blockedForPlayer = new bool[Cols, Rows];
            blockedForOther = new bool[Cols, Rows];

            // Two-pass processing to handle overlapping zones correctly.
            // Door/shop zones (100-119) and trap zones (30-40) overlap with building
            // body zones (120+) at the entrance area. Zone() returns the lowest-numbered
            // zone, so doors take priority for trigger checks. The grid must match:
            // pass 1 blocks terrain, pass 2 carves out door/trap passability for the player.

            // Pass 1: static terrain (walls, buildings, rocks) - blocks everyone.
            foreach (var zone in screens.GetZones())
            {
                if (zone.Number < MinStaticZoneId) continue;
                var isShopOrTrap = (zone.Number > 100 && zone.Number < 120) || (zone.Number > 30 && zone.Number < 41);
                if (isShopOrTrap) continue; // handled in pass 2

                MarkBlocked(zone, width, height, blockPlayer: true, blockOther: true);
            }

            // Pass 2: door/shop/trap zones - passable for the player army only.
            // These carve out cells that pass 1 may have blocked (e.g. building body overlap).
            foreach (var zone in screens.GetZones())
            {
                if (zone.Number < MinStaticZoneId) continue;
                var isShopOrTrap = (zone.Number > 100 && zone.Number < 120) || (zone.Number > 30 && zone.Number < 41);
                if (!isShopOrTrap) continue;

                MarkPassableForPlayer(zone, width, height);
            }

            builtForEpoch = screens.ZoneEpoch;
        }

        private void MarkBlocked(Zone zone, int width, int height, bool blockPlayer, bool blockOther)
        {
            var rawX1 = Math.Max(0, Math.Min(zone.X1, zone.X2));
            var rawY1 = Math.Max(0, Math.Min(zone.Y1, zone.Y2));
            var rawX2 = Math.Min(width - 1, Math.Max(zone.X1, zone.X2));
            var rawY2 = Math.Min(height - 1, Math.Max(zone.Y1, zone.Y2));
            if (rawX2 < rawX1 || rawY2 < rawY1) return;

            var cellX1 = rawX1 / CellSize;
            var cellY1 = rawY1 / CellSize;
            var cellX2 = rawX2 / CellSize;
            var cellY2 = rawY2 / CellSize;

            for (var y = cellY1; y <= cellY2; y++)
            {
                for (var x = cellX1; x <= cellX2; x++)
                {
                    if (blockPlayer) blockedForPlayer[x, y] = true;
                    if (blockOther) blockedForOther[x, y] = true;
                }
            }
        }

        private void MarkPassableForPlayer(Zone zone, int width, int height)
        {
            var rawX1 = Math.Max(0, Math.Min(zone.X1, zone.X2));
            var rawY1 = Math.Max(0, Math.Min(zone.Y1, zone.Y2));
            var rawX2 = Math.Min(width - 1, Math.Max(zone.X1, zone.X2));
            var rawY2 = Math.Min(height - 1, Math.Max(zone.Y1, zone.Y2));
            if (rawX2 < rawX1 || rawY2 < rawY1) return;

            var cellX1 = rawX1 / CellSize;
            var cellY1 = rawY1 / CellSize;
            var cellX2 = rawX2 / CellSize;
            var cellY2 = rawY2 / CellSize;

            for (var y = cellY1; y <= cellY2; y++)
            {
                for (var x = cellX1; x <= cellX2; x++)
                {
                    blockedForPlayer[x, y] = false;
                    blockedForOther[x, y] = true;
                }
            }
        }

        public bool InBounds(int cellX, int cellY) => cellX >= 0 && cellX < Cols && cellY >= 0 && cellY < Rows;

        public bool IsWalkableCell(bool isPlayerArmy, int cellX, int cellY)
        {
            if (!InBounds(cellX, cellY)) return false;
            if (dynamicBlocked.Contains((cellX, cellY))) return false;
            return isPlayerArmy ? !blockedForPlayer[cellX, cellY] : !blockedForOther[cellX, cellY];
        }

        public bool IsWalkablePixel(bool isPlayerArmy, int pixelX, int pixelY)
        {
            var (cellX, cellY) = ToCell(pixelX, pixelY);
            return IsWalkableCell(isPlayerArmy, cellX, cellY);
        }

        public (int X, int Y) ToCell(int pixelX, int pixelY) => (pixelX / CellSize, pixelY / CellSize);

        public (int X, int Y) ToPixelCenter(int cellX, int cellY) => (cellX * CellSize + (CellSize / 2), cellY * CellSize + (CellSize / 2));
    }
}
