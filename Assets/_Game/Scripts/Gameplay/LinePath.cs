using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Draws a connecting line path between matched tiles using LineRenderer (3D version).
/// Line floats slightly above tiles on the XZ plane.
/// Auto-fades and destroys after display.
/// </summary>
public class LinePath : MonoBehaviour
{
    private LineRenderer lr;
    private float fadeDuration = 0.35f;

    public void Init(List<Vector3> worldPoints, float lineWidth = 0.06f)
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = worldPoints.Count;
        lr.SetPositions(worldPoints.ToArray());

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // URP compatible material
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        mat.color = new Color(0.1f, 1f, 0.3f, 1f);

        // Enable transparency
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_Blend", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.renderQueue = 3000;

        lr.material = mat;
        lr.startColor = new Color(0.1f, 1f, 0.3f, 1f);
        lr.endColor = new Color(0.1f, 1f, 0.3f, 1f);

        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(0.15f); // show briefly

        float elapsed = 0;
        Color startColor = lr.startColor;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            Color c = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lr.startColor = c;
            lr.endColor = c;
            if (lr.material != null)
                lr.material.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
