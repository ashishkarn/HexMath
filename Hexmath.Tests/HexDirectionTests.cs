namespace Hexmath.Tests;

public class HexDirectionTests
{
    #region ToOffset

    [Theory]
    [InlineData(HexDirection.East, 1, 0)]
    [InlineData(HexDirection.SouthEast, 1, -1)]
    [InlineData(HexDirection.SouthWest, 0, -1)]
    [InlineData(HexDirection.West, -1, 0)]
    [InlineData(HexDirection.NorthWest, -1, 1)]
    [InlineData(HexDirection.NorthEast, 0, 1)]
    public void ToOffset_ReturnsCorrectOffset(HexDirection direction, int expectedQ, int expectedR)
    {
        var offset = direction.ToOffset();

        Assert.Equal(expectedQ, offset.Q);
        Assert.Equal(expectedR, offset.R);
        Assert.Equal(-expectedQ - expectedR, offset.S);
    }

    [Theory]
    [InlineData(HexDirection.East)]
    [InlineData(HexDirection.SouthEast)]
    [InlineData(HexDirection.SouthWest)]
    [InlineData(HexDirection.West)]
    [InlineData(HexDirection.NorthWest)]
    [InlineData(HexDirection.NorthEast)]
    public void ToOffset_SatisfiesHexConstraint(HexDirection direction)
    {
        var offset = direction.ToOffset();

        Assert.Equal(0, offset.Q + offset.R + offset.S);
    }

    #endregion

    #region Next

    [Theory]
    [InlineData(HexDirection.East, HexDirection.SouthEast)]
    [InlineData(HexDirection.SouthEast, HexDirection.SouthWest)]
    [InlineData(HexDirection.SouthWest, HexDirection.West)]
    [InlineData(HexDirection.West, HexDirection.NorthWest)]
    [InlineData(HexDirection.NorthWest, HexDirection.NorthEast)]
    [InlineData(HexDirection.NorthEast, HexDirection.East)]
    public void Next_Clockwise_ReturnsNextDirection(HexDirection current, HexDirection expected)
    {
        var next = current.Next(clockwise: true);

        Assert.Equal(expected, next);
    }

    [Theory]
    [InlineData(HexDirection.East, HexDirection.NorthEast)]
    [InlineData(HexDirection.NorthEast, HexDirection.NorthWest)]
    [InlineData(HexDirection.NorthWest, HexDirection.West)]
    [InlineData(HexDirection.West, HexDirection.SouthWest)]
    [InlineData(HexDirection.SouthWest, HexDirection.SouthEast)]
    [InlineData(HexDirection.SouthEast, HexDirection.East)]
    public void Next_CounterClockwise_ReturnsPreviousDirection(HexDirection current, HexDirection expected)
    {
        var next = current.Next(clockwise: false);

        Assert.Equal(expected, next);
    }

    [Fact]
    public void Next_SixClockwiseSteps_ReturnsToStart()
    {
        var start = HexDirection.East;
        var current = start;

        for (int i = 0; i < 6; i++)
            current = current.Next(clockwise: true);

        Assert.Equal(start, current);
    }

    [Fact]
    public void Next_SixCounterClockwiseSteps_ReturnsToStart()
    {
        var start = HexDirection.West;
        var current = start;

        for (int i = 0; i < 6; i++)
            current = current.Next(clockwise: false);

        Assert.Equal(start, current);
    }

    #endregion

    #region Opposite

    [Theory]
    [InlineData(HexDirection.East, HexDirection.West)]
    [InlineData(HexDirection.SouthEast, HexDirection.NorthWest)]
    [InlineData(HexDirection.SouthWest, HexDirection.NorthEast)]
    [InlineData(HexDirection.West, HexDirection.East)]
    [InlineData(HexDirection.NorthWest, HexDirection.SouthEast)]
    [InlineData(HexDirection.NorthEast, HexDirection.SouthWest)]
    public void Opposite_ReturnsOppositeDirection(HexDirection direction, HexDirection expected)
    {
        var opposite = direction.Opposite();

        Assert.Equal(expected, opposite);
    }

    [Theory]
    [InlineData(HexDirection.East)]
    [InlineData(HexDirection.SouthEast)]
    [InlineData(HexDirection.SouthWest)]
    [InlineData(HexDirection.West)]
    [InlineData(HexDirection.NorthWest)]
    [InlineData(HexDirection.NorthEast)]
    public void Opposite_DoubleOpposite_ReturnsSame(HexDirection direction)
    {
        var doubleOpposite = direction.Opposite().Opposite();

        Assert.Equal(direction, doubleOpposite);
    }

    [Theory]
    [InlineData(HexDirection.East)]
    [InlineData(HexDirection.SouthEast)]
    [InlineData(HexDirection.SouthWest)]
    [InlineData(HexDirection.West)]
    [InlineData(HexDirection.NorthWest)]
    [InlineData(HexDirection.NorthEast)]
    public void Opposite_Offsets_AreNegatives(HexDirection direction)
    {
        var offset = direction.ToOffset();
        var oppositeOffset = direction.Opposite().ToOffset();

        Assert.Equal(-offset.Q, oppositeOffset.Q);
        Assert.Equal(-offset.R, oppositeOffset.R);
    }

    #endregion

    #region ToVector3

    [Theory]
    [InlineData(HexDirection.East)]
    [InlineData(HexDirection.SouthEast)]
    [InlineData(HexDirection.SouthWest)]
    [InlineData(HexDirection.West)]
    [InlineData(HexDirection.NorthWest)]
    [InlineData(HexDirection.NorthEast)]
    public void ToVector3_MatchesToOffset(HexDirection direction)
    {
        var offset = direction.ToOffset();
        var vec = direction.ToVector3();

        Assert.Equal(offset.Q, vec.X);
        Assert.Equal(offset.R, vec.Y);
        Assert.Equal(offset.S, vec.Z);
    }

    #endregion

    #region HMath.NextDirection

    [Fact]
    public void NextDirection_WrapsExtensionMethod()
    {
        var direction = HexDirection.SouthWest;

        var resultCW = HMath.NextDirection(direction, clockwise: true);
        var resultCCW = HMath.NextDirection(direction, clockwise: false);

        Assert.Equal(HexDirection.West, resultCW);
        Assert.Equal(HexDirection.SouthEast, resultCCW);
    }

    #endregion

    #region HMath.AllDirections

    [Fact]
    public void AllDirections_ContainsSixDirections()
    {
        Assert.Equal(6, HMath.AllDirections.Count);
    }

    [Fact]
    public void AllDirections_ContainsAllEnumValues()
    {
        var all = HMath.AllDirections;

        Assert.Contains(HexDirection.East, all);
        Assert.Contains(HexDirection.SouthEast, all);
        Assert.Contains(HexDirection.SouthWest, all);
        Assert.Contains(HexDirection.West, all);
        Assert.Contains(HexDirection.NorthWest, all);
        Assert.Contains(HexDirection.NorthEast, all);
    }

    [Fact]
    public void AllDirections_InClockwiseOrder()
    {
        var all = HMath.AllDirections;

        Assert.Equal(HexDirection.East, all[0]);
        Assert.Equal(HexDirection.SouthEast, all[1]);
        Assert.Equal(HexDirection.SouthWest, all[2]);
        Assert.Equal(HexDirection.West, all[3]);
        Assert.Equal(HexDirection.NorthWest, all[4]);
        Assert.Equal(HexDirection.NorthEast, all[5]);
    }

    #endregion
}
