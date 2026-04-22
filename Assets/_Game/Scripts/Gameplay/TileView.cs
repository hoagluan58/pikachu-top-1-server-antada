using UnityEngine;

/// <summary>
/// Visual representation of a single tile on the board (3D version).
/// Uses a 3D cube for the tile body and a quad for the icon on top.
/// Handles rendering, selection highlight, and click detection.
/// </summary>
public class TileView : MonoBehaviour
{
    public int Row { get; private set; }
    public int Col { get; private set; }
    public int TileType { get; private set; }

    private MeshRenderer bodyRenderer;
    private MeshRenderer iconRenderer;
    private Material bodyMaterial;
    private Material bodyOriginalMaterial;
    private Color originalColor;
    private bool isSelected;

    // 3D tile dimensions
    private const float TILE_WIDTH = 0.88f;
    private const float TILE_HEIGHT = 0.15f;
    private const float TILE_DEPTH = 0.88f;
    private const float ICON_SIZE = 0.65f;

    public void Init(int row, int col, int tileType, Material tileMat, Material iconMat)
    {
        Row = row;
        Col = col;
        TileType = tileType;

        // ── Tile Body (flattened cube) ──
        var bodyGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bodyGO.name = "Body";
        bodyGO.transform.SetParent(transform, false);
        bodyGO.transform.localScale = new Vector3(TILE_WIDTH, TILE_HEIGHT, TILE_DEPTH);
        bodyGO.transform.localPosition = Vector3.zero;

        bodyRenderer = bodyGO.GetComponent<MeshRenderer>();
        bodyMaterial = new Material(tileMat);
        bodyRenderer.material = bodyMaterial;
        originalColor = bodyMaterial.color;

        // Remove the body's collider (we use one on parent)
        var bodyCol = bodyGO.GetComponent<Collider>();
        if (bodyCol != null) Object.Destroy(bodyCol);

        // ── Icon Quad (on top of tile) ──
        var iconGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        iconGO.name = "Icon";
        iconGO.transform.SetParent(transform, false);
        iconGO.transform.localPosition = new Vector3(0f, TILE_HEIGHT / 2f + 0.001f, 0f);
        iconGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // face up
        iconGO.transform.localScale = Vector3.one * ICON_SIZE;

        iconRenderer = iconGO.GetComponent<MeshRenderer>();
        iconRenderer.material = iconMat;

        // Remove the icon's collider
        var iconCol = iconGO.GetComponent<Collider>();
        if (iconCol != null) Object.Destroy(iconCol);

        // ── Main Collider (on parent GO for click detection) ──
        var boxCol = gameObject.AddComponent<BoxCollider>();
        boxCol.size = new Vector3(TILE_WIDTH, TILE_HEIGHT + 0.05f, TILE_DEPTH);
        boxCol.center = new Vector3(0f, 0f, 0f);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (bodyMaterial != null)
        {
            if (selected)
            {
                // Bright green glow
                bodyMaterial.color = new Color(0.3f, 0.9f, 0.4f);
                bodyMaterial.EnableKeyword("_EMISSION");
                bodyMaterial.SetColor("_EmissionColor", new Color(0.1f, 0.5f, 0.15f) * 1.5f);
            }
            else
            {
                bodyMaterial.color = originalColor;
                bodyMaterial.DisableKeyword("_EMISSION");
                bodyMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    public void SetHintHighlight(bool on)
    {
        if (bodyMaterial != null)
        {
            if (on)
            {
                // Golden hint glow
                bodyMaterial.color = new Color(1f, 0.85f, 0.3f);
                bodyMaterial.EnableKeyword("_EMISSION");
                bodyMaterial.SetColor("_EmissionColor", new Color(0.6f, 0.45f, 0.05f) * 1.2f);
            }
            else if (!isSelected)
            {
                bodyMaterial.color = originalColor;
                bodyMaterial.DisableKeyword("_EMISSION");
                bodyMaterial.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    /// <summary>
    /// Animate tile removal (scale down + sink + destroy).
    /// </summary>
    public void AnimateRemove(float duration = 0.3f)
    {
        StartCoroutine(RemoveCoroutine(duration));
    }

    private System.Collections.IEnumerator RemoveCoroutine(float duration)
    {
        float elapsed = 0;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.localPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = t * t; // ease-in
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
            transform.localPosition = startPos + Vector3.down * (eased * 0.3f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
