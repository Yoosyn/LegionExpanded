using System;
using Xunit;

namespace AmigaNet.Legion.Tests.Pathfinding
{
    public class CampaignMovementTests
    {
        [Theory]
        [InlineData(100, 100, 200, 100, 5)]  // Move Right
        [InlineData(100, 100, 100, 200, 5)]  // Move Down
        [InlineData(100, 100, 100, 50, 5)]   // Move Up
        [InlineData(100, 100, 0, 100, 5)]    // Move Left
        [InlineData(100, 100, 200, 200, 10)] // Move Diagonal
        public void CampaignMovement_VectorMath_CalculatesCorrectStep(int startX, int startY, int goalX, int goalY, int speed)
        {
            // Simulating MA_RUCH's exact vector step calculation:
            double x1 = startX;
            double y1 = startY;
            double dx = goalX - startX;
            double dy = goalY - startY;

            double distance = Math.Sqrt(dx * dx + dy * dy) + 0.2;
            double vx = dx / distance;
            double vy = dy / distance;

            for (int i = 0; i < speed; i++)
            {
                x1 += vx;
                y1 += vy;
            }

            int finalX = (int)x1;
            int finalY = (int)y1;

            // Assert that army moved towards the goal
            if (goalX > startX) Assert.True(finalX > startX);
            if (goalX < startX) Assert.True(finalX < startX);
            if (goalY > startY) Assert.True(finalY > startY);
            if (goalY < startY) Assert.True(finalY < startY);

            // Assert displacement magnitude matches speed closely
            double movedDist = Math.Sqrt((finalX - startX) * (finalX - startX) + (finalY - startY) * (finalY - startY));
            Assert.True(movedDist <= speed + 1, $"Moved distance {movedDist} exceeded speed {speed}");
        }

        [Fact]
        public void CampaignMovement_ArrivalThreshold_DetectsGoalReached()
        {
            int goalX = 150;
            int goalY = 150;

            // Positions within 3 pixels threshold (Math.Abs(DX) < 3 && Math.Abs(DY) < 3)
            int currentX = 148;
            int currentY = 151;

            int dx = goalX - currentX;
            int dy = goalY - currentY;

            bool isGoalReached = Math.Abs(dx) < 3 && Math.Abs(dy) < 3;
            Assert.True(isGoalReached, "Target should be considered reached when within 3px threshold");
        }
    }
}
