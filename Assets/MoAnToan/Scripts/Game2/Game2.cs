using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Vòng lặp gameplay Game 2: "Trực ban an toàn".
    /// Giám sát camera hầm lò, theo dõi cảm biến SCADA, tiếp nhận báo cáo và ra quyết định phê duyệt / dừng lò.
    /// </summary>
    public class Game2
    {
        int curIncidentIdx = 0;
        int hearts = 3;
        int stars = 0;

        RectTransform root;
        Text heartsTxt;
        Text tipTxt;

        public void Start(int incidentIdx = 0)
        {
            curIncidentIdx = Mathf.Clamp(incidentIdx, 0, Data2.Incidents.Count - 1);
            hearts = 3;
            RenderScreen();
        }

        void RenderScreen()
        {
            root = App.I.NewScreen();
            var incident = Data2.Incidents[curIncidentIdx];

            ScrollRect sr;
            var c = UIK.Scroll(root, 10, 12, out sr);
            UIK.Stretch((RectTransform)sr.transform);

            // 1. Top HUD
            var hud = UIK.Card(c, UIK.Hex("#131c29"), 10, 8);
            var hudRow = UIK.RT("HudRow", hud.transform);
            var hl = UIK.HL(hudRow.gameObject, 8, 0, false);
            hl.childForceExpandHeight = false;

            // Nút Menu
            var bBtn = UIK.Btn(hudRow, "Menu", UIK.Hex("#1e2b3e"), UIK.Ink, 13, () => App.I.ShowMenu(), 36);
            UIK.LE(bBtn.gameObject, 60, 36);

            // Tiêu đề & Cấp
            var titleBox = UIK.RT("TitleBox", hudRow);
            UIK.VL(titleBox.gameObject, 0);
            UIK.LE(titleBox.gameObject, -1, -1, 1);
            UIK.Txt(titleBox, $"CA TRỰC {incident.id}: {incident.title.ToUpper()}", 13, UIK.Hex("#6fc3ff"), FontStyle.Bold);
            UIK.Txt(titleBox, "Giám sát — Phát hiện — Xử lý tình huống", 11, UIK.Mute);

            // Tim
            var heartBox = UIK.Box(hudRow, UIK.Hex("#3a1515"), UIK.Round, "Hearts");
            UIK.LE(heartBox.gameObject, 65, 36);
            heartsTxt = UIK.Txt(heartBox.transform, $"♥ {hearts}", 15, UIK.R, FontStyle.Bold);
            heartsTxt.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(heartsTxt.rectTransform);

            // 2. Màn hình CCTV & SCADA trung tâm
            var visual = Scene2.CreateVisual(c, incident, OnSelectCamera);

            // 3. Hộp thông tin từ hiện trường
            var reportCard = UIK.Card(c, UIK.P, 12, 10);
            var repHead = UIK.RT("RepHead", reportCard.transform);
            UIK.HL(repHead.gameObject, 6, 0, false);
            var repIc = VG.Icon(repHead, "hazard", UIK.Y, 18);
            UIK.LE(repIc.gameObject, 18, 18);
            UIK.Txt(repHead, "THÔNG TIN TỪ HIỆN TRƯỜNG", 12, UIK.Y, FontStyle.Bold);

            // Nội dung tin nhắn của tổ sản xuất
            var repMsgBox = UIK.Box(reportCard.transform, UIK.P2, UIK.Round, "Msg");
            UIK.LE(repMsgBox.gameObject, -1, 60);
            var repMsgTxt = UIK.Txt(repMsgBox.transform, incident.reportText, 13, UIK.Ink);
            repMsgTxt.alignment = TextAnchor.MiddleLeft;
            UIK.Stretch(repMsgTxt.rectTransform);
            repMsgTxt.rectTransform.offsetMin = new Vector2(10, 5);
            repMsgTxt.rectTransform.offsetMax = new Vector2(-10, -5);

            // Dòng gợi ý
            var tipRow = UIK.RT("TipRow", reportCard.transform);
            UIK.HL(tipRow.gameObject, 6, 0, false);
            var bulbIc = VG.Icon(tipRow, "star", UIK.Hex("#ffdb4d"), 16);
            UIK.LE(bulbIc.gameObject, 16, 16);
            tipTxt = UIK.Txt(tipRow, $"Gợi ý: {incident.tip}", 11, UIK.Hex("#ffd97d"), FontStyle.Italic);
            UIK.LE(tipTxt.gameObject, -1, -1, 1);

            // 4. Các nút Quyết Định Lớn
            var actCard = UIK.Card(c, UIK.Hex("#141b24"), 10, 10);
            UIK.Txt(actCard.transform, "BẠN SẼ RA QUYẾT ĐỊNH XỬ LÝ NHƯ THẾ NÀO?", 13, UIK.Ink, FontStyle.Bold);

            var actRow = UIK.RT("ActRow", actCard.transform);
            UIK.HL(actRow.gameObject, 12, 0, true);
            UIK.LE(actRow.gameObject, -1, 52);

            // Nút Cho phép làm việc
            var btnAllow = UIK.Btn(actRow, "CHO PHÉP LÀM VIỆC", UIK.G, UIK.Dark, 14, () => MakeDecision(false), 52);
            UIK.LE(btnAllow.gameObject, -1, 52, 1);

            // Nút Dừng sản xuất
            var btnStop = UIK.Btn(actRow, "DỪNG SẢN XUẤT", UIK.R, UIK.Ink, 14, () => MakeDecision(true), 52);
            UIK.LE(btnStop.gameObject, -1, 52, 1);

            UIK.Txt(c, "Hãy kiểm tra kỹ camera và đối chiếu sổ quy chuẩn trước khi ra quyết định!", 11, UIK.Mute);
            UIK.Spacer(c, 20);
        }

        void OnSelectCamera(int camIdx)
        {
            var incident = Data2.Incidents[curIncidentIdx];
            string detail = camIdx switch
            {
                1 => incident.cam01Status,
                2 => incident.cam02Status,
                3 => incident.cam03Status,
                4 => incident.cam04Status,
                _ => "Đang kết nối camera..."
            };

            Action close;
            var box = UIK.Modal(out close);
            UIK.Txt(box.transform, $"CAMERA 0{camIdx} — CẬN CẢNH HIỆN TRƯỜNG", 16, UIK.Hex("#6fc3ff"), FontStyle.Bold);

            // Giả lập khung hình phóng to
            var zoomBox = UIK.Box(box.transform, UIK.Hex("#0b121c"), UIK.Round, "ZoomFrame");
            UIK.LE(zoomBox.gameObject, -1, 140);
            var ic = VG.Icon(zoomBox.transform, "cam", UIK.Hex("#2a4c73"), 48);
            ic.rectTransform.anchoredPosition = Vector2.zero;

            UIK.Txt(box.transform, $"Hình ảnh truyền về:\n• {detail}", 13, UIK.Ink);
            UIK.Spacer(box.transform, 10);
            UIK.Btn(box.transform, "Đóng Camera", UIK.P2, UIK.Ink, 14, close, 42);
        }

        void MakeDecision(bool choseStop)
        {
            var incident = Data2.Incidents[curIncidentIdx];

            if (choseStop == incident.requireStop)
            {
                // Quyết định chính xác
                Sfx.Play("win");
                stars += 3;
                Action close;
                var box = UIK.Modal(out close);
                UIK.Txt(box.transform, "QUYẾT ĐỊNH CHÍNH XÁC!", 22, UIK.G, FontStyle.Bold);
                UIK.Txt(box.transform, incident.explanation, 14, UIK.Ink);

                if (curIncidentIdx + 1 < Data2.Incidents.Count)
                {
                    UIK.Btn(box.transform, "Xử Lý Ca Tiếp Theo", UIK.G, UIK.Dark, 16, () => { close(); Start(curIncidentIdx + 1); }, 48);
                }
                else
                {
                    UIK.Btn(box.transform, "Hoàn Thành Ca Trực", UIK.Y, UIK.Dark, 16, () => { close(); App.I.ShowMenu(); }, 48);
                }
            }
            else
            {
                // Quyết định sai sót
                hearts--;
                Sfx.Play("bad");
                if (heartsTxt != null) heartsTxt.text = $"♥ {hearts}";

                Action close;
                var box = UIK.Modal(out close);
                UIK.Txt(box.transform, "QUYẾT ĐỊNH CHƯA PHÙ HỢP!", 20, UIK.R, FontStyle.Bold);
                UIK.Txt(box.transform, incident.explanation, 13, UIK.Ink);

                if (hearts <= 0)
                {
                    UIK.Txt(box.transform, "Hết lượt thử trong ca trực này!", 13, UIK.R, FontStyle.Bold);
                    UIK.Btn(box.transform, "Thử Lại Từ Đầu", UIK.Y, UIK.Dark, 15, () => { close(); Start(curIncidentIdx); }, 44);
                    UIK.Btn(box.transform, "Về Menu", UIK.P2, UIK.Ink, 14, () => { close(); App.I.ShowMenu(); }, 40);
                }
                else
                {
                    UIK.Btn(box.transform, "Xem Lại Camera & Dữ Liệu", UIK.Y, UIK.Dark, 15, close, 44);
                }
            }
        }
    }
}
