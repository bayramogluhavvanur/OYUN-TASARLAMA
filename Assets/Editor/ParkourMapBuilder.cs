// Assets/Editor/ParkourMapBuilder.cs  (mutlaka "Editor" klasörüne koy)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

#pragma warning disable 618 // StaticEditorFlags.NavigationStatic yeni Unity sürümlerinde obsolete uyarısı verebilir

public static class ParkourMapBuilder
{
    const string RootName = "ParkourMap";
    const string MatFolder = "Assets/Materials/Parkour";
    const float RoomStep = 21f;   // Odalar arka arkaya, ortak duvar (kapılar) ile doğrudan bağlı
    const float WallH = 7f;

    static readonly string[] Tags =
        { "Wall", "Ground", "Obstacle", "Switch", "Door", "Gate", "Glass", "Portal", "Agent" };

    static readonly Dictionary<string, Material> MatCache = new Dictionary<string, Material>();

    static readonly Color[] RoomColors =
    {
        new Color(0.35f, 0.55f, 0.85f),
        new Color(0.40f, 0.75f, 0.50f),
        new Color(0.85f, 0.65f, 0.30f),
        new Color(0.75f, 0.40f, 0.70f)
    };

    // Oda 1-3 ajan şalteri (oda lokal koordinatı, ajan tarafı x>0)
    static readonly Vector3[] SwitchPos =
    {
        new Vector3(7f, 0f, 7.5f),
        new Vector3(2f, 0f, 7.5f),
        new Vector3(5f, 0f, 8f)
    };

