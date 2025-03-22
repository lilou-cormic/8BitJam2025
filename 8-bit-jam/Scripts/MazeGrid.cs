using Godot;
using System.Text;

public class MazeGrid
{
    private const int width = 8;
    private const int height = 6;

    private MazeCell[,] MazeCells { get; }

    public const int RowCount = (height * 2) + 1;

    public const int ColumnCount = (width * 2) + 1;

    public MazeLocation Entrance { get; }

    public MazeLocation PlayerStartLocation { get; private set; }

    private MazeGrid(bool[,] verticalWalls, bool[,] horizontalWalls)
    {
        Entrance = new MazeLocation(ColumnCount - 4, RowCount - 1);

        MazeCells = new MazeCell[ColumnCount, RowCount];

        AddBorders();
        AddIntersections();
        AddVerticalWalls(verticalWalls);
        AddHorizontalWalls(horizontalWalls);
        AddSpaces();

        PlayerStartLocation = new MazeLocation(2, 2);

        #region MazeString

        StringBuilder mazeString = new StringBuilder();

        for (int row = 0; row < MazeCells.GetLength(1); row++)
        {
            for (int col = 0; col < MazeCells.GetLength(0); col++)
            {
                switch (MazeCells[col, row].CellType)
                {
                    case CellType.LeftBorder:
                        mazeString.Append("██");
                        break;
                    case CellType.TopBorder:
                        mazeString.Append("██");
                        break;
                    case CellType.RightBorder:
                        mazeString.Append("██");
                        break;
                    case CellType.BottomBorder:
                        mazeString.Append("██");
                        break;
                    case CellType.CornerBorder:
                        mazeString.Append("██");
                        break;
                    case CellType.HorizontalWall:
                        mazeString.Append("██");
                        break;
                    case CellType.VerticalWall:
                        mazeString.Append("██");
                        break;
                    case CellType.Intersection:
                        mazeString.Append("██");
                        break;
                    case CellType.Space:
                        mazeString.Append("  ");
                        break;
                    default:
                        mazeString.Append("  ");
                        break;
                }
            }
            mazeString.AppendLine();
        }

        GD.Print(mazeString.ToString());
        #endregion
    }

    public static MazeGrid CreateMaze()
    {
        bool[,] verticalWalls = CreateVerticalWalls(width, height);
        bool[,] horizontalWalls = CreateHorizontalWalls(width, height);
        int[] sets = CreateSets(width, height);

        Direction direction = 0;
        int cellIndex = 0;

        while (!AreAllSameSet(sets))
        {
            while (true)
            {
                cellIndex = GD.RandRange(0, sets.Length - 1);
                direction = DirectionExtensions.GetRandomDirection();

                if (!IsValidDirection(direction, cellIndex, width, height))
                    continue;

                GetSets(direction, sets, cellIndex, width, out int classe1, out int classe2);

                if (classe1 == classe2)
                    continue;

                RemoveWall(direction, verticalWalls, horizontalWalls, cellIndex, width);

                UnionSets(sets, classe1, classe2);

                break;
            }
        }

        return new MazeGrid(verticalWalls, horizontalWalls);
    }

    private void AddBorders()
    {
        int minRow = 0;
        int maxRow = RowCount - 1;
        int minColumn = 0;
        int maxColumn = ColumnCount - 1;

        for (int row = minRow; row <= maxRow; row++)
        {
            MazeCells[minColumn, row] = new MazeCell(CellType.LeftBorder);
            MazeCells[maxColumn, row] = new MazeCell(CellType.RightBorder);
        }

        for (int column = minColumn; column <= maxColumn; column++)
        {
            MazeCells[column, minRow] = new MazeCell(CellType.TopBorder);
            MazeCells[column, maxRow] = new MazeCell(CellType.BottomBorder);
        }

        MazeCells[minColumn, minRow] = new MazeCell(CellType.CornerBorder);
        MazeCells[minColumn, maxRow] = new MazeCell(CellType.CornerBorder);
        MazeCells[maxColumn, minRow] = new MazeCell(CellType.CornerBorder);
        MazeCells[maxColumn, maxRow] = new MazeCell(CellType.CornerBorder);
    }

    private void AddIntersections()
    {
        for (int column = 2; column < ColumnCount - 1; column += 2)
        {
            for (int row = 2; row < RowCount - 1; row += 2)
            {
                MazeCells[column, row] = new MazeCell(CellType.Intersection);
            }
        }
    }

    private void AddVerticalWalls(bool[,] verticalWalls)
    {
        for (int wallColumn = 0, column = 2; wallColumn < verticalWalls.GetLength(0); wallColumn++, column += 2)
        {
            for (int wallRow = 0, row = 1; wallRow < verticalWalls.GetLength(1); wallRow++, row += 2)
            {
                MazeCells[column, row] = new MazeCell((verticalWalls[wallColumn, wallRow] ? CellType.VerticalWall : CellType.Space));
            }
        }
    }

    private void AddHorizontalWalls(bool[,] horizontalWalls)
    {
        for (int wallColumn = 0, column = 1; wallColumn < horizontalWalls.GetLength(0); wallColumn++, column += 2)
        {
            for (int wallRow = 0, row = 2; wallRow < horizontalWalls.GetLength(1); wallRow++, row += 2)
            {
                MazeCells[column, row] = new MazeCell((horizontalWalls[wallColumn, wallRow] ? CellType.HorizontalWall : CellType.Space));
            }
        }
    }

