// Assets/Editor/ParkourMaterialFixer.cs  (mutlaka "Editor" klasörüne koy)
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ParkourMaterialFixer
{
    const string MatFolder = "Assets/Materials/Parkour";

    static readonly Color[] RoomColors =
    {
        new Color(0.35f, 0.55f, 0.85f),
        new Color(0.40f, 0.75f, 0.50f),
        new Color(0.85f, 0.65f, 0.30f),
        new Color(0.75f, 0.40f, 0.70f)
    };

    static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

    [MenuItem("Tools/Parkour/Fix Missing Materials")]
    public static void Fix()
    {
        var root = GameObject.Find("ParkourMap");
        if (root == null) { Debug.LogError("[ParkourMaterialFixer] 'ParkourMap' objesi sahnede yok."); return; }

        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(MatFolder)) AssetDatabase.CreateFolder("Assets/Materials", "Parkour");
        Cache.Clear();

        int fixedCount = 0;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r.sharedMaterial != null) continue;   // sadece eksik olanları doldur
            string name = MaterialFor(r.gameObject);
            if (name == null) continue;

            Undo.RecordObject(r, "Fix Parkour Materials");
            r.sharedMaterial = Get(name);
            EditorUtility.SetDirty(r);
            fixedCount++;
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ParkourMaterialFixer] " + fixedCount + " renderer'a materyal atandı. Sahneyi kaydet (Ctrl+S).");
    }

    static string MaterialFor(GameObject go)
    {
        int room = RoomIndex(go.transform);
        string n = go.name;

        if (n == "Console") return "Console";
        if (n == "Lever" && go.transform.parent != null)
            return go.transform.parent.name.StartsWith("Switch_J") ? "LeverJ" : "LeverK";

        switch (go.tag)
        {
            case "Ground": return "Floor_" + room;
            case "Wall": return "Wall_" + room;
            case "Glass": return "Glass";
            case "Door": return "Door";
            case "Gate": return "Gate";
            case "Obstacle": return "Obstacle";
            case "Portal": return "Portal";
            case "Agent": return "Agent";
            case "Switch": return n == "Switch_Base" ? "SwitchBase" : "SwitchLever";
        }
        return null;
    }

    static int RoomIndex(Transform t)
    {
        for (; t != null; t = t.parent)
            if (t.name.StartsWith("Room_") && int.TryParse(t.name.Substring(5), out int i))
                return Mathf.Clamp(i - 1, 0, 3);
        return 0;
    }

    static Material Get(string name)
    {
        if (Cache.TryGetValue(name, out var cached)) return cached;
        string path = MatFolder + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Color c = ColorFor(name);
            m = new Material(shader) { color = c };
            if (name == "Glass") MakeTransparent(m, c);
            AssetDatabase.CreateAsset(m, path);
        }
        Cache[name] = m;
        return m;
    }

    static Color ColorFor(string name)
    {
        if (name.StartsWith("Floor_") || name.StartsWith("Wall_"))
        {
            Color c = RoomColors[int.Parse(name.Substring(name.IndexOf('_') + 1))];
            return name[0] == 'F' ? new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 1f) : Color.Lerp(c, Color.white, 0.5f);
        }
        switch (name)
        {
            case "Glass": return new Color(0.7f, 0.9f, 1f, 0.25f);
            case "Door": return new Color(0.65f, 0.15f, 0.15f);
            case "Gate": return new Color(1f, 0.5f, 0.1f);
            case "Obstacle": return new Color(0.3f, 0.3f, 0.35f);
            case "Portal": return new Color(0.2f, 0.9f, 1f);
            case "Agent": return new Color(0.1f, 0.9f, 0.3f);
            case "SwitchBase": return new Color(0.2f, 0.2f, 0.2f);
            case "SwitchLever": return new Color(1f, 0.85f, 0.1f);
            case "Console": return new Color(0.15f, 0.15f, 0.2f);
            case "LeverJ": return new Color(0.2f, 1f, 0.3f);
            case "LeverK": return new Color(1f, 0.25f, 0.25f);
        }
        return Color.gray;
    }

    static void MakeTransparent(Material m, Color col)
    {
        m.color = col;
        if (m.HasProperty("_Surface")) // URP
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else // Built-in Standard
        {
            m.SetFloat("_Mode", 3f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
        }
        m.renderQueue = 3000;
    }
}