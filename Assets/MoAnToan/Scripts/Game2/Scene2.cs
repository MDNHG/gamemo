using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoAnToan
{
    /// <summary>
    /// Dựng khung cảnh phòng điều khiển trung tâm Game 2 (Cụm 4 camera quan sát, bảng SCADA và trực ban).
    /// </summary>
    public class Scene2 : MonoBehaviour
    {
        public static RectTransform CreateVisual(Transform parent, Incident2 incident, Action<int> onSelectCam)
        {
            var root = UIK.RT("Scene2Visual", parent);
            UIK.LE(root.gameObject, -1, 230);

            // Nền phòng điều hành xanh sẫm
            var bg = UIK.Flat(root, UIK.Hex("#131a26"), "ConsoleBg");
            UIK.Stretch(bg.rectTransform);

            // Dải tiêu đề "CÔNG TY THAN MÔNG DƯƠNG - PHÒNG ĐIỀU HÀNH TRUNG TÂM"
            var headBar = UIK.Flat(root, UIK.Hex("#1d2b3f"), "HeadBar");
            headBar.rectTransform.anchorMin = new Vector2(0, 0.88f);
            headBar.rectTransform.anchorMax = new Vector2(1, 1);
            var title = UIK.Txt(headBar.transform, "CÔNG TY THAN MÔNG DƯƠNG — PHÒNG ĐIỀU HÀNH TRUNG TÂM", 10, UIK.Hex("#8cb5e5"), FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(title.rectTransform);

            // Khung nội dung chính chia 3 phần:
            // Cột trái: Đồng hồ thông số SCADA
            // Khung giữa: Cụm 4 camera quan sát
            // Cột phải: Bảng trạng thái & Sổ quy định

            var mainRow = UIK.RT("MainRow", root);
            mainRow.anchorMin = new Vector2(0.02f, 0.04f);
            mainRow.anchorMax = new Vector2(0.98f, 0.86f);
            var hl = UIK.HL(mainRow.gameObject, 6, 0, false);
            hl.childForceExpandHeight = true;

            // 1. Cột trái: SCADA Sensors
            var scadaCol = UIK.Card(mainRow, UIK.Hex("#172233"), 6, 4);
            UIK.LE(scadaCol.gameObject, 105, -1);
            UIK.Txt(scadaCol.transform, "THÔNG SỐ", 10, UIK.Hex("#729acc"), FontStyle.Bold);

            // Sensor CH4
            Color ch4Color = incident.ch4 >= 1.0f ? UIK.R : incident.ch4 >= 0.7f ? UIK.Y : UIK.G;
            CreateSensorWidget(scadaCol.transform, "CH4", $"{incident.ch4:F2}%", ch4Color);

            // Sensor Nhiệt độ
            CreateSensorWidget(scadaCol.transform, "Nhiệt độ", $"{incident.temp:F0}°C", incident.temp >= 30 ? UIK.Y : UIK.Ink);

            // Sensor Độ ẩm
            CreateSensorWidget(scadaCol.transform, "Độ ẩm", $"{incident.humidity:F0}%", UIK.Blue);

            // Vị trí quan trắc
            var locBox = UIK.Box(scadaCol.transform, UIK.Hex("#0f1724"), UIK.Round, "Loc");
            UIK.LE(locBox.gameObject, -1, 34);
            var locTxt = UIK.Txt(locBox.transform, $"Khu vực:\n{incident.location}", 8, UIK.Mute);
            locTxt.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(locTxt.rectTransform);

            // 2. Khung giữa: Cụm 4 camera CCTV
            var camGrid = UIK.RT("CamGrid", mainRow);
            UIK.LE(camGrid.gameObject, -1, -1, 1);
            var grid = camGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(90, 80);
            grid.spacing = new Vector2(4, 4);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            CreateCamWidget(camGrid, "Camera 01", incident.cam01Status, () => onSelectCam(1));
            CreateCamWidget(camGrid, "Camera 02", incident.cam02Status, () => onSelectCam(2));
            CreateCamWidget(camGrid, "Camera 03", incident.cam03Status, () => onSelectCam(3));
            CreateCamWidget(camGrid, "Camera 04", incident.cam04Status, () => onSelectCam(4));

            // 3. Cột phải: Trạng thái & Sổ tra cứu
            var rightCol = UIK.Card(mainRow, UIK.Hex("#172233"), 6, 4);
            UIK.LE(rightCol.gameObject, 85, -1);
            UIK.Txt(rightCol.transform, "HỆ THỐNG", 10, UIK.Hex("#729acc"), FontStyle.Bold);

            // Box trạng thái chung
            var statBox = UIK.Box(rightCol.transform, UIK.Hex("#0f1724"), UIK.Round, "Status");
            UIK.LE(statBox.gameObject, -1, 46);
            string statStr = incident.ch4 >= 1.0f ? "NGUY HIỂM" : incident.ch4 >= 0.7f ? "CHÚ Ý" : "BÌNH THƯỜNG";
            Color statCol = incident.ch4 >= 1.0f ? UIK.R : incident.ch4 >= 0.7f ? UIK.Y : UIK.G;
            var statTxt = UIK.Txt(statBox.transform, $"Trạng thái:\n{statStr}", 9, statCol, FontStyle.Bold);
            statTxt.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(statTxt.rectTransform);

            // Nút mở Sổ quy chuẩn
            var bookBtn = UIK.Btn(rightCol.transform, "Sổ tay\nQCVN", UIK.Hex("#203450"), UIK.Ink, 9, () =>
            {
                Action close;
                var box = UIK.Modal(out close);
                UIK.Txt(box.transform, "SỔ TAY QUY CHUẨN AN TOÀN HẦM LÒ", 16, UIK.Y, FontStyle.Bold);
                UIK.Txt(box.transform, "Trích dẫn QCVN 01:2011/BCT:", 13, UIK.Hex("#6fc3ff"), FontStyle.Bold);
                UIK.Txt(box.transform, "• Nồng độ CH4 cho phép tại luồng gió hồi: < 1.0%.\n• Khi phát hiện ống gió mềm bị rách, gập hoặc lưu lượng gió giảm phải dừng ngay việc khoan đào gương than.\n• Nghiêm cấm mọi vật sinh tia lửa, lửa trần và thiết bị không phòng nổ trong mỏ.", 12, UIK.Ink);
                UIK.Spacer(box.transform, 10);
                UIK.Btn(box.transform, "Đã hiểu", UIK.Y, UIK.Dark, 14, close, 40);
            }, 40);
            UIK.LE(bookBtn.gameObject, -1, 40);

            return root;
        }

        static void CreateSensorWidget(Transform p, string name, string val, Color color)
        {
            var box = UIK.Box(p, UIK.Hex("#0e1520"), UIK.Round, name);
            UIK.LE(box.gameObject, -1, 32);
            var lbl = UIK.Txt(box.transform, $"{name}: {val}", 10, color, FontStyle.Bold);
            lbl.alignment = TextAnchor.MiddleCenter;
            UIK.Stretch(lbl.rectTransform);
        }

        static void CreateCamWidget(Transform p, string title, string statusText, Action onClick)
        {
            var btn = UIK.Btn(p, "", UIK.Hex("#0b1017"), UIK.Ink, 10, onClick, 75);
            var ic = VG.Icon(btn.transform, "cam", UIK.Hex("#446b9e"), 26);
            ic.rectTransform.anchoredPosition = new Vector2(0, 8);
            var t = UIK.Txt(btn.transform, $"• {title}", 9, UIK.Hex("#8cb5e5"), FontStyle.Bold);
            t.alignment = TextAnchor.LowerCenter;
            UIK.Stretch(t.rectTransform);
        }
    }
}
