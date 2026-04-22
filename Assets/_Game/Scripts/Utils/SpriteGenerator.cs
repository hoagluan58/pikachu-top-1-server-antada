using UnityEngine;

/// <summary>
/// Generates tile icon sprites procedurally using Texture2D.
/// Each icon is a distinct geometric shape with a unique color.
/// </summary>
public static class SpriteGenerator
{
    private static readonly Color[] IconColors = new Color[]
    {
        new Color(0.93f, 0.26f, 0.26f), // 0  Red
        new Color(0.26f, 0.52f, 0.96f), // 1  Blue
        new Color(0.30f, 0.85f, 0.40f), // 2  Green
        new Color(0.98f, 0.80f, 0.18f), // 3  Yellow
        new Color(0.96f, 0.55f, 0.15f), // 4  Orange
        new Color(0.91f, 0.40f, 0.70f), // 5  Pink
        new Color(0.61f, 0.35f, 0.85f), // 6  Purple
        new Color(0.15f, 0.80f, 0.78f), // 7  Teal
        new Color(0.65f, 0.45f, 0.25f), // 8  Brown
        new Color(0.30f, 0.85f, 0.85f), // 9  Cyan
        new Color(0.85f, 0.25f, 0.60f), // 10 Magenta
        new Color(0.35f, 0.40f, 0.85f), // 11 Indigo
        new Color(0.95f, 0.75f, 0.10f), // 12 Gold
        new Color(0.80f, 0.15f, 0.20f), // 13 Crimson
        new Color(0.40f, 0.75f, 0.95f), // 14 Sky Blue
        new Color(0.55f, 0.90f, 0.25f), // 15 Lime
        new Color(0.95f, 0.50f, 0.45f), // 16 Coral
        new Color(0.58f, 0.28f, 0.75f), // 17 Violet
    };

    private const int TEX_SIZE = 64;
    private const float PPU = 64f;

