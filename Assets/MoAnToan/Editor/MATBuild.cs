#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoAnToan.EditorTools
{
    /// <summary>
    /// Tự tạo scene chính lần đầu mở project, và menu "Mỏ An Toàn" để build Windows / Android / Web.
    /// Bản build nằm trong thư mục Builds/ cạnh Assets.
    /// </summary>
    [InitializeOnLoad]
    public static class MATBuild
    {
        public const string ScenePath = "Assets/MoAnToan/Scenes/MoAnToan.unity";
        const string Product = "An toan ham lo - ban test";

        static MATBuild()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (!File.Exists(ScenePath)) CreateScene(true);
            };
        }

        [MenuItem("Mỏ An Toàn/Tạo lại scene chính", false, 1)]
        static void CreateSceneMenu() { CreateScene(true); }

        static void CreateScene(bool open)
        {
            var active = SceneManager.GetActiveScene();
            if (active.isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.07f, 0.08f, 0.09f);
            c.orthographic = true;
            new GameObject("MoAnToanApp").AddComponent<MoAnToan.App>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("[MoAnToan] Đã tạo scene " + ScenePath + " và đặt làm scene build. Bấm Play để chơi.");
            if (open) EditorSceneManager.OpenScene(ScenePath);
        }

        static void Settings()
        {
            if (!File.Exists(ScenePath)) CreateScene(false);
            PlayerSettings.productName = Product;
            PlayerSettings.companyName = "MoAnToan";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 450;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.moantoan.test");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.moantoan.test");
        }

        static string Root { get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds")); } }

        [MenuItem("Mỏ An Toàn/Build Windows (.exe)", false, 20)]
        public static void BuildWindows()
        {
            Settings();
            Build(BuildTarget.StandaloneWindows64, Path.Combine(Root, "Windows", "AnToanHamLo.exe"));
        }

        [MenuItem("Mỏ An Toàn/Build Android (.apk)", false, 21)]
        public static void BuildAndroid()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Fail("Chưa cài Android Build Support. Unity Hub > Installs > bánh răng > Add modules > Android Build Support.");
                return;
            }
            Settings();
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, Path.Combine(Root, "Android", "AnToanHamLo-" + PlayerSettings.bundleVersion + ".apk"));
        }

        [MenuItem("Mỏ An Toàn/Build Web (WebGL)", false, 22)]
        public static void BuildWeb()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                Fail("Chưa cài Web Build Support. Unity Hub > Installs > bánh răng > Add modules > Web Build Support.");
                return;
            }
            Settings();
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.defaultWebScreenWidth = 450;
            PlayerSettings.defaultWebScreenHeight = 900;
            Build(BuildTarget.WebGL, "Builds/WebGL");
        }

        static bool Build(BuildTarget target, string path)
        {
            if (target != BuildTarget.WebGL) Directory.CreateDirectory(Path.GetDirectoryName(path));
            var o = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = path, target = target, options = BuildOptions.None };
            BuildReport report = BuildPipeline.BuildPlayer(o);
            var s = report.summary;
            if (s.result != BuildResult.Succeeded)
            {
                Fail("Build " + s.result + " với " + s.totalErrors + " lỗi. Xem cửa sổ Console.");
                return false;
            }
            Debug.Log("[MoAnToan] Build xong: " + path + " (" + (s.totalSize / (1024f * 1024f)).ToString("0.0") + " MB)");
            if (!Application.isBatchMode) EditorUtility.RevealInFinder(target == BuildTarget.WebGL ? Path.GetFullPath(path) : path);
            return true;
        }

        static void Fail(string msg)
        {
            Debug.LogError("[MoAnToan] " + msg);
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Mỏ An Toàn", msg, "OK");
        }
    }
}
#endif
