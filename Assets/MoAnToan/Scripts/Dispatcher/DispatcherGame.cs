using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Màn 1: Trực ban an toàn — Giám sát, Phát hiện & Xử lý sự cố theo QCVN 01:2011/BCT.
    /// Giao diện đồ họa chuẩn Casual 2D Chibi theo thiết kế minh họa.
    /// </summary>
    public class DispatcherGame : MonoBehaviour
    {
        public static DispatcherGame I;

        [Serializable]
        public class Scenario
        {
            public string title;
            public string report;
            public string hint;
            public float ch4;
            public int temp;
            public int humidity;
            public string location;
            public bool isNormal; // true = bình thường, false = cảnh báo
            public bool correctDecisionIsWork; // true = Cho phép làm việc, false = Dừng sản xuất
            public string explanation;
        }

        static readonly List<Scenario> SCENARIOS = new List<Scenario>
        {
            new Scenario
            {
                title = "Ống gió mềm bị gập",
                report = "Tổ sản xuất báo cáo: Tại gương lò III, ống gió mềm bị gập nhẹ.\n<b>Bạn sẽ xử lý như thế nào?</b>",
                hint = "Ống gió bị gập làm giảm lưu lượng gió sạch tới gương lò cụt, dễ gây tích tụ khí độc và khí mê-tan (CH₄).",
                ch4 = 0.8f,
                temp = 32,
                humidity = 68,
                location = "Gương lò III",
                isNormal = false,
                correctDecisionIsWork = false, // Dừng sản xuất
                explanation = "CHÍNH XÁC!\nTheo quy chuẩn an toàn thông gió mỏ, khi ống gió bị gập hoặc rách phải lập tức khắc phục nắn thẳng hoặc thay thế để đảm bảo đủ gió sạch tới gương lò trước khi tiếp tục làm việc."
            },
            new Scenario
            {
                title = "Nồng độ khí CH₄ tăng cao",
                report = "Cảm biến cố định báo: Nồng độ khí mê-tan (CH₄) tại luồng gió hồi đạt 1.3%.\n<b>Bạn sẽ xử lý như thế nào?</b>",
                hint = "QCVN 01:2011/BCT quy định: Nồng độ CH₄ trong luồng gió chung hoặc gió hồi vượt quá 1.0% là cực kỳ nguy hiểm.",
                ch4 = 1.3f,
                temp = 34,
                humidity = 72,
                location = "Lò hồi mức -150",
                isNormal = false,
                correctDecisionIsWork = false, // Dừng sản xuất
                explanation = "QUYẾT ĐỊNH XUẤT SẮC!\nKhi nồng độ CH₄ ≥ 1.0%, bắt buộc phải dừng sản xuất, cắt điện toàn bộ khu vực, rút công nhân ra luồng gió sạch và tăng cường thông gió giải tỏa khí."
            },
            new Scenario
            {
                title = "Đầu ca làm việc bình thường",
                report = "Đội kiểm tra báo cáo: Cửa gió sổ đôi đóng kín đúng quy định, công nhân đủ trang bị bảo hộ, nồng độ khí ổn định.\n<b>Bạn sẽ xử lý như thế nào?</b>",
                hint = "Thông số khí CH₄ < 0.5%, nhiệt độ dưới 30°C và quy trình bảo hộ hoàn toàn đạt chuẩn.",
                ch4 = 0.3f,
                temp = 26,
                humidity = 62,
                location = "Gương lò I",
                isNormal = true,
                correctDecisionIsWork = true, // Cho phép làm việc
                explanation = "CHÍNH XÁC!\nTất cả chỉ số an toàn và tư trang bảo hộ đều đáp ứng quy chuẩn kỹ thuật. Cho phép phân xưởng tiến hành sản xuất ca làm việc an toàn!"
            },
            new Scenario
            {
                title = "Vỉa than bốc khói mỏng",
                report = "Công nhân báo cáo: Xuất hiện vệt khói mỏng và mùi khét nhựa thông bốc lên từ vỉa than cũ.\n<b>Bạn sẽ xử lý như thế nào?</b>",
                hint = "Mùi nhựa thông và khói mỏng là dấu hiệu điển hình giai đoạn ủ nhiệt của hiện tượng THAN TỰ CHÁY.",
                ch4 = 0.6f,
                temp = 37,
                humidity = 80,
                location = "Vỉa than cũ số 9",
                isNormal = false,
                correctDecisionIsWork = false, // Dừng sản xuất
                explanation = "XỬ LÝ CHÍNH XÁC!\nThan tự cháy có thể gây nổ khí và ngộ độc khí CO hàng loạt. Phải dừng sản xuất, phong tỏa khu vực và báo đội Cấp cứu mỏ xử lý ngay lập tức!"
            }
        };

        int currentScenarioIdx = 0;
        int hearts = 3;
        int xp = 0;

        RectTransform rootScreen;
        Text ch4Txt, tempTxt, humTxt, locTxt;
        Text statusTxt;
        Image statusIcon;
        Text reportTxt, hintTxt;
        Text heartDisplayTxt, xpDisplayTxt;
        public Button btnWork, btnStop;

        public static void Open()
        {
            var go = new GameObject("DispatcherGameController");
            I = go.AddComponent<DispatcherGame>();
            I.StartGame();
        }

        void StartGame()
        {
            currentScenarioIdx = 0;
            hearts = 3;
            xp = 0;
            BuildUI();
            LoadScenario(currentScenarioIdx);
        }

        void BuildUI()
        {
            rootScreen = App.I.NewScreen();

            // Background Control Room Image
            var bgImg = MATArt.Img(rootScreen, MATArt.Env("BG_ControlRoom"), "ControlRoom_BG");
            UIK.Stretch(bgImg.rectTransform);

            // Container chính giới hạn 420-560px
            var col = UIK.RT("DispatcherColumn", rootScreen);
            UIK.Stretch(col);
            var vl = UIK.VL(col.gameObject, 6, 8, false);
            vl.childControlWidth = true;
            vl.childForceExpandWidth = true;

            // 1. TOP BAR
            BuildTopBar(col);

            // 2. SUB HEADER (Logo + Tên công ty)
            BuildSubHeader(col);

            // 3. MAIN DASHBOARD MONITOR (Matrix)
            BuildDashboardMonitor(col);

            // 4. DISPATCHER DESK & CHARACTER
            BuildDispatcherArea(col);

            // 5. FIELD REPORT SPEECH BUBBLE
            BuildDialogueBubble(col);

            // 6. ACTION & HINT ROW
            BuildActionRow(col);

            // 7. FOOTER BAR
            BuildFooter(col);
        }

        // ========================================================================= 1. TOP BAR
        void BuildTopBar(Transform parent)
        {
            var bar = UIK.RT("TopBar", parent);
            UIK.LE(bar.gameObject, -1, 56, -1, -1, 56);
            var hl = UIK.HL(bar.gameObject, 6, 2, false);
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;
            hl.childControlWidth = false;
            hl.childControlHeight = false;

            // Avatar & Level Box
            var avaBox = UIK.RT("AvatarBox", bar);
            avaBox.sizeDelta = new Vector2(52, 52);
            var avaImg = MATArt.Img(avaBox, MATArt.Icon("Icon_Avatar_Chibi"), "Avatar");
            UIK.Stretch(avaImg.rectTransform);
            var lvlPill = UIK.RT("LvlPill", avaBox);
            lvlPill.anchorMin = new Vector2(0, 0); lvlPill.anchorMax = new Vector2(1, 0); lvlPill.pivot = new Vector2(0.5f, 0);
            lvlPill.sizeDelta = new Vector2(0, 16);
            lvlPill.anchoredPosition = new Vector2(0, -4);
            var pillBg = MATArt.Sliced(lvlPill, MATArt.UI("Slot_Card"));
            UIK.Stretch(pillBg.rectTransform);
            var lvlTxt = UIK.Txt(lvlPill, "Lv.1 ⭐0", 9, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIK.Stretch(lvlTxt.rectTransform);

            // Title Badge
            var badgeObj = UIK.RT("TitleBadge", bar);
            badgeObj.sizeDelta = new Vector2(230, 46);
            var badgeBg = MATArt.Sliced(badgeObj, MATArt.UI("Badge_Title"), "BadgeBg");
            UIK.Stretch(badgeBg.rectTransform);
            var bhl = UIK.HL(badgeObj.gameObject, 6, 6, false);
            bhl.childAlignment = TextAnchor.MiddleCenter;
            bhl.childForceExpandWidth = false;
            bhl.childForceExpandHeight = false;
            bhl.childControlWidth = false;
            bhl.childControlHeight = false;

            var shieldImg = MATArt.Img(badgeObj, MATArt.Icon("Icon_Shield"), "Shield");
            shieldImg.rectTransform.sizeDelta = new Vector2(28, 28);
            var bTxt = UIK.Txt(badgeObj, "Trực ban an toàn\n<size=10><color=#93c5fd>Giám sát • Phát hiện • Xử lý</color></size>", 12, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            bTxt.rectTransform.sizeDelta = new Vector2(180, 36);

            // Hearts & Settings
            var statBox = UIK.RT("Stats", bar);
            statBox.sizeDelta = new Vector2(100, 44);
            var shl = UIK.HL(statBox.gameObject, 4, 0, false);
            shl.childAlignment = TextAnchor.MiddleRight;
            shl.childForceExpandWidth = false;
            shl.childForceExpandHeight = false;
            shl.childControlWidth = false;
            shl.childControlHeight = false;

            var hImg = MATArt.Img(statBox, MATArt.Icon("Icon_Heart"), "HeartIcon");
            hImg.rectTransform.sizeDelta = new Vector2(22, 22);
            heartDisplayTxt = UIK.Txt(statBox, "3", 15, UIK.Hex("#ff4d4d"), FontStyle.Bold, TextAnchor.MiddleLeft);
            heartDisplayTxt.rectTransform.sizeDelta = new Vector2(16, 26);

            var gearBtn = UIK.Btn(statBox, "", UIK.Hex("#1e293b"), UIK.Ink, 14, () => App.I.ShowMenu(), 36);
            gearBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
            var gImg = MATArt.Img(gearBtn.transform, MATArt.Icon("Icon_Gear"), "Gear");
            UIK.Stretch(gImg.rectTransform, 4, 4, 4, 4);
        }

        // ========================================================================= 2. SUB HEADER
        void BuildSubHeader(Transform parent)
        {
            var sh = UIK.RT("SubHeader", parent);
            UIK.LE(sh.gameObject, -1, 24, -1, -1, 24);
            var hl = UIK.HL(sh.gameObject, 6, 2, false);
            hl.childAlignment = TextAnchor.MiddleCenter;

            var logo = MATArt.Img(sh, MATArt.Icon("Logo_MongDuong"), "Logo");
            UIK.LE(logo.gameObject, 30, 20, -1, 30, 20);
            UIK.Txt(sh, "CÔNG TY THAN MÔNG DƯƠNG - VINACOMIN", 12, UIK.Hex("#93c5fd"), FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        // ========================================================================= 3. DASHBOARD MONITOR
        void BuildDashboardMonitor(Transform parent)
        {
            var monObj = UIK.RT("MonitorFrame", parent);
            UIK.LE(monObj.gameObject, -1, 250, -1, -1, 250);
            var bgHud = MATArt.Sliced(monObj, MATArt.UI("Frame_HUD"), "HUD_Bg");
            UIK.Stretch(bgHud.rectTransform);

            var hl = UIK.HL(monObj.gameObject, 6, 8, false);
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = true;
            hl.childForceExpandWidth = false;

            // --- CỘT TRÁI: THÔNG SỐ HIỆN TẠI (Telemetry) ---
            var leftCol = UIK.RT("LeftCol", monObj);
            UIK.LE(leftCol.gameObject, 100, 234, -1, 100, 234);
            var lvl = UIK.VL(leftCol.gameObject, 4, 4, false);
            lvl.childAlignment = TextAnchor.UpperCenter;

            UIK.Txt(leftCol, "Thông số hiện tại", 11, UIK.Hex("#93c5fd"), FontStyle.Bold, TextAnchor.MiddleCenter);
            ch4Txt = CreateTelemetryRow(leftCol, "Icon_Gas", "Khí CH₄", "0.8 %", UIK.Hex("#38bdf8"));
            tempTxt = CreateTelemetryRow(leftCol, "Icon_Temp", "Nhiệt độ", "32 °C", UIK.Hex("#f87171"));
            humTxt = CreateTelemetryRow(leftCol, "Icon_Humidity", "Độ ẩm", "68 %", UIK.Hex("#60a5fa"));
            locTxt = CreateTelemetryRow(leftCol, "Icon_Location", "Khu vực", "Gương III", UIK.Hex("#fbbf24"));

            // --- CỘT GIỮA: 2x2 CAMERA CCTV ---
            var midCol = UIK.RT("MidCol", monObj);
            UIK.LE(midCol.gameObject, 185, 234, -1, 185, 234);
            var mgl = midCol.gameObject.AddComponent<GridLayoutGroup>();
            mgl.cellSize = new Vector2(88, 112);
            mgl.spacing = new Vector2(5, 5);
            mgl.padding = new RectOffset(2, 2, 2, 2);

            CreateCCTVBox(midCol, "Camera 01", "CCTV_Cam01");
            CreateCCTVBox(midCol, "Camera 02", "CCTV_Cam02");
            CreateCCTVBox(midCol, "Camera 03", "CCTV_Cam03");
            CreateCCTVBox(midCol, "Camera 04", "CCTV_Cam04");

            // --- CỘT PHẢI: TÌNH TRẠNG CHUNG & SỔ QUY ĐỊNH ---
            var rightCol = UIK.RT("RightCol", monObj);
            UIK.LE(rightCol.gameObject, 105, 234, -1, 105, 234);
            var rvl = UIK.VL(rightCol.gameObject, 4, 4, false);
            rvl.childAlignment = TextAnchor.UpperCenter;

            UIK.Txt(rightCol, "Tình trạng chung", 11, UIK.Hex("#93c5fd"), FontStyle.Bold, TextAnchor.MiddleCenter);

            // Status Card
            var statCard = UIK.RT("StatusCard", rightCol);
            UIK.LE(statCard.gameObject, 96, 70, -1, 96, 70);
            var scImg = MATArt.Sliced(statCard, MATArt.UI("Slot_Card"));
            UIK.Stretch(scImg.rectTransform);
            var scvl = UIK.VL(statCard.gameObject, 2, 4, false);
            scvl.childAlignment = TextAnchor.MiddleCenter;
            statusIcon = MATArt.Img(statCard, MATArt.Icon("Icon_Check"), "StatusIcon");
            UIK.LE(statusIcon.gameObject, 32, 32, -1, 32, 32);
            statusTxt = UIK.Txt(statCard, "BÌNH THƯỜNG", 11, UIK.Hex("#22c55e"), FontStyle.Bold, TextAnchor.MiddleCenter);

            // Graph Wave
            var gImg = MATArt.Img(rightCol, MATArt.Icon("Icon_Graph"), "GraphWave");
            UIK.LE(gImg.gameObject, 96, 50, -1, 96, 50);

            // Sổ quy định Button
            var ruleBtn = UIK.Btn(rightCol, "📖 Sổ quy định >", UIK.Hex("#1e3a8a"), UIK.Ink, 11, ShowRuleModal, 34);
            UIK.LE(ruleBtn.gameObject, 98, 34, -1, 98, 34);
        }

        Text CreateTelemetryRow(Transform parent, string iconName, string label, string val, Color valColor)
        {
            var row = UIK.RT("Row_" + label, parent);
            UIK.LE(row.gameObject, 96, 44, -1, 96, 44);
            var bg = MATArt.Sliced(row, MATArt.UI("Slot_Card"));
            UIK.Stretch(bg.rectTransform);

            var hl = UIK.HL(row.gameObject, 4, 4, false);
            hl.childAlignment = TextAnchor.MiddleLeft;

            var ic = MATArt.Img(row, MATArt.Icon(iconName));
            UIK.LE(ic.gameObject, 20, 20, -1, 20, 20);

            var tCol = UIK.RT("TextCol", row);
            var vl = UIK.VL(tCol.gameObject, 1, 0, false);
            UIK.Txt(tCol, label, 10, UIK.Hex("#94a3b8"), FontStyle.Normal, TextAnchor.MiddleLeft);
            var vTxt = UIK.Txt(tCol, val, 12, valColor, FontStyle.Bold, TextAnchor.MiddleLeft);
            return vTxt;
        }

        void CreateCCTVBox(Transform parent, string camName, string spriteName)
        {
            var box = UIK.RT(camName, parent);
            var f = MATArt.Sliced(box, MATArt.UI("Frame_Camera"));
            UIK.Stretch(f.rectTransform);

            var vl = UIK.VL(box.gameObject, 2, 2, false);
            vl.childAlignment = TextAnchor.UpperCenter;

            // Title Row
            var titleRow = UIK.RT("TitleRow", box);
            UIK.LE(titleRow.gameObject, -1, 14, 1, -1, 14);
            var thl = UIK.HL(titleRow.gameObject, 2, 0, false);
            thl.childAlignment = TextAnchor.MiddleLeft;
            var dot = UIK.Box(titleRow, UIK.Hex("#22c55e"), null, "Dot");
            UIK.LE(dot.gameObject, 6, 6, -1, 6, 6);
            UIK.Txt(titleRow, camName, 9, UIK.Hex("#93c5fd"), FontStyle.Bold, TextAnchor.MiddleLeft);

            // Screen Image
            var cImg = MATArt.Img(box, MATArt.CCTV(spriteName), "Feed");
            UIK.LE(cImg.gameObject, 82, 80, -1, 82, 80);

            // Add click to inspect
            var b = box.gameObject.AddComponent<Button>();
            b.onClick.AddListener(() =>
            {
                Sfx.Play("tap");
                UIK.Modal((m, close) =>
                {
                    UIK.Txt(m, "📸 " + camName + " — Quan sát trực tiếp", 16, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
                    var bigImg = MATArt.Img(m, MATArt.CCTV(spriteName), "BigFeed");
                    UIK.LE(bigImg.gameObject, 280, 210, -1, 280, 210);
                    UIK.Txt(m, "Đường truyền cáp quang phòng nổ: Tín hiệu 100%\nTheo dõi luồng gió, đường ray và thiết bị hầm lò.", 13, UIK.Hex("#cbd5e1"), FontStyle.Normal, TextAnchor.MiddleCenter);
                    UIK.Btn(m, "Đóng", UIK.Hex("#2563eb"), UIK.Ink, 14, () => close(), 42);
                });
            });
        }

        // ========================================================================= 4. DISPATCHER AREA
        void BuildDispatcherArea(Transform parent)
        {
            var area = UIK.RT("DispatcherArea", parent);
            UIK.LE(area.gameObject, -1, 100, -1, -1, 100);

            // Dispatcher Character
            var charObj = MATArt.Img(area, MATArt.Env("Char_Dispatcher"), "DispatcherChar");
            var rt = charObj.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(100, 0);
            rt.anchoredPosition = new Vector2(10, -5);

            // Desk Mini Monitor & Radios
            var pcCard = UIK.RT("MiniDeskPC", area);
            pcCard.anchorMin = new Vector2(0.35f, 0.1f);
            pcCard.anchorMax = new Vector2(0.95f, 0.9f);
            var pcImg = MATArt.Sliced(pcCard, MATArt.UI("Slot_Card"));
            UIK.Stretch(pcImg.rectTransform);
            var pcvl = UIK.VL(pcCard.gameObject, 3, 6, false);
            UIK.Txt(pcCard, "📻 BÀN ĐIỀU HÀNH TRUNG TÂM", 11, UIK.Hex("#60a5fa"), FontStyle.Bold, TextAnchor.MiddleLeft);
            UIK.Txt(pcCard, "Kênh bộ đàm: Kênh 03 (Phân xưởng khai thác)\nCáp quang truyền dữ liệu: Hoạt động ổn định", 10, UIK.Hex("#94a3b8"), FontStyle.Normal, TextAnchor.MiddleLeft);
        }

        // ========================================================================= 5. DIALOGUE BUBBLE
        void BuildDialogueBubble(Transform parent)
        {
            var bubble = UIK.RT("DialogueBubble", parent);
            UIK.LE(bubble.gameObject, -1, 95, -1, -1, 95);
            var bImg = MATArt.Sliced(bubble, MATArt.UI("Frame_Dialogue"), "BubbleBg");
            UIK.Stretch(bImg.rectTransform);

            var vl = UIK.VL(bubble.gameObject, 4, 12, false);
            vl.childAlignment = TextAnchor.UpperLeft;
            vl.childForceExpandWidth = false;
            vl.childForceExpandHeight = false;
            vl.childControlWidth = false;
            vl.childControlHeight = false;

            // Title Pill Badge
            var titlePill = UIK.RT("TitlePill", bubble);
            titlePill.sizeDelta = new Vector2(165, 24);
            var pBg = MATArt.Sliced(titlePill, MATArt.UI("Badge_Title"));
            UIK.Stretch(pBg.rectTransform);
            var pTxt = UIK.Txt(titlePill, "📢 Thông tin từ hiện trường", 10, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIK.Stretch(pTxt.rectTransform);

            // Report Content
            reportTxt = UIK.Txt(bubble, "Tổ sản xuất báo cáo: Tại gương lò III, ống gió mềm bị gập nhẹ.\n<b>Bạn sẽ xử lý như thế nào?</b>", 12, UIK.Hex("#0f172a"), FontStyle.Normal, TextAnchor.UpperLeft);
            reportTxt.lineSpacing = 1.15f;
            reportTxt.rectTransform.sizeDelta = new Vector2(375, 52);
        }

        // ========================================================================= 6. ACTION & HINT ROW
        void BuildActionRow(Transform parent)
        {
            var row = UIK.RT("ActionRow", parent);
            UIK.LE(row.gameObject, -1, 115, -1, -1, 115);
            var hl = UIK.HL(row.gameObject, 8, 4, false);
            hl.childAlignment = TextAnchor.MiddleCenter;

            // Hint Card (Left)
            var hintCard = UIK.RT("HintCard", row);
            UIK.LE(hintCard.gameObject, 150, 110, -1, 150, 110);
            var hBg = MATArt.Sliced(hintCard, MATArt.UI("Frame_Hint"));
            UIK.Stretch(hBg.rectTransform);

            var hvl = UIK.VL(hintCard.gameObject, 3, 6, false);
            var htop = UIK.RT("HeaderRow", hintCard);
            UIK.LE(htop.gameObject, -1, 20, 1, -1, 20);
            var hthl = UIK.HL(htop.gameObject, 4, 0, false);
            hthl.childAlignment = TextAnchor.MiddleLeft;
            var bulb = MATArt.Img(htop, MATArt.Icon("Icon_Bulb"));
            UIK.LE(bulb.gameObject, 18, 18, -1, 18, 18);
            UIK.Txt(htop, "Gợi ý an toàn", 11, UIK.Hex("#b45309"), FontStyle.Bold, TextAnchor.MiddleLeft);

            hintTxt = UIK.Txt(hintCard, "Ống gió bị gập làm giảm lượng gió sạch đến gương lò...", 10, UIK.Hex("#78350f"), FontStyle.Normal, TextAnchor.MiddleLeft);
            hintTxt.lineSpacing = 1.1f;
            UIK.LE(hintTxt.gameObject, -1, -1, 1);

            // 2 Chunky Decision Buttons (Right)
            var btnCol = UIK.RT("BtnCol", row);
            UIK.LE(btnCol.gameObject, -1, 110, 1, -1, 110);
            var bvl = UIK.VL(btnCol.gameObject, 6, 0, false);
            bvl.childAlignment = TextAnchor.MiddleCenter;

            // Button "Cho phép làm việc" (Green)
            btnWork = CreateChunkyBtn(btnCol, "Btn_Green", "Icon_Check", "Cho phép làm việc", () => OnMakeDecision(true));
            // Button "Dừng sản xuất" (Red)
            btnStop = CreateChunkyBtn(btnCol, "Btn_Red", "Icon_Cross", "Dừng sản xuất", () => OnMakeDecision(false));
        }

        Button CreateChunkyBtn(Transform parent, string bgSprite, string iconSprite, string label, Action onClick)
        {
            var btnObj = UIK.RT("Btn_" + label, parent);
            UIK.LE(btnObj.gameObject, -1, 50, 1, -1, 50);
            var img = MATArt.Sliced(btnObj, MATArt.UI(bgSprite));
            UIK.Stretch(img.rectTransform);

            var btn = btnObj.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                Sfx.Play("tap");
                onClick?.Invoke();
            });

            var hl = UIK.HL(btnObj.gameObject, 8, 8, false);
            hl.childAlignment = TextAnchor.MiddleCenter;

            var ic = MATArt.Img(btnObj, MATArt.Icon(iconSprite));
            UIK.LE(ic.gameObject, 24, 24, -1, 24, 24);

            UIK.Txt(btnObj, label, 14, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            return btn;
        }

        // ========================================================================= 7. FOOTER BAR
        void BuildFooter(Transform parent)
        {
            var footer = UIK.RT("Footer", parent);
            UIK.LE(footer.gameObject, -1, 28, -1, -1, 28);
            var fBg = MATArt.Sliced(footer, MATArt.UI("Slot_Card"));
            UIK.Stretch(fBg.rectTransform);

            var hl = UIK.HL(footer.gameObject, 6, 4, false);
            hl.childAlignment = TextAnchor.MiddleCenter;
            UIK.Txt(footer, "ℹ Hãy kiểm tra kỹ thông tin và đưa ra quyết định chính xác!", 11, UIK.Hex("#93c5fd"), FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        // ========================================================================= SCENARIO LOGIC
        void LoadScenario(int index)
        {
            if (index >= SCENARIOS.Count)
            {
                // Hoàn thành xuất sắc tất cả kịch bản!
                ShowVictoryModal();
                return;
            }

            var s = SCENARIOS[index];
            reportTxt.text = s.report;
            hintTxt.text = s.hint;
            ch4Txt.text = s.ch4.ToString("0.0") + " %";
            ch4Txt.color = s.ch4 >= 1.0f ? UIK.Hex("#ef4444") : (s.ch4 >= 0.7f ? UIK.Hex("#f59e0b") : UIK.Hex("#38bdf8"));
            tempTxt.text = s.temp + " °C";
            humTxt.text = s.humidity + " %";
            locTxt.text = s.location;

            if (s.isNormal)
            {
                statusTxt.text = "BÌNH THƯỜNG";
                statusTxt.color = UIK.Hex("#22c55e");
                statusIcon.sprite = MATArt.Icon("Icon_Check");
            }
            else
            {
                statusTxt.text = "CẢNH BÁO";
                statusTxt.color = UIK.Hex("#ef4444");
                statusIcon.sprite = MATArt.Icon("Icon_Cross");
            }

            heartDisplayTxt.text = hearts.ToString();
        }

        public void OnMakeDecision(bool chooseWork)
        {
            var s = SCENARIOS[currentScenarioIdx];
            bool isCorrect = (chooseWork == s.correctDecisionIsWork);

            if (isCorrect)
            {
                Sfx.Play("ok");
                xp += 25;
                UIK.Modal((m, close) =>
                {
                    var ic = MATArt.Img(m, MATArt.Icon("Icon_Check"));
                    UIK.LE(ic.gameObject, 48, 48, -1, 48, 48);
                    UIK.Txt(m, "XỬ LÝ CHÍNH XÁC! (+25 XP)", 18, UIK.Hex("#22c55e"), FontStyle.Bold, TextAnchor.MiddleCenter);
                    var t = UIK.Txt(m, s.explanation, 13, UIK.Ink, FontStyle.Normal, TextAnchor.MiddleLeft);
                    t.lineSpacing = 1.15f;
                    UIK.Btn(m, "Tiếp tục nhiệm vụ >", UIK.Hex("#22c55e"), UIK.Ink, 15, () =>
                    {
                        close();
                        currentScenarioIdx++;
                        LoadScenario(currentScenarioIdx);
                    }, 46);
                });
            }
            else
            {
                Sfx.Play("bad");
                hearts--;
                heartDisplayTxt.text = hearts.ToString();

                UIK.Modal((m, close) =>
                {
                    var ic = MATArt.Img(m, MATArt.Icon("Icon_Cross"));
                    UIK.LE(ic.gameObject, 48, 48, -1, 48, 48);
                    UIK.Txt(m, "QUYẾT ĐỊNH CHƯA AN TOÀN! (-1 Tim)", 17, UIK.Hex("#ef4444"), FontStyle.Bold, TextAnchor.MiddleCenter);
                    var t = UIK.Txt(m, "Hành động này tiềm ẩn nguy cơ mất an toàn nghiêm trọng trong hầm lò.\n\n<b>Quy chuẩn an toàn:</b>\n" + s.hint, 13, UIK.Ink, FontStyle.Normal, TextAnchor.MiddleLeft);
                    t.lineSpacing = 1.15f;

                    if (hearts <= 0)
                    {
                        UIK.Btn(m, "Thử lại từ đầu", UIK.Hex("#ef4444"), UIK.Ink, 15, () =>
                        {
                            close();
                            StartGame();
                        }, 46);
                    }
                    else
                    {
                        UIK.Btn(m, "Đã hiểu, tiếp tục", UIK.Hex("#2563eb"), UIK.Ink, 15, () => close(), 46);
                    }
                });
            }
        }

        void ShowRuleModal()
        {
            Sfx.Play("tap");
            UIK.Modal((m, close) =>
            {
                UIK.Txt(m, "📖 SỔ QUY ĐỊNH AN TOÀN MỎ\n<size=12><color=#93c5fd>Trích lược QCVN 01:2011/BCT</color></size>", 16, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
                var body = UIK.Txt(m,
                    "<b>1. Nồng độ khí Mê-tan (CH₄):</b>\n" +
                    "• Gương khai thác: không quá 1.0%\n" +
                    "• Luồng gió thải/gió hồi: không quá 1.0%\n" +
                    "• Vượt quá 1.0%: Phải cắt điện, ngừng việc, rút người.\n\n" +
                    "<b>2. Thông gió & Ống gió:</b>\n" +
                    "• Ống gió mềm phải treo thẳng, không gập, không rách.\n" +
                    "• Miệng ống gió cách gương lò ≤ 5m (lò có khí nổ).\n\n" +
                    "<b>3. Dấu hiệu than tự cháy:</b>\n" +
                    "• Nhiệt độ tăng bất thường (>35°C), hơi nước đọng, khói mỏng, mùi nhựa thông.\n" +
                    "• Xuất hiện khí CO: Báo động nguy cơ cháy ngầm.",
                    12, UIK.Hex("#cbd5e1"), FontStyle.Normal, TextAnchor.MiddleLeft);
                body.lineSpacing = 1.15f;
                UIK.Btn(m, "Đóng sổ quy định", UIK.Hex("#2563eb"), UIK.Ink, 14, () => close(), 42);
            });
        }

        void ShowVictoryModal()
        {
            Sfx.Play("win");
            UIK.Modal((m, close) =>
            {
                var ic = MATArt.Img(m, MATArt.Icon("Icon_Shield"));
                UIK.LE(ic.gameObject, 56, 56, -1, 56, 56);
                UIK.Txt(m, "XUẤT SẮC HOÀN THÀNH CA TRỰC!", 18, UIK.Hex("#facc15"), FontStyle.Bold, TextAnchor.MiddleCenter);
                var t = UIK.Txt(m, "Bạn đã phát hiện và xử lý chính xác 100% tình huống sự cố tại mỏ Than Mông Dương theo đúng Quy chuẩn Quốc gia QCVN 01:2011/BCT.\n\nTổng điểm nhận được: <b>+100 XP</b>", 13, UIK.Ink, FontStyle.Normal, TextAnchor.MiddleCenter);
                t.lineSpacing = 1.15f;
                UIK.Btn(m, "Về Menu Chính", UIK.Hex("#22c55e"), UIK.Ink, 15, () =>
                {
                    close();
                    App.I.ShowMenu();
                }, 46);
            });
        }
    }
}
