using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Đồ họa vector đơn giản cho uGUI: đa giác, đường, hình tròn trong hệ tọa độ thiết kế W×H (y hướng xuống),
    /// tự co giãn theo RectTransform. Dùng cho cảnh đường lò và icon.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class VG : MaskableGraphic, IPointerClickHandler
    {
        public float W = 360, H = 440;
        public Action<Vector2> OnTap;

        readonly List<Vector2> pos = new List<Vector2>();
        readonly List<Color32> col = new List<Color32>();
        readonly List<int> tris = new List<int>();

        public void Clear() { pos.Clear(); col.Clear(); tris.Clear(); }
        public void Commit() { SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            var v = UIVertex.simpleVert;
            for (int i = 0; i < pos.Count; i++)
            {
                v.position = new Vector3(r.xMin + pos[i].x / W * r.width, r.yMax - pos[i].y / H * r.height, 0);
                v.color = col[i];
                vh.AddVert(v);
            }
            for (int i = 0; i + 2 < tris.Count; i += 3) vh.AddTriangle(tris[i], tris[i + 1], tris[i + 2]);
        }

        public Vector2 ScreenToDesign(PointerEventData e)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, e.position, e.pressEventCamera, out local);
            var r = rectTransform.rect;
            return new Vector2((local.x - r.xMin) / r.width * W, (r.yMax - local.y) / r.height * H);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (OnTap != null) OnTap(ScreenToDesign(e));
        }

        // ------------------------------------------------------------------ primitives
        int V(Vector2 p, Color c) { pos.Add(p); col.Add(c); return pos.Count - 1; }

        public void Tri(Vector2 a, Vector2 b, Vector2 c, Color k)
        {
            tris.Add(V(a, k)); tris.Add(V(b, k)); tris.Add(V(c, k));
        }

        public void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color k)
        {
            int i0 = V(a, k), i1 = V(b, k), i2 = V(c, k), i3 = V(d, k);
            tris.Add(i0); tris.Add(i1); tris.Add(i2);
            tris.Add(i0); tris.Add(i2); tris.Add(i3);
        }

        /// <summary>Đa giác tô kín (dùng quạt từ trọng tâm: đúng với hình lồi/hình sao).</summary>
        public void Poly(IList<Vector2> p, Color k)
        {
            if (p.Count < 3) return;
            Vector2 c = Vector2.zero;
            for (int i = 0; i < p.Count; i++) c += p[i];
            c /= p.Count;
            int ic = V(c, k);
            int first = pos.Count;
            for (int i = 0; i < p.Count; i++) V(p[i], k);
            for (int i = 0; i < p.Count; i++)
            {
                tris.Add(ic); tris.Add(first + i); tris.Add(first + (i + 1) % p.Count);
            }
        }

        public void Rect(float x, float y, float w, float h, Color k)
        {
            Quad(new Vector2(x, y), new Vector2(x + w, y), new Vector2(x + w, y + h), new Vector2(x, y + h), k);
        }

        public void Outline(float x, float y, float w, float h, float t, Color k)
        {
            Rect(x, y, w, t, k); Rect(x, y + h - t, w, t, k);
            Rect(x, y, t, h, k); Rect(x + w - t, y, t, h, k);
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color k, int seg = 28)
        {
            var p = new List<Vector2>(seg);
            for (int i = 0; i < seg; i++)
            {
                float a = i * Mathf.PI * 2 / seg;
                p.Add(new Vector2(cx + Mathf.Cos(a) * rx, cy + Mathf.Sin(a) * ry));
            }
            Poly(p, k);
        }

        public void Circle(float cx, float cy, float r, Color k, int seg = 24) { Ellipse(cx, cy, r, r, k, seg); }

        /// <summary>Nửa hình tròn phía trên (mũ bảo hộ).</summary>
        public void Dome(float cx, float cy, float r, Color k)
        {
            var p = new List<Vector2>();
            for (int i = 0; i <= 18; i++)
            {
                float a = Mathf.PI + i * Mathf.PI / 18;
                p.Add(new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r));
            }
            Poly(p, k);
        }

        public void Line(Vector2 a, Vector2 b, float w, Color k)
        {
            var d = b - a;
            if (d.sqrMagnitude < 1e-6f) return;
            var n = new Vector2(-d.y, d.x).normalized * (w / 2);
            Quad(a + n, b + n, b - n, a - n, k);
        }

        public void Polyline(IList<Vector2> p, float w, Color k, bool joints = true)
        {
            for (int i = 0; i + 1 < p.Count; i++) Line(p[i], p[i + 1], w, k);
            if (joints && w > 2.5f)
                for (int i = 0; i < p.Count; i++) Circle(p[i].x, p[i].y, w / 2, k, 10);
        }

        // ------------------------------------------------------------------ curves
        public static List<Vector2> Cubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int n)
        {
            var r = new List<Vector2>(n + 1);
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n, u = 1 - t;
                r.Add(u * u * u * a + 3 * u * u * t * b + 3 * u * t * t * c + t * t * t * d);
            }
            return r;
        }

        public static List<Vector2> Quadr(Vector2 a, Vector2 b, Vector2 c, int n)
        {
            var r = new List<Vector2>(n + 1);
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n, u = 1 - t;
                r.Add(u * u * a + 2 * u * t * b + t * t * c);
            }
            return r;
        }

        // ------------------------------------------------------------------ icons (24×24)
        public static VG Icon(Transform parent, string kind, Color k, float size)
        {
            var rt = UIK.RT("Icon_" + kind, parent);
            rt.sizeDelta = new Vector2(size, size);
            var g = rt.gameObject.AddComponent<VG>();
            g.W = 24; g.H = 24; g.raycastTarget = false;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = size; le.preferredHeight = size; le.minWidth = size; le.minHeight = size;
            DrawIcon(g, kind, k);
            g.Commit();
            return g;
        }

        public static void DrawIcon(VG g, string kind, Color k)
        {
            switch (kind)
            {
                case "heart":
                    g.Circle(8.3f, 9.3f, 4.8f, k); g.Circle(15.7f, 9.3f, 4.8f, k);
                    g.Tri(new Vector2(3.7f, 11f), new Vector2(20.3f, 11f), new Vector2(12, 20.5f), k);
                    g.Rect(8, 8, 8, 4, k);
                    break;
                case "flame":
                    g.Ellipse(12, 15, 6.2f, 6.5f, k);
                    g.Tri(new Vector2(6, 14), new Vector2(18, 14), new Vector2(13, 2), k);
                    g.Tri(new Vector2(6.5f, 13), new Vector2(11, 8), new Vector2(7.5f, 6), k);
                    break;
                case "gem":
                    g.Poly(new[] { new Vector2(12, 2), new Vector2(20, 9), new Vector2(12, 22), new Vector2(4, 9) }, k);
                    break;
                case "book":
                    g.Outline(3, 5, 7.5f, 14, 2.4f, k); g.Outline(13.5f, 5, 7.5f, 14, 2.4f, k);
                    break;
                case "helmet":
                    g.Dome(12, 16, 8.5f, k); g.Rect(2, 15.5f, 20, 3.4f, k); g.Circle(12, 11.5f, 2.3f, UIK.Hex("#fff6c9"));
                    break;
                case "star":
                    {
                        var p = new List<Vector2>();
                        for (int i = 0; i < 10; i++)
                        {
                            float a = -Mathf.PI / 2 + i * Mathf.PI / 5, r = i % 2 == 0 ? 10f : 4.4f;
                            p.Add(new Vector2(12 + Mathf.Cos(a) * r, 12.5f + Mathf.Sin(a) * r));
                        }
                        g.Poly(p, k);
                        break;
                    }
                case "lock":
                    g.Rect(5, 10.5f, 14, 9.5f, k);
                    {
                        var arc = new List<Vector2>();
                        for (int i = 0; i <= 10; i++) { float a = Mathf.PI + i * Mathf.PI / 10; arc.Add(new Vector2(12 + Mathf.Cos(a) * 4.2f, 8.5f + Mathf.Sin(a) * 4.2f)); }
                        arc.Insert(0, new Vector2(7.8f, 11)); arc.Add(new Vector2(16.2f, 11));
                        g.Polyline(arc, 2.4f, k, false);
                    }
                    break;
                case "check":
                    g.Polyline(new[] { new Vector2(5, 12.5f), new Vector2(10, 17.5f), new Vector2(19, 7) }, 3.4f, k);
                    break;
                case "x":
                    g.Line(new Vector2(6, 6), new Vector2(18, 18), 2.6f, k); g.Line(new Vector2(18, 6), new Vector2(6, 18), 2.6f, k);
                    break;
                case "back":
                    g.Polyline(new[] { new Vector2(14, 5), new Vector2(7, 12), new Vector2(14, 19) }, 2.8f, k);
                    break;
            }
        }
    }
}
