using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    [Serializable]
    public class Save6
    {
        public int unl = 1;
        public List<int> doneZ = new List<int>();
        public List<int> doneResets = new List<int>();
    }

    /// <summary>Ý tưởng 6 — Đường lò không sai sót: quan sát và báo cáo điểm bất thường.</summary>
    public class Game6
    {
        const int SEG = 6;
        const string KEY = "mat6_v1";

        class Zone { public string n, d; public string[] an; }
        static readonly Zone[] ZN =
        {
            new Zone { n = "Cửa lò – nhà đèn", d = "Bảo hộ và vật cấm", an = new[] { "no_helmet", "no_rescuer", "lighter", "cigarette", "phone" } },
            new Zone { n = "Đường lò thông gió", d = "Cửa gió, ống gió", an = new[] { "doors_open", "duct_kink", "duct_torn" } },
            new Zone { n = "Khu vực khí mỏ", d = "Cảm biến và nồng độ CH₄", an = new[] { "sensor_floor", "ch4_high" } },
            new Zone { n = "Khu than tự cháy", d = "Nhiệt độ, khói", an = new[] { "temp_high", "smoke" } },
            new Zone { n = "Khu nguy cơ bục nước", d = "Rỉ nước + tổng hợp mọi lỗi", an = new[] { "water" } },
        };

        static readonly Dictionary<string, string[]> AN = new Dictionary<string, string[]>
        {
            { "no_helmet", new[] { "Công nhân không đội mũ bảo hộ", "Mọi người xuống lò phải đội mũ bảo hộ có gắn đèn lò cá nhân." } },
            { "no_rescuer", new[] { "Thiếu bình tự cứu ở thắt lưng", "Người xuống lò phải luôn mang bình tự cứu cá nhân bên người." } },
            { "lighter", new[] { "Cầm bật lửa trong lò", "Cấm mang lửa trần, bật lửa, diêm xuống lò vì có thể gây cháy nổ khí mê-tan và bụi than." } },
            { "cigarette", new[] { "Hút thuốc trong lò", "Cấm mang thuốc lá và hút thuốc trong lò." } },
            { "phone", new[] { "Dùng điện thoại thường", "Cấm mang thiết bị điện không đạt chuẩn phòng nổ xuống lò vì có thể phát tia lửa." } },
            { "doors_open", new[] { "Hai cửa gió cùng mở", "Cụm cửa gió phải luôn có ít nhất một cửa đóng; đi qua cửa nào đóng ngay cửa đó, tránh đoản mạch gió." } },
            { "duct_kink", new[] { "Ống gió bị gập", "Ống gió phải treo thẳng, không gập, không rách để đưa đủ gió tới gương lò cụt." } },
            { "duct_torn", new[] { "Ống gió bị rách, gió thoát ra ngoài", "Ống gió rách làm mất gió phía cuối; phải báo để vá, thay ngay." } },
            { "sensor_floor", new[] { "Cảm biến CH₄ đặt dưới nền lò", "Cảm biến mê-tan phải treo sát nóc lò đúng vị trí quy định, vì mê-tan nhẹ hơn không khí và tích tụ ở nóc." } },
            { "ch4_high", new[] { "Bảng đo báo nồng độ CH₄ cao bất thường", "Khi nồng độ mê-tan vượt giới hạn cho phép phải dừng việc, cắt điện, rút người ra luồng gió sạch và báo cáo." } },
            { "temp_high", new[] { "Nhiệt độ không khí tăng bất thường", "Nhiệt độ tăng là dấu hiệu sớm của than tự cháy; báo cáo ngay và kiểm tra khí CO." } },
            { "smoke", new[] { "Khói, hơi bốc ra từ vỉa than", "Khói, hơi nước đọng, mùi nhựa thông là dấu hiệu than tự cháy; báo cáo ngay." } },
            { "water", new[] { "Vách lò rỉ nước, nền đọng nước", "Vách “đổ mồ hôi”, nước rỉ tăng dần là dấu hiệu bục nước: dừng việc, rút người lên cao, báo cáo." } },
        };

        static readonly object[][] CHK =
        {
            new object[] { 0, "Mũ bảo hộ có đèn lò", "helmet" },
            new object[] { 0, "Bình tự cứu đeo ở thắt lưng", "resc" },
            new object[] { 0, "Tay không cầm bật lửa, thuốc lá, điện thoại", "hand" },
            new object[] { 1, "Cụm cửa gió: luôn có ít nhất 1 cửa đóng", "door" },
            new object[] { 1, "Ống gió treo thẳng, không gập, không rách", "duct" },
            new object[] { 2, "Cảm biến CH₄ treo sát nóc lò", "sensor" },
            new object[] { 2, "Bảng đo: CH₄ thấp, khoảng 0,2–0,4 %", "board" },
            new object[] { 3, "Bảng đo: nhiệt độ 25–29 °C", "board" },
            new object[] { 3, "Vỉa than không có khói, hơi bốc lên", "seam" },
            new object[] { 4, "Vách lò khô, nền không đọng nước", "wall" },
        };

        Save6 S;
        int z, prog, resets, okCount, tries;
        float t0;
        Seg cur;
        string lastAn, phase;
        SceneView view;
        RectTransform anim, ctl, banner, dots;

        public void Start()
        {
            var j = PlayerPrefs.GetString(KEY, "");
            if (!string.IsNullOrEmpty(j)) { try { S = JsonUtility.FromJson<Save6>(j); } catch { S = null; } }
            if (S == null) S = new Save6();
            Menu();
        }

        void Save() { PlayerPrefs.SetString(KEY, JsonUtility.ToJson(S)); PlayerPrefs.Save(); }

        static float R01() { return UnityEngine.Random.value; }

        // ------------------------------------------------------------------ sinh đoạn lò
        static Seg Base(int zone)
        {
            var c = new Seg
            {
                doors = zone == 1 || (zone > 1 && R01() < .5f),
                ds = R01() < .5f ? "none" : "A",
                worker = (zone == 0 || R01() < .65f) ? new Worker { left = R01() < .5f } : null,
                ch4 = (2 + UnityEngine.Random.Range(0, 3)) / 10f,
                temp = 25 + UnityEngine.Random.Range(0, 5),
                cart = R01() < .35f,
                sign = "Đoạn " + UnityEngine.Random.Range(11, 91),
            };
            return c;
        }

        static void Apply(Seg c, string a)
        {
            Func<Worker> W = () => { if (c.worker == null) c.worker = new Worker { left = R01() < .5f }; return c.worker; };
            switch (a)
            {
                case "no_helmet": W().helmet = false; break;
                case "no_rescuer": W().resc = false; break;
                case "lighter": case "cigarette": case "phone": W().hand = a; break;
                case "doors_open": c.doors = true; c.ds = "AB"; break;
                case "duct_kink": c.duct = "kink"; break;
                case "duct_torn": c.duct = "torn"; break;
                case "sensor_floor": c.sensor = "floor"; break;
                case "ch4_high": c.ch4 = (12 + UnityEngine.Random.Range(0, 8)) / 10f; break;
                case "temp_high": c.temp = 36 + UnityEngine.Random.Range(0, 8); break;
                case "smoke": c.smoke = true; break;
                case "water": c.water = true; break;
            }
            c.an = new List<string> { a };
        }

        Seg Gen()
        {
            var c = Base(z);
            if (R01() < .6f)
            {
                var prev = ZN.Take(z).SelectMany(x => x.an).ToList();
                string a = null;
                for (int k = 0; k < 4; k++)
                {
                    a = (prev.Count > 0 && R01() < (z == 4 ? .55f : .3f)) ? UIK.Pick(prev) : UIK.Pick(ZN[z].an);
                    if (a != lastAn) break;
                }
                Apply(c, a);
            }
            lastAn = c.an.Count > 0 ? c.an[0] : null;
            return c;
        }

        // ------------------------------------------------------------------ khung chung
        static RectTransform Header(RectTransform parent, string title, string sub, Action back, out RectTransform extra)
        {
            var head = UIK.RT("Head", parent);
            UIK.LE(head.gameObject, 72, 72);
            var hl = UIK.HL(head.gameObject, 10, 0, false);
            hl.padding = new RectOffset(12, 12, 10, 10); hl.childForceExpandHeight = false; hl.childAlignment = TextAnchor.MiddleLeft;
            var b = UIK.Btn(head, "", UIK.P, UIK.Ink, 16, back, 46);
            UIK.LE(b.gameObject, 46, 46, -1, 46, 46);
            VG.Icon(b.transform, "back", UIK.Ink, 22);
            var col = UIK.RT("Titles", head);
            UIK.VL(col.gameObject, 4); UIK.LE(col.gameObject, -1, -1, 1);
            UIK.Txt(col, title, 16, UIK.Ink, FontStyle.Bold);
            extra = col;
            if (sub != null) UIK.Txt(col, sub, 13, UIK.Mute);
            return head;
        }

        /// <summary>Ô chứa cảnh, có nền đen và cắt mép.</summary>
        static RectTransform SceneHolder(Transform parent, float h, out RectTransform animRoot)
        {
            var box = UIK.Box(parent, Color.black, UIK.Round, "SceneBox");
            if (h > 0) UIK.LE(box.gameObject, h, h);
            box.gameObject.AddComponent<RectMask2D>();
            animRoot = UIK.RT("Anim", box.transform);
            UIK.Stretch(animRoot, 4, 4, 4, 4);
            return box.rectTransform;
        }

        List<KeyValuePair<string, int>> Badges(int zone, out List<string> legend)
        {
            var b = new List<KeyValuePair<string, int>>();
            legend = new List<string>();
            var seen = new Dictionary<string, int>();
            int n = 0;
            foreach (var k in CHK)
            {
                if ((int)k[0] > zone) continue;
                n++;
                string key = (string)k[2];
                if (!seen.ContainsKey(key)) { seen[key] = n; b.Add(new KeyValuePair<string, int>(key, n)); }
                legend.Add(seen[key] + "|" + (string)k[1]);
            }
            return b;
        }

        static Seg RefSeg(int zone)
        {
            return new Seg { doors = zone >= 1, ds = "A", worker = new Worker { left = true }, sign = "Hình mẫu", ch4 = .3f, temp = 27 };
        }

        static void Legend(Transform p, List<string> legend)
        {
            foreach (var l in legend)
            {
                var parts = l.Split('|');
                var row = UIK.RT("Leg", p);
                var h = UIK.HL(row.gameObject, 10, 0, false); h.childForceExpandHeight = false; h.childAlignment = TextAnchor.UpperLeft;
                var c = UIK.Box(row, UIK.Y, UIK.Circle, "Num");
                UIK.LE(c.gameObject, 26, 26, 0, 26, 26);
                var nt = UIK.Txt(c.transform, parts[0], 14, UIK.Dark, FontStyle.Bold, TextAnchor.MiddleCenter);
                UIK.Stretch(nt.rectTransform);
                var t = UIK.Txt(row, parts[1], 16, UIK.Ink);
                UIK.LE(t.gameObject, -1, -1, 1);
            }
        }

        // ------------------------------------------------------------------ menu
        void Menu()
        {
            var root = App.I.NewScreen();
            ScrollRect sr;
            var c = UIK.Scroll(root, 12, 14, out sr);
            UIK.Stretch((RectTransform)sr.transform);
            var back = UIK.Btn(c, "Về menu chính", UIK.P, UIK.Ink, 15, () => App.I.ShowMenu(), 44);
            UIK.Txt(c, "Đường lò không sai sót", 34, UIK.Ink, FontStyle.Bold);
            UIK.Txt(c, "Đi qua từng đoạn lò. Thấy gì sai khác hình mẫu thì báo cáo, an toàn thì đi tiếp.", 16, UIK.Mute);
            RectTransform a;
            SceneHolder(c, Mathf.Min(App.ColumnWidth - 28, 420) * 440f / 360f, out a);
            var sv = new SceneView(a);
            var demo = RefSeg(1); demo.cart = true; demo.sign = "Đoạn 01";
            sv.Render(demo);

            string[,] rules = { { "Đi tiếp", "khi đoạn lò đạt chuẩn" }, { "Báo cáo / Quay lại", "khi thấy bất thường, rồi chạm vào chỗ sai" }, { "Bỏ sót hoặc báo nhầm", "là chơi lại khu vực từ đầu" }, { SEG + " đoạn đúng liên tiếp", "để mở khu vực mới" } };
            for (int r = 0; r < 2; r++)
            {
                var row = UIK.RT("Rules", c);
                var h = UIK.HL(row.gameObject, 10, 0, true); h.childForceExpandHeight = true;
                for (int k = 0; k < 2; k++)
                {
                    var card = UIK.Card(row, UIK.P, 12, 2);
                    UIK.Txt(card.transform, rules[r * 2 + k, 0], 16, UIK.Ink, FontStyle.Bold);
                    UIK.Txt(card.transform, rules[r * 2 + k, 1], 14, UIK.Mute);
                }
            }
            for (int i = 0; i < ZN.Length; i++)
            {
                int zi = i;
                bool done = S.doneZ.Contains(i), open = i < S.unl;
                var b = UIK.Box(c, UIK.P, UIK.Round, "Zone");
                var btn = b.gameObject.AddComponent<Button>();
                var h = UIK.HL(b.gameObject, 12, 14, false); h.childForceExpandHeight = false; h.childAlignment = TextAnchor.MiddleLeft;
                UIK.LE(b.gameObject, 68);
                var n = UIK.Box(b.transform, done ? UIK.G : open ? UIK.Y : UIK.P2, UIK.Circle, "N");
                UIK.LE(n.gameObject, 40, 40, 0, 40, 40);
                var nt = UIK.Txt(n.transform, (i + 1).ToString(), 18, open || done ? UIK.Dark : UIK.Mute, FontStyle.Bold, TextAnchor.MiddleCenter);
                UIK.Stretch(nt.rectTransform);
                var tc = UIK.RT("T", b.transform); UIK.VL(tc.gameObject, 2); UIK.LE(tc.gameObject, -1, -1, 1);
                UIK.Txt(tc, ZN[i].n, 17, open ? UIK.Ink : UIK.Mute, FontStyle.Bold);
                string sub = !open ? "Chưa mở khóa" : ZN[i].d;
                int di = S.doneZ.IndexOf(i);
                if (di >= 0 && di < S.doneResets.Count) sub += " · Đã qua, chơi lại " + S.doneResets[di] + " lần";
                UIK.Txt(tc, sub, 13, UIK.Mute);
                btn.onClick.AddListener(() => { if (zi < S.unl) { Sfx.Play("tap"); Intro(zi); } else UIK.Toast("Hoàn thành khu trước để mở khóa"); });
            }
            UIK.Txt(c, "Nội dung quy định là bản mẫu để test, cần cán bộ an toàn rà soát theo QCVN 01:2011/BCT và nội quy của đơn vị. Ngưỡng trên bảng đo là quy ước của trò chơi.", 13, UIK.Mute);
            UIK.Spacer(c, 16);
        }

        // ------------------------------------------------------------------ hình mẫu
        void Intro(int zone)
        {
            var root = App.I.NewScreen();
            var page = UIK.RT("Page", root); UIK.Stretch(page);
            UIK.VL(page.gameObject, 0).childForceExpandHeight = false;
            RectTransform extra;
            Header(page, "Khu " + (zone + 1) + " · " + ZN[zone].n, "Ghi nhớ hình mẫu đạt chuẩn", Menu, out extra);
            ScrollRect sr;
            var c = UIK.Scroll(page, 12, 14, out sr);
            UIK.LE(sr.gameObject, 100).flexibleHeight = 1;
            RectTransform a;
            SceneHolder(c, Mathf.Min(App.ColumnWidth - 28, 420) * 440f / 360f, out a);
            List<string> legend;
            var badges = Badges(zone, out legend);
            new SceneView(a).Render(RefSeg(zone), null, null, badges);
            Legend(c, legend);
            UIK.Txt(c, "Biển số đoạn, vị trí người, xe goòng, cửa gió nào đang đóng có thể thay đổi giữa các đoạn — đó không phải lỗi.", 13, UIK.Mute);
            UIK.Btn(c, "Bắt đầu đi vào lò", UIK.Y, UIK.Dark, 18, () => Begin(zone), 56);
            UIK.Spacer(c, 12);
        }

        void ShowRef()
        {
            Action close;
            var m = UIK.Modal(out close);
            UIK.Txt(m, "Hình mẫu đạt chuẩn", 22, UIK.Ink, FontStyle.Bold);
            RectTransform a;
            SceneHolder(m, Mathf.Min(320, Screen.height * 0.3f), out a);
            List<string> legend;
            var badges = Badges(z, out legend);
            new SceneView(a).Render(RefSeg(z), null, null, badges);
            Legend(m, legend);
            UIK.Btn(m, "Quay lại đoạn lò", UIK.Y, UIK.Dark, 17, close, 54);
        }

        // ------------------------------------------------------------------ chơi
        void Begin(int zone)
        {
            z = zone; prog = 0; resets = 0; okCount = 0; tries = 0; lastAn = null;
            t0 = Time.realtimeSinceStartup;
            Next();
        }

        void Next()
        {
            cur = Gen();
            phase = "look";
            Render(true);
        }

        void Render(bool enter)
        {
            var root = App.I.NewScreen();
            var page = UIK.RT("Page", root); UIK.Stretch(page);
            UIK.VL(page.gameObject, 0).childForceExpandHeight = false;

            RectTransform col;
            var head = Header(page, "Khu " + (z + 1) + " · " + ZN[z].n, null, ConfirmLeave, out col);
            dots = UIK.RT("Dots", col);
            UIK.HL(dots.gameObject, 5, 0, true);
            UIK.LE(dots.gameObject, 8, 8);
            for (int i = 0; i < SEG; i++) UIK.Box(dots, i < prog ? UIK.Y : UIK.P2, UIK.RoundSmall, "Dot");
            var rf = UIK.Btn(head, "Hình mẫu", UIK.P, UIK.Ink, 14, ShowRef, 44);
            UIK.LE(rf.gameObject, 44, 44, 0, 96, 96);

            var stage = UIK.RT("Stage", page);
            UIK.LE(stage.gameObject, 200).flexibleHeight = 1;
            RectTransform a;
            var box = SceneHolder(stage, -1, out a);
            UIK.Stretch(box, 12, 0, 12, 0);
            anim = a;
            view = new SceneView(a);
            view.Render(cur);
            view.G.OnTap = OnTap;

            var bn = UIK.Box(box, UIK.A(UIK.Hex("#14120f"), .9f), UIK.Round, "Banner");
            banner = bn.rectTransform;
            banner.anchorMin = new Vector2(0, 0); banner.anchorMax = new Vector2(1, 0); banner.pivot = new Vector2(0.5f, 0);
            banner.offsetMin = new Vector2(16, 12); banner.offsetMax = new Vector2(-16, 60);
            var bt = UIK.Txt(banner, "Chạm vào điểm bất thường trên hình", 16, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            UIK.Stretch(bt.rectTransform, 8, 4, 8, 4);
            bn.raycastTarget = false;
            banner.gameObject.SetActive(false);

            ctl = UIK.RT("Controls", page);
            UIK.LE(ctl.gameObject, 92, 92);
            var ch = UIK.HL(ctl.gameObject, 10, 0, true);
            ch.padding = new RectOffset(12, 12, 12, 16);
            LookButtons();

            if (enter) App.I.StartCoroutine(Enter());
        }

        void LookButtons()
        {
            UIK.Clear(ctl);
            UIK.Btn(ctl, "Đi tiếp", UIK.Y, UIK.Dark, 19, GoOn, 64);
            UIK.Btn(ctl, "Báo cáo / Quay lại", UIK.Hex("#3a1715"), UIK.Hex("#ffd0cb"), 17, Report, 64);
        }

        IEnumerator Enter()
        {
            var a = anim;
            for (float t = 0; t < 1 && a != null; t += Time.unscaledDeltaTime * 3f)
            {
                float s = Mathf.Lerp(0.92f, 1f, t);
                a.localScale = new Vector3(s, s, 1);
                yield return null;
            }
            if (a != null) a.localScale = Vector3.one;
        }

        IEnumerator Walk(Action then)
        {
            var a = anim;
            var cg = a.gameObject.AddComponent<CanvasGroup>();
            for (float t = 0; t < 1 && a != null; t += Time.unscaledDeltaTime * 2.2f)
            {
                float s = Mathf.Lerp(1f, 1.45f, t * t);
                a.localScale = new Vector3(s, s, 1);
                cg.alpha = 1 - t;
                yield return null;
            }
            then();
        }

        void ConfirmLeave()
        {
            Action close;
            var m = UIK.Modal(out close);
            UIK.Txt(m, "Rời khu vực?", 22, UIK.Ink, FontStyle.Bold);
            UIK.Txt(m, "Tiến độ khu này sẽ bị mất.", 16, UIK.Mute);
            UIK.Btn(m, "Ở lại", UIK.Y, UIK.Dark, 17, close, 54);
            UIK.Btn(m, "Về menu", UIK.P2, UIK.Ink, 16, () => { close(); Menu(); }, 52);
        }

        void GoOn()
        {
            if (phase != "look") return;
            tries++;
            if (cur.an.Count == 0) Success(null); else Fail("miss", null);
        }

        void Report()
        {
            if (phase != "look") return;
            tries++;
            if (cur.an.Count == 0) { Fail("false", null); return; }
            phase = "tap";
            banner.gameObject.SetActive(true);
            UIK.Clear(ctl);
            UIK.Btn(ctl, "Hủy, quan sát lại", UIK.P, UIK.Ink, 17, () => { tries--; phase = "look"; banner.gameObject.SetActive(false); LookButtons(); }, 64);
        }

        void OnTap(Vector2 p)
        {
            if (phase != "tap") return;
            const float pad = 12;
            bool hit = view.Hits.Any(h => p.x >= h.r.x - pad && p.x <= h.r.xMax + pad && p.y >= h.r.y - pad && p.y <= h.r.yMax + pad);
            if (hit) Success(p); else Fail("spot", p);
        }

        string LawText()
        {
            return string.Join("\n\n", cur.an.Select(a => "<b><color=#ffc21a>" + AN[a][0] + ".</color></b> " + AN[a][1]).ToArray());
        }

        void Success(Vector2? tap)
        {
            phase = "fb"; okCount++;
            Sfx.Play("ok");
            if (tap.HasValue)
            {
                banner.gameObject.SetActive(false);
                view.Render(cur, UIK.G, tap);
                Action close;
                var m = UIK.Modal(out close);
                UIK.Txt(m, "Báo cáo chính xác!", 24, UIK.G, FontStyle.Bold);
                var law = UIK.Card(m, UIK.P2, 14, 0);
                UIK.Txt(law.transform, LawText(), 16, UIK.Ink);
                UIK.Btn(m, "Đi tiếp", UIK.Y, UIK.Dark, 18, () => { close(); Advance(); }, 56);
            }
            else Advance();
        }

        void Advance()
        {
            prog++;
            if (prog >= SEG) { Clear(); return; }
            Sfx.Play("step");
            App.I.StartCoroutine(Walk(Next));
        }

        void Fail(string kind, Vector2? tap)
        {
            phase = "fb";
            Sfx.Play("bad");
            banner.gameObject.SetActive(false);
            view.Render(cur, UIK.R, tap);
            string title = kind == "miss" ? "Bỏ sót lỗi!" : kind == "false" ? "Báo nhầm!" : "Chưa chỉ đúng chỗ sai";
            Action close;
            var m = UIK.Modal(out close);
            UIK.Txt(m, title, 24, UIK.R, FontStyle.Bold);
            if (kind == "false")
                UIK.Txt(m, "Đoạn lò này đạt chuẩn, không có điểm bất thường. Báo nhầm làm gián đoạn sản xuất không cần thiết — hãy đối chiếu kỹ với hình mẫu.", 16, UIK.Ink);
            else
            {
                UIK.Txt(m, (kind == "miss" ? "Đoạn lò có điểm không an toàn mà bạn đã đi qua." : "Bạn đã báo cáo đúng là có lỗi, nhưng chạm sai vị trí.") + " Điểm sai được khoanh đỏ trên hình.", 16, UIK.Ink);
                var law = UIK.Card(m, UIK.P2, 14, 0);
                UIK.Txt(law.transform, LawText(), 16, UIK.Ink);
            }
            UIK.Txt(m, "Chơi lại khu vực từ đoạn đầu (" + prog + "/" + SEG + " đoạn bị hủy).", 14, UIK.Mute);
            UIK.Btn(m, "Chơi lại chặng", UIK.Y, UIK.Dark, 18, () => { close(); resets++; prog = 0; Next(); }, 56);
            UIK.Btn(m, "Xem lại hình mẫu", UIK.P2, UIK.Ink, 16, () => { close(); resets++; prog = 0; Next(); ShowRef(); }, 52);
        }

        void Clear()
        {
            Sfx.Play("win");
            int sec = Mathf.RoundToInt(Time.realtimeSinceStartup - t0);
            int di = S.doneZ.IndexOf(z);
            if (di < 0) { S.doneZ.Add(z); S.doneResets.Add(resets); }
            else if (di < S.doneResets.Count) S.doneResets[di] = resets;
            if (S.unl < z + 2 && z + 1 < ZN.Length) S.unl = z + 2;
            Save();
            int acc = Mathf.RoundToInt(okCount * 100f / Mathf.Max(1, tries));
            bool last = z == ZN.Length - 1;

            var root = App.I.NewScreen();
            ScrollRect sr;
            var c = UIK.Scroll(root, 14, 18, out sr);
            UIK.Stretch((RectTransform)sr.transform);
            UIK.Spacer(c, 30);
            UIK.Txt(c, last ? "Hoàn thành toàn tuyến!" : "Qua khu vực an toàn!", 34, UIK.Y, FontStyle.Bold);
            UIK.Txt(c, "Khu " + (z + 1) + " · " + ZN[z].n, 17, UIK.Mute);
            var tiles = UIK.RT("Tiles", c);
            UIK.HL(tiles.gameObject, 10, 0, true);
            UIK.LE(tiles.gameObject, 84);
            Tile(tiles, "Thời gian", UIK.Mmss(sec), UIK.Ink);
            Tile(tiles, "Chơi lại", resets.ToString(), UIK.Ink);
            Tile(tiles, "Chính xác", acc + "%", UIK.G);
            var info = UIK.Card(c, UIK.P2, 14, 0);
            UIK.Txt(info.transform, last ? "<b><color=#ffc21a>Bạn đã đi hết 5 khu vực.</color></b> Chơi lại để gặp tổ hợp lỗi ngẫu nhiên khác."
                : "<b><color=#ffc21a>Đã mở khóa Khu " + (z + 2) + ":</color></b> " + ZN[z + 1].n + ". Các lỗi của khu trước vẫn có thể xuất hiện.", 16, UIK.Ink);
            if (!last) { int nz = z + 1; UIK.Btn(c, "Vào Khu " + (z + 2), UIK.Y, UIK.Dark, 18, () => Intro(nz), 56); }
            UIK.Btn(c, "Về menu", UIK.P2, UIK.Ink, 16, Menu, 52);
        }

        static void Tile(Transform p, string label, string val, Color vc)
        {
            var t = UIK.Box(p, UIK.P, UIK.Round, "Tile");
            UIK.VL(t.gameObject, 2, 6, 6, 12, 10).childAlignment = TextAnchor.MiddleCenter;
            UIK.Txt(t.transform, label, 14, UIK.Mute, FontStyle.Normal, TextAnchor.UpperCenter);
            UIK.Txt(t.transform, val, 24, vc, FontStyle.Bold, TextAnchor.UpperCenter);
        }
    }
}
