namespace AmigaNet.Legion.Pathfinding
{
    /// <summary>
    /// A* pathfinding over a <see cref="NavGrid"/>. Used by A_RUCH to route battle-screen
    /// units around static terrain instead of the original's single-probe greedy walk,
    /// which got stuck on concave obstacles. Never throws on an unreachable target - callers
    /// get <c>null</c> back and are expected to fall back to direct-line movement.
    /// </summary>
    public class Pathfinder
    {
        // Move costs scaled by 10 so the octile heuristic can stay in integers (14 ~= 10*sqrt(2)).
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;
        private const int MaxExpansions = 20000;
        private const int GoalSearchRadius = 6;

        private static readonly (int dx, int dy)[] Neighbors =
        {
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (1, 1), (1, -1), (-1, 1), (-1, -1),
        };

        /// <summary>
        /// Finds a path from (startX,startY) to (goalX,goalY) in pixel coordinates.
        /// Returns simplified pixel waypoints ending exactly at (goalX,goalY), or
        /// <c>null</c> if no path could be found.
        /// </summary>
        public List<(int X, int Y)> FindPath(NavGrid grid, bool isPlayerArmy, int startX, int startY, int goalX, int goalY)
        {
            var (startCellX, startCellY) = grid.ToCell(Math.Clamp(startX, 0, grid.Cols * NavGrid.CellSize - 1), Math.Clamp(startY, 0, grid.Rows * NavGrid.CellSize - 1));
            var (goalCellX, goalCellY) = grid.ToCell(Math.Clamp(goalX, 0, grid.Cols * NavGrid.CellSize - 1), Math.Clamp(goalY, 0, grid.Rows * NavGrid.CellSize - 1));

            if (!grid.InBounds(startCellX, startCellY)) return null!;
            if (!TryFindWalkableGoalCell(grid, isPlayerArmy, goalCellX, goalCellY, out goalCellX, out goalCellY)) return null!;

            if (startCellX == goalCellX && startCellY == goalCellY)
            {
                return new List<(int X, int Y)> { (goalX, goalY) };
            }

            var cellPath = RunAStar(grid, isPlayerArmy, startCellX, startCellY, goalCellX, goalCellY);
            if (cellPath == null) return null!;

            var waypoints = FunnelSimplify(grid, isPlayerArmy, cellPath, goalX, goalY);
            return waypoints;
        }

        private bool TryFindWalkableGoalCell(NavGrid grid, bool isPlayerArmy, int goalCellX, int goalCellY, out int foundX, out int foundY)
        {
            if (grid.IsWalkableCell(isPlayerArmy, goalCellX, goalCellY))
            {
                foundX = goalCellX;
                foundY = goalCellY;
                return true;
            }

            for (var radius = 1; radius <= GoalSearchRadius; radius++)
            {
                for (var dy = -radius; dy <= radius; dy++)
                {
                    for (var dx = -radius; dx <= radius; dx++)
                    {
                        if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius) continue;
                        var x = goalCellX + dx;
                        var y = goalCellY + dy;
                        if (grid.IsWalkableCell(isPlayerArmy, x, y))
                        {
                            foundX = x;
                            foundY = y;
                            return true;
                        }
                    }
                }
            }

            foundX = goalCellX;
            foundY = goalCellY;
            return false;
        }

