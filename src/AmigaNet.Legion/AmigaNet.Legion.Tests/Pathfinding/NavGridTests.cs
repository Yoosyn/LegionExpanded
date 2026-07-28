using AmigaNet.Amos.Screens;
using AmigaNet.Legion.Pathfinding;
using Xunit;

namespace AmigaNet.Legion.Tests.Pathfinding
{
    public class NavGridTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(7, 7, 0, 0)]
        [InlineData(8, 8, 1, 1)]
        [InlineData(15, 23, 1, 2)]
        public void ToCell_ConvertsPixelCoordinatesToCellIndices(int px, int py, int expectedCellX, int expectedCellY)
        {
            var grid = new NavGrid();
            var (cx, cy) = grid.ToCell(px, py);

            Assert.Equal(expectedCellX, cx);
            Assert.Equal(expectedCellY, cy);
        }

        [Theory]
        [InlineData(0, 0, 4, 4)]
        [InlineData(1, 1, 12, 12)]
        [InlineData(2, 3, 20, 28)]
        public void ToPixelCenter_ConvertsCellToCenterPixel(int cellX, int cellY, int expectedPx, int expectedPy)
        {
            var grid = new NavGrid();
            var (px, py) = grid.ToPixelCenter(cellX, cellY);

            Assert.Equal(expectedPx, px);
            Assert.Equal(expectedPy, py);
        }

        [Fact]
        [System.Obsolete]
        public void RebuildIfNeeded_MarksStaticZoneAsBlocked()
        {
            var screens = new ScreensManager(string.Empty, null!, null!);
            screens.ScreenOpen(0, 320, 200, 16, PixelMode.Lowres);
            screens.SetZone(21, 16, 16, 32, 32); // Static obstacle (Zone >= 21)

            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            // Cell (2,2) corresponds to pixel (16, 16), which should be blocked
            Assert.False(grid.IsWalkableCell(isPlayerArmy: true, cellX: 2, cellY: 2));
            Assert.False(grid.IsWalkableCell(isPlayerArmy: false, cellX: 2, cellY: 2));

            // Cell (0,0) corresponds to pixel (0,0), which should be walkable
            Assert.True(grid.IsWalkableCell(isPlayerArmy: true, cellX: 0, cellY: 0));
        }

        [Fact]
        [System.Obsolete]
        public void RebuildIfNeeded_HandlesDoorZonePassabilityForPlayerOnly()
        {
            var screens = new ScreensManager(string.Empty, null!, null!);
            screens.ScreenOpen(0, 320, 200, 16, PixelMode.Lowres);
            
            // Pass 1: Wall (Zone 120) from (16, 16) to (64, 64)
            screens.SetZone(120, 16, 16, 64, 64);
            // Pass 2: Door (Zone 105) overlapping entrance at (16, 16) to (24, 24)
            screens.SetZone(105, 16, 16, 24, 24);

            var grid = new NavGrid();
            grid.RebuildIfNeeded(screens);

            // Cell (2, 2) [16x16] is a door area: walkable for player, blocked for others
            Assert.True(grid.IsWalkableCell(isPlayerArmy: true, cellX: 2, cellY: 2));
            Assert.False(grid.IsWalkableCell(isPlayerArmy: false, cellX: 2, cellY: 2));

            // Cell (5, 5) [40x40] is a solid building wall: blocked for both
            Assert.False(grid.IsWalkableCell(isPlayerArmy: true, cellX: 5, cellY: 5));
            Assert.False(grid.IsWalkableCell(isPlayerArmy: false, cellX: 5, cellY: 5));
        }

        [Fact]
        public void SetDynamicBlockers_BlocksDynamicUnitCells()
        {
            var grid = new NavGrid();
            var screens = new ScreensManager(string.Empty, null!, null!);
            screens.ScreenOpen(0, 320, 200, 16, PixelMode.Lowres);
            grid.RebuildIfNeeded(screens);

            var unitRects = new System.Collections.Generic.List<(int Left, int Top, int Width, int Height)>
            {
                (16, 16, 8, 8)
            };

            grid.SetDynamicBlockers(unitRects);

            // Cell (2, 2) [16x16] should now be blocked by dynamic unit
            Assert.False(grid.IsWalkableCell(isPlayerArmy: true, cellX: 2, cellY: 2));

            grid.ClearDynamicBlockers();
            Assert.True(grid.IsWalkableCell(isPlayerArmy: true, cellX: 2, cellY: 2));
        }
    }
}
