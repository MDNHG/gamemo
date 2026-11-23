using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    public class Worker { public bool left = true, helmet = true, resc = true; public string hand; }

    /// <summary>Cấu hình một đoạn đường lò.</summary>
    public class Seg
    {
        public bool doors; public string ds = "none"; // none: cửa 1 đóng · A: cửa 1 mở, cửa 2 đóng · AB: cả hai mở
        public Worker worker;
        public string duct = "ok", sensor = "roof";
        public float ch4 = 0.3f; public int temp = 27;
        public bool smoke, water, cart;
        public string sign = "Đoạn 01";
        public List<string> an = new List<string>();
    }

    public struct Hit { public string id; public Rect r; }

    /// <summary>Giữ tỉ lệ khung cảnh 720×880 vừa với ô cha.</summary>
    public class ScaleFit : MonoBehaviour
    {
        public Vector2 design = new Vector2(720, 880);
        RectTransform rt;
        void Awake() { rt = (RectTransform)transform; }
        void LateUpdate()
        {
            var p = rt.parent as RectTransform;
            if (p == null) return;
            float s = Mathf.Min(p.rect.width / design.x, p.rect.height / design.y);
            if (s > 0) rt.localScale = new Vector3(s, s, 1);
        }
    }

    /// <summary>Vẽ cảnh đường lò phối cảnh bằng VG + chữ, trả về vùng chạm của các lỗi.</summary>
    public class SceneView
    {
        public RectTransform Root;   // khung 720×880, scale theo ô cha
        public VG G;
        RectTransform labels;
        public List<Hit> Hits = new List<Hit>();
        Dictionary<string, Vector2> anchors = new Dictionary<string, Vector2>();
        Seg c;

        public SceneView(Transform holder)
        {
            Root = UIK.RT("Scene", holder);
            Root.anchorMin = Root.anchorMax = new Vector2(0.5f, 0.5f);
            Root.sizeDelta = new Vector2(720, 880);
            Root.gameObject.AddComponent<ScaleFit>();
            var mask = Root.gameObject.AddComponent<RectMask2D>();
            mask.softness = Vector2Int.zero;
            var g = UIK.RT("VG", Root); UIK.Stretch(g);
            G = g.gameObject.AddComponent<VG>();
            G.W = 360; G.H = 440;
            labels = UIK.RT("Labels", Root); UIK.Stretch(labels);
        }

        // ------------------------------------------------------------------ hình học
        static Vector2 P(float X, float Y, float s) { return new Vector2(180 + (X - 180) * s, 175 + (Y - 175) * s); }

        static List<Vector2> ArchPts(float s)
        {
            var r = new List<Vector2> { P(14, 440, s) };
            r.AddRange(VG.Cubic(P(14, 175, s), P(14, -10, s), P(346, -10, s), P(346, 175, s), 22));
            r.Add(P(346, 440, s));
            return r;
        }

        static List<Vector2> Clip(List<Vector2> poly, bool axisX, float v, bool keepLess)
        {
            var o = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 a = poly[i], b = poly[(i + 1) % poly.Count];
                float av = axisX ? a.x : a.y, bv = axisX ? b.x : b.y;
                bool ain = keepLess ? av <= v : av >= v, bin = keepLess ? bv <= v : bv >= v;
                if (ain) o.Add(a);
                if (ain != bin)
                {
                    float t = (v - av) / (bv - av);
                    o.Add(a + (b - a) * t);
                }
            }
            return o;
        }

        void Arch(float s)
        {
            var pts = ArchPts(s);
            G.Polyline(pts, 15 * s, UIK.Hex("#565a60"));
            var hl = new List<Vector2>();
            foreach (var p in pts) hl.Add(p + new Vector2(-4 * s, 0));
            G.Polyline(hl, 2.5f * s, UIK.Hex("#9298a0"), false);
        }

        void Door(float s, bool open, int n)
        {
            var arch = ArchPts(s);
            Vector2 h0 = P(125, 215, s), h1 = P(235, 440, s);
            Color c1 = UIK.Hex("#4b3b2b"), c2 = UIK.Hex("#433427");
            int band = 0;
            for (float Y = 20; Y < 440; Y += 34, band++)
            {
                float y0 = P(0, Y, s).y, y1 = P(0, Y + 34, s).y;
                var bp = Clip(Clip(arch, false, y0, false), false, y1, true);
                if (bp.Count < 3) continue;
                Color col = band % 2 == 0 ? c1 : c2;
                if (y1 > h0.y)
                {
                    // dải giao với ô cửa: chia làm phần trên ô cửa, trái, phải
                    if (y0 < h0.y) G.Poly(Clip(bp, false, h0.y, true), col);
                    var low = Clip(bp, false, h0.y, false);
                    G.Poly(Clip(low, true, h0.x, true), col);
                    G.Poly(Clip(low, true, h1.x, false), col);
                }
                else G.Poly(bp, col);
            }
            if (!open)
            {
                G.Rect(h0.x, h0.y, h1.x - h0.x, h1.y - h0.y, UIK.Hex("#6d5136"));
                G.Outline(h0.x, h0.y, h1.x - h0.x, h1.y - h0.y, 3 * s, UIK.Hex("#2a1f15"));
                var w = P(160, 252, s); G.Rect(w.x, w.y, 40 * s, 26 * s, UIK.Hex("#0c0a08"));
                var hd = P(222, 330, s); G.Circle(hd.x, hd.y, 5 * s, UIK.Hex("#c9a24a"));
            }
            else
            {
                G.Quad(P(125, 215, s), P(125, 440, s), P(86, 462, s), P(86, 198, s), UIK.Hex("#5a4229"));
            }
            var sg = P(140, 170, s);
            G.Rect(sg.x, sg.y, 80 * s, 28 * s, UIK.Hex("#d9b000"));
            var sc = P(180, 184, s);
            Label(sc.x, sc.y, "CỬA GIÓ " + n, 13 * s, UIK.Hex("#1c1600"), true);
        }

        void AddHit(string id, float x, float y, float w, float h)
        {
            if (c.an.Contains(id)) Hits.Add(new Hit { id = id, r = new Rect(x, y, w, h) });
        }

        public void Label(float x, float y, string s, float size, Color col, bool bold)
        {
            var t = UIK.Txt(labels, s, Mathf.RoundToInt(size * 2), col, bold ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(x / 360f, 1 - y / 440f);
            rt.sizeDelta = new Vector2(300, size * 3);
            rt.anchoredPosition = Vector2.zero;
        }

        // ------------------------------------------------------------------ vẽ
        public void Render(Seg seg, Color? hl = null, Vector2? tap = null, List<KeyValuePair<string, int>> badges = null)
        {
            c = seg;
            G.Clear(); Hits.Clear(); anchors.Clear();
            UIK.Clear(labels);

            G.Rect(0, 0, 360, 440, UIK.Hex("#2c2620"));
            float[,] blot = { { 30, 200, .9f }, { 330, 140, .8f }, { 40, 330, .7f }, { 320, 400, .95f }, { 60, 90, .6f }, { 300, 60, .7f } };
            for (int i = 0; i < blot.GetLength(0); i++) { var p = P(blot[i, 0], blot[i, 1], blot[i, 2]); G.Ellipse(p.x, p.y, 26 * blot[i, 2], 12 * blot[i, 2], UIK.A(UIK.Hex("#3a3229"), .7f)); }
            G.Quad(P(14, 440, 1), P(346, 440, 1), P(346, 440, .12f), P(14, 440, .12f), UIK.Hex("#383129"));
            G.Quad(P(346, 265, 1), P(346, 265, .12f), P(346, 325, .12f), P(346, 325, 1), UIK.Hex("#0c0c0d"));
            G.Line(P(346, 282, 1), P(346, 282, .12f), 2, UIK.Hex("#30343a"));
            G.Line(P(346, 305, 1), P(346, 305, .12f), 2, UIK.Hex("#30343a"));
            foreach (float s in new[] { 1f, .8f, .64f, .52f, .42f, .34f, .27f, .21f, .16f, .12f }) G.Line(P(116, 440, s), P(244, 440, s), 9 * s, UIK.Hex("#4a3b2c"));
            G.Line(P(140, 440, 1), P(140, 440, .12f), 3, UIK.Hex("#8d939b"));
            G.Line(P(220, 440, 1), P(220, 440, .12f), 3, UIK.Hex("#8d939b"));
            foreach (float s in new[] { .13f, .17f, .23f }) Arch(s);
            if (c.cart) { var a = P(128, 330, .27f); var b = P(232, 425, .27f); G.Rect(a.x, a.y, b.x - a.x, b.y - a.y, UIK.Hex("#59606a")); }
            // bóng tối phía xa
            G.Circle(180, 175, 70, UIK.A(Color.black, .55f), 40);
            G.Circle(180, 175, 42, UIK.A(Color.black, .6f), 32);
            G.Circle(180, 175, 22, UIK.A(Color.black, .7f), 24);
            if (c.doors) Door(.31f, c.ds == "AB", 2);
            if (c.doors) G.Rect(P(14, 0, .31f).x, P(0, 20, .31f).y, P(346, 0, .31f).x - P(14, 0, .31f).x, P(0, 440, .31f).y - P(0, 20, .31f).y, UIK.A(Color.black, .35f));
            Arch(.31f); Arch(.41f);
            if (c.doors) { Door(.55f, c.ds != "none", 1); anchors["door"] = new Vector2(180, 168); AddHit("doors_open", 138, 180, 84, 150); }
            Arch(.55f);

            // ống gió
            var DS = new List<float> { 1, .86f, .74f, .62f, .55f, .45f, .36f, .28f, .2f, .15f };
            if (c.doors) DS.RemoveAll(s => s < .55f);
            var dp = new List<Vector3>();
            foreach (var s in DS) { var p = P(46, 112, s); dp.Add(new Vector3(p.x, p.y, 24 * s)); }
            Color dy = UIK.Hex("#e3a400"), dr = UIK.Hex("#9a6d00");
            for (int i = dp.Count - 1; i > 0; i--)
            {
                Vector3 a = dp[i], b = dp[i - 1];
                G.Line(new Vector2(a.x, a.y - a.z / 2), new Vector2(a.x, a.y - a.z / 2 - 14 * DS[i]), 1.5f, UIK.Hex("#777777"));
                if (c.duct == "kink" && Mathf.Approximately(DS[i], .62f))
                {
                    var k = P(64, 200, .68f);
                    DuctSeg(a, new Vector3(k.x, k.y, 5), dy, dr);
                    DuctSeg(new Vector3(k.x, k.y, 5), b, dy, dr);
                    G.Circle(k.x, k.y, 4, dr);
                    AddHit("duct_kink", k.x - 30, b.y - 14, 62, k.y - b.y + 34);
                }
                else DuctSeg(a, b, dy, dr);
            }
            anchors["duct"] = new Vector2(78, 96);
            if (c.duct == "torn" && dp.Count > 1)
            {
                var m = new Vector2((dp[0].x + dp[1].x) / 2, (dp[0].y + dp[1].y) / 2);
                var star = new List<Vector2>();
                for (int i = 0; i < 12; i++) { float an = i * Mathf.PI / 6, r = i % 2 == 0 ? 11 : 5; star.Add(m + new Vector2(Mathf.Cos(an) * r, Mathf.Sin(an) * r * .8f)); }
                G.Poly(star, UIK.Hex("#1a1208"));
                Color air = UIK.A(UIK.Hex("#e8f3ff"), .75f);
                G.Polyline(VG.Quadr(m + new Vector2(0, 8), m + new Vector2(10, 28), m + new Vector2(30, 34), 8), 2, air, false);
                G.Polyline(VG.Quadr(m + new Vector2(6, 6), m + new Vector2(24, 18), m + new Vector2(40, 16), 8), 2, air, false);
                G.Polyline(VG.Quadr(m + new Vector2(-6, 10), m + new Vector2(-4, 32), m + new Vector2(8, 44), 8), 2, air, false);
                AddHit("duct_torn", m.x - 26, m.y - 24, 64, 64);
            }
            Arch(.74f);

            // khói từ vỉa than
            if (c.smoke)
            {
                G.Ellipse(296, 262, 22, 14, UIK.A(UIK.Hex("#ff6a2f"), .22f));
                Color sm = UIK.A(UIK.Hex("#d3d5d8"), .5f);
                G.Polyline(VG.Cubic(new Vector2(292, 262), new Vector2(276, 240), new Vector2(304, 210), new Vector2(288, 178), 12), 7, sm);
                G.Polyline(VG.Cubic(new Vector2(304, 258), new Vector2(316, 236), new Vector2(292, 214), new Vector2(306, 186), 12), 7, sm);
                G.Polyline(VG.Quadr(new Vector2(282, 266), new Vector2(268, 254), new Vector2(280, 238), 8), 7, sm);
                AddHit("smoke", 254, 168, 64, 104);
            }
            anchors["seam"] = new Vector2(312, 228);

            // nước rỉ
            if (c.water)
            {
                Color wc = UIK.A(UIK.Hex("#7cc6f0"), .55f);
                float[] wx = { 24, 40, 58, 76 }; float[] wy = { 120, 140, 128, 160 };
                for (int i = 0; i < 4; i++)
                {
                    var pts = VG.Cubic(new Vector2(wx[i], wy[i]), new Vector2(wx[i] + 6, wy[i] + 50), new Vector2(wx[i] - 7, wy[i] + 120), new Vector2(wx[i] + 1, wy[i] + 230 - i * 20), 14);
                    G.Polyline(pts, 3, wc, false);
                    G.Circle(wx[i] + 1, wy[i] + 90 + i * 30, 3.5f, UIK.Hex("#9fd8f7"));
                }
                G.Ellipse(100, 428, 72, 8, UIK.A(UIK.Hex("#4f8fb5"), .55f));
                G.Ellipse(88, 426, 30, 3, UIK.A(UIK.Hex("#cdeeff"), .4f));
                AddHit("water", 12, 108, 96, 330);
            }
            anchors["wall"] = new Vector2(30, 300);

            // cảm biến CH4
            if (c.sensor == "roof")
            {
                G.Line(new Vector2(269, 66), new Vector2(269, 92), 2, UIK.Hex("#222222"));
                G.Rect(256, 92, 26, 18, UIK.Hex("#2b5aa8")); G.Rect(260, 96, 12, 6, UIK.Hex("#9ff0b8")); G.Circle(277, 99, 2.2f, UIK.G);
            }
            else
            {
                G.Polyline(VG.Quadr(new Vector2(269, 66), new Vector2(300, 240), new Vector2(262, 386), 16), 2, UIK.Hex("#222222"), false);
                G.Quad(new Vector2(250, 386), new Vector2(276, 382), new Vector2(279, 400), new Vector2(253, 404), UIK.Hex("#2b5aa8"));
                G.Quad(new Vector2(254, 389), new Vector2(266, 387), new Vector2(267, 393), new Vector2(255, 395), UIK.Hex("#9ff0b8"));
                AddHit("sensor_floor", 236, 366, 56, 46);
            }
            anchors["sensor"] = new Vector2(292, 88);
            Arch(1);

            // bảng đo
            G.Rect(270, 326, 66, 50, UIK.Hex("#4a5059"));
            G.Rect(272, 328, 62, 46, UIK.Hex("#0e1216"));
            Label(303, 342, "CH₄ " + c.ch4.ToString("0.0").Replace('.', ',') + "%", 11.5f, UIK.Hex("#4be08a"), true);
            Label(303, 362, c.temp + " °C", 11.5f, UIK.Hex("#4be08a"), true);
            AddHit("ch4_high", 266, 322, 74, 58); AddHit("temp_high", 266, 322, 74, 58);
            anchors["board"] = new Vector2(264, 320);

            // biển số đoạn
            G.Rect(136, 46, 88, 22, UIK.Hex("#1f5d3a"));
            Label(180, 57, c.sign, 12, UIK.Hex("#e9fff1"), true);

            if (c.worker != null) DrawWorker(c.worker);

            // ánh sáng / viền tối
            G.Rect(0, 0, 360, 14, UIK.A(Color.black, .35f));
            G.Rect(0, 0, 10, 440, UIK.A(Color.black, .3f)); G.Rect(350, 0, 10, 440, UIK.A(Color.black, .3f));

            if (hl.HasValue)
                foreach (var h in Hits) G.Outline(h.r.x - 4, h.r.y - 4, h.r.width + 8, h.r.height + 8, 3.5f, hl.Value);
            if (tap.HasValue) { G.Circle(tap.Value.x, tap.Value.y, 12, UIK.A(Color.white, .9f)); G.Circle(tap.Value.x, tap.Value.y, 9, UIK.A(Color.black, .6f)); }
            if (badges != null)
                foreach (var b in badges)
                {
                    Vector2 a;
                    if (!anchors.TryGetValue(b.Key, out a)) continue;
                    G.Circle(a.x, a.y, 12.5f, UIK.Dark); G.Circle(a.x, a.y, 11, UIK.Y);
                    Label(a.x, a.y, b.Value.ToString(), 12, UIK.Dark, true);
                }
            G.Commit();
        }

        void DuctSeg(Vector3 a, Vector3 b, Color dy, Color dr)
        {
            Vector2 pa = new Vector2(a.x, a.y), pb = new Vector2(b.x, b.y);
            float w = (a.z + b.z) / 2;
            G.Line(pa, pb, w, dy);
            G.Circle(pa.x, pa.y, a.z / 2, dy, 14);
            var d = pb - pa; float len = d.magnitude; if (len < 1) return;
            var n = new Vector2(-d.y, d.x) / len;
            int rings = Mathf.Max(1, (int)(len / 9));
            for (int i = 1; i <= rings; i++)
            {
                var m = Vector2.Lerp(pa, pb, i / (float)(rings + 1));
                float ww = Mathf.Lerp(a.z, b.z, i / (float)(rings + 1));
                G.Line(m - n * ww / 2, m + n * ww / 2, 1.6f, dr);
            }
        }

        void DrawWorker(Worker w)
        {
            float cx = w.left ? 118 : 226, y = 420;
            Color skin = UIK.Hex("#d6a27a");
            G.Ellipse(cx, y + 2, 34, 6, UIK.A(Color.black, .4f));
            G.Rect(cx - 19, y - 82, 16, 66, UIK.Hex("#263a5c")); G.Rect(cx + 3, y - 82, 16, 66, UIK.Hex("#263a5c"));
            G.Rect(cx - 19, y - 46, 16, 5, UIK.Hex("#dfe6ec")); G.Rect(cx + 3, y - 46, 16, 5, UIK.Hex("#dfe6ec"));
            G.Rect(cx - 21, y - 18, 19, 18, UIK.Hex("#111111")); G.Rect(cx + 2, y - 18, 19, 18, UIK.Hex("#111111"));
            G.Rect(cx - 35, y - 134, 12, 52, UIK.Hex("#c95d17")); G.Rect(cx + 23, y - 134, 12, 52, UIK.Hex("#c95d17"));
            G.Rect(cx - 24, y - 140, 48, 64, UIK.Hex("#e8731c"));
            G.Rect(cx - 24, y - 114, 48, 6, UIK.Hex("#eef2f5"));
            G.Rect(cx - 25, y - 84, 50, 7, UIK.Hex("#1c1c1c"));
            G.Rect(cx - 22, y - 96, 12, 15, UIK.Hex("#333333"));
            if (w.resc) { G.Rect(cx + 6, y - 100, 17, 23, UIK.Hex("#b8bec6")); G.Rect(cx + 6, y - 100, 17, 6, UIK.Hex("#c33a2f")); }
            G.Circle(cx - 29, y - 80, 5.5f, skin); G.Circle(cx + 29, y - 80, 5.5f, skin);
            G.Rect(cx - 6, y - 146, 12, 8, UIK.Hex("#c99470"));
            G.Circle(cx, y - 156, 15, skin);
            if (w.helmet)
            {
                G.Dome(cx, y - 158, 19, UIK.Y);
                G.Rect(cx - 23, y - 160, 46, 5, UIK.Hex("#d39b00"));
                G.Rect(cx - 5, y - 172, 10, 8, UIK.Hex("#3a3f49"));
                G.Circle(cx, y - 168, 3, UIK.Hex("#fff6c9"));
                G.Polyline(VG.Quadr(new Vector2(cx - 17, y - 160), new Vector2(cx - 31, y - 140), new Vector2(cx - 19, y - 98), 10), 2, UIK.Hex("#222222"), false);
            }
            else
            {
                G.Dome(cx, y - 157, 15.5f, UIK.Hex("#2a1d14"));
            }
            if (w.hand == "lighter")
            {
                G.Rect(cx + 25, y - 94, 9, 13, UIK.Hex("#dd2222"));
                G.Ellipse(cx + 29.5f, y - 99, 3.2f, 5.5f, UIK.Hex("#ffb020"));
            }
            else if (w.hand == "phone")
            {
                G.Circle(cx + 29, y - 88, 16, UIK.A(UIK.Blue, .18f));
                G.Rect(cx + 22, y - 100, 14, 22, UIK.Hex("#111111")); G.Rect(cx + 24, y - 98, 10, 17, UIK.Blue);
            }
            else if (w.hand == "cigarette")
            {
                G.Line(new Vector2(cx + 4, y - 150), new Vector2(cx + 20, y - 148), 3, UIK.Ink);
                G.Circle(cx + 21, y - 148, 2, UIK.Hex("#ff6a2f"));
                G.Polyline(VG.Cubic(new Vector2(cx + 22, y - 152), new Vector2(cx + 28, y - 160), new Vector2(cx + 18, y - 168), new Vector2(cx + 24, y - 178), 10), 1.6f, UIK.A(UIK.Hex("#bbbbbb"), .6f), false);
            }
            AddHit("no_helmet", cx - 24, y - 180, 48, 42); AddHit("no_rescuer", cx - 2, y - 106, 34, 36);
            AddHit("lighter", cx + 12, y - 116, 34, 46); AddHit("phone", cx + 12, y - 116, 34, 46); AddHit("cigarette", cx - 6, y - 176, 42, 40);
            anchors["helmet"] = new Vector2(cx + 30, y - 176);
            anchors["resc"] = new Vector2(cx + 40, y - 96);
            anchors["hand"] = new Vector2(cx + 46, y - 70);
        }
    }
}
