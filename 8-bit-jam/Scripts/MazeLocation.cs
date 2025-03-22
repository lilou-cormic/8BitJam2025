using System;

public class MazeLocation : IEquatable<MazeLocation>
{
    public int Column { get; set; }

    public int Row { get; set; }

    public MazeLocation()
        : this(0, 0)
    { }

    public MazeLocation(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public MazeLocation Clone()
    {
        return new MazeLocation(Column, Row);
    }

    public override string ToString()
    {
        return $"({Column}, {Row})";
    }

    public override bool Equals(object obj)
    {
        MazeLocation other = obj as MazeLocation;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return (Row * 7717) + Column;
    }

    public bool Equals(MazeLocation other)
    {
        return other?.Column == Column && other?.Row == Row;
    }

    public static bool operator ==(MazeLocation a, MazeLocation b)
    {
        return (a?.Equals(b)) == true;
    }

    public static bool operator !=(MazeLocation a, MazeLocation b)
    {
        return !(a == b);
    }

    public MazeLocation GetAdjacent(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                return new MazeLocation(Column + 1, Row);

            case Direction.Down:
                return new MazeLocation(Column, Row + 1);

            case Direction.Left:
                return new MazeLocation(Column - 1, Row);

            case Direction.Up:
                return new MazeLocation(Column, Row - 1);
        }

        return null;
    }
}
