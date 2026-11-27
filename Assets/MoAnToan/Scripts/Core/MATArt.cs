using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Quản lý nạp và cache toàn bộ Sprites, Icons, UI frames cho hệ thống đồ họa mới.
    /// </summary>
    public static class MATArt
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            if (cache.TryGetValue(relativePath, out var sp) && sp != null) return sp;

            sp = Resources.Load<Sprite>("Sprites/" + relativePath);
            if (sp != null)
            {
                cache[relativePath] = sp;
                return sp;
            }

            Debug.LogWarning("[MATArt] Không tìm thấy Sprite: Sprites/" + relativePath);
            return null;
        }

        public static Sprite UI(string name) => Get("UI/" + name);
        public static Sprite Icon(string name) => Get("Icons/" + name);
        public static Sprite CCTV(string name) => Get("CCTV/" + name);
        public static Sprite Env(string name) => Get("Environment/" + name);

        /// <summary>
        /// Tạo Image hiển thị Sprite tự động căn chỉnh.
        /// </summary>
        public static Image Img(Transform parent, Sprite sprite, string name = "Image", Image.Type type = Image.Type.Simple)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = type;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// Tạo 9-slice panel hoặc card với sprite có border.
        /// </summary>
        public static Image Sliced(Transform parent, Sprite sprite, string name = "Panel")
        {
            var img = Img(parent, sprite, name, Image.Type.Sliced);
            img.raycastTarget = false;
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            return img;
        }
    }
}
