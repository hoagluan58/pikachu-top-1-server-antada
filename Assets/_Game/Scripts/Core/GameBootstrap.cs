using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Auto-initializes the entire Pikachu game when Play is pressed.
/// No manual setup required - uses [RuntimeInitializeOnLoadMethod].
/// 3D version: perspective camera, directional light, table surface.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Debug.Log("[Pikachu] GameBootstrap: Auto-initializing 3D game...");

        // ── Camera (Perspective, looking down at board) ──
        Camera cam = Camera.main;
        if (cam != null)
        {
            SetupCamera(cam);
        }
        else
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            SetupCamera(cam);
        }

        // Disable any existing URP camera data auto-added features that may conflict
        var camData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (camData != null)
        {
            camData.renderShadows = true;
        }

        // ── Directional Light ──
        var existingLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        bool hasDirectionalLight = false;
        foreach (var l in existingLights)
        {
            if (l.type == LightType.Directional)
            {
                hasDirectionalLight = true;
                SetupDirectionalLight(l);
                break;
            }
        }
        if (!hasDirectionalLight)
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            SetupDirectionalLight(light);
        }

        // ── EventSystem ──
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }

        // ── Table Surface (green felt) ──
        CreateTableSurface();

        // ── Game Root ──
        var root = new GameObject("PikachuGame");
        root.AddComponent<AudioManager>();
        root.AddComponent<UIManager>();
        root.AddComponent<BoardManager>();
        root.AddComponent<GameManager>();

        Debug.Log("[Pikachu] GameBootstrap: 3D Game initialized successfully!");
    }

    private static void SetupCamera(Camera cam)
    {
        cam.orthographic = false;
        cam.fieldOfView = 45f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.15f);

        // Position: above and behind, looking down at ~55°
        cam.transform.position = new Vector3(0f, 12f, -6.5f);
        cam.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
    }

    private static void SetupDirectionalLight(Light light)
    {
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.color = new Color(1f, 0.97f, 0.92f);
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.5f;
    }

    private static void CreateTableSurface()
    {
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "TableSurface";
        table.transform.position = new Vector3(0f, -0.3f, 0f);
        table.transform.localScale = new Vector3(20f, 0.5f, 14f);

        var renderer = table.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.12f, 0.28f, 0.18f); // dark green felt
        mat.SetFloat("_Smoothness", 0.15f);
        renderer.material = mat;

        // Remove collider so it doesn't interfere with tile clicks
        var col = table.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
    }
}
