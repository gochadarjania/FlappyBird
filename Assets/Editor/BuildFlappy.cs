using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

public static class BuildFlappy
{
    static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/" + name + ".png");
    static GameObject SpriteObject(string name, Sprite sprite, Vector3 position, int order)
    {
        var obj = new GameObject(name);
        obj.transform.position = position;
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        return obj;
    }
    static Text Label(Transform parent, string name, float y, int size)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.15f, 0.22f, 0.24f);
        text.raycastTarget = false;
        text.rectTransform.sizeDelta = new Vector2(290, 90);
        text.rectTransform.anchoredPosition = new Vector2(0, y);
        return text;
    }

    [MenuItem("Flappy/Rebuild Scene")]
    public static void Create()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" }))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 2.56f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.32f, 0.75f, 0.79f);
        camera.gameObject.AddComponent<AudioListener>();
        SpriteObject("Background", Art("background-day"), Vector3.zero, -10);
        var ground = SpriteObject("Ground", Art("base"), new Vector3(0, -2.45f, 0), 2);
        ground.transform.localScale = Vector3.one;
        ground.AddComponent<BoxCollider2D>();

        var bird = SpriteObject("Bird", Art("yellowbird-midflap"), new Vector3(-0.6f, 0.2f, 0), 3);
        var rb = bird.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.6f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bird.AddComponent<CircleCollider2D>().radius = 0.11f;
        var game = bird.AddComponent<FlappyGame>();
        game.bird = rb;
        game.birdImage = bird.GetComponent<SpriteRenderer>();
        game.wingFrames = new[] { Art("yellowbird-upflap"), Art("yellowbird-midflap"), Art("yellowbird-downflap") };
        game.audioSource = bird.AddComponent<AudioSource>();
        game.audioSource.playOnAwake = false;
        game.flapSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/wing.wav");
        game.hitSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/hit.wav");
        game.pointSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/point.wav");

        var pair = new GameObject("PipePair");
        pair.AddComponent<PipePair>();
        for (int i = 0; i < 2; i++)
        {
            float sign = i == 0 ? -1 : 1;
            var pipe = SpriteObject(i == 0 ? "LowerPipe" : "UpperPipe", Art("pipe"), new Vector3(0, sign * 2.4f, 0), 1);
            pipe.transform.SetParent(pair.transform);
            if (i == 1) pipe.transform.rotation = Quaternion.Euler(0, 0, 180);
            pipe.AddComponent<BoxCollider2D>();
        }
        game.pipePrefab = PrefabUtility.SaveAsPrefabAsset(pair, "Assets/PipePair.prefab");
        Object.DestroyImmediate(pair);

        var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(360, 640);
        scaler.matchWidthOrHeight = 1;
        game.scoreText = Label(canvas.transform, "Score", 245, 48);
        game.scoreText.color = Color.white;
        var panel = new GameObject("Menu", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 280);
        panel.GetComponent<Image>().color = new Color(1, 0.98f, 0.87f, 0.97f);
        game.panel = panel;
        game.titleText = Label(panel.transform, "Title", 85, 32);
        game.detailText = Label(panel.transform, "Details", 5, 20);
        var button = new GameObject("PlayRestart", typeof(RectTransform), typeof(Image), typeof(Button));
        button.transform.SetParent(panel.transform, false);
        var rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220, 55);
        rect.anchoredPosition = new Vector2(0, -85);
        button.GetComponent<Image>().color = new Color(0.95f, 0.65f, 0.19f);
        game.buttonText = Label(button.transform, "Label", 0, 24);
        UnityEventTools.AddPersistentListener(button.GetComponent<Button>().onClick, game.PlayOrRestart);
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var scene = EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/Scenes/FlappyBird.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/FlappyBird.unity", true) };
        PlayerSettings.productName = "Flappy Bird";
        PlayerSettings.defaultScreenWidth = 432;
        PlayerSettings.defaultScreenHeight = 768;
        PlayerSettings.resizableWindow = false;
        AssetDatabase.SaveAssets();
        Debug.Log("FLAPPY_SETUP_COMPLETE");
    }
}

