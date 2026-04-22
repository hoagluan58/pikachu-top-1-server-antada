using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the visual representation of the board (3D version).
/// Tiles are placed on the XZ plane. Y is up.
/// Creates/destroys TileView GameObjects and maps board coords to world positions.
/// </summary>
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    private Board board;
    private TileView[,] tileViews;
    private Material[] iconMaterials;
    private Material tileMaterial;
    private Transform boardContainer;

    public float CellSize { get; private set; } = 1.0f;
    public Board Board => board;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(int rows, int cols, int tileTypes)
    {
        // Generate 3D materials
        iconMaterials = SpriteGenerator.GenerateIconMaterials(tileTypes);
        tileMaterial = SpriteGenerator.GenerateTileMaterial();

        // Create board logic
        board = new Board(rows, cols, tileTypes);

        // Container
        if (boardContainer != null)
            Destroy(boardContainer.gameObject);

        boardContainer = new GameObject("BoardContainer").transform;
        boardContainer.SetParent(transform, false);

        // Create tile views
        tileViews = new TileView[board.TotalRows, board.TotalCols];
        for (int r = 1; r <= board.Rows; r++)
        {
            for (int c = 1; c <= board.Cols; c++)
            {
                int tileType = board.GetTile(r, c);
                if (tileType != 0)
                    CreateTileView(r, c, tileType);
            }
        }

        // Center the board
        CenterBoard();
    }

    /// <summary>Convert board (row, col) to world position on XZ plane.</summary>
    public Vector3 BoardToWorld(int row, int col)
    {
        float x = col * CellSize;
        float z = -row * CellSize;
        return boardContainer.position + new Vector3(x, 0f, z);
    }

    /// <summary>Convert board (row, col) to world position for Vector2Int.</summary>
    public Vector3 BoardToWorld(Vector2Int pos) => BoardToWorld(pos.x, pos.y);

    public TileView GetTileView(int row, int col)
    {
        if (board.IsPlayableArea(row, col))
            return tileViews[row, col];
        return null;
    }

    /// <summary>Remove a matched tile from the board and view.</summary>
    public void RemoveTile(int row, int col)
    {
        board.RemoveTile(row, col);
        if (tileViews[row, col] != null)
        {
            tileViews[row, col].AnimateRemove();
            tileViews[row, col] = null;
        }
    }

    /// <summary>Draw a connecting line path (slightly above tiles).</summary>
    public void DrawLinePath(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;

        List<Vector3> worldPoints = new List<Vector3>();
        foreach (var p in path)
        {
            Vector3 wp = BoardToWorld(p);
            wp.y = 0.12f; // slightly above tile surface
            worldPoints.Add(wp);
        }

        var lineGO = new GameObject("LinePath");
        var line = lineGO.AddComponent<LinePath>();
        line.Init(worldPoints);
    }

    /// <summary>Shuffle the board and recreate visuals.</summary>
    public void ShuffleBoard()
    {
        board.Shuffle();
        RebuildVisuals();
    }

    /// <summary>Rebuild all tile views from current board state.</summary>
    public void RebuildVisuals()
    {
        // Destroy existing views
        for (int r = 1; r <= board.Rows; r++)
        {
            for (int c = 1; c <= board.Cols; c++)
            {
                if (tileViews[r, c] != null)
                {
                    Destroy(tileViews[r, c].gameObject);
                    tileViews[r, c] = null;
                }
            }
        }

        // Recreate
        for (int r = 1; r <= board.Rows; r++)
        {
            for (int c = 1; c <= board.Cols; c++)
            {
                int tileType = board.GetTile(r, c);
                if (tileType != 0)
                    CreateTileView(r, c, tileType);
            }
        }
    }

    /// <summary>Full restart with a new board.</summary>
    public void Restart(int rows, int cols, int tileTypes)
    {
        Init(rows, cols, tileTypes);
    }

    private void CreateTileView(int row, int col, int tileType)
    {
        var go = new GameObject($"Tile_{row}_{col}");
        go.transform.SetParent(boardContainer, false);
        go.transform.localPosition = new Vector3(col * CellSize, 0f, -row * CellSize);

        var tv = go.AddComponent<TileView>();
        tv.Init(row, col, tileType, tileMaterial, iconMaterials[tileType - 1]);
        tileViews[row, col] = tv;
    }

    private void CenterBoard()
    {
        // Center so the board middle is at (0, 0, 0)
        float midX = (1 + board.Cols) * 0.5f * CellSize;
        float midZ = -(1 + board.Rows) * 0.5f * CellSize;
        boardContainer.position = new Vector3(-midX, 0f, -midZ);
    }
}