    [MenuItem("Tools/Parkour/Build 4-Room ML-Agents Map")]
    public static void Build()
    {
        foreach (var t in Tags)
            if (!InternalEditorUtility.tags.Contains(t)) InternalEditorUtility.AddTag(t);

        var old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);

        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(MatFolder)) AssetDatabase.CreateFolder("Assets/Materials", "Parkour");
        MatCache.Clear();

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Parkour Map");

        for (int i = 0; i < 4; i++) BuildRoom(i, root.transform);
        CreateAgent(root.transform);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        Debug.Log("[ParkourMapBuilder] 4 odalı harita oluşturuldu. NavMesh için Navigation/AI Navigation penceresinden Bake yapın.");
    }

    // ------------------------------------------------------------------ ODA
    static void BuildRoom(int i, Transform root)
    {
        var room = new GameObject("Room_" + (i + 1));
        room.transform.SetParent(root, false);
        room.transform.position = new Vector3(0f, 0f, i * RoomStep);
        Transform rt = room.transform;

        var st = Group("Structure", rt);
        var doors = Group("DoorPair", rt);
        var obs = Group("Obstacles", rt);

        Color c = RoomColors[i];
        string floorM = "Floor_" + i, wallM = "Wall_" + i;
        Mat(floorM, new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 1f));
        Mat(wallM, Color.Lerp(c, Color.white, 0.5f));
        Mat("Glass", new Color(0.7f, 0.9f, 1f, 0.25f), true);
        Mat("Door", new Color(0.65f, 0.15f, 0.15f));

        // Zemin (kapı eşikleri dahil)
        P(PrimitiveType.Cube, "Floor", st, new Vector3(0, -0.25f, 0.5f), new Vector3(22, 0.5f, 21), "Ground", floorM);

        // Yan duvarlar
        P(PrimitiveType.Cube, "Wall_Left", st, new Vector3(-10.5f, WallH / 2, 0), new Vector3(1, WallH, 20), "Wall", wallM);
        P(PrimitiveType.Cube, "Wall_Right", st, new Vector3(10.5f, WallH / 2, 0), new Vector3(1, WallH, 20), "Wall", wallM);
        if (i == 0)
            P(PrimitiveType.Cube, "Wall_Back", st, new Vector3(0, WallH / 2, -10.5f), new Vector3(22, WallH, 1), "Wall", wallM);

        // Ön duvar (sonraki odanın arka duvarı): iki kapı boşluğu x=-5 ve x=+5
        P(PrimitiveType.Cube, "Wall_Front_L", st, new Vector3(-9f, WallH / 2, 10.5f), new Vector3(4, WallH, 1), "Wall", wallM);
        P(PrimitiveType.Cube, "Wall_Front_M", st, new Vector3(0f, WallH / 2, 10.5f), new Vector3(6, WallH, 1), "Wall", wallM);
        P(PrimitiveType.Cube, "Wall_Front_R", st, new Vector3(9f, WallH / 2, 10.5f), new Vector3(4, WallH, 1), "Wall", wallM);
        float lintelY = 3f + (WallH - 3f) / 2f;
        P(PrimitiveType.Cube, "Lintel_Player", st, new Vector3(-5f, lintelY, 10.5f), new Vector3(4, WallH - 3f, 1), "Wall", wallM);
        P(PrimitiveType.Cube, "Lintel_Agent", st, new Vector3(5f, lintelY, 10.5f), new Vector3(4, WallH - 3f, 1), "Wall", wallM);

        // Cam bölme: sol = oyuncu, sağ = ajan
        P(PrimitiveType.Cube, "Glass_Divider", st, new Vector3(0, WallH / 2, 0), new Vector3(0.2f, WallH, 20), "Glass", "Glass");

        // Birbirine bağlı iki kapı (aynı anda açılacak: DoorPair altında)
        Door(doors, "Door_Player", new Vector3(-5f, 1.5f, 10.5f));
        Door(doors, "Door_Agent", new Vector3(5f, 1.5f, 10.5f));

        // Spawn noktaları
        Spawn("SpawnPoint_Player", rt, new Vector3(-5f, 0.5f, -8.5f));
        Spawn("SpawnPoint_Agent", rt, new Vector3(5f, 0.5f, -8.5f));

        Mat("Obstacle", new Color(0.3f, 0.3f, 0.35f));
        Mat("SwitchBase", new Color(0.2f, 0.2f, 0.2f));
        Mat("SwitchLever", new Color(1f, 0.85f, 0.1f));

        switch (i)
        {
            case 0:
                Slab(obs, "LowWall_1", 3.8f, -3f, 7.4f, 1.5f);
                Pillar(obs, "Pillar_A", new Vector3(6f, 1.5f, 3f));
                BuildSwitch(rt, "Switch", SwitchPos[0], "SwitchLever");
                break;
            case 1:
                Slab(obs, "Wall_A", 3.8f, -4f, 7.4f, 2f);
                Slab(obs, "Wall_B", 6.5f, 1f, 7f, 2f);
                BuildSwitch(rt, "Switch", SwitchPos[1], "SwitchLever");
                break;
            case 2:
                foreach (float x in new[] { 2f, 8f })
                    foreach (float z in new[] { -4f, 4f })
                        Pillar(obs, $"Pillar_{x}_{z}", new Vector3(x, 1.5f, z));
                Pillar(obs, "Pillar_Mid", new Vector3(5f, 1.5f, 0f));
                BuildSwitch(rt, "Switch", SwitchPos[2], "SwitchLever");
                break;
            case 3:
                BuildRoom4(rt, st, obs);
                BuildFinish(rt, st);
                break;
        }

        // Işık
        var lightGo = new GameObject("RoomLight");
        lightGo.transform.SetParent(rt, false);
        lightGo.transform.localPosition = new Vector3(0, 6f, 0);
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Point; l.range = 35f; l.intensity = 1.5f;
    }

    // ------------------------------------------------------------------ ODA 4
    // "Kör Kılavuz ve Flip-Flop Labirenti"
    static void BuildRoom4(Transform rt, Transform st, Transform obs)
    {
        Mat("Gate", new Color(1f, 0.5f, 0.1f));
        Mat("Console", new Color(0.15f, 0.15f, 0.2f));
        Mat("LeverJ", new Color(0.2f, 1f, 0.3f));
        Mat("LeverK", new Color(1f, 0.25f, 0.25f));
        string wallM = "Wall_3", floorM = "Floor_3";

        // ---------- AJAN TARAFI: labirent (x>0), duvarlar kuş bakışı görünsün diye alçak
        var maze = Group("Maze", rt);
        var gates = Group("Gates", rt);
        Slab(maze, "Maze_Wall_1", 3.8f, -5f, 7.4f, 2.5f);   // boşluk sağda  -> Gate_1A
        Slab(maze, "Maze_Wall_2", 6.5f, 0f, 7f, 2.5f);      // boşluk solda  -> Gate_1B
        Slab(maze, "Maze_Wall_3", 3.8f, 5f, 7.4f, 2.5f);    // boşluk sağda  -> Gate_2A
        Slab(maze, "Maze_Wall_4", 6.5f, 8f, 7f, 2.5f);      // boşluk solda  -> Gate_2B

        var g1a = Gate(gates, "Gate_1A", new Vector3(8.75f, 1.25f, -5f), 2.5f);
        var g1b = Gate(gates, "Gate_1B", new Vector3(1.55f, 1.25f, 0f), 2.9f);
        var g2a = Gate(gates, "Gate_2A", new Vector3(8.75f, 1.25f, 5f), 2.5f);
        var g2b = Gate(gates, "Gate_2B", new Vector3(1.55f, 1.25f, 8f), 2.9f);
        // Flip-flop başlangıç durumu: A kapıları kapalı, B kapıları açık (SetActive=false => açık)
        g1b.SetActive(false);
        g2b.SetActive(false);

        // Labirent çıkış şalteri (ajan buna ulaşınca iki oda kapısı açılır)
        BuildSwitch(rt, "Exit_Switch", new Vector3(6f, 0f, 9.2f), "SwitchLever");

        // ---------- OYUNCU TARAFI: rampa + üst kat camlı balkon (x<0)
        var balcony = Group("Balcony", rt);
        float ang = Mathf.Atan2(3f, 6f) * Mathf.Rad2Deg;           // 26.57°
        float len = Mathf.Sqrt(36f + 9f);
        var ramp = P(PrimitiveType.Cube, "Ramp", balcony, new Vector3(-8.5f, 1.37f, -5f), new Vector3(3, 0.3f, len), "Ground", floorM);
        ramp.transform.localRotation = Quaternion.Euler(-ang, 0, 0);
        P(PrimitiveType.Cube, "Balcony_Floor", balcony, new Vector3(-5.05f, 2.85f, 1f), new Vector3(9.9f, 0.3f, 6f), "Ground", floorM);
        P(PrimitiveType.Cube, "Balcony_Glass_Front", balcony, new Vector3(-5.05f, 3.6f, 3.9f), new Vector3(9.9f, 1.2f, 0.1f), "Glass", "Glass");
        P(PrimitiveType.Cube, "Balcony_Glass_Back", balcony, new Vector3(-3.55f, 3.6f, -1.95f), new Vector3(6.9f, 1.2f, 0.1f), "Glass", "Glass");

        // ---------- JK flip-flop kontrol paneli
        var panel = Group("FlipFlopPanel", balcony);
        P(PrimitiveType.Cube, "Console", panel, new Vector3(-4f, 3.5f, 3.1f), new Vector3(6f, 1f, 0.8f), "Wall", "Console");
        FlipSwitch(panel, "Switch_J1", new Vector3(-5.6f, 4f, 3.1f), "LeverJ");
        FlipSwitch(panel, "Switch_K1", new Vector3(-4.6f, 4f, 3.1f), "LeverK");
        FlipSwitch(panel, "Switch_J2", new Vector3(-3.4f, 4f, 3.1f), "LeverJ");
        FlipSwitch(panel, "Switch_K2", new Vector3(-2.4f, 4f, 3.1f), "LeverK");

        // Mantık bağlantı notları (runtime scriptinde bu isimlerle eşleştir)
        var links = Group("FlipFlopLinks", rt);
        Group("J1 => Open Gate_1A, Close Gate_1B", links);
        Group("K1 => Close Gate_1A, Open Gate_1B", links);
        Group("J2 => Open Gate_2A, Close Gate_2B", links);
        Group("K2 => Close Gate_2A, Open Gate_2B", links);
    }

    static void BuildFinish(Transform rt, Transform st)
    {
        string floorM = "Floor_3", wallM = "Wall_3";
        P(PrimitiveType.Cube, "Finish_Floor", st, new Vector3(0, -0.25f, 13.5f), new Vector3(22, 0.5f, 5), "Ground", floorM);
        P(PrimitiveType.Cube, "Finish_Wall_Back", st, new Vector3(0, WallH / 2, 16.5f), new Vector3(22, WallH, 1), "Wall", wallM);
        P(PrimitiveType.Cube, "Finish_Wall_L", st, new Vector3(-10.5f, WallH / 2, 13.5f), new Vector3(1, WallH, 5), "Wall", wallM);
        P(PrimitiveType.Cube, "Finish_Wall_R", st, new Vector3(10.5f, WallH / 2, 13.5f), new Vector3(1, WallH, 5), "Wall", wallM);
        P(PrimitiveType.Cube, "Glass_Divider_Finish", st, new Vector3(0, WallH / 2, 13.5f), new Vector3(0.2f, WallH, 5), "Glass", "Glass");
        Mat("Portal", new Color(0.2f, 0.9f, 1f, 1f));
        P(PrimitiveType.Cube, "Finish_Player", st, new Vector3(-5f, 1.5f, 13f), new Vector3(4, 3, 1), "Portal", "Portal", false, true);
        P(PrimitiveType.Cube, "Finish_Agent", st, new Vector3(5f, 1.5f, 13f), new Vector3(4, 3, 1), "Portal", "Portal", false, true);
    }

    // ------------------------------------------------------------------ ŞALTER
    static void BuildSwitch(Transform room, string name, Vector3 p, string leverMat)
    {
        var grp = Group(name, room);
        P(PrimitiveType.Cylinder, "Switch_Base", grp, new Vector3(p.x, p.y + 0.25f, p.z), new Vector3(1f, 0.25f, 1f), "Switch", "SwitchBase", false);
        var lever = P(PrimitiveType.Cube, "Switch_Lever", grp, new Vector3(p.x, p.y + 0.9f, p.z), new Vector3(0.2f, 1f, 0.2f), "Switch", leverMat, false);
        UnityEngine.Object.DestroyImmediate(lever.GetComponent<Collider>());
        P(PrimitiveType.Cube, "Switch_Trigger", grp, new Vector3(p.x, p.y + 0.75f, p.z), new Vector3(1.6f, 1.5f, 1.6f), "Switch", leverMat, false, true)
            .GetComponent<Renderer>().enabled = false;
    }

    static void FlipSwitch(Transform parent, string name, Vector3 p, string leverMat)
    {
        var grp = Group(name, parent);
        P(PrimitiveType.Cube, "Lever", grp, new Vector3(p.x, p.y + 0.3f, p.z), new Vector3(0.2f, 0.6f, 0.2f), "Switch", leverMat, false)
            .GetComponent<Collider>().isTrigger = true;
        P(PrimitiveType.Cube, "Interact_Zone", grp, new Vector3(p.x, p.y + 0.5f, p.z - 0.3f), new Vector3(0.8f, 1f, 1.2f), "Switch", leverMat, false, true)
            .GetComponent<Renderer>().enabled = false;
    }

    // ------------------------------------------------------------------ AJAN
    static void CreateAgent(Transform root)
    {
        Mat("Agent", new Color(0.1f, 0.9f, 0.3f));
        var agent = P(PrimitiveType.Capsule, "Agent", root, new Vector3(5f, 1f, -8.5f), Vector3.one, "Agent", "Agent", false);
        var rb = agent.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var sensorType = Type.GetType("Unity.MLAgents.Sensors.RayPerceptionSensorComponent3D, Unity.ML-Agents");
        if (sensorType == null)
        {
            Debug.LogWarning("[ParkourMapBuilder] ML-Agents paketi bulunamadı, RayPerceptionSensor eklenmedi.");
            return;
        }
        var sensor = agent.AddComponent(sensorType);
        void Set(string prop, object val) => sensorType.GetProperty(prop)?.SetValue(sensor, val);
        Set("SensorName", "RayPerceptionSensor");
        Set("DetectableTags", new List<string> { "Wall", "Ground", "Obstacle", "Switch", "Door", "Gate", "Glass", "Portal" });
        Set("RaysPerDirection", 5);
        Set("MaxRayDegrees", 70f);
        Set("SphereCastRadius", 0.3f);
        Set("RayLength", 20f);
        Set("StartVerticalOffset", 0.3f);
        Set("EndVerticalOffset", 0.3f);
    }

    // ------------------------------------------------------------------ YARDIMCILAR
    static GameObject Door(Transform parent, string name, Vector3 pos)
    {
        var door = P(PrimitiveType.Cube, name, parent, pos, new Vector3(4, 3, 0.6f), "Door", "Door", false);
        var o = door.AddComponent<NavMeshObstacle>();
        o.carving = true;
        o.shape = NavMeshObstacleShape.Box;
        o.size = Vector3.one;
        return door;
    }

    static GameObject Gate(Transform parent, string name, Vector3 pos, float width)
    {
        var g = P(PrimitiveType.Cube, name, parent, pos, new Vector3(width, 2.5f, 0.4f), "Gate", "Gate", false);
        var o = g.AddComponent<NavMeshObstacle>();
        o.carving = true;
        o.shape = NavMeshObstacleShape.Box;
        o.size = Vector3.one;
        return g;
    }

    static void Spawn(string name, Transform room, Vector3 localPos)
    {
        var s = new GameObject(name);
        s.tag = "Respawn";
        s.transform.SetParent(room, false);
        s.transform.localPosition = localPos;
    }

    static GameObject Slab(Transform parent, string name, float x, float z, float width, float h) =>
        P(PrimitiveType.Cube, name, parent, new Vector3(x, h / 2f, z), new Vector3(width, h, 0.6f), "Obstacle", "Obstacle");

    static GameObject Pillar(Transform parent, string name, Vector3 pos) =>
        P(PrimitiveType.Cylinder, name, parent, pos, new Vector3(1.5f, 1.5f, 1.5f), "Obstacle", "Obstacle");

    static Transform Group(string name, Transform parent)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        return g.transform;
    }

    static GameObject P(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 scale,
                        string tag, string matName, bool navStatic = true, bool trigger = false)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.tag = tag;
        go.GetComponent<Renderer>().sharedMaterial = Mat(matName, Color.gray);
        if (trigger) go.GetComponent<Collider>().isTrigger = true;

        var flags = navStatic
            ? (StaticEditorFlags.NavigationStatic | StaticEditorFlags.BatchingStatic)
            : (StaticEditorFlags)0;
        GameObjectUtility.SetStaticEditorFlags(go, flags);
        return go;
    }

    static Material Mat(string name, Color color, bool glass = false)
    {
        if (MatCache.TryGetValue(name, out var cached)) return cached;
        string path = $"{MatFolder}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            m = new Material(shader) { color = color };
            if (glass) MakeTransparent(m, color);
            AssetDatabase.CreateAsset(m, path);
        }
        MatCache[name] = m;
        return m;
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