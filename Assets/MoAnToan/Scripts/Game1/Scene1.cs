using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Dựng khung cảnh 2D vector trực quan cho Game 1: Cửa lò, bốt kiểm soát, nhân vật thợ mỏ và ba lô.
    /// </summary>
    public class Scene1 : MonoBehaviour
    {
        public static RectTransform CreateVisual(Transform parent)
        {
            var root = UIK.RT("Scene1Visual", parent);
            UIK.LE(root.gameObject, -1, 230);

            // Nền trời & núi non phía xa
            var sky = UIK.Flat(root, UIK.Hex("#4a6d8c"), "Sky");
            UIK.Stretch(sky.rectTransform);

            // Đồi núi & cây cối phía sau
            var hills = UIK.Box(root, UIK.Hex("#2d4838"), UIK.Circle, "Hills");
            hills.rectTransform.anchorMin = new Vector2(0, 0.4f);
            hills.rectTransform.anchorMax = new Vector2(1, 1.2f);

            // Mặt đất sỏi đá cửa lò
            var ground = UIK.Flat(root, UIK.Hex("#7a6552"), "Ground");
            ground.rectTransform.anchorMin = new Vector2(0, 0);
            ground.rectTransform.anchorMax = new Vector2(1, 0.45f);

            // Vòm gỗ cửa lò (Mine portal tunnel)
            var portal = UIK.Box(root, UIK.Hex("#1e1a17"), UIK.Round, "Portal");
            portal.rectTransform.anchorMin = new Vector2(0.35f, 0.25f);
            portal.rectTransform.anchorMax = new Vector2(0.85f, 0.85f);
            portal.rectTransform.anchoredPosition = Vector2.zero;

            // Dầm gỗ chống cửa lò
            var beamH = UIK.Flat(portal.transform, UIK.Hex("#634127"), "BeamH");
            beamH.rectTransform.anchorMin = new Vector2(0, 0.85f);
            beamH.rectTransform.anchorMax = new Vector2(1, 1);
            var beamL = UIK.Flat(portal.transform, UIK.Hex("#4f321d"), "BeamL");
            beamL.rectTransform.anchorMin = new Vector2(0, 0);
            beamL.rectTransform.anchorMax = new Vector2(0.12f, 1);
            var beamR = UIK.Flat(portal.transform, UIK.Hex("#4f321d"), "BeamR");
            beamR.rectTransform.anchorMin = new Vector2(0.88f, 0);
            beamR.rectTransform.anchorMax = new Vector2(1, 1);

            // Đường ray xe goòng
            var rail = UIK.Flat(root, UIK.Hex("#3a3a3a"), "Rail");
            rail.rectTransform.anchorMin = new Vector2(0.48f, 0.05f);
            rail.rectTransform.anchorMax = new Vector2(0.72f, 0.28f);

            // Bốt "Trạm kiểm soát" bên trái
            var booth = UIK.Box(root, UIK.Hex("#274268"), UIK.Round, "Booth");
            booth.rectTransform.anchorMin = new Vector2(0.04f, 0.35f);
            booth.rectTransform.anchorMax = new Vector2(0.32f, 0.82f);
            var sign = UIK.Box(booth.transform, UIK.Hex("#182944"), UIK.Round, "Sign");
            sign.rectTransform.anchorMin = new Vector2(0.05f, 0.72f);
            sign.rectTransform.anchorMax = new Vector2(0.95f, 0.95f);
            var stxt = UIK.Txt(sign.transform, "TRẠM\nKIỂM SOÁT", 10, UIK.Ink, FontStyle.Bold);
            stxt.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(stxt.rectTransform);

            // Cửa sổ trạm gác + anh bảo vệ
            var win = UIK.Box(booth.transform, UIK.Hex("#7ec8e3"), UIK.Round, "Window");
            win.rectTransform.anchorMin = new Vector2(0.15f, 0.15f);
            win.rectTransform.anchorMax = new Vector2(0.85f, 0.65f);
            var guardIcon = VG.Icon(win.transform, "helmet", UIK.Hex("#1c3b70"), 24);
            guardIcon.rectTransform.anchoredPosition = Vector2.zero;

            // Rào chắn kiểm soát
            var barrier = UIK.Box(root, UIK.Hex("#d63e2a"), UIK.Round, "Barrier");
            barrier.rectTransform.anchorMin = new Vector2(0.02f, 0.26f);
            barrier.rectTransform.anchorMax = new Vector2(0.38f, 0.32f);

            // Nhân vật thợ mỏ đứng chuẩn bị
            var miner = UIK.RT("MinerChar", root);
            miner.anchorMin = new Vector2(0.25f, 0.12f);
            miner.anchorMax = new Vector2(0.42f, 0.55f);
            var body = UIK.Box(miner, UIK.Hex("#2b5999"), UIK.Round, "Body");
            body.rectTransform.anchorMin = new Vector2(0.15f, 0.1f);
            body.rectTransform.anchorMax = new Vector2(0.85f, 0.65f);
            var vest = UIK.Box(body.transform, UIK.Hex("#e68a00"), UIK.Round, "Vest");
            vest.rectTransform.anchorMin = new Vector2(0.15f, 0.2f);
            vest.rectTransform.anchorMax = new Vector2(0.85f, 0.95f);
            var head = UIK.Box(miner, UIK.Hex("#ffd099"), UIK.Circle, "Head");
            head.rectTransform.anchorMin = new Vector2(0.2f, 0.55f);
            head.rectTransform.anchorMax = new Vector2(0.8f, 0.95f);
            var helmet = VG.Icon(head.transform, "helmet", UIK.Hex("#ffc21a"), 24);
            helmet.rectTransform.anchoredPosition = new Vector2(0, 3);

            return root;
        }
    }
}