        private List<(int X, int Y)> RunAStar(NavGrid grid, bool isPlayerArmy, int startX, int startY, int goalX, int goalY)
        {
            var open = new PriorityQueue<(int X, int Y), int>();
            var cameFrom = new Dictionary<(int X, int Y), (int X, int Y)>();
            var costSoFar = new Dictionary<(int X, int Y), int> { [(startX, startY)] = 0 };

            open.Enqueue((startX, startY), Heuristic(startX, startY, goalX, goalY));

            var expansions = 0;
            while (open.Count > 0)
            {
                if (++expansions > MaxExpansions) return null!;

                var current = open.Dequeue();
                if (current.X == goalX && current.Y == goalY)
                {
                    return ReconstructPath(cameFrom, current, (startX, startY));
                }

                foreach (var (dx, dy) in Neighbors)
                {
                    var next = (X: current.X + dx, Y: current.Y + dy);
                    if (!grid.IsWalkableCell(isPlayerArmy, next.X, next.Y)) continue;

                    // Prevent cutting across a blocked diagonal corner.
                    if (dx != 0 && dy != 0)
                    {
                        if (!grid.IsWalkableCell(isPlayerArmy, current.X + dx, current.Y) ||
                            !grid.IsWalkableCell(isPlayerArmy, current.X, current.Y + dy))
                        {
                            continue;
                        }
                    }

                    var moveCost = dx != 0 && dy != 0 ? DiagonalCost : StraightCost;
                    var newCost = costSoFar[(current.X, current.Y)] + moveCost;

                    if (!costSoFar.TryGetValue(next, out var existingCost) || newCost < existingCost)
                    {
                        costSoFar[next] = newCost;
                        cameFrom[next] = (current.X, current.Y);
                        open.Enqueue(next, newCost + Heuristic(next.X, next.Y, goalX, goalY));
                    }
                }
            }

            return null!;
        }

        private static int Heuristic(int x, int y, int goalX, int goalY)
        {
            var dx = Math.Abs(x - goalX);
            var dy = Math.Abs(y - goalY);
            return StraightCost * (dx + dy) + (DiagonalCost - 2 * StraightCost) * Math.Min(dx, dy);
        }

