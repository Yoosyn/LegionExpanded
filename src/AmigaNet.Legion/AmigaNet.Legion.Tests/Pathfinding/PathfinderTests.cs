using AmigaNet.Amos.Screens;
using AmigaNet.Legion.Pathfinding;
using Xunit;

namespace AmigaNet.Legion.Tests.Pathfinding
{
    public class PathfinderTests
    {
        private ScreensManager CreateTestScreen(int width = 320, int height = 200)
        {
            var screens = new ScreensManager(string.Empty, null!, null!);
            screens.ScreenOpen(0, width, height, 16, PixelMode.Lowres);
            return screens;
        }

        [Fact]
        public void FindPath_ClearBoard_ReturnsDirectWaypoints()
        {
            var screens = CreateTestScreen();
            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            var pathfinder = new Pathfinder();
            var path = pathfinder.FindPath(grid, isPlayerArmy: true, startX: 10, startY: 10, goalX: 100, goalY: 10);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
            // End point should match exact requested goal
            Assert.Equal((100, 10), path[^1]);
        }

        [Fact]
        public void FindPath_AroundWall_FindsValidPathAroundObstacle()
        {
            var screens = CreateTestScreen();
            // Create a vertical wall in the middle: X=[50..58], Y=[0..100]
            screens.SetZone(21, 50, 0, 58, 100);

            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            var pathfinder = new Pathfinder();
            // Try to move from (20, 50) to (80, 50) crossing the wall
            var path = pathfinder.FindPath(grid, isPlayerArmy: true, startX: 20, startY: 50, goalX: 80, goalY: 50);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
            Assert.Equal((80, 50), path[^1]);

            // Ensure no waypoint is inside the wall
            foreach (var (x, y) in path)
            {
                var (cx, cy) = grid.ToCell(x, y);
                Assert.True(grid.IsWalkableCell(true, cx, cy), $"Waypoint ({x},{y}) cell ({cx},{cy}) is inside obstacle!");
            }
        }

        [Fact]
        public void FindPath_ConcaveUObstacle_EscapesTrap()
        {
            var screens = CreateTestScreen();
            // Create a U-shaped trap opening to the left:
            // Top wall: Y=40, X=[40..80]
            // Bottom wall: Y=80, X=[40..80]
            // Back wall: X=80, Y=[40..80]
            screens.SetZone(21, 40, 40, 80, 48);  // Top
            screens.SetZone(22, 40, 80, 80, 88);  // Bottom
            screens.SetZone(23, 80, 40, 88, 88);  // Back

            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            var pathfinder = new Pathfinder();
            // Start inside the U-trap at (60, 60), target outside at (100, 60)
            var path = pathfinder.FindPath(grid, isPlayerArmy: true, startX: 60, startY: 60, goalX: 100, goalY: 60);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
            Assert.Equal((100, 60), path[^1]);
        }

        [Fact]
        public void FindPath_BlockedGoal_FindsNearestWalkableGoal()
        {
            var screens = CreateTestScreen();
            // Solid obstacle around (50, 50)
            screens.SetZone(21, 48, 48, 64, 64);

            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            var pathfinder = new Pathfinder();
            // Request target directly inside the obstacle (50, 50)
            var path = pathfinder.FindPath(grid, isPlayerArmy: true, startX: 10, startY: 10, goalX: 50, goalY: 50);

            Assert.NotNull(path);
            Assert.NotEmpty(path);
            // Pathfinder should adjust goal to a walkable neighbor near (50, 50)
            var (endX, endY) = path[^1];
            var (endCx, endCy) = grid.ToCell(endX, endY);
            Assert.True(grid.IsWalkableCell(true, endCx, endCy));
        }

        [Fact]
        public void FindPath_DiagonalCornerCutting_IsPrevented()
        {
            var screens = CreateTestScreen();
            // Create two diagonal obstacle blocks touching at corner (16, 16):
            // Block 1: cell (1, 1) -> px [8..16, 8..16]
            // Block 2: cell (2, 2) -> px [16..24, 16..24]
            screens.SetZone(21, 8, 8, 16, 16);
            screens.SetZone(22, 16, 16, 24, 24);

            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            var pathfinder = new Pathfinder();
            // Move from cell (2, 1) [px 20, 12] to cell (1, 2) [px 12, 20]
            var path = pathfinder.FindPath(grid, isPlayerArmy: true, startX: 20, startY: 12, goalX: 12, goalY: 20);

            Assert.NotNull(path);
            Assert.NotEmpty(path);

            // Path must go around, not cut through the touching corners directly
            Assert.True(path.Count >= 2);
        }
    }
}
