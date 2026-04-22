using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Main game controller. Handles game flow, input, scoring, and timer.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    private int rows = 8;
    private int cols = 12;
    private int tileTypes = 18;
    private float totalTime = 300f; // 5 minutes

    private enum GameState { Playing, Won, Lost }
    private GameState state;

    private int score;
    private float timeRemaining;
    private TileView selectedTile;
    private TileView hintTile1, hintTile2;
    private bool inputLocked;

    private BoardManager boardManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        boardManager = GetComponent<BoardManager>();
        SetupUI();
        StartNewGame();
    }

    private void Update()
    {
        if (state != GameState.Playing) return;

        // Timer
        timeRemaining -= Time.deltaTime;
        UIManager.Instance.UpdateTimer(timeRemaining);

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            state = GameState.Lost;
            UIManager.Instance.ShowLosePanel();
            UIManager.Instance.SetButtonsInteractable(false);
            return;
        }

        // Input (New Input System — supports both mouse and touch)
        if (!inputLocked)
        {
            var mouse = Mouse.current;
            var touch = Touchscreen.current;

            bool clicked = (mouse != null && mouse.leftButton.wasPressedThisFrame);
            bool tapped = (touch != null && touch.primaryTouch.press.wasPressedThisFrame);

            if (clicked || tapped)
                HandleClick();
        }
    }

    private void SetupUI()
    {
        var ui = UIManager.Instance;
        ui.OnHintClicked = OnHint;
        ui.OnShuffleClicked = OnShuffle;
        ui.OnRestartClicked = () => StartNewGame();
    }

    public void StartNewGame()
    {
        state = GameState.Playing;
        score = 0;
        timeRemaining = totalTime;
        selectedTile = null;
        hintTile1 = hintTile2 = null;
        inputLocked = false;

        boardManager.Init(rows, cols, tileTypes);

        // Ensure solvable
        EnsureSolvable();

        var ui = UIManager.Instance;
        ui.HideAllPanels();
        ui.UpdateScore(score);
        ui.UpdateTimer(timeRemaining);
        ui.UpdateRemaining(boardManager.Board.RemainingTiles());
        ui.SetButtonsInteractable(true);
    }

    private void HandleClick()
    {
        // Check if clicking on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Get pointer position (mouse or touch)
        Vector2 pointerPos;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            pointerPos = Mouse.current.position.ReadValue();
        else
            return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(pointerPos.x, pointerPos.y, 0));

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

        TileView clickedTile = hit.collider.GetComponent<TileView>();
        if (clickedTile == null) return;

        ClearHintHighlight();
        AudioManager.Instance.PlayClick();

        if (selectedTile == null)
        {
            // First selection
            selectedTile = clickedTile;
            selectedTile.SetSelected(true);
        }
        else if (selectedTile == clickedTile)
        {
            // Deselect
            selectedTile.SetSelected(false);
            selectedTile = null;
        }
        else
        {
            // Second selection - try match
            TryMatch(selectedTile, clickedTile);
        }
    }

    private void TryMatch(TileView tile1, TileView tile2)
    {
        Vector2Int from = new Vector2Int(tile1.Row, tile1.Col);
        Vector2Int to = new Vector2Int(tile2.Row, tile2.Col);

        List<Vector2Int> path = PathFinder.FindPath(boardManager.Board, from, to);

        if (path != null)
        {
            // Match found!
            inputLocked = true;
            tile1.SetSelected(false);

            // Draw line
            boardManager.DrawLinePath(path);

            // Remove tiles
            boardManager.RemoveTile(tile1.Row, tile1.Col);
            boardManager.RemoveTile(tile2.Row, tile2.Col);

            // Score
            score += 10;
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateRemaining(boardManager.Board.RemainingTiles());

            AudioManager.Instance.PlayMatch();

            selectedTile = null;

            // Check win
            if (boardManager.Board.IsBoardEmpty())
            {
                state = GameState.Won;
                UIManager.Instance.ShowWinPanel(score);
                UIManager.Instance.SetButtonsInteractable(false);
                AudioManager.Instance.PlayWin();
                return;
            }

            // Check if moves available (delayed to allow animation)
            Invoke(nameof(CheckMovesAvailable), 0.3f);
            Invoke(nameof(UnlockInput), 0.3f);
        }
        else
        {
            // No match
            tile1.SetSelected(false);
            tile2.SetSelected(false);
            selectedTile = null;
            AudioManager.Instance.PlayNoMatch();
        }
    }

    private void UnlockInput()
    {
        inputLocked = false;
    }

    private void CheckMovesAvailable()
    {
        if (state != GameState.Playing) return;

        if (!PathFinder.HasValidMoves(boardManager.Board))
        {
            Debug.Log("No valid moves! Auto-shuffling...");
            boardManager.ShuffleBoard();
            EnsureSolvable();
        }
    }

    private void EnsureSolvable()
    {
        int maxAttempts = 100;
        int attempts = 0;
        while (!PathFinder.HasValidMoves(boardManager.Board) && attempts < maxAttempts)
        {
            boardManager.ShuffleBoard();
            attempts++;
        }
        if (attempts >= maxAttempts)
            Debug.LogWarning("Could not find solvable board after max attempts!");
    }

    private void OnHint()
    {
        if (state != GameState.Playing) return;

        ClearHintHighlight();

        var hint = PathFinder.FindHint(boardManager.Board);
        if (hint.HasValue)
        {
            score = Mathf.Max(0, score - 10); // penalty
            UIManager.Instance.UpdateScore(score);

            hintTile1 = boardManager.GetTileView(hint.Value.from.x, hint.Value.from.y);
            hintTile2 = boardManager.GetTileView(hint.Value.to.x, hint.Value.to.y);

            if (hintTile1 != null) hintTile1.SetHintHighlight(true);
            if (hintTile2 != null) hintTile2.SetHintHighlight(true);
        }
    }

    private void OnShuffle()
    {
        if (state != GameState.Playing) return;

        ClearSelection();
        ClearHintHighlight();

        score = Mathf.Max(0, score - 20); // penalty
        UIManager.Instance.UpdateScore(score);

        boardManager.ShuffleBoard();
        EnsureSolvable();
    }

    private void ClearSelection()
    {
        if (selectedTile != null)
        {
            selectedTile.SetSelected(false);
            selectedTile = null;
        }
    }

    private void ClearHintHighlight()
    {
        if (hintTile1 != null) { hintTile1.SetHintHighlight(false); hintTile1 = null; }
        if (hintTile2 != null) { hintTile2.SetHintHighlight(false); hintTile2 = null; }
    }
}
