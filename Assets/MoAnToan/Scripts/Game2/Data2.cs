using System;
using System.Collections.Generic;

namespace MoAnToan
{
    [Serializable]
    public class Incident2
    {
        public int id;
        public string title;
        public string location;
        public float ch4;
        public float temp;
        public float humidity;
        public string reportText;
        public string tip;
        public bool requireStop; // Quyết định đúng là Dừng sản xuất hay Cho phép làm việc
        public string explanation;
        public string cam01Status;
        public string cam02Status;
        public string cam03Status;
        public string cam04Status;
    }

    public static class Data2
    {
        public static List<Incident2> Incidents = new List<Incident2>
        {
            new Incident2
            {
                id = 1,
                title = "Ống gió mềm bị gập",
                location = "Gương lò III",
                ch4 = 0.8f,
                temp = 32f,
                humidity = 68f,
                reportText = "Tổ sản xuất báo cáo: Tại gương lò III, ống gió mềm bị gập nhẹ. Bạn sẽ xử lý như thế nào?",
                tip = "Ống gió bị gập làm giảm lưu lượng gió sạch, nồng độ CH4 đang ở mức 0.8% cận ngưỡng nguy hiểm!",
                requireStop = true, // Cần dừng hoặc yêu cầu khắc phục ngay trước khi đào
                explanation = "Theo QCVN 01:2011/BCT, khi hệ thống thông gió có sự cố làm giảm lưu lượng gió tới gương lò, phải lập tức dừng sản xuất, nắn phẳng ống gió để khôi phục lưu thông khí trước khi công nhân làm việc tiếp.",
                cam01Status = "Đường lò chính: Bình thường, ánh sáng tốt.",
                cam02Status = "Gương lò III: Ống gió vải bạt bị gấp khúc gập góc 90 độ!",
                cam03Status = "Trạm điện gương lò: Trạng thái đóng cắt bình thường.",
                cam04Status = "Ngã ba gió thải: Lưu lượng gió ra có chiều hướng giảm nhẹ."
            },
            new Incident2
            {
                id = 2,
                title = "Nồng độ Metan CH4 vượt ngưỡng",
                location = "Gương khấu chợ số 2",
                ch4 = 1.25f,
                temp = 30f,
                humidity = 75f,
                reportText = "Đầu đo tự động báo: CH4 đạt 1.25% tại ngã ba gió hồi. Tổ trưởng báo vẫn còn 2 xe goòng than muốn xúc nốt.",
                tip = "Quy định QCVN: CH4 >= 1.0% tại luồng gió hồi phải cắt điện và dừng ngay!",
                requireStop = true,
                explanation = "QCVN 01:2011/BCT quy định rõ: Nồng độ CH4 trong luồng gió hồi vượt quá 1.0% phải lập tức cắt điện, dừng mọi hoạt động sản xuất và rút thợ mỏ ra vị trí gió sạch an toàn.",
                cam01Status = "Đường sắt vận tải: Xe goòng đang dừng chờ.",
                cam02Status = "Gương khấu chợ 2: Bụi than lơ lửng, cảm biến CH4 nhấp nháy đèn đỏ.",
                cam03Status = "Trạm quạt cục bộ: Vẫn đang hoạt động liên tục.",
                cam04Status = "Đường thoát hiểm: Thông thoáng, sẵn sàng di tản."
            },
            new Incident2
            {
                id = 3,
                title = "Kiểm tra thông số ca đêm",
                location = "Đường lò vận tải chính -150",
                ch4 = 0.2f,
                temp = 26f,
                humidity = 60f,
                reportText = "Tổ vận tải báo: Đoạn lò -150 đã kiểm tra xong vì chống, đường ray sạch, thông gió tốt.",
                tip = "CH4 chỉ 0.2%, nhiệt độ 26°C mát mẻ, hình ảnh camera không có dấu hiệu bất thường.",
                requireStop = false, // Cho phép làm việc
                explanation = "Tất cả các chỉ số vi khí hậu đều nằm trong giới hạn an toàn tối ưu (CH4 < 0.5%, nhiệt độ < 30°C, vách lò ổn định). Trực ban phê duyệt lệnh tiếp tục vận hành.",
                cam01Status = "Tuyến ray -150: Ray sạch sẽ, tà vẹt chắc chắn.",
                cam02Status = "Khu vực trạm tời: Dây cáp cuộn đều, không có người trong vùng nguy hiểm.",
                cam03Status = "Cửa gió đôi: Cửa thứ nhất đóng, cửa thứ hai mở bình thường.",
                cam04Status = "Trạm đo khí tự động: Các đèn sensor hiển thị màu xanh an toàn."
            }
        };
    }
}
