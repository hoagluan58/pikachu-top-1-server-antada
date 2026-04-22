using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pikachu board logic. Grid uses 1-based indexing for playable area.
/// Row 0, Row(Rows+1), Col 0, Col(Cols+1) are padding (always empty) for edge paths.
/// </summary>
public class Board
{
    public int Rows { get; private set; }
    public int Cols { get; private set; }
    public int TotalRows => Rows + 2;
    public int TotalCols => Cols + 2;

    private int[,] grid; // 0 = empty, 1..N = tile type

    public Board(int rows, int cols, int tileTypes)
    {
        Rows = rows;
        Cols = cols;
        grid = new int[TotalRows, TotalCols];
        GenerateBoard(tileTypes);
    }

    public int GetTile(int row, int col) => grid[row, col];
    public void RemoveTile(int row, int col) => grid[row, col] = 0;
    public bool IsEmpty(int row, int col) => grid[row, col] == 0;
    public bool IsInBounds(int row, int col) => row >= 0 && row < TotalRows && col >= 0 && col < TotalCols;
    public bool IsPlayableArea(int row, int col) => row >= 1 && row <= Rows && col >= 1 && col <= Cols;

    public bool IsBoardEmpty()
    {
        for (int r = 1; r <= Rows; r++)
            for (int c = 1; c <= Cols; c++)
                if (grid[r, c] != 0) return false;
        return true;
    }

    public int RemainingTiles()
    {
        int count = 0;
        for (int r = 1; r <= Rows; r++)
            for (int c = 1; c <= Cols; c++)
                if (grid[r, c] != 0) count++;
        return count;
    }

    private void GenerateBoard(int tileTypes)
    {
        System.Array.Clear(grid, 0, grid.Length);

        int totalTiles = Rows * Cols;
        int totalPairs = totalTiles / 2;
        int basePairs = totalPairs / tileTypes;
        int extraPairs = totalPairs % tileTypes;

        List<int> tiles = new List<int>(totalTiles);

        for (int t = 1; t <= tileTypes; t++)
        {
            int pairs = basePairs + (t <= extraPairs ? 1 : 0);
            for (int i = 0; i < pairs * 2; i++)
                tiles.Add(t);
        }

        ShuffleList(tiles);

        int idx = 0;
        for (int r = 1; r <= Rows; r++)
            for (int c = 1; c <= Cols; c++)
                grid[r, c] = tiles[idx++];
    }

    /// <summary>
    /// Shuffle remaining tiles in-place, preserving empty cells.
    /// </summary>
    public void Shuffle()
    {
        List<int> remaining = new List<int>();
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int r = 1; r <= Rows; r++)
        {
            for (int c = 1; c <= Cols; c++)
            {
                if (grid[r, c] != 0)
                {
                    remaining.Add(grid[r, c]);
                    positions.Add(new Vector2Int(r, c));
                }
            }
        }

        ShuffleList(remaining);

        for (int i = 0; i < positions.Count; i++)
            grid[positions[i].x, positions[i].y] = remaining[i];
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
