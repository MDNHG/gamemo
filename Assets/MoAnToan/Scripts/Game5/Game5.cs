using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    [Serializable]
    public class Save5
    {
        public int xp, hearts = 5, streak, xpDay;
        public long hts;
        public string last = "", day = "", name = "", team = "";
        public List<int> done = new List<int>();
        public List<string> wrong = new List<string>();
    }

    /// <summary>Ý tưởng 5 — Chinh phục đường lò: học theo trạm ngắn kiểu Duolingo.</summary>
    public class Game5
    {
        const int MAXH = 5, GOAL = 30;
        const long REGEN = 20 * 60;
        const string KEY = "mat5_v1";

        Save5 S;
        string lbMode = "team";

        // trạng thái bài học
        List<Q> qs; int qi, mis, ok, total, li, si; bool rev, free; float t0;
        RectTransform body, footer; Image barFill; Button chk; Text heartTxt;

        static long Now() { return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
        static string DKey(DateTime d) { return d.ToString("yyyy-MM-dd"); }
        static string Today() { return DKey(DateTime.Now); }
        static string Yesterday() { return DKey(DateTime.Now.AddDays(-1)); }

        public void Start()
        {
            var j = PlayerPrefs.GetString(KEY, "");
            if (!string.IsNullOrEmpty(j)) { try { S = JsonUtility.FromJson<Save5>(j); } catch { S = null; } }
            if (S == null) S = new Save5 { hts = Now() };
            Home("map");
        }

        void Save() { PlayerPrefs.SetString(KEY, JsonUtility.ToJson(S)); PlayerPrefs.Save(); }

        void Regen()
        {
            if (S.hearts >= MAXH) { S.hts = Now(); return; }
            long n = (Now() - S.hts) / REGEN;
            if (n > 0) { S.hearts = (int)Math.Min(MAXH, S.hearts + n); S.hts += n * REGEN; Save(); }
        }

        void DayRoll()
        {
            if (S.day != Today()) { S.day = Today(); S.xpDay = 0; }
            if (S.last != "" && S.last != Today() && S.last != Yesterday()) S.streak = 0;
            Save();
        }

        bool IsOpen(int g) { return g == 0 || S.done.Contains(g - 1); }

        // ================================================================== khung màn chính
        void Home(string tab)
        {
            Regen(); DayRoll();
            var root = App.I.NewScreen();

            // thanh trên
            var top = UIK.Flat(root, UIK.Hex("#16181c"), "Top");
            var trt = top.rectTransform;
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1); trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(0, 64); trt.anchoredPosition = Vector2.zero;
            var hl = UIK.HL(top.gameObject, 14, 0, false);
            hl.padding = new RectOffset(12, 16, 8, 8);
            hl.childForceExpandHeight = false; hl.childAlignment = TextAnchor.MiddleLeft;
            var back = UIK.Btn(top.transform, "", UIK.P, UIK.Ink, 16, () => App.I.ShowMenu(), 44);
            UIK.LE(back.gameObject, 44, 44, -1, 44, 44);
            VG.Icon(back.transform, "back", UIK.Ink, 22);
            var title = UIK.Txt(top.transform, "Chinh phục\nđường lò", 15, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            UIK.LE(title.gameObject, -1, -1, 1);
            Stat(top.transform, "flame", UIK.Orange, S.streak.ToString());
            Stat(top.transform, "heart", UIK.R, S.hearts.ToString());
            Stat(top.transform, "gem", UIK.Blue, S.xp.ToString());

            // thanh tab dưới
            var tabs = UIK.Flat(root, UIK.P, "Tabs");
            var tb = tabs.rectTransform;
            tb.anchorMin = new Vector2(0, 0); tb.anchorMax = new Vector2(1, 0); tb.pivot = new Vector2(0.5f, 0);
            tb.sizeDelta = new Vector2(0, 66); tb.anchoredPosition = Vector2.zero;
            var thl = UIK.HL(tabs.gameObject, 6, 6, true);
            string[] keys = { "map", "rev", "lb" };
            string[] names = { "Đường lò", "Ôn tập" + (S.wrong.Count > 0 ? " (" + S.wrong.Count + ")" : ""), "Xếp hạng" };
            for (int i = 0; i < 3; i++)
            {
                string k = keys[i];
                bool on = k == tab;
                UIK.Btn(tabs.transform, names[i], on ? UIK.P2 : UIK.P, on ? UIK.Y : UIK.Mute, 16, () => Home(k), 52);
            }

            // thân
            ScrollRect sr;
            var c = UIK.Scroll(root, 12, 16, out sr);
            UIK.Stretch((RectTransform)sr.transform, 0, 66, 0, 64);
            if (tab == "map") MapView(c, sr);
            else if (tab == "rev") RevView(c);
            else LbView(c);
        }

        static void Stat(Transform p, string icon, Color col, string val)
        {
            var rt = UIK.RT("Stat", p);
            var h = UIK.HL(rt.gameObject, 3, 0, false);
            h.childForceExpandHeight = false; h.childAlignment = TextAnchor.MiddleLeft;
            VG.Icon(rt, icon, col, 20);
            var t = UIK.Txt(rt, val, 18, col, FontStyle.Bold, TextAnchor.MiddleLeft);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            UIK.LE(t.gameObject, -1, -1, -1, -1, t.preferredWidth + 2);
        }

        static void Meter(Transform p, float pct, Color fill)
        {
            var bg = UIK.Box(p, UIK.P2, UIK.RoundSmall, "Meter");
            UIK.LE(bg.gameObject, 12, 12);
            var f = UIK.Box(bg.transform, fill, UIK.RoundSmall, "Fill");
            var rt = f.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(Mathf.Clamp01(pct), 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            if (pct <= 0) f.enabled = false;
        }

        static RectTransform Row(Transform p, string left, string right, Color lc, Color rc, int size = 17)
        {
            var r = UIK.RT("Row", p);
            var h = UIK.HL(r.gameObject, 8, 0, false); h.childForceExpandHeight = false;
            var a = UIK.Txt(r, left, size, lc, FontStyle.Bold); UIK.LE(a.gameObject, -1, -1, 1);
            var b = UIK.Txt(r, right, size, rc, FontStyle.Bold, TextAnchor.UpperRight);
            b.horizontalOverflow = HorizontalWrapMode.Overflow;
            UIK.LE(b.gameObject, -1, -1, 0, -1, b.preferredWidth + 4);
            return r;
        }

        // ------------------------------------------------------------------ bản đồ
        void MapView(RectTransform c, ScrollRect sr)
        {
            var goal = UIK.Card(c, UIK.P, 16, 8);
            Row(goal.transform, "Thử thách hôm nay", Math.Min(S.xpDay, GOAL) + "/" + GOAL + " KN", UIK.Ink, UIK.Ink);
            Meter(goal.transform, S.xpDay / (float)GOAL, UIK.Y);
            UIK.Txt(goal.transform, S.xpDay >= GOAL ? "Đã đạt mục tiêu ngày. Giữ chuỗi vào ngày mai!" : "Hoàn thành 2–3 trạm để giữ chuỗi " + S.streak + " ngày.", 14, UIK.Mute);

            float[] offs = { -60, 48, -8 };
            RectTransform firstOpen = null;
            var LV = Data5.LV;
            for (int l = 0; l < LV.Count; l++)
            {
                bool lk = !IsOpen(l * 3);
                var band = UIK.Box(c, lk ? UIK.Hex("#1a1c20") : UIK.Hex("#2b2410"), UIK.RoundSmall, "Level");
                var bh = UIK.HL(band.gameObject, 10, 14, false); bh.childForceExpandHeight = false; bh.childAlignment = TextAnchor.MiddleLeft;
                var tc = UIK.RT("T", band.transform); UIK.VL(tc.gameObject, 2); UIK.LE(tc.gameObject, -1, -1, 1);
                UIK.Txt(tc, "Tầng " + (l + 1) + " · " + LV[l].name, 18, lk ? UIK.Mute : UIK.Ink, FontStyle.Bold);
                UIK.Txt(tc, LV[l].sub, 14, UIK.Mute);
                var dp = UIK.Txt(band.transform, "Mức −" + ((l + 1) * 50), 14, lk ? UIK.Mute : UIK.Y, FontStyle.Bold, TextAnchor.MiddleRight);
                dp.horizontalOverflow = HorizontalWrapMode.Overflow;
                UIK.LE(dp.gameObject, -1, -1, 0, -1, dp.preferredWidth + 4);

                for (int s = 0; s < 3; s++)
                {
                    int g = l * 3 + s;
                    bool done = S.done.Contains(g), open = IsOpen(g);
                    var row = UIK.RT("NodeRow", c);
                    UIK.LE(row.gameObject, 118, 118);
                    // đường ray nét đứt
                    for (int k = 0; k < 6; k++)
                    {
                        var d = UIK.Flat(row, UIK.Hex("#3a3f49"), "Dash");
                        d.rectTransform.anchorMin = d.rectTransform.anchorMax = new Vector2(0.5f, 1);
                        d.rectTransform.sizeDelta = new Vector2(3, 10);
                        d.rectTransform.anchoredPosition = new Vector2(0, -8 - k * 20);
                        d.raycastTarget = false;
                    }
                    Color face = done ? UIK.Hex("#d9a514") : open ? UIK.Y : UIK.Hex("#2b2f37");
                    Color shade = done ? UIK.Hex("#8a6600") : open ? UIK.Y2 : UIK.Hex("#1a1d22");
                    var sh = UIK.Box(row, shade, UIK.Circle, "Shadow");
                    Place(sh.rectTransform, offs[s], -16 - 7, 76, 70);
                    sh.raycastTarget = false;
                    var nb = UIK.Box(row, face, UIK.Circle, "Node");
                    Place(nb.rectTransform, offs[s], -16, 76, 70);
                    var btn = nb.gameObject.AddComponent<Button>();
                    string ic = done ? "check" : open ? Data5.StationIcons[s] : "lock";
                    var icon = VG.Icon(nb.transform, ic, open || done ? UIK.Hex("#2a1d00") : UIK.Hex("#5b616c"), 32);
                    icon.rectTransform.anchoredPosition = Vector2.zero;
                    var lab = UIK.Txt(row, Data5.Stations[s], 14, open || done ? UIK.Ink : UIK.Mute, FontStyle.Bold, TextAnchor.UpperCenter);
                    Place(lab.rectTransform, offs[s], -94, 120, 22);
                    if (open && !done && firstOpen == null)
                    {
                        firstOpen = row;
                        var bub = UIK.Box(row, UIK.Ink, UIK.RoundSmall, "Bubble");
                        Place(bub.rectTransform, offs[s] + 70, -30, 88, 30);
                        var bt = UIK.Txt(bub.transform, "Bắt đầu", 14, UIK.Bg, FontStyle.Bold, TextAnchor.MiddleCenter);
                        UIK.Stretch(bt.rectTransform);
                        bub.raycastTarget = false;
                    }
                    btn.onClick.AddListener(() =>
                    {
                        if (!IsOpen(g)) { UIK.Toast("Hoàn thành trạm trước để mở khóa"); return; }
                        Regen();
                        if (S.hearts <= 0) { NoHearts(); return; }
                        Sfx.Play("tap");
                        StartLesson(g / 3, g % 3);
                    });
                }
            }
            UIK.Txt(c, "Nội dung câu hỏi là bản mẫu để test, cần cán bộ an toàn rà soát theo QCVN 01:2011/BCT và nội quy của đơn vị.", 13, UIK.Mute);
            if (firstOpen != null) App.I.StartCoroutine(ScrollTo(sr, firstOpen));
        }

        static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x, y);
        }

        static IEnumerator ScrollTo(ScrollRect sr, RectTransform target)
        {
            yield return null;
            if (sr == null || target == null) yield break;
            Canvas.ForceUpdateCanvases();
            float ch = sr.content.rect.height, vh = sr.viewport.rect.height;
            if (ch <= vh) yield break;
            float y = -sr.content.InverseTransformPoint(target.position).y - vh / 2;
            sr.verticalNormalizedPosition = 1 - Mathf.Clamp01(y / (ch - vh));
        }

        // ------------------------------------------------------------------ ôn tập
        void RevView(RectTransform c)
        {
            var list = S.wrong.Where(id => Data5.QM.ContainsKey(id)).Select(id => Data5.QM[id]).ToList();
            int unlocked = 0;
            for (int l = 0; l < Data5.LV.Count; l++) if (IsOpen(l * 3)) unlocked++;
            UIK.Txt(c, "Trạm ôn tập", 24, UIK.Ink, FontStyle.Bold);
            var card = UIK.Card(c, UIK.P, 16, 10);
            Row(card.transform, "Câu đã trả lời sai", list.Count.ToString(), UIK.Ink, UIK.Ink);
            UIK.Txt(card.transform, "Ôn không mất tim. Mỗi lượt ôn xong hồi 1 tim.", 14, UIK.Mute);
            var rb = UIK.Btn(card.transform, list.Count > 0 ? "Ôn " + Math.Min(6, list.Count) + " câu sai" : "Chưa có câu sai", UIK.Y, UIK.Dark, 17, () => StartRev(list, false), 54);
            rb.interactable = list.Count > 0;
            var pool = new List<Q>();
            for (int l = 0; l < unlocked; l++) { pool.AddRange(Data5.LV[l].kt); pool.AddRange(Data5.LV[l].vd); }
            UIK.Btn(card.transform, "Luyện tập tự do (" + unlocked + " tầng đã mở)", UIK.P2, UIK.Ink, 16, () => StartRev(pool, true), 52);
            UIK.Txt(c, "Danh sách", 20, UIK.Ink, FontStyle.Bold);
            if (list.Count == 0) UIK.Txt(c, "Chưa có câu sai nào. Tốt lắm!", 15, UIK.Mute);
            foreach (var q in list)
            {
                var r = UIK.Card(c, UIK.P, 14, 2);
                UIK.Txt(r.transform, "Tầng " + (q.lv + 1), 13, UIK.Y, FontStyle.Bold);
                UIK.Txt(r.transform, q.q, 16, UIK.Ink);
            }
        }

        // ------------------------------------------------------------------ xếp hạng
        void LbView(RectTransform c)
        {
            UIK.Txt(c, "Bảng xếp hạng tuần", 24, UIK.Ink, FontStyle.Bold);
            int me = string.IsNullOrEmpty(S.team) ? -1 : Array.IndexOf(Data5.Teams, S.team);
            if (me < 0)
            {
                var card = UIK.Card(c, UIK.P, 16, 10);
                UIK.Txt(card.transform, "Chọn tổ của bạn để lên bảng", 18, UIK.Ink, FontStyle.Bold);
                UIK.Txt(card.transform, "Họ tên", 15, UIK.Mute, FontStyle.Bold);
                var input = InputBox(card.transform, "Ví dụ: Nguyễn Văn A");
                UIK.Txt(card.transform, "Tổ / phân xưởng", 15, UIK.Mute, FontStyle.Bold);
                int pickT = 0;
                var btns = new List<Button>();
                for (int i = 0; i < Data5.Teams.Length; i++)
                {
                    int ii = i;
                    var b = UIK.Btn(card.transform, Data5.Teams[i], i == 0 ? UIK.Hex("#3a3215") : UIK.P2, UIK.Ink, 16, null, 46, false, TextAnchor.MiddleLeft);
                    btns.Add(b);
                    b.onClick.AddListener(() =>
                    {
                        pickT = ii; Sfx.Play("tick");
                        for (int k = 0; k < btns.Count; k++) btns[k].image.color = k == ii ? UIK.Hex("#3a3215") : UIK.P2;
                    });
                }
                UIK.Btn(card.transform, "Tham gia bảng xếp hạng", UIK.Y, UIK.Dark, 17, () =>
                {
                    S.name = string.IsNullOrEmpty(input.text.Trim()) ? "Bạn" : input.text.Trim();
                    S.team = Data5.Teams[pickT]; Save(); Home("lb");
                }, 54);
            }
            else
            {
                var seg = UIK.RT("Seg", c);
                UIK.HL(seg.gameObject, 6, 0, true);
                UIK.LE(seg.gameObject, 48);
                UIK.Btn(seg, "Tổ / phân xưởng", lbMode == "team" ? UIK.P2 : UIK.P, lbMode == "team" ? UIK.Ink : UIK.Mute, 15, () => { lbMode = "team"; Home("lb"); }, 46);
                UIK.Btn(seg, "Trong tổ", lbMode == "me" ? UIK.P2 : UIK.P, lbMode == "me" ? UIK.Ink : UIK.Mute, 15, () => { lbMode = "me"; Home("lb"); }, 46);
            }

            var rows = new List<KeyValuePair<string, int>>();
            int meRow = -1;
            if (lbMode == "me" && me >= 0)
            {
                for (int i = 0; i < Data5.Mates.Length; i++) rows.Add(new KeyValuePair<string, int>(Data5.Mates[i], Data5.MateXp[i]));
                rows.Add(new KeyValuePair<string, int>(S.name + " (bạn)", S.xp));
            }
            else
                for (int i = 0; i < Data5.Teams.Length; i++) rows.Add(new KeyValuePair<string, int>(Data5.Teams[i], Data5.TeamBase[i] + (i == me ? S.xp : 0)));
            rows = rows.OrderByDescending(r => r.Value).ToList();
            string meKey = lbMode == "me" && me >= 0 ? S.name + " (bạn)" : (me >= 0 ? S.team : null);
            for (int i = 0; i < rows.Count; i++)
            {
                bool isMe = rows[i].Key == meKey;
                if (isMe) meRow = i;
                var r = UIK.Box(c, isMe ? UIK.Hex("#2a2615") : UIK.P, UIK.Round, "LbRow");
                var h = UIK.HL(r.gameObject, 12, 14, false); h.childForceExpandHeight = false; h.childAlignment = TextAnchor.MiddleLeft;
                var n = UIK.Txt(r.transform, (i + 1).ToString(), 17, UIK.Mute, FontStyle.Bold); UIK.LE(n.gameObject, -1, -1, 0, 26, 26);
                var nm = UIK.Txt(r.transform, rows[i].Key, 16, UIK.Ink); UIK.LE(nm.gameObject, -1, -1, 1);
                var sc = UIK.Txt(r.transform, rows[i].Value + " KN", 16, isMe ? UIK.Y : UIK.Ink, FontStyle.Bold, TextAnchor.MiddleRight);
                sc.horizontalOverflow = HorizontalWrapMode.Overflow;
                UIK.LE(sc.gameObject, -1, -1, 0, -1, sc.preferredWidth + 4);
            }
            UIK.Txt(c, "Điểm của các tổ khác và đồng nghiệp là dữ liệu mẫu cho bản test." + (me >= 0 ? " Bạn đang ở " + S.team + "." : ""), 13, UIK.Mute);
        }

        static InputField InputBox(Transform p, string placeholder)
        {
            var bg = UIK.Box(p, UIK.P2, UIK.Round, "Input");
            UIK.LE(bg.gameObject, 52, 52);
            var t = UIK.Txt(bg.transform, "", 18, UIK.Ink, FontStyle.Normal, TextAnchor.MiddleLeft);
            t.supportRichText = false;
            UIK.Stretch(t.rectTransform, 14, 6, 14, 6);
            var ph = UIK.Txt(bg.transform, placeholder, 18, UIK.Mute, FontStyle.Italic, TextAnchor.MiddleLeft);
            UIK.Stretch(ph.rectTransform, 14, 6, 14, 6);
            var f = bg.gameObject.AddComponent<InputField>();
            f.targetGraphic = bg; f.textComponent = t; f.placeholder = ph; f.characterLimit = 40;
            return f;
        }

        void NoHearts()
        {
            long left = REGEN - (Now() - S.hts);
            int mins = Math.Max(1, (int)Math.Ceiling(left / 60.0));
            Action close;
            var m = UIK.Modal(out close);
            UIK.Txt(m, "Hết tim rồi", 24, UIK.Ink, FontStyle.Bold);
            UIK.Txt(m, "Tim hồi 1 sau mỗi 20 phút (tim tiếp theo sau khoảng " + mins + " phút). Ôn lại câu sai để hồi ngay 1 tim.", 16, UIK.Mute);
            UIK.Btn(m, "Vào trạm ôn tập", UIK.Y, UIK.Dark, 17, () => { close(); Home("rev"); }, 54);
            UIK.Btn(m, "Để sau", UIK.P2, UIK.Ink, 16, () => { close(); Home("map"); }, 52);
        }

        // ================================================================== bài học
        void StartLesson(int l, int s)
        {
            var L = Data5.LV[l];
            var list = new List<Q>();
            if (s == 0)
            {
                list.Add(new Q { t = QT.Rule, lv = l });
                var kt = new List<Q>(L.kt); UIK.Shuffle(kt); list.AddRange(kt);
            }
            else if (s == 1) { list.AddRange(L.vd); UIK.Shuffle(list); }
            else
            {
                var mine = new List<Q>(L.kt); mine.AddRange(L.vd); UIK.Shuffle(mine);
                var prev = new List<Q>();
                for (int i = 0; i < l; i++) { prev.AddRange(Data5.LV[i].kt); prev.AddRange(Data5.LV[i].vd); }
                UIK.Shuffle(prev);
                if (l == 0) list.AddRange(mine.Take(5));
                else { list.AddRange(mine.Take(2)); list.AddRange(prev.Take(3)); }
                UIK.Shuffle(list);
            }
            qs = list; qi = 0; mis = 0; ok = 0; total = list.Count; li = l; si = s; rev = false; free = false;
            t0 = Time.realtimeSinceStartup;
            ShowQ();
        }

        void StartRev(List<Q> pool, bool isFree)
        {
            var list = new List<Q>(pool); UIK.Shuffle(list);
            list = list.Take(isFree ? 5 : 6).ToList();
            if (list.Count == 0) return;
            qs = list; qi = 0; mis = 0; ok = 0; total = list.Count; rev = true; free = isFree;
            t0 = Time.realtimeSinceStartup;
            ShowQ();
        }

        void ShowQ()
        {
            if (qi >= qs.Count) { Finish(); return; }
            var q = qs[qi];
            var root = App.I.NewScreen();
            var lesson = UIK.RT("Lesson", root);
            UIK.Stretch(lesson);
            var v = UIK.VL(lesson.gameObject, 0);
            v.childForceExpandHeight = false;

            // đầu trang
            var head = UIK.RT("Head", lesson);
            UIK.LE(head.gameObject, 64, 64);
            var hh = UIK.HL(head.gameObject, 12, 0, false);
            hh.padding = new RectOffset(12, 16, 10, 10); hh.childForceExpandHeight = false; hh.childAlignment = TextAnchor.MiddleLeft;
            var x = UIK.Btn(head, "", UIK.Bg, UIK.Mute, 16, ConfirmExit, 44);
            UIK.LE(x.gameObject, 44, 44, -1, 44, 44);
            VG.Icon(x.transform, "x", UIK.Mute, 26);
            var bar = UIK.Box(head, UIK.P2, UIK.RoundSmall, "Bar");
            UIK.LE(bar.gameObject, 14, 14, 1);
            barFill = UIK.Box(bar.transform, UIK.Y, UIK.RoundSmall, "Fill");
            SetBar();
            if (rev) { var rt = UIK.Txt(head, "Ôn", 18, UIK.Blue, FontStyle.Bold, TextAnchor.MiddleRight); UIK.LE(rt.gameObject, -1, -1, 0, -1, 34); }
            else
            {
                VG.Icon(head, "heart", UIK.R, 20);
                heartTxt = UIK.Txt(head, S.hearts.ToString(), 18, UIK.R, FontStyle.Bold, TextAnchor.MiddleLeft);
                UIK.LE(heartTxt.gameObject, -1, -1, 0, -1, 18);
            }

            // thân
            ScrollRect sr;
            body = UIK.Scroll(lesson, 12, 18, out sr);
            UIK.LE(sr.gameObject, 100).flexibleHeight = 1;

            // chân
            var ft = UIK.Flat(lesson, UIK.Bg, "Footer");
            footer = ft.rectTransform;
            UIK.VL(ft.gameObject, 10, 18, 18, 14, 22);
            chk = UIK.Btn(footer, "Kiểm tra", UIK.Y, UIK.Dark, 18, null, 56);
            chk.interactable = false;

            switch (q.t)
            {
                case QT.Rule: RRule(q); break;
                case QT.MC: RMC(q); break;
                case QT.TF: RTF(q); break;
                case QT.Bin: RBin(q); break;
                case QT.Order: ROrder(q); break;
                case QT.Match: RMatch(q); break;
            }
        }

        void SetBar()
        {
            var rt = barFill.rectTransform;
            float p = total > 0 ? ok / (float)total : 0;
            rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(Mathf.Max(0.001f, p), 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            barFill.enabled = p > 0;
        }

        void ConfirmExit()
        {
            Action close;
            var m = UIK.Modal(out close);
            UIK.Txt(m, "Rời trạm?", 24, UIK.Ink, FontStyle.Bold);
            UIK.Txt(m, "Tiến độ của trạm này sẽ không được lưu.", 16, UIK.Mute);
            UIK.Btn(m, "Ở lại học tiếp", UIK.Y, UIK.Dark, 17, close, 54);
            UIK.Btn(m, "Rời trạm", UIK.P2, UIK.Ink, 16, () => { close(); Home("map"); }, 52);
        }

        Text Kind(string s) { return UIK.Txt(body, s, 15, UIK.Y, FontStyle.Bold); }
        Text Title(string s) { return UIK.Txt(body, s, 23, UIK.Ink, FontStyle.Bold); }

        void SetChk(string label, Action a)
        {
            UIK.Label(chk).text = label;
            chk.onClick.RemoveAllListeners();
            chk.onClick.AddListener(() => { if (chk.interactable) a(); });
        }

        static void Lock(RectTransform r)
        {
            var cg = r.gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = r.gameObject.AddComponent<CanvasGroup>();
            cg.interactable = false; cg.blocksRaycasts = false;
        }

        // ------------------------------------------------------------------ các dạng câu hỏi
        void RRule(Q q)
        {
            var L = Data5.LV[q.lv];
            var card = UIK.Card(body, UIK.P, 18, 8);
            UIK.Txt(card.transform, "Quy định mới · Tầng " + (q.lv + 1), 15, UIK.Y, FontStyle.Bold);
            var art = UIK.Box(card.transform, UIK.Hex("#0d0f12"), UIK.Round, "ArtBg");
            UIK.LE(art.gameObject, 150, 150);
            var g = UIK.RT("Art", art.transform); UIK.Stretch(g);
            var vg = g.gameObject.AddComponent<VG>(); vg.W = 300; vg.H = 150; vg.raycastTarget = false;
            DrawArt(vg, q.lv); vg.Commit();
            UIK.Txt(card.transform, L.name, 23, UIK.Ink, FontStyle.Bold);
            UIK.Txt(card.transform, L.rule, 18, UIK.Ink);
            chk.interactable = true;
            SetChk("Đã hiểu, làm bài", () => { ok++; qi++; Sfx.Play("tick"); ShowQ(); });
        }

        static void DrawArt(VG g, int lv)
        {
            Color yel = UIK.Y, dk = UIK.Hex("#22262d");
            switch (lv)
            {
                case 0:
                    g.Tri(new Vector2(180, 82), new Vector2(300, 40), new Vector2(300, 125), UIK.A(UIK.Hex("#fff6c9"), .18f));
                    g.Ellipse(150, 128, 96, 10, dk);
                    g.Dome(150, 118, 68, yel);
                    g.Rect(66, 112, 168, 13, UIK.Hex("#d39b00"));
                    g.Rect(138, 74, 26, 18, UIK.Hex("#3a3f49")); g.Circle(151, 83, 6, UIK.Hex("#fff6c9"));
                    g.Rect(238, 96, 30, 30, UIK.Hex("#c33a2f")); g.Rect(242, 100, 22, 6, UIK.Ink);
                    break;
                case 1:
                    g.Rect(20, 20, 110, 118, UIK.Hex("#2b2f37")); g.Rect(30, 30, 90, 108, UIK.Hex("#5a4630")); g.Rect(66, 50, 18, 14, UIK.Hex("#1a1d22"));
                    g.Polyline(VG.Cubic(new Vector2(150, 40), new Vector2(190, 40), new Vector2(200, 70), new Vector2(240, 70), 12), 22, yel);
                    g.Line(new Vector2(240, 70), new Vector2(300, 60), 22, yel);
                    g.Line(new Vector2(160, 110), new Vector2(240, 110), 5, UIK.Blue);
                    g.Polyline(new[] { new Vector2(226, 100), new Vector2(240, 110), new Vector2(226, 120) }, 5, UIK.Blue);
                    break;
                case 2:
                    g.Rect(0, 0, 300, 26, UIK.Hex("#2b2f37")); g.Rect(148, 26, 4, 6, UIK.Hex("#222"));
                    g.Rect(104, 30, 92, 72, UIK.Hex("#3a3f49")); g.Rect(114, 42, 72, 32, UIK.Hex("#0d1a12"));
                    g.Rect(126, 52, 48, 12, UIK.G); g.Circle(182, 90, 4, UIK.G);
                    g.Polyline(VG.Quadr(new Vector2(40, 60), new Vector2(50, 46), new Vector2(60, 60), 8), 3, UIK.A(UIK.Blue, .6f));
                    g.Polyline(VG.Quadr(new Vector2(210, 52), new Vector2(220, 38), new Vector2(230, 52), 8), 3, UIK.A(UIK.Blue, .6f));
                    break;
                case 3:
                    g.Poly(new[] { new Vector2(30, 140), new Vector2(80, 70), new Vector2(120, 100), new Vector2(170, 50), new Vector2(230, 110), new Vector2(270, 80), new Vector2(290, 140) }, UIK.Hex("#1b1d21"));
                    g.Ellipse(155, 104, 14, 22, UIK.Hex("#ff7a2f")); g.Ellipse(157, 110, 6, 12, yel);
                    g.Rect(236, 20, 16, 80, UIK.Ink); g.Rect(240, 44, 8, 52, UIK.R); g.Circle(244, 104, 12, UIK.R);
                    break;
                default:
                    g.Rect(0, 0, 300, 150, UIK.Hex("#1f2228")); g.Rect(0, 0, 300, 40, UIK.Hex("#2b2f37"));
                    float[] xs = { 80, 150, 220, 116, 186 }; float[] ys = { 58, 54, 60, 94, 100 };
                    for (int i = 0; i < xs.Length; i++) { g.Circle(xs[i], ys[i] + 6, 6, UIK.Blue); g.Tri(new Vector2(xs[i] - 5.5f, ys[i] + 4), new Vector2(xs[i] + 5.5f, ys[i] + 4), new Vector2(xs[i], ys[i] - 8), UIK.Blue); }
                    g.Ellipse(150, 136, 110, 9, UIK.A(UIK.Hex("#2a5b7a"), .8f));
                    break;
            }
        }

        void RMC(Q q)
        {
            Kind("Chọn đáp án đúng");
            Title(q.q);
            var ord = Enumerable.Range(0, q.o.Length).ToList(); UIK.Shuffle(ord);
            int sel = -1;
            var btns = new Dictionary<int, Button>();
            int n = 1;
            foreach (int i in ord)
            {
                int ii = i;
                var b = UIK.Btn(body, n + ".   " + q.o[i], UIK.P, UIK.Ink, 17, null, 58, true, TextAnchor.MiddleLeft);
                UIK.Label(b).fontStyle = FontStyle.Normal;
                btns[i] = b; n++;
                b.onClick.AddListener(() =>
                {
                    sel = ii; Sfx.Play("tap");
                    foreach (var kv in btns) kv.Value.image.color = kv.Key == ii ? UIK.Hex("#3a3215") : UIK.P;
                    chk.interactable = true;
                });
            }
            SetChk("Kiểm tra", () =>
            {
                Lock(body);
                foreach (var kv in btns)
                    kv.Value.image.color = kv.Key == q.a ? UIK.Hex("#1d4a2d") : kv.Key == sel ? UIK.Hex("#5a2420") : UIK.P;
                Answer(sel == q.a, q);
            });
        }

        void RTF(Q q)
        {
            Kind("Vuốt phải nếu ĐÚNG, vuốt trái nếu SAI");
            var holder = UIK.RT("Deck", body);
            UIK.LE(holder.gameObject, 270, 270);
            var card = UIK.Box(holder, UIK.P2, UIK.Round, "Card");
            var crt = card.rectTransform;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(Mathf.Min(App.ColumnWidth - 40, 380), 240);
            var t1 = UIK.Txt(card.transform, "Nhận định", 14, UIK.Mute, FontStyle.Bold);
            UIK.Stretch(t1.rectTransform, 22, 0, 22, 20);
            var t2 = UIK.Txt(card.transform, q.q, 22, UIK.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            UIK.Stretch(t2.rectTransform, 22, 20, 22, 44);
            var sw = card.gameObject.AddComponent<SwipeCard>();
            sw.Frame = card;
            sw.OnDecide = v => Answer(v == q.tf, q);

            var row = UIK.RT("Btns", body);
            UIK.HL(row.gameObject, 12, 0, true);
            UIK.LE(row.gameObject, 58);
            UIK.Btn(row, "Sai", UIK.Hex("#3a1715"), UIK.Hex("#ffb3ad"), 18, () => { Lock(body); sw.Decide(false); }, 56);
            UIK.Btn(row, "Đúng", UIK.Hex("#14301f"), UIK.Hex("#9be7b5"), 18, () => { Lock(body); sw.Decide(true); }, 56);
            chk.gameObject.SetActive(false);
        }

        void RBin(Q q)
        {
            Kind("Kéo thả thẻ vào ô (hoặc chạm thẻ rồi chạm ô)");
            Title(q.q);
            var place = new Dictionary<int, int>();
            DragChip sel = null;
            var chips = new List<DragChip>();

            var pool = UIK.Box(body, UIK.Hex("#181a1e"), UIK.Round, "Pool");
            UIK.VL(pool.gameObject, 8, 10, 10, 10, 10);
            UIK.LE(pool.gameObject, 60);
            var pz = pool.gameObject.AddComponent<DropZone>(); pz.index = -1; pz.content = pool.rectTransform;

            var binsRow = UIK.RT("Bins", body);
            var bh = UIK.HL(binsRow.gameObject, 10, 0, true); bh.childForceExpandHeight = true;
            var zones = new List<DropZone>();
            for (int k = 0; k < q.bins.Length; k++)
            {
                var b = UIK.Box(binsRow, UIK.P, UIK.Round, "Bin");
                UIK.VL(b.gameObject, 6, 10, 10, 10, 10);
                UIK.LE(b.gameObject, 170);
                UIK.Txt(b.transform, q.bins[k], 16, UIK.Y, FontStyle.Bold);
                var cont = UIK.RT("In", b.transform);
                UIK.VL(cont.gameObject, 6);
                var z = b.gameObject.AddComponent<DropZone>(); z.index = k; z.content = cont;
                zones.Add(z);
                var bbtn = b.gameObject.AddComponent<Button>(); bbtn.transition = Selectable.Transition.None;
                var zz = z;
                bbtn.onClick.AddListener(() => { if (sel != null) Put(sel, zz); });
            }

            Action refresh = () =>
            {
                chk.interactable = place.Count == q.items.Length;
                foreach (var ch in chips) ch.GetComponent<Image>().color = ch == sel ? UIK.Hex("#3a3215") : UIK.P2;
            };
            // gán sau để Put dùng được refresh
            putImpl = (chip, zone) =>
            {
                chip.MoveTo(zone.content);
                if (zone.index < 0) place.Remove(chip.item); else place[chip.item] = zone.index;
                sel = null; Sfx.Play("tick"); refresh();
            };

            var order = Enumerable.Range(0, q.items.Length).ToList(); UIK.Shuffle(order);
            foreach (int i in order)
            {
                var im = UIK.Box(pool.transform, UIK.P2, UIK.Round, "Chip");
                UIK.VL(im.gameObject, 0, 14, 14, 10, 10);
                UIK.LE(im.gameObject, 44);
                UIK.Txt(im.transform, q.items[i], 16, UIK.Ink);
                var chip = im.gameObject.AddComponent<DragChip>();
                chip.item = i;
                chip.OnDrop = (c, z) => Put(c, z);
                chip.OnTap = c =>
                {
                    if (c.transform.parent != pool.transform) { Put(c, pz); return; }
                    sel = c; Sfx.Play("tap"); refresh();
                };
                chips.Add(chip);
            }

            SetChk("Kiểm tra", () =>
            {
                Lock(body);
                bool all = true;
                foreach (var ch in chips)
                {
                    int got; bool r = place.TryGetValue(ch.item, out got) && got == q.ib[ch.item];
                    ch.Locked = true;
                    ch.GetComponent<Image>().color = r ? UIK.Hex("#1d4a2d") : UIK.Hex("#5a2420");
                    if (!r) all = false;
                }
                Answer(all, q);
            });
        }

        Action<DragChip, DropZone> putImpl;
        void Put(DragChip c, DropZone z) { if (putImpl != null) putImpl(c, z); }

        void ROrder(Q q)
        {
            Kind("Chạm các bước theo đúng thứ tự");
            Title(q.q);
            var ans = new List<int>();
            var ansBox = UIK.Box(body, UIK.Hex("#181a1e"), UIK.Round, "Answer");
            UIK.VL(ansBox.gameObject, 8, 10, 10, 10, 10);
            UIK.LE(ansBox.gameObject, 60);
            UIK.Txt(body, "Các bước", 14, UIK.Mute, FontStyle.Bold);
            var poolBox = UIK.RT("Pool", body);
            UIK.VL(poolBox.gameObject, 8);
            var poolBtns = new Dictionary<int, Button>();
            var ansBtns = new List<Button>();

            Action draw = null;
            draw = () =>
            {
                UIK.Clear(ansBox.transform);
                ansBtns.Clear();
                if (ans.Count == 0) UIK.Txt(ansBox.transform, "Thứ tự của bạn sẽ hiện ở đây. Chạm để bỏ chọn.", 15, UIK.Mute);
                for (int k = 0; k < ans.Count; k++)
                {
                    int kk = k;
                    var b = UIK.Btn(ansBox.transform, (k + 1) + ".  " + q.items[ans[k]], UIK.Hex("#3a3215"), UIK.Ink, 16, () => { ans.RemoveAt(kk); draw(); }, 48, true, TextAnchor.MiddleLeft);
                    ansBtns.Add(b);
                }
                foreach (var kv in poolBtns)
                {
                    bool used = ans.Contains(kv.Key);
                    kv.Value.interactable = !used;
                    var cg = kv.Value.GetComponent<CanvasGroup>();
                    cg.alpha = used ? 0.25f : 1f;
                }
                chk.interactable = ans.Count == q.items.Length;
            };

            var order = Enumerable.Range(0, q.items.Length).ToList(); UIK.Shuffle(order);
            foreach (int i in order)
            {
                int ii = i;
                var b = UIK.Btn(poolBox, q.items[i], UIK.P2, UIK.Ink, 16, () => { if (!ans.Contains(ii)) { ans.Add(ii); Sfx.Play("tick"); draw(); } }, 48, true, TextAnchor.MiddleLeft);
                b.gameObject.AddComponent<CanvasGroup>();
                poolBtns[i] = b;
            }
            draw();
            SetChk("Kiểm tra", () =>
            {
                Lock(body);
                bool all = true;
                for (int k = 0; k < ans.Count; k++)
                {
                    bool r = ans[k] == k;
                    if (!r) all = false;
                    if (k < ansBtns.Count) ansBtns[k].image.color = r ? UIK.Hex("#1d4a2d") : UIK.Hex("#5a2420");
                }
                Answer(all, q);
            });
        }

        void RMatch(Q q)
        {
            Kind("Ghép nối các cặp · sai quá 1 lần sẽ tính là sai");
            Title(q.q);
            var cols = UIK.RT("Cols", body);
            var h = UIK.HL(cols.gameObject, 10, 0, true); h.childForceExpandHeight = false; h.childAlignment = TextAnchor.UpperLeft;
            var c0 = UIK.RT("A", cols); UIK.VL(c0.gameObject, 10);
            var c1 = UIK.RT("B", cols); UIK.VL(c1.gameObject, 10);
            var A = Enumerable.Range(0, q.pa.Length).ToList(); UIK.Shuffle(A);
            var B = Enumerable.Range(0, q.pb.Length).ToList(); UIK.Shuffle(B);
            Button selB = null; int selSide = -1, selIdx = -1, matched = 0, miss = 0;
            var done = new HashSet<Button>();
            chk.gameObject.SetActive(false);

            Action<Button, int, int> click = null;
            click = (b, side, idx) =>
            {
                if (done.Contains(b)) return;
                if (selB == null || selSide == side)
                {
                    if (selB != null) selB.image.color = UIK.P;
                    selB = b; selSide = side; selIdx = idx; b.image.color = UIK.Hex("#3a3215"); Sfx.Play("tap");
                    return;
                }
                if (selIdx == idx)
                {
                    foreach (var x in new[] { selB, b }) { x.image.color = UIK.Hex("#1d4a2d"); done.Add(x); x.GetComponent<CanvasGroup>().alpha = 0.55f; }
                    Sfx.Play("tick"); matched++;
                    selB = null; selSide = -1;
                    if (matched == q.pa.Length) { Lock(body); App.I.StartCoroutine(Later(0.3f, () => Answer(miss < 2, q))); }
                }
                else
                {
                    miss++; Sfx.Play("bad");
                    var a1 = selB; var a2 = b;
                    a1.image.color = UIK.Hex("#5a2420"); a2.image.color = UIK.Hex("#5a2420");
                    App.I.StartCoroutine(Later(0.4f, () => { if (!done.Contains(a1) && a1) a1.image.color = UIK.P; if (!done.Contains(a2) && a2) a2.image.color = UIK.P; }));
                    selB = null; selSide = -1;
                }
            };
            foreach (int i in A)
            {
                int ii = i; Button b = null;
                b = UIK.Btn(c0, q.pa[i], UIK.P, UIK.Ink, 15, () => click(b, 0, ii), 64, true, TextAnchor.MiddleLeft);
                b.gameObject.AddComponent<CanvasGroup>();
            }
            foreach (int i in B)
            {
                int ii = i; Button b = null;
                b = UIK.Btn(c1, q.pb[i], UIK.P, UIK.Ink, 15, () => click(b, 1, ii), 64, true, TextAnchor.MiddleLeft);
                b.gameObject.AddComponent<CanvasGroup>();
            }
        }

        static IEnumerator Later(float s, Action a)
        {
            yield return new WaitForSecondsRealtime(s);
            a();
        }

        static string Sol(Q q)
        {
            switch (q.t)
            {
                case QT.MC: return q.o[q.a];
                case QT.TF: return "Nhận định này " + (q.tf ? "ĐÚNG" : "SAI");
                case QT.Order: return string.Join(" > ", q.items);
                case QT.Bin:
                    var parts = new List<string>();
                    for (int k = 0; k < q.bins.Length; k++)
                    {
                        var its = new List<string>();
                        for (int i = 0; i < q.items.Length; i++) if (q.ib[i] == k) its.Add(q.items[i]);
                        parts.Add(q.bins[k] + ": " + string.Join(", ", its));
                    }
                    return string.Join(" · ", parts);
            }
            return "";
        }

        void Answer(bool right, Q q)
        {
            if (footer == null) return;
            Sfx.Play(right ? "ok" : "bad");
            if (right)
            {
                ok++;
                if (rev && !free) S.wrong.Remove(q.id);
            }
            else
            {
                mis++; qs.Add(q);
                if (!rev) { if (S.hearts == MAXH) S.hts = Now(); S.hearts = Math.Max(0, S.hearts - 1); }
                if (q.id != null && !S.wrong.Contains(q.id)) S.wrong.Add(q.id);
            }
            Save();
            SetBar();
            if (heartTxt != null) heartTxt.text = S.hearts.ToString();

            UIK.Clear(footer);
            footer.GetComponent<Image>().color = right ? UIK.Hex("#132a1b") : UIK.Hex("#2e1513");
            UIK.Txt(footer, right ? UIK.Pick(Data5.Praise) : "Chưa đúng", 22, right ? UIK.G : UIK.R, FontStyle.Bold);
            if (!right) { var s = Sol(q); if (s != "") UIK.Txt(footer, "Đáp án: " + s, 16, UIK.Ink, FontStyle.Bold); }
            if (!string.IsNullOrEmpty(q.ex)) UIK.Txt(footer, q.ex, 16, UIK.Ink);
            UIK.Btn(footer, "Tiếp tục", right ? UIK.G : UIK.R, right ? UIK.Hex("#06200f") : UIK.Hex("#2b0705"), 18, () =>
            {
                qi++;
                if (!rev && S.hearts <= 0) { NoHearts(); return; }
                ShowQ();
            }, 56);
        }

        void Finish()
        {
            int sec = Mathf.RoundToInt(Time.realtimeSinceStartup - t0);
            DayRoll();
            int gain; string unlock = "";
            if (rev) { gain = free ? 5 : 8; if (!free) S.hearts = Math.Min(MAXH, S.hearts + 1); }
            else
            {
                gain = 10 + (mis == 0 ? 5 : 0);
                int g = li * 3 + si;
                if (!S.done.Contains(g)) S.done.Add(g);
                if (si == 2 && li < Data5.LV.Count - 1) unlock = "Đã mở khóa Tầng " + (li + 2) + ": " + Data5.LV[li + 1].name;
                if (si == 2 && li == Data5.LV.Count - 1) unlock = "Bạn đã chinh phục toàn bộ đường lò!";
            }
            S.xp += gain; S.xpDay += gain;
            bool up = false;
            if (S.last != Today()) { S.streak = S.last == Yesterday() ? S.streak + 1 : 1; S.last = Today(); up = true; }
            Save();
            Sfx.Play("win");
            int acc = Mathf.RoundToInt(total * 100f / (total + mis));

            var root = App.I.NewScreen();
            ScrollRect sr;
            var c = UIK.Scroll(root, 16, 20, out sr);
            UIK.Stretch((RectTransform)sr.transform);
            UIK.Spacer(c, 40);
            UIK.Txt(c, rev ? "Ôn tập xong!" : "Hoàn thành trạm!", 36, UIK.Y, FontStyle.Bold, TextAnchor.UpperCenter);
            UIK.Txt(c, rev ? (free ? "Luyện thêm một lượt, kiến thức chắc hơn." : "Đã hồi 1 tim. Các câu làm đúng được bỏ khỏi danh sách ôn.") : Data5.LV[li].name + " · " + Data5.Stations[si], 17, UIK.Mute, FontStyle.Normal, TextAnchor.UpperCenter);
            var tiles = UIK.RT("Tiles", c);
            UIK.HL(tiles.gameObject, 10, 0, true);
            UIK.LE(tiles.gameObject, 84);
            Tile(tiles, "Kinh nghiệm", "+" + gain, UIK.Blue);
            Tile(tiles, "Thời gian", UIK.Mmss(sec), UIK.Ink);
            Tile(tiles, "Chính xác", acc + "%", UIK.G);
            var st = UIK.Card(c, UIK.P, 16, 4);
            Row(st.transform, "Chuỗi " + S.streak + " ngày", up ? "Tăng chuỗi!" : "Đã tính hôm nay", UIK.Orange, UIK.Ink);
            if (unlock != "")
            {
                var u = UIK.Card(c, UIK.Hex("#2a2615"), 16, 4);
                UIK.Txt(u.transform, unlock, 18, UIK.Y, FontStyle.Bold);
            }
            UIK.Btn(c, "Tiếp tục", UIK.Y, UIK.Dark, 18, () => Home("map"), 56);
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
