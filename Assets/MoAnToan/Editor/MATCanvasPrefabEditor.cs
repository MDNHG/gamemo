using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan.Editor
{
    public static class MATCanvasPrefabEditor
    {
        const string PrefabPath = "Assets/MoAnToan/Scripts/Game2/Canvas.prefab";
        const string GenPath1 = "Assets/MoAnToan/Art/Sprites/Gen/Chibi Mining Safety Control Room UI-1.png";
        const string GenPath2 = "Assets/MoAnToan/Art/Sprites/Gen/Colorful Chibi Mining Game UI Asset Sheet.png";

        [MenuItem("Mỏ An Toàn/Cập Nhật Canvas Prefab Game 2")]
        public static void UpdateCanvasPrefab()
        {
            // 1. Load sub-sprites from Gen textures
            var sprites1 = LoadSpritesDict(GenPath1);
            var sprites2 = LoadSpritesDict(GenPath2);

            // 2. Open Prefab for editing
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (root == null)
            {
                Debug.LogError("Không thể load prefab: " + PrefabPath);
                return;
            }

            try
            {
                // Ensure CanvasScaler settings
                var scaler = root.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(420, 860);
                    scaler.matchWidthOrHeight = 0f; // Width match for portrait
                }

                var col = root.transform.Find("Column");
                var dispCol = col.Find("DispatcherColumn");

                // --- A. BỐ CỤC LƯỚI CAMERA 2x2 TRONG MONITOR FRAME ---
                var mf = dispCol.Find("MonitorFrame");
                var hl = mf.GetComponent<HorizontalLayoutGroup>();
                if (hl != null)
                {
                    hl.childControlWidth = true;
                    hl.childControlHeight = true;
                    hl.childForceExpandWidth = false;
                    hl.childForceExpandHeight = true;
                    hl.spacing = 6;
                    hl.padding = new RectOffset(6, 6, 6, 6);
                }

                var leftLE = mf.Find("LeftCol").GetComponent<LayoutElement>();
                if (leftLE != null) { leftLE.minWidth = 92; leftLE.preferredWidth = 96; leftLE.flexibleWidth = -1; }

                var midCol = mf.Find("MidCol");
                var midLE = midCol.GetComponent<LayoutElement>();
                if (midLE != null) { midLE.minWidth = 196; midLE.preferredWidth = 200; midLE.flexibleWidth = -1; }

                var rightLE = mf.Find("RightCol").GetComponent<LayoutElement>();
                if (rightLE != null) { rightLE.minWidth = 92; rightLE.preferredWidth = 96; rightLE.flexibleWidth = -1; }

                var gl = midCol.GetComponent<GridLayoutGroup>();
                if (gl != null)
                {
                    gl.cellSize = new Vector2(96, 75);
                    gl.spacing = new Vector2(4, 4);
                    gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    gl.constraintCount = 2;
                    gl.padding = new RectOffset(2, 2, 2, 2);
                }

                // --- B. ÁNH XẠ TOÀN BỘ SPRITE TỪ SPRITES/GEN ---
                // Monitor HUD Frame
                SetImage(mf.Find("HUD_Bg"), sprites1, "Chibi Mining Safety Control Room UI-1_2", Image.Type.Sliced);

                // TopBar
                var topBar = dispCol.Find("TopBar");
                SetImage(topBar.Find("AvatarBox/Avatar"), sprites1, "Chibi Mining Safety Control Room UI-1_12", Image.Type.Simple);
                SetImage(topBar.Find("TitleBadge/BadgeBg"), sprites1, "Chibi Mining Safety Control Room UI-1_18", Image.Type.Sliced);
                SetImage(topBar.Find("Stats/Btn"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_8", Image.Type.Simple);

                // SubHeader
                var logoLE = dispCol.Find("SubHeader/Logo")?.GetComponent<LayoutElement>();
                if (logoLE != null) { logoLE.minWidth = 32; logoLE.preferredWidth = 32; logoLE.minHeight = 22; logoLE.preferredHeight = 22; }
                SetImage(dispCol.Find("SubHeader/Logo"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_4", Image.Type.Simple);

                // Telemetry rows - make panels transparent so background monitor slots show through cleanly
                var leftCol = mf.Find("LeftCol");
                ClearPanelColor(leftCol.Find("Row_Khí CH₄/Panel"));
                SetImage(leftCol.Find("Row_Khí CH₄/Image"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_10", Image.Type.Simple);

                ClearPanelColor(leftCol.Find("Row_Nhiệt độ/Panel"));
                SetImage(leftCol.Find("Row_Nhiệt độ/Image"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_5", Image.Type.Simple);

                ClearPanelColor(leftCol.Find("Row_Độ ẩm/Panel"));
                SetImage(leftCol.Find("Row_Độ ẩm/Image"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_6", Image.Type.Simple);

                ClearPanelColor(leftCol.Find("Row_Khu vực/Panel"));
                SetImage(leftCol.Find("Row_Khu vực/Image"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_4", Image.Type.Simple);

                // RightCol Status & Book - transparent panels to blend with monitor
                var rightCol = mf.Find("RightCol");
                ClearPanelColor(rightCol.Find("StatusCard/Panel"));
                SetImage(rightCol.Find("StatusCard/StatusIcon"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_13", Image.Type.Simple);
                ClearPanelColor(rightCol.Find("Btn"));
                SetImage(rightCol.Find("Btn/Image"), sprites2, "Colorful Chibi Mining Game UI Asset Sheet_9", Image.Type.Simple);

                // MidCol Cameras - transparent panels and feeds so the painted CCTV views in the monitor frame show through
                for (int i = 1; i <= 4; i++)
                {
                    var cam = midCol.Find($"Camera 0{i}");
                    if (cam != null)
                    {
                        ClearPanelColor(cam.Find("Panel"));
                        ClearPanelColor(cam.Find("Feed"));
                        var tr = cam.Find("TitleRow");
                        if (tr != null) tr.gameObject.SetActive(false); // background monitor already has "Camera 01..04"
                    }
                }

                // Dispatcher Character
                var dispArea = dispCol.Find("DispatcherArea");
                SetImage(dispArea.Find("DispatcherChar"), sprites1, "Chibi Mining Safety Control Room UI-1_3", Image.Type.Simple);
                var charRT = dispArea.Find("DispatcherChar").GetComponent<RectTransform>();
                if (charRT != null)
                {
                    charRT.anchorMin = new Vector2(0.5f, 0f);
                    charRT.anchorMax = new Vector2(0.5f, 0f);
                    charRT.pivot = new Vector2(0.5f, 0f);
                    charRT.sizeDelta = new Vector2(230, 160);
                    charRT.anchoredPosition = new Vector2(0, -10);
                }

                // Dialogue Bubble
                var bubble = dispCol.Find("DialogueBubble");
                SetImage(bubble.Find("BubbleBg"), sprites1, "Chibi Mining Safety Control Room UI-1_7", Image.Type.Sliced);

                // Action Row: Hint & Buttons
                var actRow = dispCol.Find("ActionRow");
                SetImage(actRow.Find("HintCard/Panel"), sprites1, "Chibi Mining Safety Control Room UI-1_8", Image.Type.Sliced);

                var btnCol = actRow.Find("BtnCol");
                var btnAllowObj = btnCol.Find("Btn_Cho phép làm việc");
                SetImage(btnAllowObj.Find("Panel"), sprites1, "Chibi Mining Safety Control Room UI-1_10", Image.Type.Simple);
                var imgAllow = btnAllowObj.Find("Panel")?.GetComponent<Image>();
                if (imgAllow != null) imgAllow.raycastTarget = true;
                var bAllow = btnAllowObj.GetComponent<Button>();
                if (bAllow != null && imgAllow != null) bAllow.targetGraphic = imgAllow;

                var btnStopObj = btnCol.Find("Btn_Dừng sản xuất");
                SetImage(btnStopObj.Find("Panel"), sprites1, "Chibi Mining Safety Control Room UI-1_11", Image.Type.Simple);
                var imgStop = btnStopObj.Find("Panel")?.GetComponent<Image>();
                if (imgStop != null) imgStop.raycastTarget = true;
                var bStop = btnStopObj.GetComponent<Button>();
                if (bStop != null && imgStop != null) bStop.targetGraphic = imgStop;

                // Ensure Rulebook & Settings buttons have raycastTarget = true
                var imgRule = rightCol.Find("Btn")?.GetComponent<Image>();
                if (imgRule != null) imgRule.raycastTarget = true;
                var imgSettings = topBar.Find("Stats/Btn")?.GetComponent<Image>();
                if (imgSettings != null) imgSettings.raycastTarget = true;

                // Footer
                SetImage(dispCol.Find("Footer/Panel"), sprites1, "Chibi Mining Safety Control Room UI-1_9", Image.Type.Sliced);

                // --- C. GẮN COMPONENT GAME2VIEW VÀ SERIALIZE REFERENCES ---
                var view = root.GetComponent<Game2View>();
                if (view == null) view = root.AddComponent<Game2View>();

                // TopBar fields
                view.levelText = topBar.Find("AvatarBox/LvlPill/Text")?.GetComponent<Text>();
                view.titleBadgeText = topBar.Find("TitleBadge/Text")?.GetComponent<Text>();
                view.heartsText = topBar.Find("Stats/Text")?.GetComponent<Text>();
                view.btnSettings = topBar.Find("Stats/Btn")?.GetComponent<Button>();

                // Telemetry fields
                view.ch4Text = leftCol.Find("Row_Khí CH₄/TextCol")?.GetComponentsInChildren<Text>()[1];
                view.tempText = leftCol.Find("Row_Nhiệt độ/TextCol")?.GetComponentsInChildren<Text>()[1];
                view.humidityText = leftCol.Find("Row_Độ ẩm/TextCol")?.GetComponentsInChildren<Text>()[1];
                view.locationText = leftCol.Find("Row_Khu vực/TextCol")?.GetComponentsInChildren<Text>()[1];

                // Camera Feeds & Buttons (Enable transparent Image for raycast clicks)
                var camButtons = new List<Button>();
                var camFeeds = new List<Image>();
                for (int i = 1; i <= 4; i++)
                {
                    var c = midCol.Find($"Camera 0{i}");
                    if (c != null)
                    {
                        var btn = c.GetComponent<Button>();
                        if (btn == null) btn = c.gameObject.AddComponent<Button>();
                        var img = c.GetComponent<Image>();
                        if (img == null) img = c.gameObject.AddComponent<Image>();
                        img.color = Color.clear;
                        img.raycastTarget = true;
                        btn.targetGraphic = img;

                        camButtons.Add(btn);
                        camFeeds.Add(c.Find("Feed")?.GetComponent<Image>());
                    }
                }
                view.cameraButtons = camButtons.ToArray();
                view.cctvFeeds = camFeeds.ToArray();

                // Status & Rulebook
                view.statusText = rightCol.Find("StatusCard/Text")?.GetComponent<Text>();
                view.statusIcon = rightCol.Find("StatusCard/StatusIcon")?.GetComponent<Image>();
                view.btnRuleBook = rightCol.Find("Btn")?.GetComponent<Button>();

                // Report & Hint
                view.reportText = bubble.Find("Text")?.GetComponent<Text>();
                view.hintText = actRow.Find("HintCard/Panel/Text")?.GetComponent<Text>();

                // Decisions
                view.btnAllowWork = btnCol.Find("Btn_Cho phép làm việc")?.GetComponent<Button>();
                view.btnStopProduction = btnCol.Find("Btn_Dừng sản xuất")?.GetComponent<Button>();

                // Save Prefab
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("<color=#35c76a><b>[Canvas Prefab]</b> Đã cập nhật Canvas.prefab thành công với Sprites/Gen và Game2View!</color>");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
                System.IO.Directory.CreateDirectory("Assets/MoAnToan/Resources/Game2");
                System.IO.File.Copy(PrefabPath, "Assets/MoAnToan/Resources/Game2/Canvas.prefab", true);
                AssetDatabase.Refresh();
            }
        }

        static Dictionary<string, Sprite> LoadSpritesDict(string texturePath)
        {
            var dict = new Dictionary<string, Sprite>();
            var assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (var o in assets)
            {
                if (o is Sprite s) dict[s.name] = s;
            }
            return dict;
        }

        static void ClearPanelColor(Transform target)
        {
            if (target == null) return;
            var img = target.GetComponent<Image>();
            if (img != null) img.color = Color.clear;
        }

        static void SetImage(Transform target, Dictionary<string, Sprite> dict, string spriteName, Image.Type type)
        {
            if (target == null) return;
            var img = target.GetComponent<Image>();
            if (img == null) return;
            if (dict.TryGetValue(spriteName, out var s) && s != null)
            {
                img.sprite = s;
                img.type = type;
            }
        }
    }
}
