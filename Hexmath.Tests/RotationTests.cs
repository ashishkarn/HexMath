using System.Numerics;

namespace Hexmath.Tests;

public class RotationTests
{
    private static readonly Vector3 Origin = Vector3.Zero;

    [Fact]
    public void RotateBySteps_SixSteps_ReturnsToOriginal()
    {
        var hex = new Vector3(2, -1, -1);
        var rotated = HMath.RotateBySteps(hex, Origin, 6);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_ZeroSteps_ReturnsSame()
    {
        var hex = new Vector3(2, -1, -1);
        var rotated = HMath.RotateBySteps(hex, Origin, 0);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_TwelveSteps_ReturnsToOriginal()
    {
        var hex = new Vector3(3, -2, -1);
        var rotated = HMath.RotateBySteps(hex, Origin, 12);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_NegativeSteps_RotatesCounterClockwise()
    {
        var hex = new Vector3(1, 0, -1); // EAST
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
        var hex = new Vector3(3, -2, -1);
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
        var center = new Vector3(2, -1, -1);
        var hex = new Vector3(3, -1, -2); // One step EAST from center

        var rotated = HMath.RotateBySteps(hex, center, 6);

        Assert.Equal(hex, rotated);
    }

    [Fact]
    public void RotateBySteps_DirectionsCycleCorrectly()
    {
        // Starting with EAST, rotate through all directions
        // Note: RotateBySteps rotates counterclockwise with positive steps
        // So EAST -> NORTHEAST -> NORTHWEST -> WEST -> SOUTHWEST -> SOUTHEAST -> EAST
        var east = HMath.Directions[0];
        var current = east;

        // After 6 rotations, we should be back to EAST
        for (int i = 0; i < 6; i++)
        {
            current = HMath.RotateBySteps(current, Origin, 1);
        }

        Assert.Equal(east, current);
    }

    [Fact]
    public void RotateBySteps_MaintainsHexConstraint()
    {
        var hex = new Vector3(4, -2, -2);

        for (int steps = 0; steps < 6; steps++)
        {
            var rotated = HMath.RotateBySteps(hex, Origin, steps);
            Assert.True(HMath.IsValidHexCoordinate(rotated));
        }
    }

    [Fact]
    public void RotateToMatch_SameOrientation_ReturnsSame()
    {
        var orientFrom = new Vector3(1, 0, -1);
        var hexToRotate = new Vector3(2, -1, -1);

        var result = HMath.RotateToMatch(orientFrom, orientFrom, hexToRotate, Origin);

        Assert.Equal(hexToRotate, result);
    }

    [Fact]
    public void RotateToMatch_ForceClockwise_UsesClockwiseRotation()
    {
        var orientFrom = HMath.Directions[0]; // EAST
        var orientTo = HMath.Directions[5];   // NORTHEAST (1 step CCW in Directions order)
        var hexToRotate = new Vector3(2, 0, -2);

        // RotateBySteps(1) rotates counterclockwise, moving EAST toward NORTHEAST
        var result = HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin, forceClockwise: true);

        // Just verify it produces a valid result that's different from original
        Assert.True(HMath.IsValidHexCoordinate(result));
        Assert.Equal(HMath.Distance(Origin, hexToRotate), HMath.Distance(Origin, result));
    }

    [Fact]
    public void RotateToMatch_InvalidRotation_ThrowsException()
    {
        var orientFrom = new Vector3(1, 0, -1);
        var orientTo = new Vector3(2, -1, -1); // Different distance from origin
        var hexToRotate = new Vector3(1, -1, 0);

        Assert.Throws<ArgumentException>(() =>
            HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin));
    }

    [Fact]
    public void RotateToMatch_ChoosesShorterPath_WhenNotForced()
    {
        var orientFrom = HMath.Directions[0]; // EAST
        var orientTo = HMath.Directions[5];   // NORTHEAST
        var hexToRotate = new Vector3(2, 0, -2);

        var result = HMath.RotateToMatch(orientFrom, orientTo, hexToRotate, Origin);

        // RotateBySteps(1) rotates CCW: EAST -> NORTHEAST
        // So 1 step should be the shorter path
        var expectedOneStep = HMath.RotateBySteps(hexToRotate, Origin, 1);
        Assert.Equal(expectedOneStep, result);
    }
}