        private static List<(int X, int Y)> ReconstructPath(Dictionary<(int X, int Y), (int X, int Y)> cameFrom, (int X, int Y) current, (int X, int Y) start)
        {
            var path = new List<(int X, int Y)> { current };
            while (current != start)
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Simplifies an A* cell path using a funnel algorithm (Tomlin, 2000).
        /// Produces shorter, more natural pixel waypoints that cut corners through
        /// narrow passages instead of following cell centers. Falls back to the
        /// original Bresenham-based simplification when the path is trivial.
        /// </summary>
        private List<(int X, int Y)> FunnelSimplify(NavGrid grid, bool isPlayerArmy, List<(int X, int Y)> cellPath, int goalPixelX, int goalPixelY)
        {
            if (cellPath.Count <= 2)
            {
                // Trivial path: just convert start+end to pixels.
                var (px, py) = grid.ToPixelCenter(cellPath[^1].X, cellPath[^1].Y);
                return new List<(int X, int Y)> { (px, py) };
            }

            // Build portal edges between consecutive cells.
            // Each portal is a pair of points (left, right) on the shared edge
            // between cell[i] and cell[i+1], oriented relative to the path direction.
            var portals = new List<((int X, int Y) Left, (int X, int Y) Right)>(cellPath.Count - 1);
            for (var i = 0; i < cellPath.Count - 1; i++)
            {
                var (fromX, fromY) = cellPath[i];
                var (toX, toY) = cellPath[i + 1];
                portals.Add(GetPortalEdge(fromX, fromY, toX, toY, NavGrid.CellSize));
            }

            var waypoints = new List<(int X, int Y)>();
            var apex = grid.ToPixelCenter(cellPath[0].X, cellPath[0].Y);
            var portalLeft = apex;
            var portalRight = apex;

            waypoints.Add(apex);

            for (var i = 0; i < portals.Count; i++)
            {
                var (newLeft, newRight) = portals[i];

                // Narrow the funnel on the left side.
                if (Cross(apex, portalRight, newLeft) <= 0)
                {
                    if (Equal(apex, portalRight) || Cross(apex, portalLeft, newLeft) > 0)
                    {
                        portalLeft = newLeft;
                    }
                    else
                    {
                        // Left portal crossed over right - flip apex to right and restart.
                        waypoints.Add(portalRight);
                        apex = portalRight;
                        portalLeft = apex;
                        portalRight = apex;
                        // Restart from current portal.
                        i--;
                        continue;
                    }
                }

                // Narrow the funnel on the right side.
                if (Cross(apex, portalLeft, newRight) >= 0)
                {
                    if (Equal(apex, portalLeft) || Cross(apex, portalRight, newRight) < 0)
                    {
                        portalRight = newRight;
                    }
                    else
                    {
                        // Right portal crossed over left - flip apex to left and restart.
                        waypoints.Add(portalLeft);
                        apex = portalLeft;
                        portalLeft = apex;
                        portalRight = apex;
                        i--;
                        continue;
                    }
                }
            }

            // Add the final destination.
            waypoints.Add((goalPixelX, goalPixelY));

            // Remove zigzag: if 3 consecutive waypoints alternate in horizontal
            // direction while Y barely changes, drop the middle one.
            waypoints = RemoveZigzag(waypoints, minDistance: 12);

            // Verify the simplified path doesn't cut through blocked cells.
            // If it does, fall back to Bresenham simplification.
            for (var i = 0; i < waypoints.Count - 1; i++)
            {
                if (!HasLineOfSightPixels(grid, isPlayerArmy, waypoints[i], waypoints[i + 1]))
                {
                    return SimplifyAndConvertFallback(grid, isPlayerArmy, cellPath, goalPixelX, goalPixelY);
                }
            }

            return waypoints;
        }

        /// <summary>
        /// Removes zigzag waypoints: when 3 consecutive waypoints alternate in one
        /// axis direction while the other axis barely changes, the middle waypoint
        /// is removed. This prevents the sprite from flipping direction rapidly.
        /// </summary>
        private static List<(int X, int Y)> RemoveZigzag(List<(int X, int Y)> waypoints, int minDistance)
        {
            if (waypoints.Count < 3) return waypoints;

            var result = new List<(int X, int Y)> { waypoints[0] };
            for (var i = 1; i < waypoints.Count - 1; i++)
            {
                var prev = result[^1];
                var curr = waypoints[i];
                var next = waypoints[i + 1];

                var dxPrev = curr.X - prev.X;
                var dyPrev = curr.Y - prev.Y;
                var dxNext = next.X - curr.X;
                var dyNext = next.Y - curr.Y;

                // Detect zigzag: opposite horizontal signs with small vertical change.
                var isZigzagX = dxPrev != 0 && dxNext != 0
                    && Math.Sign(dxPrev) != Math.Sign(dxNext)
                    && Math.Abs(dyPrev) < minDistance && Math.Abs(dyNext) < minDistance;

                var isZigzagY = dyPrev != 0 && dyNext != 0
                    && Math.Sign(dyPrev) != Math.Sign(dyNext)
                    && Math.Abs(dxPrev) < minDistance && Math.Abs(dxNext) < minDistance;

                if (isZigzagX || isZigzagY) continue; // skip the zigzag waypoint

                result.Add(curr);
            }
            result.Add(waypoints[^1]);
            return result;
        }

        /// <summary>
        /// Computes the portal edge between two adjacent cells. The portal is the
        /// shared boundary (or corner for diagonal moves) represented as two points
        /// that define the left and right sides of the funnel opening.
        /// </summary>
        private static ((int X, int Y) Left, (int X, int Y) Right) GetPortalEdge(
            int fromCellX, int fromCellY, int toCellX, int toCellY, int cellSize)
        {
            var half = cellSize / 2;

            // Cardinal directions: portal is the full shared edge.
            if (toCellX > fromCellX) // moving right
            {
                var edgeX = (fromCellX + 1) * cellSize;
                return ((edgeX, fromCellY * cellSize + half),
                        (edgeX, (fromCellY + 1) * cellSize + half));
            }
            if (toCellX < fromCellX) // moving left
            {
                var edgeX = fromCellX * cellSize;
                return ((edgeX, fromCellY * cellSize + half),
                        (edgeX, (fromCellY + 1) * cellSize + half));
            }
            if (toCellY > fromCellY) // moving down
            {
                var edgeY = (fromCellY + 1) * cellSize;
                return ((fromCellX * cellSize + half, edgeY),
                        ((fromCellX + 1) * cellSize + half, edgeY));
            }
            // moving up
            {
                var edgeY = fromCellY * cellSize;
                return ((fromCellX * cellSize + half, edgeY),
                        ((fromCellX + 1) * cellSize + half, edgeY));
            }
        }

        private static float Cross((int X, int Y) a, (int X, int Y) b, (int X, int Y) c)
        {
            return (float)(b.X - a.X) * (c.Y - a.Y) - (float)(b.Y - a.Y) * (c.X - a.X);
        }

        private static bool Equal((int X, int Y) a, (int X, int Y) b) => a.X == b.X && a.Y == b.Y;

        /// <summary>
        /// Checks line-of-sight between two pixel-coordinate points by walking
        /// the Bresenham line through grid cells.
        /// </summary>
        private bool HasLineOfSightPixels(NavGrid grid, bool isPlayerArmy, (int X, int Y) from, (int X, int Y) to)
        {
            var (x0, y0) = grid.ToCell(from.X, from.Y);
            var (x1, y1) = grid.ToCell(to.X, to.Y);

            var dx = Math.Abs(x1 - x0);
            var dy = -Math.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx + dy;

            var x = x0;
            var y = y0;
            while (true)
            {
                if (!grid.IsWalkableCell(isPlayerArmy, x, y)) return false;
                if (x == x1 && y == y1) break;

                var e2 = 2 * err;
                var stepX = 0;
                var stepY = 0;
                if (e2 >= dy) { err += dy; x += sx; stepX = sx; }
                if (e2 <= dx) { err += dx; y += sy; stepY = sy; }

                if (stepX != 0 && stepY != 0)
                {
                    if (!grid.IsWalkableCell(isPlayerArmy, x - stepX, y) || !grid.IsWalkableCell(isPlayerArmy, x, y - stepY))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Fallback path simplification using Bresenham line-of-sight checks.
        /// Used when the funnel algorithm produces a path that cuts through blocked cells.
        /// </summary>
        private List<(int X, int Y)> SimplifyAndConvertFallback(NavGrid grid, bool isPlayerArmy, List<(int X, int Y)> cellPath, int goalPixelX, int goalPixelY)
        {
            var simplifiedCells = new List<(int X, int Y)> { cellPath[0] };
            var anchor = 0;
            for (var i = 1; i < cellPath.Count - 1; i++)
            {
                if (!HasLineOfSight(grid, isPlayerArmy, cellPath[anchor], cellPath[i + 1]))
                {
                    simplifiedCells.Add(cellPath[i]);
                    anchor = i;
                }
            }
            simplifiedCells.Add(cellPath[^1]);

            var waypoints = new List<(int X, int Y)>(simplifiedCells.Count - 1);
            for (var i = 1; i < simplifiedCells.Count; i++)
            {
                var (px, py) = grid.ToPixelCenter(simplifiedCells[i].X, simplifiedCells[i].Y);
                waypoints.Add((px, py));
            }
            waypoints[^1] = (goalPixelX, goalPixelY);

            return waypoints;
        }

        private bool HasLineOfSight(NavGrid grid, bool isPlayerArmy, (int X, int Y) from, (int X, int Y) to)
        {
            // Bresenham line walk over grid cells, rejecting the line if it would cut a blocked diagonal corner.
            var x0 = from.X;
            var y0 = from.Y;
            var x1 = to.X;
            var y1 = to.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = -Math.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx + dy;

            var x = x0;
            var y = y0;
            while (true)
            {
                if (!grid.IsWalkableCell(isPlayerArmy, x, y)) return false;
                if (x == x1 && y == y1) break;

                var e2 = 2 * err;
                var stepX = 0;
                var stepY = 0;
                if (e2 >= dy) { err += dy; x += sx; stepX = sx; }
                if (e2 <= dx) { err += dx; y += sy; stepY = sy; }

                if (stepX != 0 && stepY != 0)
                {
                    if (!grid.IsWalkableCell(isPlayerArmy, x - stepX, y) || !grid.IsWalkableCell(isPlayerArmy, x, y - stepY))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
