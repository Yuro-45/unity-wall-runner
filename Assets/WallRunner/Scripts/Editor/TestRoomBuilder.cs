using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

// Generates the greybox test room in L_TestRoom. Everything it makes lives under
// the "Environment" object, which is wiped and rebuilt each run, so the room can
// be regenerated any time from the menu: WallRunner > Rebuild Test Room.
public static class TestRoomBuilder
{
    const string SceneName = "L_TestRoom";
    const string MaterialFolder = "Assets/WallRunner/Materials";
    const string GridTexturePath = MaterialFolder + "/T_Grid.png";

    const float WallHeight = 6f;
    const float WallThickness = 1f;

    static Transform root;
    static Transform station;
    static Material floorMat, runnableMat, propMat, boundaryMat;
    static int runnableLayer;

    [MenuItem("WallRunner/Rebuild Test Room")]
    public static void Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != SceneName)
        {
            EditorUtility.DisplayDialog("Rebuild Test Room",
                "Open " + SceneName + " first. The builder only runs in that scene.", "OK");
            return;
        }

        runnableLayer = LayerMask.NameToLayer("Runnable");
        Texture2D grid = GetGridTexture();
        floorMat = GetMaterial("M_Floor", new Color(0.42f, 0.44f, 0.48f), grid);
        runnableMat = GetMaterial("M_Runnable", new Color(1f, 0.5f, 0.1f), grid);
        propMat = GetMaterial("M_Prop", new Color(0.25f, 0.45f, 0.75f), grid);
        boundaryMat = GetMaterial("M_Boundary", new Color(0.2f, 0.2f, 0.23f), grid);

        GameObject existing = GameObject.Find("Environment");
        if (existing != null) Object.DestroyImmediate(existing);
        root = new GameObject("Environment").transform;

        BuildShell();
        BuildJumpGaps();
        BuildHeightSteps();
        BuildWallRun();
        BuildWallChain();
        BuildAngles();
        BuildSlide();
        BuildClimb();
        BuildCurvedWall();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Floor is 72 x 112 m. The player spawns at the origin facing +Z.
    static void BuildShell()
    {
        Station("Shell");
        Box("Floor", new Vector3(0f, -0.5f, 44f), new Vector3(72f, 1f, 112f), floorMat);
        Box("Boundary_West", new Vector3(-36.5f, 2.5f, 44f), new Vector3(1f, 5f, 114f), boundaryMat);
        Box("Boundary_East", new Vector3(36.5f, 2.5f, 44f), new Vector3(1f, 5f, 114f), boundaryMat);
        Box("Boundary_South", new Vector3(0f, 2.5f, -12.5f), new Vector3(72f, 5f, 1f), boundaryMat);
        Box("Boundary_North", new Vector3(0f, 2.5f, 100.5f), new Vector3(72f, 5f, 1f), boundaryMat);
        Box("SpawnPad", new Vector3(0f, 0.01f, 0f), new Vector3(3f, 0.02f, 3f), propMat, collider: false);
    }

    // Station 1: platforms 1 m high with gaps of 2 to 6 m, for tuning jump distance,
    // coyote time and jump buffering.
    static void BuildJumpGaps()
    {
        Station("1_JumpGaps");
        const float x = -27f;
        Label("1  JUMP GAPS", new Vector3(x, 4.5f, 8f), 1f);
        Box("Step", new Vector3(x, 0.25f, 9f), new Vector3(4f, 0.5f, 2f), propMat);

        float z = 10f;
        for (int gap = 2; gap <= 7; gap++)
        {
            Box("Platform_" + (gap - 2), new Vector3(x, 0.5f, z + 2f), new Vector3(4f, 1f, 4f), propMat);
            z += 4f;
            if (gap == 7) break;
            Label(gap + " m", new Vector3(x, 2.2f, z + gap * 0.5f), 0.6f);
            z += gap;
        }
    }

    // Station 2: blocks from 0.5 to 3 m high, for tuning jump height now and mantling later.
    static void BuildHeightSteps()
    {
        Station("2_HeightSteps");
        const float x = -33f;
        Label("2  HEIGHTS", new Vector3(x, 6f, 8f), 0.8f);
        for (int i = 0; i < 6; i++)
        {
            float height = 0.5f * (i + 1);
            float z = 12f + i * 7f;
            Box("Step_" + height.ToString("0.0"), new Vector3(x, height * 0.5f, z), new Vector3(3f, height, 3f), propMat);
            Label(height.ToString("0.0") + " m", new Vector3(x, height + 0.9f, z), 0.6f);
        }
    }

    // Station 3: one long wall on the left, then one on the right facing a grey wall
    // that is not on the Runnable layer, to prove the layer filter works.
    static void BuildWallRun()
    {
        Station("3_WallRun");
        Label("3  WALL RUN", new Vector3(-9f, 7.5f, 9f), 1f);
        Wall("Wall_Left", new Vector3(-12f, 0f, 22f), 24f, 0f, WallHeight, true);
        Wall("Wall_Right", new Vector3(-6f, 0f, 46f), 20f, 0f, WallHeight, true);
        Wall("Wall_NotRunnable", new Vector3(-12f, 0f, 46f), 20f, 0f, WallHeight, false);
        Label("NOT RUNNABLE", new Vector3(-12f, 7f, 36f), 0.6f);
    }

    // Station 4: two platforms 3 m up with a 30 m gap between them. The only way across
    // is to chain three staggered walls with wall jumps.
    static void BuildWallChain()
    {
        Station("4_WallChain");
        const float x = 9f;
        Label("4  WALL CHAIN", new Vector3(x, 10f, 13f), 1f);
        Ramp("Ramp_Up", new Vector3(x, 0f, -1f), new Vector3(x, 3f, 8f), 4f, propMat);
        Box("Platform_Start", new Vector3(x, 1.5f, 11f), new Vector3(6f, 3f, 6f), propMat);
        Wall("Wall_1", new Vector3(x - 4f, 0f, 19f), 12f, 0f, 8f, true);
        Wall("Wall_2", new Vector3(x + 4f, 0f, 29f), 12f, 0f, 8f, true);
        Wall("Wall_3", new Vector3(x - 4f, 0f, 39f), 12f, 0f, 8f, true);
        Box("Platform_End", new Vector3(x, 1.5f, 47f), new Vector3(6f, 3f, 6f), propMat);
    }

    // Station 5: angled walls, an inside corner and a free-standing pillar, for
    // checking wall normals and what happens where a wall ends.
    static void BuildAngles()
    {
        Station("5_Angles");
        const float x = 27f;
        Label("5  ANGLES & CORNERS", new Vector3(x, 7.5f, 7f), 1f);
        Wall("Wall_Angled_A", new Vector3(x - 3f, 0f, 14f), 10f, 20f, WallHeight, true);
        Wall("Wall_Angled_B", new Vector3(x + 3f, 0f, 25f), 10f, -20f, WallHeight, true);
        Wall("Corner_Side", new Vector3(x + 5f, 0f, 38f), 10f, 0f, WallHeight, true);
        Wall("Corner_Back", new Vector3(x + 0.5f, 0f, 43.5f), 10f, 90f, WallHeight, true);
        Box("Pillar", new Vector3(x - 2f, WallHeight * 0.5f, 52f), new Vector3(4f, WallHeight, 4f), runnableMat, runnable: true);
    }

    // Station 6: a ramp down to build speed, then a 1.2 m high gap that only a slide fits under.
    static void BuildSlide()
    {
        Station("6_Slide");
        const float x = -18f;
        Label("6  SLIDE", new Vector3(x, 6.5f, 64f), 1f);
        Ramp("Ramp_Up", new Vector3(x + 12f, 0f, 67f), new Vector3(x + 3f, 3f, 67f), 4f, propMat);
        Box("Platform_Top", new Vector3(x, 1.5f, 67f), new Vector3(6f, 3f, 6f), propMat);
        Ramp("Ramp_Down", new Vector3(x, 0f, 82f), new Vector3(x, 3f, 70f), 6f, propMat);
        Box("Tunnel_Roof", new Vector3(x, 1.45f, 90f), new Vector3(7f, 0.5f, 6f), propMat);
        Box("Tunnel_PostLeft", new Vector3(x - 3.25f, 0.6f, 90f), new Vector3(0.5f, 1.2f, 6f), propMat);
        Box("Tunnel_PostRight", new Vector3(x + 3.25f, 0.6f, 90f), new Vector3(0.5f, 1.2f, 6f), propMat);
        Label("1.2 m", new Vector3(x, 2.4f, 86.8f), 0.6f);
    }

    // Station 7: two tall walls 4 m apart with a 10 m ledge behind them, for climbing
    // by wall jumping from side to side.
    static void BuildClimb()
    {
        Station("7_WallJumpClimb");
        Label("7  WALL-JUMP CLIMB", new Vector3(0f, 15.5f, 75f), 1f);
        Wall("Wall_ClimbLeft", new Vector3(-2.5f, 0f, 79f), 6f, 0f, 14f, true);
        Wall("Wall_ClimbRight", new Vector3(2.5f, 0f, 79f), 6f, 0f, 14f, true);
        Box("Ledge", new Vector3(0f, 5f, 86f), new Vector3(8f, 10f, 8f), propMat);
        Label("10 m", new Vector3(0f, 11f, 82f), 0.6f);
    }

    // Station 8: a 120 degree arc made of short straight segments, so the wall normal
    // changes a little every couple of metres.
    static void BuildCurvedWall()
    {
        Station("8_CurvedWall");
        Vector3 centre = new Vector3(20f, 0f, 80f);
        const float radius = 12f;
        const int segments = 10;
        const float arc = 120f;
        Label("8  CURVED WALL", new Vector3(centre.x, 7.5f, 66f), 1f);

        float step = arc / segments;
        float length = 2f * (radius + WallThickness) * Mathf.Sin(step * 0.5f * Mathf.Deg2Rad);
        for (int i = 0; i < segments; i++)
        {
            float angle = -arc * 0.5f + step * (i + 0.5f);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = centre + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * (radius + WallThickness * 0.5f);
            Wall("Curve_" + i, pos, length, -angle, WallHeight, true);
        }
    }

    static void Station(string name)
    {
        station = new GameObject(name).transform;
        station.SetParent(root, false);
    }

    // A wall standing on the floor at basePos, running along its local Z axis.
    static void Wall(string name, Vector3 basePos, float length, float yaw, float height, bool runnable)
    {
        Vector3 pos = basePos + Vector3.up * (height * 0.5f);
        Vector3 size = new Vector3(WallThickness, height, length);
        Box(name, pos, size, runnable ? runnableMat : boundaryMat, Quaternion.Euler(0f, yaw, 0f), runnable);
    }

    // A sloped slab whose top surface runs from the low point to the high point.
    static void Ramp(string name, Vector3 low, Vector3 high, float width, Material mat)
    {
        const float thickness = 0.5f;
        Vector3 slope = high - low;
        Quaternion rot = Quaternion.LookRotation(slope.normalized, Vector3.up);
        Vector3 pos = (low + high) * 0.5f - rot * Vector3.up * (thickness * 0.5f);
        Box(name, pos, new Vector3(width, thickness, slope.magnitude), mat, rot);
    }

    static void Box(string name, Vector3 pos, Vector3 size, Material mat,
        Quaternion? rotation = null, bool runnable = false, bool collider = true)
    {
        ProBuilderMesh pb = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
        GameObject go = pb.gameObject;
        go.name = name;
        go.transform.SetParent(station, false);
        go.transform.SetPositionAndRotation(pos, rotation ?? Quaternion.identity);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        pb.ToMesh();
        pb.Refresh();

        if (collider) go.AddComponent<BoxCollider>().size = size;
        if (runnable) go.layer = runnableLayer;
        go.isStatic = true;
    }

    // Floating text that reads correctly when looking along +Z from the spawn.
    static void Label(string text, Vector3 pos, float height)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject go = new GameObject("Label_" + text);
        go.transform.SetParent(station, false);
        go.transform.position = pos;

        TextMesh mesh = go.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.font = font;
        mesh.fontSize = 96;
        mesh.characterSize = height * 0.1f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = Color.white;
        go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
    }

    static Material GetMaterial(string name, Color colour, Texture2D grid)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", colour);
        mat.SetTexture("_BaseMap", grid);
        mat.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // A white tile with a darker border. ProBuilder's auto UVs repeat it once per metre,
    // so every surface shows a 1 m grid for judging distances.
    static Texture2D GetGridTexture()
    {
        if (!File.Exists(GridTexturePath))
        {
            const int size = 128;
            const int line = 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool edge = x < line || y < line || x >= size - line || y >= size - line;
                    float v = edge ? 0.7f : 1f;
                    tex.SetPixel(x, y, new Color(v, v, v, 1f));
                }
            }
            File.WriteAllBytes(GridTexturePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(GridTexturePath);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(GridTexturePath);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.anisoLevel = 8;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(GridTexturePath);
    }
}
