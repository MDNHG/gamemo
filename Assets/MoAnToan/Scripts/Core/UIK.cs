using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>Bộ dựng giao diện uGUI bằng code: màu, font, sprite bo góc, nút, chữ, layout, modal, toast.</summary>
    public static class UIK
    {
        public static readonly Color Bg = Hex("#121417"), P = Hex("#1c1f24"), P2 = Hex("#252930"), Line = Hex("#323741"),
            Ink = Hex("#f3f1ea"), Mute = Hex("#9da3ad"), Y = Hex("#ffc21a"), Y2 = Hex("#b98400"), G = Hex("#35c76a"),
            R = Hex("#ff5b4f"), Blue = Hex("#6fc3ff"), Orange = Hex("#ff9b2f"), Dark = Hex("#241900");

        public static Font Font;
        public static Sprite Round, RoundSmall, Circle;
        public static RectTransform OverlayRoot; // nơi đặt modal, toast, thẻ đang kéo

        public static Color Hex(string h)
        {
            Color c;
            ColorUtility.TryParseHtmlString(h, out c);
            return c;
        }
        public static Color A(Color c, float a) { c.a = a; return c; }

        public static void Init()
        {
            if (Font != null) return;
            Font = Resources.Load<Font>("MAT_Font");
            if (Font == null) Font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Roboto", "Noto Sans" }, 32);
            Round = MakeRound(96, 28);
            RoundSmall = MakeRound(48, 12);
            Circle = MakeRound(128, 64);
        }

        static Sprite MakeRound(int size, int r)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, r, size - r);
                    float cy = Mathf.Clamp(y + 0.5f, r, size - r);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            t.SetPixels32(px);
            t.Apply();
            int b = Mathf.Min(r, size / 2 - 1);
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        // ------------------------------------------------------------------ cơ bản
        public static RectTransform RT(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        public static Image Box(Transform p, Color c, Sprite s = null, string name = "Box")
        {
            var rt = RT(name, p);
            var im = rt.gameObject.AddComponent<Image>();
            im.color = c;
            im.sprite = s == null ? Round : s;
            im.type = im.sprite == Circle ? Image.Type.Simple : Image.Type.Sliced;
            if (im.sprite == Round) im.pixelsPerUnitMultiplier = 1.75f;
            else if (im.sprite == RoundSmall) im.pixelsPerUnitMultiplier = 1.5f;
            return im;
        }

        public static Image Flat(Transform p, Color c, string name = "Flat")
        {
            var rt = RT(name, p);
            var im = rt.gameObject.AddComponent<Image>();
            im.color = c;
            return im;
        }

        public static Text Txt(Transform p, string s, int size, Color c, FontStyle st = FontStyle.Normal, TextAnchor a = TextAnchor.UpperLeft)
        {
            var rt = RT("Text", p);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font; t.text = s; t.fontSize = size; t.color = c; t.fontStyle = st; t.alignment = a;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true; t.raycastTarget = false; t.lineSpacing = 1.05f;
            return t;
        }

        public static LayoutElement LE(GameObject go, float minH = -1, float prefH = -1, float flexW = -1, float minW = -1, float prefW = -1)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            if (minH >= 0) le.minHeight = minH;
            if (prefH >= 0) le.preferredHeight = prefH;
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (minW >= 0) le.minWidth = minW;
            if (prefW >= 0) le.preferredWidth = prefW;
            return le;
        }

        public static VerticalLayoutGroup VL(GameObject go, float spacing, int padL = 0, int padR = 0, int padT = 0, int padB = 0, bool fit = false)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing; v.padding = new RectOffset(padL, padR, padT, padB);
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            if (fit) Fit(go);
            return v;
        }

        public static HorizontalLayoutGroup HL(GameObject go, float spacing, int pad = 0, bool expand = true)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing; h.padding = new RectOffset(pad, pad, pad, pad);
            h.childControlWidth = true; h.childControlHeight = true;
            h.childForceExpandWidth = expand; h.childForceExpandHeight = true;
            h.childAlignment = TextAnchor.MiddleLeft;
            return h;
        }

        public static void Fit(GameObject go)
        {
            var f = go.GetComponent<ContentSizeFitter>();
            if (f == null) f = go.AddComponent<ContentSizeFitter>();
            f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>Khung bo góc có layout dọc, cao theo nội dung.</summary>
        public static Image Card(Transform p, Color bg, int pad = 16, float spacing = 8)
        {
            var im = Box(p, bg, Round, "Card");
            VL(im.gameObject, spacing, pad, pad, pad, pad);
            return im;
        }

        /// <summary>Nút có chữ. autoH: nút cao theo chữ (nhiều dòng).</summary>
        public static Button Btn(Transform p, string label, Color bg, Color fg, int size, Action onClick, float minH = 56, bool autoH = false, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var im = Box(p, bg, Round, "Btn");
            var b = im.gameObject.AddComponent<Button>();
            var cb = b.colors;
            cb.highlightedColor = new Color(1, 1, 1, 1); cb.pressedColor = new Color(.82f, .82f, .82f, 1);
            cb.disabledColor = new Color(.6f, .6f, .6f, .6f);
            b.colors = cb;
            var t = Txt(im.transform, label, size, fg, FontStyle.Bold, align);
            if (autoH)
            {
                var v = VL(im.gameObject, 0, 16, 16, 14, 14);
                v.childAlignment = TextAnchor.MiddleLeft;
            }
            else Stretch(t.rectTransform, 12, 4, 12, 4);
            LE(im.gameObject, minH);
            if (onClick != null) b.onClick.AddListener(() => { Sfx.Play("tap"); onClick(); });
            return b;
        }

        public static Text Label(Button b) { return b.GetComponentInChildren<Text>(); }

        /// <summary>ScrollRect dọc; trả về content (có VerticalLayout + fitter).</summary>
        public static RectTransform Scroll(Transform p, float spacing, int pad, out ScrollRect sr)
        {
            var root = RT("Scroll", p);
            sr = root.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 30;
            var vp = RT("Viewport", root);
            Stretch(vp);
            vp.gameObject.AddComponent<RectMask2D>();
            var hit = vp.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0);
            var content = RT("Content", vp);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
            VL(content.gameObject, spacing, pad, pad, pad, pad, true);
            sr.viewport = vp; sr.content = content;
            return content;
        }

        public static RectTransform Spacer(Transform p, float h)
        {
            var rt = RT("Spacer", p);
            LE(rt.gameObject, h, h);
            return rt;
        }

        // ------------------------------------------------------------------ modal / toast
        public static RectTransform Modal(out Action close)
        {
            var dim = Flat(OverlayRoot, new Color(0, 0, 0, 0.72f), "Modal");
            Stretch(dim.rectTransform);
            var sheet = Box(dim.transform, P, Round, "Sheet");
            var rt = sheet.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0); rt.pivot = new Vector2(0.5f, 0);
            float w = Mathf.Min(App.ColumnWidth, 560);
            rt.sizeDelta = new Vector2(w, 0); rt.anchoredPosition = new Vector2(0, -24);
            VL(sheet.gameObject, 12, 20, 20, 22, 46, true);
            var go = dim.gameObject;
            close = () => { if (go != null) UnityEngine.Object.Destroy(go); };
            return rt;
        }

        public static void Toast(string msg)
        {
            var im = Box(OverlayRoot, Ink, Round, "Toast");
            var rt = im.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0); rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 110);
            var t = Txt(im.transform, msg, 17, Bg, FontStyle.Bold, TextAnchor.MiddleCenter);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            Stretch(t.rectTransform, 14, 8, 14, 8);
            rt.sizeDelta = new Vector2(Mathf.Min(t.preferredWidth + 32, App.ColumnWidth - 20), 46);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            UnityEngine.Object.Destroy(im.gameObject, 2.2f);
        }

        public static string Mmss(int sec) { return (sec / 60) + ":" + (sec % 60).ToString("00"); }

        public static void Shuffle<T>(IList<T> a)
        {
            for (int i = a.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T tmp = a[i]; a[i] = a[j]; a[j] = tmp;
            }
        }
        public static T Pick<T>(IList<T> a) { return a[UnityEngine.Random.Range(0, a.Count)]; }
    }
}
