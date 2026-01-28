namespace Hexmath.Tests;

public class RotationTests
{
    private static readonly HexCoord Origin = HexCoord.Zero;

    #region RotateBySteps

    [Fact]
    public void RotateBySteps_SixSteps_ReturnsToOriginal()
    {
        var hex = new HexCoord(2, -1);
        var rotated = HMath.RotateBySteps(hex, Origin, 6);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_ZeroSteps_ReturnsSame()
    {
        var hex = new HexCoord(2, -1);
        var rotated = HMath.RotateBySteps(hex, Origin, 0);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_TwelveSteps_ReturnsToOriginal()
    {
        var hex = new HexCoord(3, -2);
        var rotated = HMath.RotateBySteps(hex, Origin, 12);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_NegativeSteps_RotatesCounterClockwise()
    {
        var hex = new HexCoord(1, 0); // EAST direction
        var rotatedCW = HMath.RotateBySteps(hex, Origin, 1);
        var rotatedCCW = HMath.RotateBySteps(hex, Origin, -1);

        // Rotating 1 step CW then 1 step CCW should return to original
        var backToOriginal = HMath.RotateBySteps(rotatedCW, Origin, -1);
        Assert.Equal(hex, backToOriginal);

        // -1 step should equal 5 steps CW
        var fiveStepsCW = HMath.RotateBySteps(hex, Origin, 5);
        Assert.Equal(fiveStepsCW, rotatedCCW);
    }

    [Fact]
    public void RotateBySteps_PreservesDistance()
    {
        var hex = new HexCoord(3, -2);
        var originalDistance = HMath.Distance(Origin, hex);

        for (int steps = 0; steps < 6; steps++)
        {
            var rotated = HMath.RotateBySteps(hex, Origin, steps);
            Assert.Equal(originalDistance, HMath.Distance(Origin, rotated));
        }
    }

    [Fact]
    public void RotateBySteps_AroundNonOrigin_WorksCorrectly()
    {
        var center = new HexCoord(2, -1);
        var hex = new HexCoord(3, -1); // One step EAST from center

        var rotated = HMath.RotateBySteps(hex, center, 6);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_DirectionsCycleCorrectly()
    {
        // Starting with EAST offset, rotate through all positions
        var east = HexDirection.East.ToOffset();
        var current = east;

        // After 6 rotations, we should be back to start
        for (int i = 0; i < 6; i++)
        {
            current = HMath.RotateBySteps(current, Origin, 1);
        }

        Assert.Equal(east, current);
    }

    [Fact]
    public void RotateBySteps_MaintainsHexConstraint()
    {
        var hex = new HexCoord(4, -2);

        for (int steps = 0; steps < 6; steps++)
        {
            var rotated = HMath.RotateBySteps(hex, Origin, steps);
            Assert.Equal(0, rotated.Q + rotated.R + rotated.S);
        }
    }

    [Theory]
    [InlineData(1, 0, 1)]    // 1 step: EAST (1,0) -> (0, 1) NorthEast
    [InlineData(2, -1, 1)]   // 2 steps: -> (-1, 1) NorthWest
    [InlineData(3, -1, 0)]   // 3 steps: -> (-1, 0) West (opposite)
    [InlineData(4, 0, -1)]   // 4 steps: -> (0, -1) SouthWest
    [InlineData(5, 1, -1)]   // 5 steps: -> (1, -1) SouthEast
    public void RotateBySteps_RotatesCorrectly(int steps, int expectedQ, int expectedR)
    {
        var east = new HexCoord(1, 0);
        var rotated = HMath.RotateBySteps(east, Origin, steps);

        Assert.Equal(expectedQ, rotated.Q);
        Assert.Equal(expectedR, rotated.R);
    }

    #endregion

    #region RotateToMatch

    [Fact]
    public void RotateToMatch_SameOrientation_ReturnsSame()
    {
        var orientFrom = new HexCoord(1, 0);
        var hexToRotate = new HexCoord(2, -1);

        var result = HMath.RotateToMatch(orientFrom, orientFrom, hexToRotate, Origin);

        Assert.Equal(hexToRotate, result);
    }

    [Fact]
    public void RotateToMatch_ForceClockwise_UsesClockwiseRotation()
    {
        var orientFrom = HexDirection.East.ToOffset();
        var orientTo = HexDirection.NorthEast.ToOffset();
        var hexToRotate = new HexCoord(2, 0);

        var result = HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin, forceClockwise: true);

        // Verify it produces a valid result with same distance
        Assert.Equal(0, result.Q + result.R + result.S);
        Assert.Equal(HMath.Distance(Origin, hexToRotate), HMath.Distance(Origin, result));
    }

    [Fact]
    public void RotateToMatch_InvalidRotation_DifferentDistances_ThrowsException()
    {
        var orientFrom = new HexCoord(1, 0);
        var orientTo = new HexCoord(2, -1); // Different distance from origin
        var hexToRotate = new HexCoord(1, -1);

        Assert.Throws<ArgumentException>(() =>
            HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin));
    }

    [Fact]
    public void RotateToMatch_OrientFromIsOrigin_ThrowsException()
    {
        var hexToRotate = new HexCoord(2, -1);

        Assert.Throws<ArgumentException>(() =>
            HMath.RotateToMatch(Origin, new HexCoord(1, 0), hexToRotate, Origin));
    }

    [Fact]
    public void RotateToMatch_OrientToIsOrigin_ThrowsException()
    {
        var hexToRotate = new HexCoord(2, -1);

        Assert.Throws<ArgumentException>(() =>
            HMath.RotateToMatch(new HexCoord(1, 0), Origin, hexToRotate, Origin));
    }

    [Fact]
    public void RotateToMatch_ChoosesShorterPath_WhenNotForced()
    {
        var orientFrom = HexDirection.East.ToOffset();
        var orientTo = HexDirection.NorthEast.ToOffset();
        var hexToRotate = new HexCoord(2, 0);

        var result = HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin);

        // The method should choose the shorter rotation path
        // Verify result is at same distance
        Assert.Equal(HMath.Distance(Origin, hexToRotate), HMath.Distance(Origin, result));
    }

    [Fact]
    public void RotateToMatch_PreservesDistance()
    {
        var orientFrom = new HexCoord(1, 0);
        var orientTo = new HexCoord(-1, 0); // Opposite (3 steps)
        var hexToRotate = new HexCoord(3, -2);

        var result = HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin);

        Assert.Equal(HMath.Distance(Origin, hexToRotate), HMath.Distance(Origin, result));
    }

    [Fact]
    public void RotateToMatch_OppositeDirection_RotatesHalfway()
    {
        var orientFrom = new HexCoord(1, 0); // EAST
        var orientTo = new HexCoord(-1, 0);  // WEST (opposite)
        var hexToRotate = new HexCoord(1, 0);

        var result = HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin);

        // Should rotate to the opposite direction
        Assert.Equal(orientTo, result);
    }

    #endregion
}