    private void AddSpaces()
    {
        for (int column = 1; column < ColumnCount; column += 2)
        {
            for (int row = 1; row < RowCount; row += 2)
            {
                MazeCells[column, row] = new MazeCell(CellType.Space);
            }
        }

        for (int column = 1; column <= 3; column++)
        {
            for (int row = 1; row <= 3; row++)
            {
                MazeCells[column, row] = new MazeCell(CellType.Space);
            }
        }

        MazeCells[2, 2] = new MazeCell(CellType.Space);

        MazeCells[Entrance.Column, Entrance.Row] = new MazeCell(CellType.Space);
    }

    public MazeCell GetCell(MazeLocation location)
    {
        return MazeCells[location.Column, location.Row];
    }

    public MazeCell GetCell(int column, int row)
    {
        return MazeCells[column, row];
    }

    public bool CanMove(MazeLocation currentLocation, Direction direction)
    {
        MazeLocation adjacentLocation = currentLocation.GetAdjacent(direction);

        if (adjacentLocation == null)
            return false;

        switch (direction)
        {
            case Direction.Right:
                return currentLocation.Column < ColumnCount - 1 && GetCell(adjacentLocation).IsWalkable;

            case Direction.Down:
                return currentLocation.Row < RowCount - 1 && GetCell(adjacentLocation).IsWalkable;

            case Direction.Left:
                return currentLocation.Column > 0 && GetCell(adjacentLocation).IsWalkable;

            case Direction.Up:
                return currentLocation.Row > 0 && GetCell(adjacentLocation).IsWalkable;

            case Direction.None:
            default:
                return false;
        }
    }

    private static bool[,] CreateVerticalWalls(int width, int height)
    {
        bool[,] outVerticalWalls = new bool[width - 1, height];

        for (int column = 0; column < outVerticalWalls.GetLength(0); column++)
        {
            for (int row = 0; row < outVerticalWalls.GetLength(1); row++)
            {
                outVerticalWalls[column, row] = true;
            }
        }

        return outVerticalWalls;
    }


    private static bool[,] CreateHorizontalWalls(int width, int height)
    {
        bool[,] outHorizontalWalls = new bool[width, height - 1];

        for (int column = 0; column < outHorizontalWalls.GetLength(0); column++)
        {
            for (int row = 0; row < outHorizontalWalls.GetLength(1); row++)
            {
                outHorizontalWalls[column, row] = true;
            }
        }

        return outHorizontalWalls;
    }

    private static bool IsValidDirection(Direction direction, int cellIndex, int width, int height)
    {
        switch (direction)
        {
            case Direction.Right:
                return (cellIndex + 1) % width > 0;

            case Direction.Down:
                return cellIndex + width <= (width * height) - 1;

            case Direction.Left:
                return cellIndex % width > 0;

            case Direction.Up:
                return cellIndex - width >= 0;
        }

        return false;
    }

    public MazeLocation GetRandomLocation()
    {
        int row;
        int column;

        do
        {
            column = GD.RandRange(1, ColumnCount - 1 - 1);
            row = GD.RandRange(1, RowCount - 1 - 1);
        } while (!GetCell(column, row).IsWalkable);

        return new MazeLocation(column, row);
    }

    private static int[] CreateSets(int width, int height)
    {
        int[] outSets = new int[width * height];

        for (int index = 0; index < outSets.Length; index++)
        {
            outSets[index] = -1;
        }

        return outSets;
    }

    private static bool AreAllSameSet(int[] sets)
    {
        for (int index = 1; index < sets.Length; index++)
        {
            if (FindSet(sets, index) != FindSet(sets, index - 1))
                return false;
        }

        return true;
    }

    private static void GetSets(Direction direction, int[] sets, int cellIndex, int width, out int set1, out int set2)
    {
        set1 = FindSet(sets, cellIndex);

        switch (direction)
        {
            case Direction.Right:
                set2 = FindSet(sets, cellIndex + 1);
                break;

            case Direction.Down:
                set2 = FindSet(sets, cellIndex + width);
                break;

            case Direction.Left:
                set2 = FindSet(sets, cellIndex - 1);
                break;

            case Direction.Up:
                set2 = FindSet(sets, cellIndex - width);
                break;

            default:
                set2 = FindSet(sets, cellIndex);
                break;
        }
    }

    private static void RemoveWall(Direction direction, bool[,] verticalWalls, bool[,] horizontalWalls, int cellIndex, int width)
    {
        int row = cellIndex / width;
        int column = cellIndex % width;

        switch (direction)
        {
            case Direction.Right:
                verticalWalls[column, row] = false;
                break;

            case Direction.Down:
                horizontalWalls[column, row] = false;
                break;

            case Direction.Left:
                verticalWalls[column - 1, row] = false;
                break;

            case Direction.Up:
                horizontalWalls[column, row - 1] = false;
                break;
        }
    }

    private static int FindSet(int[] sets, int cellIndex)
    {
        if (sets[cellIndex] < 0)
            return cellIndex;
        else
            return FindSet(sets, sets[cellIndex]);
    }

    private static void UnionSets(int[] sets, int set1, int set2)
    {
        if (sets[set1] > sets[set2])
        {
            sets[set2] += sets[set1];
            sets[set1] = set2;
        }
        else
        {
            sets[set1] += sets[set2];
            sets[set2] = set1;
        }
    }
}
