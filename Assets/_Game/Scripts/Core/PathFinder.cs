using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Finds connecting paths between matching tiles with at most 2 bends (3 segments).
/// Supports paths through the padding area (outside board edges).
/// </summary>
public static class PathFinder
{
    /// <summary>
    /// Find a path between two tiles. Returns list of waypoints or null if no path exists.
    /// Tiles must be same type and non-empty.
    /// </summary>
    public static List<Vector2Int> FindPath(Board board, Vector2Int from, Vector2Int to)
    {
        if (from == to) return null;
        if (board.GetTile(from.x, from.y) == 0 || board.GetTile(to.x, to.y) == 0) return null;
        if (board.GetTile(from.x, from.y) != board.GetTile(to.x, to.y)) return null;

        // 0 bends - straight line
        if (IsLineEmpty(board, from, to))
            return new List<Vector2Int> { from, to };

        // 1 bend - L shape
        var path1 = TryOneBend(board, from, to);
        if (path1 != null) return path1;

        // 2 bends - scan all rows
        for (int r = 0; r < board.TotalRows; r++)
        {
            Vector2Int p1 = new Vector2Int(r, from.y);
            Vector2Int p2 = new Vector2Int(r, to.y);

            bool p1ok = (p1 == from) || board.IsEmpty(p1.x, p1.y);
            bool p2ok = (p2 == to) || board.IsEmpty(p2.x, p2.y);

            if (p1ok && p2ok &&
                IsLineEmpty(board, from, p1) &&
                IsLineEmpty(board, p1, p2) &&
                IsLineEmpty(board, p2, to))
            {
                var path = new List<Vector2Int> { from };
                if (p1 != from) path.Add(p1);
                if (p2 != to) path.Add(p2);
                path.Add(to);
                return path;
            }
        }

        // 2 bends - scan all cols
        for (int c = 0; c < board.TotalCols; c++)
        {
            Vector2Int p1 = new Vector2Int(from.x, c);
            Vector2Int p2 = new Vector2Int(to.x, c);

            bool p1ok = (p1 == from) || board.IsEmpty(p1.x, p1.y);
            bool p2ok = (p2 == to) || board.IsEmpty(p2.x, p2.y);

            if (p1ok && p2ok &&
                IsLineEmpty(board, from, p1) &&
                IsLineEmpty(board, p1, p2) &&
                IsLineEmpty(board, p2, to))
            {
                var path = new List<Vector2Int> { from };
                if (p1 != from) path.Add(p1);
                if (p2 != to) path.Add(p2);
                path.Add(to);
                return path;
            }
        }

        return null;
    }

    private static List<Vector2Int> TryOneBend(Board board, Vector2Int from, Vector2Int to)
    {
        // Corner at (from.row, to.col)
        Vector2Int c1 = new Vector2Int(from.x, to.y);
        if (board.IsInBounds(c1.x, c1.y) && board.IsEmpty(c1.x, c1.y) &&
            IsLineEmpty(board, from, c1) && IsLineEmpty(board, c1, to))
            return new List<Vector2Int> { from, c1, to };

        // Corner at (to.row, from.col)
        Vector2Int c2 = new Vector2Int(to.x, from.y);
        if (board.IsInBounds(c2.x, c2.y) && board.IsEmpty(c2.x, c2.y) &&
            IsLineEmpty(board, from, c2) && IsLineEmpty(board, c2, to))
            return new List<Vector2Int> { from, c2, to };

        return null;
    }

    /// <summary>
    /// Check if all cells strictly between a and b are empty.
    /// a and b must be on the same row or column.
    /// </summary>
    private static bool IsLineEmpty(Board board, Vector2Int a, Vector2Int b)
    {
        if (a == b) return true;

        if (a.x == b.x) // same row
        {
            int minC = Mathf.Min(a.y, b.y);
            int maxC = Mathf.Max(a.y, b.y);
            for (int c = minC + 1; c < maxC; c++)
            {
                if (!board.IsInBounds(a.x, c) || !board.IsEmpty(a.x, c))
                    return false;
            }
            return true;
        }

        if (a.y == b.y) // same col
        {
            int minR = Mathf.Min(a.x, b.x);
            int maxR = Mathf.Max(a.x, b.x);
            for (int r = minR + 1; r < maxR; r++)
            {
                if (!board.IsInBounds(r, a.y) || !board.IsEmpty(r, a.y))
                    return false;
            }
            return true;
        }

        return false; // not aligned
    }

    /// <summary>
    /// Find any valid matching pair on the board (for hint feature).
    /// </summary>
    public static (Vector2Int from, Vector2Int to)? FindHint(Board board)
    {
        var tiles = new List<Vector2Int>();
        for (int r = 1; r <= board.Rows; r++)
            for (int c = 1; c <= board.Cols; c++)
                if (board.GetTile(r, c) != 0)
                    tiles.Add(new Vector2Int(r, c));

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                if (board.GetTile(tiles[i].x, tiles[i].y) == board.GetTile(tiles[j].x, tiles[j].y))
                {
                    if (FindPath(board, tiles[i], tiles[j]) != null)
                        return (tiles[i], tiles[j]);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Check if there's at least one valid move remaining.
    /// </summary>
    public static bool HasValidMoves(Board board)
    {
        return FindHint(board) != null;
    }
}
