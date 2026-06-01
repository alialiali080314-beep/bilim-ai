#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SceneSetup : EditorWindow
{
    [MenuItem("TacticalFPS/Setup Scene")]
    public static void ShowWindow() => GetWindow<SceneSetup>("TacticalFPS Setup");

    private void OnGUI()
    {
        GUILayout.Label("TacticalFPS — Scene Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Run these steps in order to build the game scene automatically.",
            MessageType.Info);
        GUILayout.Space(6);

        if (GUILayout.Button("1. Create Player", GUILayout.Height(32))) CreatePlayer();
        if (GUILayout.Button("2. Create Basic Arena", GUILayout.Height(32))) CreateArena();
        if (GUILayout.Button("3. Add Game Manager", GUILayout.Height(32))) AddGameManager();
        GUILayout.Space(6);
        if (GUILayout.Button("⚡  Full Setup (all three)", GUILayout.Height(40)))
        {
            CreatePlayer();
            CreateArena();
            AddGameManager();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "After setup:\n" +
            "• Window → AI → Navigation → Bake (for NavMesh)\n" +
            "• Assign weapon prefabs to WeaponManager slots\n" +
            "• Add AudioClips to WeaponBase components",
            MessageType.Warning);
    }

    private static void CreatePlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            Debug.Log("Player already exists — skipped.");
            return;
        }

        // Root
        var player = new GameObject("Player");
        player.tag = "Player";
        SetLayer(player, "Player");

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 1f, 0);

        player.AddComponent<PlayerMovement>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<FootstepSystem>();

        // Ground check
        var gc = new GameObject("GroundCheck");
        gc.transform.SetParent(player.transform);
        gc.transform.localPosition = new Vector3(0, 0.05f, 0);

        // Camera holder
        var camHolder = new GameObject("CameraHolder");
        camHolder.transform.SetParent(player.transform);
        camHolder.transform.localPosition = new Vector3(0, 1.7f, 0);

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(camHolder.transform);
        camGO.transform.localPosition = Vector3.zero;
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView  = 60f;
        cam.nearClipPlane = 0.08f;
        camGO.AddComponent<PlayerCamera>();
        camGO.AddComponent<AudioListener>();

        // Weapon holder
        var wpnHolder = new GameObject("WeaponHolder");
        wpnHolder.transform.SetParent(camHolder.transform);
        wpnHolder.transform.localPosition = new Vector3(0.25f, -0.22f, 0.45f);
        wpnHolder.AddComponent<WeaponManager>();

        Selection.activeGameObject = player;
        Debug.Log("✅ Player created. Remember to set GroundCheck and Layer references in PlayerMovement.");
    }

    private static void CreateArena()
    {
        var map = new GameObject("Map");

        // Floor
        Cube("Floor", map.transform, new Vector3(0, -0.5f, 0), new Vector3(42, 1, 42));

        // Perimeter walls
        Cube("Wall_N",  map.transform, new Vector3(0,  2, 21),  new Vector3(42, 5, 1));
        Cube("Wall_S",  map.transform, new Vector3(0,  2, -21), new Vector3(42, 5, 1));
        Cube("Wall_E",  map.transform, new Vector3(21, 2, 0),   new Vector3(1,  5, 42));
        Cube("Wall_W",  map.transform, new Vector3(-21,2, 0),   new Vector3(1,  5, 42));

        // Cover boxes
        Cube("Cover_A", map.transform, new Vector3(-5,  0.5f,  6), new Vector3(3, 1, 1));
        Cube("Cover_B", map.transform, new Vector3( 5,  0.5f, -6), new Vector3(1, 1, 3));
        Cube("Cover_C", map.transform, new Vector3(-8,  0.5f, -8), new Vector3(2, 1, 2));
        Cube("Cover_D", map.transform, new Vector3( 8,  0.5f,  8), new Vector3(2, 1, 2));
        Cube("Cover_E", map.transform, new Vector3( 0,  0.5f,  0), new Vector3(4, 1, 1));

        // Spawn points
        var spawns = new GameObject("SpawnPoints");
        Marker("PlayerSpawn_1", spawns.transform, new Vector3( 0, 0.1f, -16));
        Marker("PlayerSpawn_2", spawns.transform, new Vector3( 3, 0.1f, -16));
        Marker("EnemySpawn_1",  spawns.transform, new Vector3( 0, 0.1f,  16));
        Marker("EnemySpawn_2",  spawns.transform, new Vector3( 8, 0.1f,  16));
        Marker("EnemySpawn_3",  spawns.transform, new Vector3(-8, 0.1f,  16));
        Marker("EnemySpawn_4",  spawns.transform, new Vector3( 0, 0.1f,  10));

        // Patrol route for AI
        var patrol = new GameObject("PatrolRoute");
        Marker("Patrol_A", patrol.transform, new Vector3(-10, 0.1f, 10));
        Marker("Patrol_B", patrol.transform, new Vector3( 10, 0.1f, 10));
        Marker("Patrol_C", patrol.transform, new Vector3( 10, 0.1f, -5));
        Marker("Patrol_D", patrol.transform, new Vector3(-10, 0.1f, -5));

        // Directional light
        var lightGO = new GameObject("Sun");
        var dl = lightGO.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.intensity = 1.1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        Debug.Log("✅ Arena created. Select map colliders and add NavMesh obstacle/surface, then bake.");
    }

    private static void AddGameManager()
    {
        if (FindObjectOfType<GameManager>() != null) { Debug.Log("GameManager already exists."); return; }

        var gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        gm.AddComponent<AudioManager>();
        gm.AddComponent<EnemySpawner>();

        Debug.Log("✅ GameManager added. Wire up references in the Inspector.");
    }

    private static void Cube(string name, Transform parent, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
    }

    private static void Marker(string name, Transform parent, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
    }

    private static void SetLayer(GameObject go, string layer)
    {
        int l = LayerMask.NameToLayer(layer);
        if (l >= 0) go.layer = l;
    }
}
#endif
