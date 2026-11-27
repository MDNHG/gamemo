using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Component View quản lý toàn bộ giao diện của Game 2 (Trực ban an toàn)
    /// Gắn trực tiếp trên Canvas.prefab để liên kết SerializedFields trực quan.
    /// </summary>
    public class Game2View : MonoBehaviour
    {
        [Header("Top Bar")]
        public Text levelText;
        public Text titleBadgeText;
        public Text heartsText;
        public Button btnSettings;

        [Header("Telemetry (Thông số hiện tại)")]
        public Text ch4Text;
        public Text tempText;
        public Text humidityText;
        public Text locationText;

        [Header("CCTV Cameras (Lưới 2x2)")]
        public Image[] cctvFeeds;
        public Button[] cameraButtons;

        [Header("Status & Rules (Tình trạng chung)")]
        public Image statusIcon;
        public Text statusText;
        public Button btnRuleBook;

        [Header("Field Report & Hints (Báo cáo hiện trường)")]
        public Text reportText;
        public Text hintText;

        [Header("Decision Buttons (Quyết định xử lý)")]
        public Button btnAllowWork;
        public Button btnStopProduction;

        /// <summary>
        /// Nạp và hiển thị thông tin ca trực / sự cố mới.
        /// </summary>
        public void BindIncident(Incident2 incident, int incidentNum)
        {
            if (incident == null) return;

            if (titleBadgeText != null)
            {
                titleBadgeText.text = $"Trực ban an toàn\n<size=10><color=#93c5fd>Ca trực {incidentNum}: {incident.title}</color></size>";
            }

            if (reportText != null) reportText.text = incident.reportText;
            if (hintText != null) hintText.text = incident.tip;

            if (ch4Text != null)
            {
                ch4Text.text = $"{incident.ch4:0.0} %";
                ch4Text.color = incident.ch4 >= 1.0f ? UIK.Hex("#ef4444") : (incident.ch4 >= 0.7f ? UIK.Hex("#f59e0b") : UIK.Hex("#38bdf8"));
            }

            if (tempText != null) tempText.text = $"{incident.temp:0} °C";
            if (humidityText != null) humidityText.text = $"{incident.humidity:0} %";
            if (locationText != null) locationText.text = incident.location;

            bool isWarning = incident.requireStop;
            if (statusText != null)
            {
                statusText.text = isWarning ? "CẢNH BÁO" : "BÌNH THƯỜNG";
                statusText.color = isWarning ? UIK.Hex("#ef4444") : UIK.Hex("#22c55e");
            }
        }

        public void SetHearts(int count)
        {
            if (heartsText != null) heartsText.text = count.ToString();
        }

        public void ShowResultFeedback(bool isCorrect, string title, string explanation, Action onContinue)
        {
            Sfx.Play(isCorrect ? "ok" : "bad");
            var overlay = transform.Find("Overlay") as RectTransform ?? (RectTransform)transform;
            CreateModalInOverlay(overlay, (m, close) =>
            {
                var ic = UIK.Box(m, isCorrect ? UIK.Hex("#22c55e") : UIK.Hex("#ef4444"), UIK.Circle, "Icon");
                UIK.LE(ic.gameObject, 48, 48, -1, 48, 48);
                VG.Icon(ic.transform, isCorrect ? "check" : "cross", Color.white, 28);

                UIK.Txt(m, title, 18, isCorrect ? UIK.Hex("#22c55e") : UIK.Hex("#ef4444"), FontStyle.Bold, TextAnchor.MiddleCenter);
                var t = UIK.Txt(m, explanation, 13, UIK.Ink, FontStyle.Normal, TextAnchor.MiddleLeft);
                t.lineSpacing = 1.15f;

                string btnText = isCorrect ? "Tiếp tục nhiệm vụ >" : "Đã hiểu, tiếp tục";
                Color btnColor = isCorrect ? UIK.Hex("#22c55e") : UIK.Hex("#2563eb");
                UIK.Btn(m, btnText, btnColor, UIK.Ink, 15, () =>
                {
                    close();
                    onContinue?.Invoke();
                }, 46);
            });
        }

        public void ShowRuleModal()
        {
            Sfx.Play("tap");
            var overlay = transform.Find("Overlay") as RectTransform ?? (RectTransform)transform;
            CreateModalInOverlay(overlay, (m, close) =>
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

        public void ShowVictoryModal(Action onFinish)
        {
            Sfx.Play("win");
            var overlay = transform.Find("Overlay") as RectTransform ?? (RectTransform)transform;
            CreateModalInOverlay(overlay, (m, close) =>
            {
                var ic = UIK.Box(m, UIK.Hex("#facc15"), UIK.Circle, "Icon");
                UIK.LE(ic.gameObject, 56, 56, -1, 56, 56);
                VG.Icon(ic.transform, "gem", UIK.Dark, 32);

                UIK.Txt(m, "XUẤT SẮC HOÀN THÀNH CA TRỰC!", 18, UIK.Hex("#facc15"), FontStyle.Bold, TextAnchor.MiddleCenter);
                var t = UIK.Txt(m, "Bạn đã phát hiện và xử lý chính xác 100% tình huống sự cố tại mỏ Than Mông Dương theo đúng Quy chuẩn Quốc gia QCVN 01:2011/BCT.\n\nTổng điểm nhận được: <b>+100 XP</b>", 13, UIK.Ink, FontStyle.Normal, TextAnchor.MiddleCenter);
                t.lineSpacing = 1.15f;
                UIK.Btn(m, "Về Menu Chính", UIK.Hex("#22c55e"), UIK.Ink, 15, () =>
                {
                    close();
                    onFinish?.Invoke();
                }, 46);
            });
        }

        static void CreateModalInOverlay(RectTransform overlayRoot, Action<RectTransform, Action> build)
        {
            var dim = UIK.Flat(overlayRoot, new Color(0, 0, 0, 0.75f), "Modal");
            UIK.Stretch(dim.rectTransform);
            var sheet = UIK.Box(dim.transform, UIK.P, UIK.Round, "Sheet");
            var rt = sheet.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            float w = Mathf.Min(App.ColumnWidth - 36, 400);
            rt.sizeDelta = new Vector2(w, 0); rt.anchoredPosition = Vector2.zero;
            UIK.VL(sheet.gameObject, 12, 20, 20, 22, 26, true);
            var go = dim.gameObject;
            Action close = () => { if (go != null) UnityEngine.Object.Destroy(go); };
            build?.Invoke(rt, close);
        }
    }
}
