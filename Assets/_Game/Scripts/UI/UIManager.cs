using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates and manages all UI elements programmatically.
/// HUD (score, timer, remaining), buttons (hint, shuffle, restart), and result panels.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private Font _cachedFont;

    private Font GetFont()
    {
        if (_cachedFont != null) return _cachedFont;
        _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_cachedFont == null) _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (_cachedFont == null) _cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
        return _cachedFont;
    }

    private Canvas canvas;
    private Text scoreText;
    private Text timerText;
    private Text remainingText;

    private GameObject winPanel;
    private Text winScoreText;

    private GameObject losePanel;

    private Button hintButton;
    private Button shuffleButton;
    private Button restartButton;

    public System.Action OnHintClicked;
    public System.Action OnShuffleClicked;
    public System.Action OnRestartClicked;

    private void Awake()
    {
        Instance = this;
        BuildUI();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void UpdateTimer(float seconds)
    {
        if (timerText != null)
        {
            int mins = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            timerText.text = $"{mins:00}:{secs:00}";

            // Flash red when low
            timerText.color = seconds < 30f ? Color.red : Color.white;
        }
    }

    public void UpdateRemaining(int count)
    {
        if (remainingText != null)
            remainingText.text = $"Tiles: {count}";
    }

    public void ShowWinPanel(int score)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winScoreText != null)
                winScoreText.text = $"Score: {score}";
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void SetButtonsInteractable(bool interactable)
    {
        if (hintButton != null) hintButton.interactable = interactable;
        if (shuffleButton != null) shuffleButton.interactable = interactable;
    }

    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("UICanvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Top bar background
        var topBar = CreatePanel(canvasGO.transform, "TopBar",
            new Color(0.12f, 0.12f, 0.18f, 0.9f),
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, -60), Vector2.zero);
        topBar.sizeDelta = new Vector2(0, 60);

        // Score
        scoreText = CreateText(topBar, "ScoreText", "Score: 0",
            new Vector2(20, -5), new Vector2(250, 50), 28, TextAnchor.MiddleLeft);

        // Timer
        timerText = CreateText(topBar, "TimerText", "05:00",
            new Vector2(-10, -5), new Vector2(150, 50), 32, TextAnchor.MiddleCenter);
        timerText.rectTransform.anchorMin = new Vector2(0.5f, 1);
        timerText.rectTransform.anchorMax = new Vector2(0.5f, 1);
        timerText.rectTransform.anchoredPosition = new Vector2(0, -30);
        timerText.fontStyle = FontStyle.Bold;

        // Remaining tiles
        remainingText = CreateText(topBar, "RemainingText", "Tiles: 96",
            new Vector2(-270, -5), new Vector2(250, 50), 28, TextAnchor.MiddleRight);
        remainingText.rectTransform.anchorMin = new Vector2(1, 1);
        remainingText.rectTransform.anchorMax = new Vector2(1, 1);
        remainingText.rectTransform.anchoredPosition = new Vector2(-20, -30);

        // Bottom bar with buttons
        var botBar = CreatePanel(canvasGO.transform, "BottomBar",
            new Color(0.12f, 0.12f, 0.18f, 0.9f),
            new Vector2(0, 0), new Vector2(1, 0),
            Vector2.zero, new Vector2(0, 60));
        botBar.sizeDelta = new Vector2(0, 60);

        // Buttons
        float btnY = 10;
        hintButton = CreateButton(botBar, "HintBtn", "💡 Hint", new Vector2(-200, btnY), new Vector2(160, 44),
            new Color(0.2f, 0.7f, 0.3f), () => OnHintClicked?.Invoke());

        shuffleButton = CreateButton(botBar, "ShuffleBtn", "🔀 Shuffle", new Vector2(0, btnY), new Vector2(160, 44),
            new Color(0.3f, 0.5f, 0.9f), () => OnShuffleClicked?.Invoke());

        restartButton = CreateButton(botBar, "RestartBtn", "🔄 Restart", new Vector2(200, btnY), new Vector2(160, 44),
            new Color(0.9f, 0.4f, 0.3f), () => OnRestartClicked?.Invoke());

        // Win Panel
        winPanel = CreateResultPanel("WinPanel", "🎉 YOU WIN! 🎉",
            new Color(0.15f, 0.6f, 0.25f), out winScoreText);
        winPanel.SetActive(false);

        // Lose Panel
        Text loseScore;
        losePanel = CreateResultPanel("LosePanel", "⏰ Time's Up!",
            new Color(0.7f, 0.2f, 0.2f), out loseScore);
        losePanel.SetActive(false);
    }

    private GameObject CreateResultPanel(string name, string title, Color headerColor, out Text scoreT)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        // Dim background
        var dimImg = panel.AddComponent<Image>();
        dimImg.color = new Color(0, 0, 0, 0.7f);

        // Center card
        var card = new GameObject("Card");
        card.transform.SetParent(panel.transform, false);
        var cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.5f, 0.5f);
        cardRT.anchorMax = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(420, 300);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.18f, 0.18f, 0.25f, 0.95f);

        // Title
        var titleT = CreateText(cardRT, "Title", title,
            new Vector2(0, 60), new Vector2(380, 60), 42, TextAnchor.MiddleCenter);
        titleT.fontStyle = FontStyle.Bold;
        titleT.color = headerColor;

        // Score
        scoreT = CreateText(cardRT, "Score", "Score: 0",
            new Vector2(0, 0), new Vector2(380, 40), 30, TextAnchor.MiddleCenter);

        // Restart button
        CreateButton(cardRT, "RestartBtn2", "Play Again", new Vector2(0, -70), new Vector2(200, 50),
            new Color(0.3f, 0.6f, 0.9f), () => OnRestartClicked?.Invoke());

        return panel;
    }

    private RectTransform CreatePanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        var img = go.AddComponent<Image>();
        img.color = color;
        return rt;
    }

    private Text CreateText(RectTransform parent, string name, string content,
        Vector2 pos, Vector2 size, int fontSize, TextAnchor alignment)
    {
        return CreateText(parent.transform, name, content, pos, size, fontSize, alignment);
    }

    private Text CreateText(Transform parent, string name, string content,
        Vector2 pos, Vector2 size, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.font = GetFont();
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        return text;
    }

    private Button CreateButton(RectTransform parent, string name, string label,
        Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        return CreateButton(parent.transform, name, label, pos, size, color, onClick);
    }

    private Button CreateButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor = color * 0.8f;
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        var text = textGO.AddComponent<Text>();
        text.text = label;
        text.fontSize = 22;
        text.font = GetFont();
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        return btn;
    }
}
