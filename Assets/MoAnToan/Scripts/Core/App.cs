using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Điểm vào: tự tạo camera, EventSystem, Canvas khi bấm Play ở bất kỳ scene nào của project này,
    /// rồi hiện menu chọn 2 game.
    /// </summary>
    public class App : MonoBehaviour
    {
        public static App I;
        public static float ColumnWidth = 420;

        public RectTransform Column;
        CanvasScaler scaler;
        RectTransform canvasRT;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (I == null && FindFirstObjectByType<App>() == null) new GameObject("MoAnToanApp").AddComponent<App>();
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            UIK.Init();
            Sfx.Init(gameObject);
            Sfx.Muted = PlayerPrefs.GetInt("mat_mute", 0) == 1;

            var cam = Camera.main;
            if (cam == null)
            {
                var cg = new GameObject("Main Camera");
                cg.tag = "MainCamera";
                cam = cg.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = UIK.Bg;
            cam.orthographic = true;
            if (FindFirstObjectByType<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                var t = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (t != null) es.AddComponent(t); else es.AddComponent<StandaloneInputModule>();
            }

            var cgo = new GameObject("Canvas", typeof(RectTransform));
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(420, 860);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cgo.AddComponent<GraphicRaycaster>();
            canvasRT = (RectTransform)cgo.transform;

            var bg = UIK.Flat(cgo.transform, UIK.Bg, "Background");
            UIK.Stretch(bg.rectTransform);
            bg.raycastTarget = false;

            Column = UIK.RT("Column", cgo.transform);
            Column.anchorMin = new Vector2(0.5f, 0); Column.anchorMax = new Vector2(0.5f, 1);
            Column.pivot = new Vector2(0.5f, 0.5f);
            Column.sizeDelta = new Vector2(420, 0);

            UIK.OverlayRoot = UIK.RT("Overlay", cgo.transform);
            UIK.Stretch(UIK.OverlayRoot);

            Layout();
            ShowMenu();
        }

        void Update() { Layout(); }

        void Layout()
        {
            if (scaler == null) return;
            scaler.matchWidthOrHeight = Screen.width > Screen.height * 0.62f ? 1f : 0f;
            float w = canvasRT.rect.width;
            if (w <= 0) return;
            ColumnWidth = Mathf.Min(w, 560);
            Column.sizeDelta = new Vector2(ColumnWidth, 0);
        }

        /// <summary>Xóa màn hình hiện tại (cột chính và overlay).</summary>
        public RectTransform NewScreen()
        {
            UIK.Clear(Column);
            UIK.Clear(UIK.OverlayRoot);
            return Column;
        }

        public void ShowMenu()
        {
            var root = NewScreen();
            ScrollRect sr;
            var c = UIK.Scroll(root, 14, 18, out sr);
            UIK.Stretch((RectTransform)sr.transform);

            UIK.Spacer(c, 10);
            UIK.Txt(c, "AN TOÀN HẦM LÒ", 15, UIK.Y, FontStyle.Bold);
            UIK.Txt(c, "Bản test 2 game", 36, UIK.Ink, FontStyle.Bold);
            UIK.Txt(c, "Ý tưởng 5 và 6 — học và nhận diện quy định an toàn lao động trong khai thác than hầm lò.", 17, UIK.Mute);
            UIK.Spacer(c, 4);

            GameCard(c, "5", "Chinh phục đường lò", "Thử thách mỗi ngày: trạm học 60–90 giây, tim, chuỗi ngày, ôn câu sai, xếp hạng tổ.", "book", () => new Game5().Start());
            GameCard(c, "6", "Đường lò không sai sót", "Quan sát từng đoạn lò: an toàn thì đi tiếp, thấy bất thường thì báo cáo và chỉ ra chỗ sai.", "helmet", () => new Game6().Start());

            Button mute = null;
            mute = UIK.Btn(c, Sfx.Muted ? "Âm thanh: Tắt" : "Âm thanh: Bật", UIK.P, UIK.Ink, 16, () =>
            {
                Sfx.Muted = !Sfx.Muted;
                PlayerPrefs.SetInt("mat_mute", Sfx.Muted ? 1 : 0);
                UIK.Label(mute).text = Sfx.Muted ? "Âm thanh: Tắt" : "Âm thanh: Bật";
            }, 52);
            UIK.Txt(c, "Nội dung câu hỏi và quy định là bản mẫu để test, cần cán bộ an toàn rà soát theo QCVN 01:2011/BCT và nội quy của đơn vị.", 13, UIK.Mute);
            UIK.Spacer(c, 20);
        }

        static void GameCard(Transform p, string num, string title, string desc, string icon, Action open)
        {
            var card = UIK.Card(p, UIK.P, 18, 10);
            var head = UIK.RT("Head", card.transform);
            UIK.HL(head.gameObject, 12, 0, false).childForceExpandHeight = false;
            UIK.LE(head.gameObject, 52);
            var badge = UIK.Box(head, UIK.Y, UIK.Circle, "Badge");
            UIK.LE(badge.gameObject, 52, 52, -1, 52, 52);
            var ic = VG.Icon(badge.transform, icon, UIK.Dark, 30);
            ic.rectTransform.anchoredPosition = Vector2.zero;
            var tcol = UIK.RT("Titles", head);
            UIK.VL(tcol.gameObject, 0);
            UIK.LE(tcol.gameObject, -1, -1, 1);
            UIK.Txt(tcol, "Ý tưởng " + num, 14, UIK.Y, FontStyle.Bold);
            UIK.Txt(tcol, title, 22, UIK.Ink, FontStyle.Bold);
            UIK.Txt(card.transform, desc, 16, UIK.Mute);
            UIK.Btn(card.transform, "Chơi", UIK.Y, UIK.Dark, 18, open, 54);
        }
    }
}
