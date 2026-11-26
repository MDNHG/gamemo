using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Vòng lặp chơi Game 1: "Hành trình thợ mỏ an toàn".
    /// Kéo trang bị vào ba lô, phân loại vật cấm, hoàn thành kiểm tra và đối thoại NPC.
    /// </summary>
    public class Game1
    {
        int curLevelIdx = 0;
        int hearts = 3;
        int stars = 0;

        RectTransform root;
        Transform backpackZoneTrans;
        Transform bannedZoneTrans;
        Transform itemsTrayTrans;

        List<Item1> itemsInBackpack = new List<Item1>();
        List<Item1> itemsInBanned = new List<Item1>();
        List<Item1> itemsRemaining = new List<Item1>();

        Text heartsTxt;
        Text speechTxt;

        public void Start(int level = 0)
        {
            curLevelIdx = Mathf.Clamp(level, 0, Data1.Levels.Count - 1);
            hearts = 3;
            RenderScreen();
        }

        void RenderScreen()
        {
            root = App.I.NewScreen();
            var curLevel = Data1.Levels[curLevelIdx];

            itemsInBackpack.Clear();
            itemsInBanned.Clear();
            itemsRemaining.Clear();
            itemsRemaining.AddRange(curLevel.items);

            ScrollRect sr;
            var c = UIK.Scroll(root, 10, 12, out sr);
            UIK.Stretch((RectTransform)sr.transform);

            // 1. Top HUD (Header)
            var hud = UIK.Card(c, UIK.P, 10, 8);
            var hudRow = UIK.RT("HudRow", hud.transform);
            var hl = UIK.HL(hudRow.gameObject, 8, 0, false);
            hl.childForceExpandHeight = false;

            // Nút Back
            var bBtn = UIK.Btn(hudRow, "Menu", UIK.P2, UIK.Ink, 13, () => App.I.ShowMenu(), 36);
            UIK.LE(bBtn.gameObject, 60, 36);

            // Tiêu đề & Cấp
            var titleBox = UIK.RT("TitleBox", hudRow);
            UIK.VL(titleBox.gameObject, 0);
            UIK.LE(titleBox.gameObject, -1, -1, 1);
            UIK.Txt(titleBox, $"MÀN {curLevel.id}: {curLevel.title.ToUpper()}", 13, UIK.Y, FontStyle.Bold);
            UIK.Txt(titleBox, curLevel.subtitle, 11, UIK.Mute);

            // Tim (Hearts)
            var heartBox = UIK.Box(hudRow, UIK.Hex("#3a1515"), UIK.Round, "Hearts");
            UIK.LE(heartBox.gameObject, 65, 36);
            heartsTxt = UIK.Txt(heartBox.transform, $"♥ {hearts}", 15, UIK.R, FontStyle.Bold);
            heartsTxt.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(heartsTxt.rectTransform);

            // 2. Banner nhắc nhở
            var tipCard = UIK.Card(c, UIK.Hex("#2a2312"), 8, 6);
            var tipRow = UIK.RT("TipRow", tipCard.transform);
            UIK.HL(tipRow.gameObject, 8, 0, false);
            var warnIc = VG.Icon(tipRow, "hazard", UIK.Y, 20);
            UIK.LE(warnIc.gameObject, 20, 20);
            var tipT = UIK.Txt(tipRow, curLevel.tip, 12, UIK.Hex("#ffd97d"), FontStyle.Bold);
            UIK.LE(tipT.gameObject, -1, -1, 1);

            // 3. Khung cảnh trực quan 2D
            var visual = Scene1.CreateVisual(c);

            // 4. Khu vực tương tác: Ba lô (ở giữa) & Khay Vật Cấm (bên phải)
            var zoneRow = UIK.RT("Zones", c);
            UIK.HL(zoneRow.gameObject, 10, 0, true);
            UIK.LE(zoneRow.gameObject, -1, 110);

            // Ba lô Zone
            var bpBox = UIK.Card(zoneRow, UIK.Hex("#232b21"), 8, 4);
            UIK.LE(bpBox.gameObject, -1, -1, 1);
            var bpHeader = UIK.RT("BpH", bpBox.transform);
            UIK.HL(bpHeader.gameObject, 6, 0, false);
            var bpIc = VG.Icon(bpHeader, "backpack", UIK.G, 18);
            UIK.LE(bpIc.gameObject, 18, 18);
            UIK.Txt(bpHeader, "BA LÔ MỎ", 12, UIK.G, FontStyle.Bold);
            backpackZoneTrans = UIK.RT("BpContent", bpBox.transform);
            UIK.HL(backpackZoneTrans.gameObject, 4, 0, false);
            UIK.LE(backpackZoneTrans.gameObject, -1, 65);

            // Vật Cấm Zone
            var banBox = UIK.Card(zoneRow, UIK.Hex("#341818"), 8, 4);
            UIK.LE(banBox.gameObject, -1, -1, 1);
            var banH = UIK.RT("BanH", banBox.transform);
            UIK.HL(banH.gameObject, 6, 0, false);
            var banIc = VG.Icon(banH, "x", UIK.R, 18);
            UIK.LE(banIc.gameObject, 18, 18);
            UIK.Txt(banH, "VẬT CẤM", 12, UIK.R, FontStyle.Bold);
            bannedZoneTrans = UIK.RT("BanContent", banBox.transform);
            UIK.HL(bannedZoneTrans.gameObject, 4, 0, false);
            UIK.LE(bannedZoneTrans.gameObject, -1, 65);

            // 5. Khay chứa các vật dụng đang chờ chọn
            var trayCard = UIK.Card(c, UIK.P2, 8, 6);
            UIK.Txt(trayCard.transform, "Trang bị & vật tư có sẵn (Chạm để chọn chuyển):", 12, UIK.Mute);
            itemsTrayTrans = UIK.RT("TrayContent", trayCard.transform);
            var gl = itemsTrayTrans.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(74, 74);
            gl.spacing = new Vector2(6, 6);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 4;
            UIK.LE(itemsTrayTrans.gameObject, -1, 160);

            // 6. Bong bóng lời thoại NPC & Lời khuyên
            var npcCard = UIK.Card(c, UIK.P, 10, 8);
            var npcRow = UIK.RT("NpcRow", npcCard.transform);
            UIK.HL(npcRow.gameObject, 10, 0, false);
            var mentorIc = VG.Icon(npcRow, "helmet", UIK.Hex("#4da6ff"), 28);
            UIK.LE(mentorIc.gameObject, 32, 32);
            speechTxt = UIK.Txt(npcRow, $"Bác thợ mỏ: \"{curLevel.speechNpc}\"", 12, UIK.Ink, FontStyle.Italic);
            UIK.LE(speechTxt.gameObject, -1, -1, 1);

            // 7. Nút Xác nhận nộp đồ
            var btnConfirm = UIK.Btn(c, "XÁC NHẬN TRANG BỊ", UIK.Y, UIK.Dark, 16, OnConfirmEquip, 48);
            UIK.Spacer(c, 15);

            RefreshItemViews();
        }

        void RefreshItemViews()
        {
            UIK.Clear(backpackZoneTrans);
            UIK.Clear(bannedZoneTrans);
            UIK.Clear(itemsTrayTrans);

            // Hiển thị đồ trong khay chờ
            foreach (var item in itemsRemaining)
            {
                CreateItemChip(item, itemsTrayTrans, () =>
                {
                    // Chạm để chuyển: nếu đồ cấm -> sang khay cấm, nếu đồ chuẩn -> sang ba lô
                    ShowItemChoiceModal(item);
                });
            }

            // Hiển thị đồ trong ba lô
            foreach (var item in itemsInBackpack)
            {
                CreateItemChip(item, backpackZoneTrans, () =>
                {
                    itemsInBackpack.Remove(item);
                    itemsRemaining.Add(item);
                    RefreshItemViews();
                });
            }

            // Hiển thị đồ trong khay cấm
            foreach (var item in itemsInBanned)
            {
                CreateItemChip(item, bannedZoneTrans, () =>
                {
                    itemsInBanned.Remove(item);
                    itemsRemaining.Add(item);
                    RefreshItemViews();
                });
            }
        }

        void CreateItemChip(Item1 item, Transform parent, Action onClick)
        {
            var btn = UIK.Btn(parent, "", UIK.P, UIK.Ink, 10, onClick, 64);
            UIK.LE(btn.gameObject, 64, 64);
            var ic = VG.Icon(btn.transform, item.icon, UIK.Y, 28);
            ic.rectTransform.anchoredPosition = new Vector2(0, 8);
            var lbl = UIK.Txt(btn.transform, item.name, 9, UIK.Ink);
            lbl.alignment = TextAnchor.LowerCenter;
            UIK.Stretch(lbl.rectTransform);
        }

        void ShowItemChoiceModal(Item1 item)
        {
            Action close;
            var box = UIK.Modal(out close);
            UIK.Txt(box.transform, item.name, 18, UIK.Ink, FontStyle.Bold);
            UIK.Txt(box.transform, item.desc, 13, UIK.Mute);

            UIK.Spacer(box.transform, 6);
            UIK.Btn(box.transform, "Cho vào Ba-lô", UIK.G, UIK.Dark, 14, () =>
            {
                itemsRemaining.Remove(item);
                itemsInBackpack.Add(item);
                close();
                RefreshItemViews();
            }, 42);

            UIK.Btn(box.transform, "Phân loại vào Vật Cấm", UIK.R, UIK.Ink, 14, () =>
            {
                itemsRemaining.Remove(item);
                itemsInBanned.Add(item);
                close();
                RefreshItemViews();
            }, 42);

            UIK.Btn(box.transform, "Để lại kiểm tra sau", UIK.P2, UIK.Mute, 13, close, 36);
        }

        void OnConfirmEquip()
        {
            var curLevel = Data1.Levels[curLevelIdx];
            List<string> errors = new List<string>();

            // 1. Kiểm tra đồ bắt buộc đã vào ba lô chưa
            foreach (var item in curLevel.items)
            {
                if (item.isRequired && !itemsInBackpack.Contains(item))
                {
                    errors.Add($"Thiếu {item.name}: {item.desc}");
                }
            }

            // 2. Kiểm tra có để lọt đồ cấm vào ba lô không
            foreach (var item in itemsInBackpack)
            {
                if (item.isBanned)
                {
                    errors.Add($"Vi phạm an toàn: Mang '{item.name}' vào hầm lò! ({item.desc})");
                }
            }

            // 3. Kiểm tra đồ cấm đã loại bỏ đúng khay chưa
            foreach (var item in curLevel.items)
            {
                if (item.isBanned && !itemsInBanned.Contains(item))
                {
                    errors.Add($"Chưa loại bỏ vật cấm: '{item.name}'!");
                }
            }

            if (errors.Count > 0)
            {
                hearts--;
                Sfx.Play("bad");
                if (heartsTxt != null) heartsTxt.text = $"♥ {hearts}";

                string errBody = string.Join("\n• ", errors);
                speechTxt.text = $"Bác thợ mỏ nhắc nhở:\n• {errBody}";

                if (hearts <= 0)
                {
                    Action close;
                    var box = UIK.Modal(out close);
                    UIK.Txt(box.transform, "HẾT TIM TRẢI NGHIỆM!", 20, UIK.R, FontStyle.Bold);
                    UIK.Txt(box.transform, "Bạn đã mắc quá nhiều lỗi an toàn nghiêm trọng trước khi vào lò. Cần ôn lại quy chuẩn an toàn QCVN!", 14, UIK.Ink);
                    UIK.Btn(box.transform, "Thử lại màn này", UIK.Y, UIK.Dark, 16, () => { close(); Start(curLevelIdx); }, 46);
                    UIK.Btn(box.transform, "Về Menu", UIK.P2, UIK.Ink, 14, () => { close(); App.I.ShowMenu(); }, 40);
                }
                else
                {
                    UIK.Toast("Chưa đạt chuẩn an toàn! Hãy xem lời nhắc của Bác thợ mỏ.");
                }
            }
            else
            {
                // Vượt qua màn chơi an toàn
                Sfx.Play("win");
                stars += 3;
                Action close;
                var box = UIK.Modal(out close);
                UIK.Txt(box.transform, "AN TOÀN XUẤT SẮC!", 22, UIK.G, FontStyle.Bold);
                UIK.Txt(box.transform, $"Bạn đã trang bị chuẩn xác và tuân thủ nghiêm ngặt quy định trước khi xuống lò!\n+3 Sao đạt được.", 14, UIK.Ink);

                if (curLevelIdx + 1 < Data1.Levels.Count)
                {
                    UIK.Btn(box.transform, "Sang Trạm Tiếp Theo", UIK.G, UIK.Dark, 16, () => { close(); Start(curLevelIdx + 1); }, 48);
                }
                else
                {
                    UIK.Btn(box.transform, "Hoàn Thành Ca Làm Việc", UIK.Y, UIK.Dark, 16, () => { close(); App.I.ShowMenu(); }, 48);
                }
            }
        }
    }
}
