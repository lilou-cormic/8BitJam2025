public enum CellType
{
    Unknown = 0,

    LeftBorder = 1,

    TopBorder = 2,

    RightBorder = 3,

    BottomBorder = 4,

    CornerBorder = 5,

    HorizontalWall = 6,

    VerticalWall = 7,

    Intersection = 8,

    Space = 9,
}

public static class CellTypeExtensions
{
    public static bool IsBorder(this CellType cellType)
    {
        return cellType == CellType.LeftBorder || cellType == CellType.RightBorder
            || cellType == CellType.TopBorder || cellType == CellType.BottomBorder
            || cellType == CellType.CornerBorder;
    }

    public static bool IsWallOrPillar(this CellType cellType)
    {
        return cellType == CellType.VerticalWall || cellType == CellType.HorizontalWall || cellType == CellType.Intersection;
    }

    public static bool IsWalkable(this CellType cellType)
    {
        return cellType == CellType.Space;
    }
}