    /// <summary>Generate an array of distinct icon sprites (kept for compatibility).</summary>
    public static Sprite[] GenerateIcons(int count)
    {
        Sprite[] sprites = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            Texture2D tex = CreateIconTexture(i);
            sprites[i] = Sprite.Create(tex, new Rect(0, 0, TEX_SIZE, TEX_SIZE), Vector2.one * 0.5f, PPU);
        }
        return sprites;
    }

    /// <summary>Generate Materials for 3D icon quads (Unlit with alpha cutout).</summary>
    public static Material[] GenerateIconMaterials(int count)
    {
        Material[] mats = new Material[count];
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");

        for (int i = 0; i < count; i++)
        {
            Texture2D tex = CreateIconTexture(i);
            var mat = new Material(shader);
            mat.mainTexture = tex;

            // Enable transparency for URP Unlit
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.SetFloat("_AlphaClip", 1);
            mat.SetFloat("_Cutoff", 0.1f);
            mat.SetOverrideTag("RenderType", "TransparentCutout");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = 2450;
            mat.EnableKeyword("_ALPHATEST_ON");

            mats[i] = mat;
        }
        return mats;
    }

    /// <summary>Generate Material for 3D tile body (URP Lit, cream colored).</summary>
    public static Material GenerateTileMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        var mat = new Material(shader);
        mat.color = new Color(0.92f, 0.88f, 0.82f); // warm cream
        mat.SetFloat("_Smoothness", 0.4f);
        mat.SetFloat("_Metallic", 0.0f);
        return mat;
    }

    private static Texture2D CreateIconTexture(int index)
    {
        Texture2D tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear; // smoother for 3D
        tex.wrapMode = TextureWrapMode.Clamp;
        ClearTexture(tex, Color.clear);
        Color col = IconColors[index % IconColors.Length];
        DrawShape(tex, index, col);
        tex.Apply();
        return tex;
    }

    /// <summary>Generate a tile background sprite (rounded-ish rectangle).</summary>
    public static Sprite GenerateTileBackground()
    {
        Texture2D tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color bg = new Color(0.95f, 0.92f, 0.88f);
        Color border = new Color(0.75f, 0.70f, 0.65f);

        ClearTexture(tex, Color.clear);

        int m = 3; // margin for rounded corners
        for (int y = 0; y < TEX_SIZE; y++)
        {
            for (int x = 0; x < TEX_SIZE; x++)
            {
                bool inside = x >= m && x < TEX_SIZE - m && y >= m && y < TEX_SIZE - m;
                // Simple rounded corners check
                if (!inside)
                {
                    // Check corners
                    bool inCorner = false;
                    if (x < m && y < m) inCorner = (m - x) + (m - y) <= m + 1;
                    else if (x >= TEX_SIZE - m && y < m) inCorner = (x - TEX_SIZE + m + 1) + (m - y) <= m + 1;
                    else if (x < m && y >= TEX_SIZE - m) inCorner = (m - x) + (y - TEX_SIZE + m + 1) <= m + 1;
                    else if (x >= TEX_SIZE - m && y >= TEX_SIZE - m) inCorner = (x - TEX_SIZE + m + 1) + (y - TEX_SIZE + m + 1) <= m + 1;
                    else inside = true; // edge but not corner

                    if (!inside && !inCorner) continue;
                    inside = true;
                }

                bool isBorder = x == m || x == TEX_SIZE - m - 1 || y == m || y == TEX_SIZE - m - 1;
                tex.SetPixel(x, y, isBorder ? border : bg);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, TEX_SIZE, TEX_SIZE), Vector2.one * 0.5f, PPU);
    }

    /// <summary>Generate a highlight overlay sprite.</summary>
    public static Sprite GenerateHighlight()
    {
        Texture2D tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        ClearTexture(tex, Color.clear);

        Color hl = new Color(0.2f, 0.9f, 0.4f, 0.45f);
        int m = 2;
        for (int y = m; y < TEX_SIZE - m; y++)
            for (int x = m; x < TEX_SIZE - m; x++)
            {
                bool isBorder = x <= m + 2 || x >= TEX_SIZE - m - 3 || y <= m + 2 || y >= TEX_SIZE - m - 3;
                if (isBorder) tex.SetPixel(x, y, new Color(0.1f, 1f, 0.3f, 0.8f));
                else tex.SetPixel(x, y, hl);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, TEX_SIZE, TEX_SIZE), Vector2.one * 0.5f, PPU);
    }

    private static void DrawShape(Texture2D tex, int shapeIndex, Color col)
    {
        int cx = TEX_SIZE / 2, cy = TEX_SIZE / 2;
        int r = TEX_SIZE / 2 - 8;

        switch (shapeIndex % 18)
        {
            case 0: FillCircle(tex, cx, cy, r, col); break;
            case 1: FillRect(tex, cx - r, cy - r, r * 2, r * 2, col); break;
            case 2: FillTriangleUp(tex, cx, cy, r, col); break;
            case 3: FillDiamond(tex, cx, cy, r, col); break;
            case 4: FillStar(tex, cx, cy, r, col); break;
            case 5: FillHeart(tex, cx, cy, r, col); break;
            case 6: FillCross(tex, cx, cy, r, col); break;
            case 7: FillHexagon(tex, cx, cy, r, col); break;
            case 8: FillPentagon(tex, cx, cy, r, col); break;
            case 9: FillArrow(tex, cx, cy, r, col); break;
            case 10: FillRing(tex, cx, cy, r, col); break;
            case 11: FillMoon(tex, cx, cy, r, col); break;
            case 12: FillLightning(tex, cx, cy, r, col); break;
            case 13: FillXMark(tex, cx, cy, r, col); break;
            case 14: FillDrop(tex, cx, cy, r, col); break;
            case 15: FillOval(tex, cx, cy, r, col); break;
            case 16: FillHouse(tex, cx, cy, r, col); break;
            case 17: FillTriangleDown(tex, cx, cy, r, col); break;
        }
    }

    #region Shape Drawing Helpers

    private static void ClearTexture(Texture2D tex, Color col)
    {
        Color[] px = new Color[tex.width * tex.height];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        tex.SetPixels(px);
    }

    private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color col)
    {
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                    SafeSetPixel(tex, x, y, col);
    }

    private static void FillRect(Texture2D tex, int x0, int y0, int w, int h, Color col)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
                SafeSetPixel(tex, x, y, col);
    }

    private static void FillTriangleUp(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int top = cy + r, bot = cy - r;
        for (int y = bot; y <= top; y++)
        {
            float t = (float)(y - bot) / (top - bot);
            int hw = (int)((1f - t) * r);
            for (int x = cx - hw; x <= cx + hw; x++)
                SafeSetPixel(tex, x, y, col);
        }
    }

    private static void FillTriangleDown(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int top = cy + r, bot = cy - r;
        for (int y = bot; y <= top; y++)
        {
            float t = (float)(y - bot) / (top - bot);
            int hw = (int)(t * r);
            for (int x = cx - hw; x <= cx + hw; x++)
                SafeSetPixel(tex, x, y, col);
        }
    }

    private static void FillDiamond(Texture2D tex, int cx, int cy, int r, Color col)
    {
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if (Mathf.Abs(x - cx) + Mathf.Abs(y - cy) <= r)
                    SafeSetPixel(tex, x, y, col);
    }

    private static void FillStar(Texture2D tex, int cx, int cy, int r, Color col)
    {
        float outerR = r;
        float innerR = r * 0.4f;
        for (int y = cy - r; y <= cy + r; y++)
        {
            for (int x = cx - r; x <= cx + r; x++)
            {
                float dx = x - cx, dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // 5-pointed star
                float a = angle + Mathf.PI / 2f;
                float segAngle = Mathf.PI * 2f / 5f;
                float localAngle = Mathf.Repeat(a, segAngle);
                float t = Mathf.Abs(localAngle - segAngle / 2f) / (segAngle / 2f);
                float limit = Mathf.Lerp(innerR, outerR, 1f - t);
                if (dist <= limit) SafeSetPixel(tex, x, y, col);
            }
        }
    }

    private static void FillHeart(Texture2D tex, int cx, int cy, int r, Color col)
    {
        for (int y = cy - r; y <= cy + r; y++)
        {
            for (int x = cx - r; x <= cx + r; x++)
            {
                float nx = (x - cx) / (float)r;
                float ny = (y - cy) / (float)r;
                // Heart equation: (x^2 + y^2 - 1)^3 - x^2*y^3 < 0
                float x2 = nx * nx, y2 = ny * ny;
                float a = x2 + y2 - 1f;
                if (a * a * a - x2 * ny * ny * ny < 0)
                    SafeSetPixel(tex, x, y, col);
            }
        }
    }

    private static void FillCross(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int w = r / 3;
        FillRect(tex, cx - w, cy - r, w * 2, r * 2, col);
        FillRect(tex, cx - r, cy - w, r * 2, w * 2, col);
    }

    private static void FillHexagon(Texture2D tex, int cx, int cy, int r, Color col)
    {
        for (int y = cy - r; y <= cy + r; y++)
        {
            for (int x = cx - r; x <= cx + r; x++)
            {
                float dx = Mathf.Abs(x - cx) / (float)r;
                float dy = Mathf.Abs(y - cy) / (float)r;
                if (dy <= 1f && dx <= 1f - dy * 0.5f)
                    SafeSetPixel(tex, x, y, col);
            }
        }
    }

    private static void FillPentagon(Texture2D tex, int cx, int cy, int r, Color col)
    {
        // Approximate pentagon using angle check
        Vector2[] pts = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            float a = Mathf.PI / 2f + i * Mathf.PI * 2f / 5f;
            pts[i] = new Vector2(cx + r * Mathf.Cos(a), cy + r * Mathf.Sin(a));
        }
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if (PointInPolygon(new Vector2(x, y), pts))
                    SafeSetPixel(tex, x, y, col);
    }

    private static void FillArrow(Texture2D tex, int cx, int cy, int r, Color col)
    {
        // Right-pointing arrow
        int hw = r / 3;
        FillRect(tex, cx - r, cy - hw, r, hw * 2, col); // shaft
        // Arrowhead triangle
        for (int y = cy - r; y <= cy + r; y++)
        {
            float t = 1f - Mathf.Abs(y - cy) / (float)r;
            int w = (int)(t * r);
            for (int x = cx; x < cx + w; x++)
                SafeSetPixel(tex, x, y, col);
        }
    }

    private static void FillRing(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int innerR = r / 2;
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                int d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (d2 <= r * r && d2 >= innerR * innerR)
                    SafeSetPixel(tex, x, y, col);
            }
    }

    private static void FillMoon(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int offset = r / 2;
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                int d1 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                int d2 = (x - cx - offset) * (x - cx - offset) + (y - cy) * (y - cy);
                int innerR = (int)(r * 0.8f);
                if (d1 <= r * r && d2 > innerR * innerR)
                    SafeSetPixel(tex, x, y, col);
            }
    }

    private static void FillLightning(Texture2D tex, int cx, int cy, int r, Color col)
    {
        Vector2[] pts = new Vector2[]
        {
            new Vector2(cx - r * 0.2f, cy + r),
            new Vector2(cx + r * 0.5f, cy + r),
            new Vector2(cx, cy + r * 0.1f),
            new Vector2(cx + r * 0.4f, cy + r * 0.1f),
            new Vector2(cx - r * 0.3f, cy - r),
            new Vector2(cx, cy - r * 0.1f),
            new Vector2(cx - r * 0.4f, cy - r * 0.1f),
        };
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if (PointInPolygon(new Vector2(x, y), pts))
                    SafeSetPixel(tex, x, y, col);
    }

    private static void FillXMark(Texture2D tex, int cx, int cy, int r, Color col)
    {
        int w = r / 3;
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                int dx = Mathf.Abs(x - cx), dy = Mathf.Abs(y - cy);
                if (Mathf.Abs(dx - dy) <= w)
                    SafeSetPixel(tex, x, y, col);
            }
    }

    private static void FillDrop(Texture2D tex, int cx, int cy, int r, Color col)
    {
        // Bottom circle + top triangle
        int cr = (int)(r * 0.6f);
        int ccy = cy - r + cr;
        FillCircle(tex, cx, ccy, cr, col);
        // Triangle top
        for (int y = ccy; y <= cy + r; y++)
        {
            float t = 1f - (float)(y - ccy) / (cy + r - ccy);
            int hw = (int)(t * cr);
            for (int x = cx - hw; x <= cx + hw; x++)
                SafeSetPixel(tex, x, y, col);
        }
    }

    private static void FillOval(Texture2D tex, int cx, int cy, int r, Color col)
    {
        float rx = r, ry = r * 0.6f;
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                float dx = (x - cx) / rx, dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    SafeSetPixel(tex, x, y, col);
            }
    }

    private static void FillHouse(Texture2D tex, int cx, int cy, int r, Color col)
    {
        // Rectangle body
        int bh = (int)(r * 1.0f);
        FillRect(tex, cx - r + 2, cy - r, (r - 2) * 2, bh, col);
        // Triangle roof
        int roofBot = cy - r + bh;
        int roofTop = cy + r;
        for (int y = roofBot; y <= roofTop; y++)
        {
            float t = (float)(y - roofBot) / (roofTop - roofBot);
            int hw = (int)((1f - t) * r);
            for (int x = cx - hw; x <= cx + hw; x++)
                SafeSetPixel(tex, x, y, col);
        }
    }

    private static void SafeSetPixel(Texture2D tex, int x, int y, Color col)
    {
        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
            tex.SetPixel(x, y, col);
    }

    private static bool PointInPolygon(Vector2 p, Vector2[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > p.y) != (polygon[j].y > p.y)) &&
                (p.x < (polygon[j].x - polygon[i].x) * (p.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                inside = !inside;
        }
        return inside;
    }

    #endregion
}
