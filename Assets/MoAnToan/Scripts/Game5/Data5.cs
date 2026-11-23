using System.Collections.Generic;

namespace MoAnToan
{
    public enum QT { Rule, MC, TF, Bin, Order, Match }

    public class Q
    {
        public QT t; public string id; public int lv;
        public string q, ex;
        public string[] o; public int a;          // MC
        public bool tf;                            // TF
        public string[] bins, items; public int[] ib; // Bin (items + ô đúng); Order dùng items theo đúng thứ tự
        public string[] pa, pb;                    // Match
    }

    public class Level
    {
        public string name, sub, rule;
        public List<Q> kt = new List<Q>(), vd = new List<Q>();
    }

    /// <summary>Nội dung mẫu cho bản test — cần cán bộ an toàn rà soát.</summary>
    public static class Data5
    {
        static Q MC(string q, string[] o, int a, string ex) { return new Q { t = QT.MC, q = q, o = o, a = a, ex = ex }; }
        static Q TF(string q, bool v, string ex) { return new Q { t = QT.TF, q = q, tf = v, ex = ex }; }
        static Q BIN(string q, string[] bins, string[] items, int[] ib, string ex) { return new Q { t = QT.Bin, q = q, bins = bins, items = items, ib = ib, ex = ex }; }
        static Q ORD(string q, string[] items, string ex) { return new Q { t = QT.Order, q = q, items = items, ex = ex }; }
        static Q MAT(string q, string[] pa, string[] pb, string ex) { return new Q { t = QT.Match, q = q, pa = pa, pb = pb, ex = ex }; }

        // QM phải khởi tạo trước LV vì Build() ghi vào QM
        public static readonly Dictionary<string, Q> QM = new Dictionary<string, Q>();
        public static readonly List<Level> LV = Build();

        static List<Level> Build()
        {
            var L = new List<Level>();
            Level l;

            l = new Level { name = "Trước khi xuống lò", sub = "Bảo hộ, vật cấm, máy đo khí",
                rule = "Trước mỗi ca phải mặc đủ bảo hộ, nhận đèn lò, bình tự cứu và máy đo khí đã kiểm tra. Tuyệt đối không mang lửa, thuốc lá hoặc thiết bị điện không phòng nổ xuống lò." };
            l.kt.Add(MC("Vật nào bị CẤM mang xuống lò?", new[] { "Bật lửa", "Bình tự cứu", "Đèn lò cá nhân", "Thẻ tên" }, 0, "Bật lửa, diêm, thuốc lá có thể gây cháy, nổ khí mê-tan và bụi than."));
            l.kt.Add(BIN("Đưa mỗi vật vào đúng ô", new[] { "Được mang", "Cấm mang" }, new[] { "Bình tự cứu", "Thuốc lá", "Đèn lò", "Điện thoại thường", "Mũ bảo hộ", "Diêm" }, new[] { 0, 1, 0, 1, 0, 1 }, "Thiết bị điện không phòng nổ như điện thoại thường có thể phát tia lửa."));
            l.kt.Add(TF("Bình tự cứu chỉ cần mang khi xuống mỏ hạng III trở lên.", false, "Mọi người xuống lò đều phải mang bình tự cứu, ở bất kỳ loại mỏ nào."));
            l.kt.Add(MAT("Ghép trang bị với công dụng", new[] { "Mũ bảo hộ", "Bình tự cứu", "Đèn lò", "Máy đo khí" }, new[] { "Chống va đập, vật rơi", "Cấp khí thở khi có khí độc", "Chiếu sáng cá nhân", "Cảnh báo nồng độ khí" }, "Mỗi trang bị bảo vệ một nguy cơ khác nhau, thiếu một thứ là không đủ điều kiện xuống lò."));
            l.vd.Add(MC("Tại nhà đèn, máy đo khí của bạn báo lỗi khi tự kiểm tra. Bạn làm gì?", new[] { "Báo nhà đèn, đổi máy đạt yêu cầu rồi mới xuống lò", "Vẫn mang xuống, vào lò sẽ tự hết lỗi", "Đi chung với bạn có máy, không cần đổi", "Bỏ máy lại cho nhẹ" }, 0, "Máy đo khí lỗi không cảnh báo được khi khí vượt ngưỡng. Không mang máy lỗi xuống lò."));
            l.vd.Add(TF("Thấy đồng nghiệp giấu bật lửa trong túi áo trước ca, bạn nhắc nhở và báo người kiểm soát.", true, "Một vật cấm của một người có thể gây nguy hiểm cho cả khu vực. Nhắc nhở và báo cáo là đúng."));
            l.vd.Add(ORD("Sắp xếp thứ tự chuẩn bị trước ca", new[] { "Mặc đầy đủ bảo hộ", "Nhận đèn lò, bình tự cứu, máy đo khí", "Kiểm tra máy đo khí hoạt động tốt", "Qua kiểm soát vật cấm tại cửa lò" }, "Trang bị đủ, kiểm tra thiết bị, qua kiểm soát vật cấm rồi mới vào lò."));
            l.vd.Add(MC("Mũ bảo hộ của bạn bị nứt trước giờ vào ca.", new[] { "Đổi mũ mới rồi mới xuống lò", "Dán băng dính tạm", "Đội tạm, hết ca đổi", "Không đội, cầm theo tay" }, 0, "Mũ nứt không còn chịu được va đập. Phải thay mũ đạt chuẩn."));
            L.Add(l);

            l = new Level { name = "Thông gió", sub = "Cửa gió, ống gió, quạt cục bộ",
                rule = "Gió sạch pha loãng và đẩy khí độc, khí nổ ra ngoài. Cụm cửa gió luôn phải có ít nhất một cửa đóng; đi qua là đóng lại. Ống gió không được gập, rách." };
            l.kt.Add(TF("Được phép mở đồng thời hai cửa của cụm cửa gió để đi cho nhanh.", false, "Mở đồng thời hai cửa làm đoản mạch gió: khu vực phía trong thiếu gió, khí mê-tan có thể tích tụ."));
            l.kt.Add(MC("Sau khi đi qua một cửa gió, bạn phải:", new[] { "Đóng cửa lại ngay", "Để mở cho người đi sau", "Chèn đá giữ cửa mở", "Báo trực ban rồi để mở" }, 0, "Cửa gió phải đóng ngay sau khi đi qua để giữ đúng hướng luồng gió."));
            l.kt.Add(MC("Ống gió ở lò cụt bị gập sẽ dẫn đến điều gì?", new[] { "Gương lò thiếu gió, khí có thể tích tụ", "Gió mạnh hơn", "Không ảnh hưởng gì", "Giảm tiếng ồn quạt" }, 0, "Ống gió gập hoặc rách làm gió không tới gương lò cụt."));
            l.kt.Add(MAT("Ghép công trình thông gió với chức năng", new[] { "Cửa gió", "Ống gió", "Quạt cục bộ", "Cầu gió" }, new[] { "Điều khiển hướng luồng gió", "Dẫn gió tới lò cụt", "Tạo gió cho lò cụt", "Hai luồng gió giao nhau không hòa trộn" }, "Mỗi công trình giữ cho gió sạch đi đúng nơi cần."));
            l.vd.Add(ORD("Đi qua cụm hai cửa gió đúng cách", new[] { "Mở cửa thứ nhất, đi qua", "Đóng cửa thứ nhất", "Mở cửa thứ hai, đi qua", "Đóng cửa thứ hai" }, "Luôn có một cửa đóng trong suốt quá trình đi qua."));
            l.vd.Add(MC("Bạn thấy ống gió bị rách giữa đường lò.", new[] { "Báo ngay tổ trưởng / trực ban để sửa", "Kệ, không phải việc của mình", "Tự cắt bỏ đoạn rách", "Tắt quạt cục bộ cho đỡ ồn" }, 0, "Ống gió rách làm mất gió phía cuối. Phải báo để xử lý kịp thời."));
            l.vd.Add(TF("Quạt cục bộ dừng, thợ ở lò cụt vẫn làm tiếp nếu chưa ngửi thấy mùi lạ.", false, "Khí mê-tan không màu, không mùi. Quạt dừng thì phải ngừng việc, cắt điện, rút ra luồng gió sạch và báo cáo."));
            l.vd.Add(MC("Cửa gió bị kẹt, không đóng kín được.", new[] { "Báo trực ban / bộ phận thông gió xử lý", "Chèn tạm bằng gỗ rồi đi", "Để nguyên, ca sau sửa", "Tháo luôn cánh cửa" }, 0, "Cửa gió hỏng làm sai luồng gió. Báo ngay để sửa."));
            L.Add(l);

            l = new Level { name = "Khí mỏ & phân hạng mỏ", sub = "Mê-tan, máy đo, hạng mỏ",
                rule = "Mỏ được xếp hạng theo lượng khí mê-tan thoát ra tương đối: hạng I dưới 5, hạng II từ 5 đến dưới 10, hạng III từ 10 đến dưới 15 m³/tấn ngày đêm; siêu hạng từ 15 trở lên hoặc có phụt khí. Mê-tan nhẹ hơn không khí nên tích tụ sát nóc lò." };
            l.kt.Add(MC("Khí mê-tan thường tích tụ ở đâu trong đường lò?", new[] { "Sát nóc lò, các hốc nóc", "Sát nền lò", "Giữa đường lò", "Trong rãnh nước" }, 0, "Mê-tan nhẹ hơn không khí nên bốc lên, tích tụ ở nóc và hốc nóc."));
            l.kt.Add(MAT("Ghép hạng mỏ với lượng khí thoát ra (m³/tấn ngày đêm)", new[] { "Hạng I", "Hạng II", "Hạng III", "Siêu hạng" }, new[] { "Dưới 5", "Từ 5 đến dưới 10", "Từ 10 đến dưới 15", "Từ 15 trở lên hoặc có phụt khí" }, "Hạng mỏ càng cao, yêu cầu thông gió và kiểm soát khí càng chặt."));
            l.kt.Add(TF("Khí mê-tan có mùi đặc trưng nên ngửi là phát hiện được.", false, "Mê-tan không màu, không mùi. Chỉ máy đo khí mới phát hiện được."));
            l.kt.Add(MC("Hỗn hợp mê-tan và không khí có thể nổ trong khoảng nồng độ nào?", new[] { "Khoảng 5 – 15 %", "0,1 – 0,5 %", "20 – 30 %", "Trên 50 %" }, 0, "Trong khoảng 5–15 % mê-tan, chỉ cần một tia lửa là có thể nổ."));
            l.vd.Add(MC("Máy đo khí cá nhân báo động khi bạn đang ở lò chợ.", new[] { "Dừng việc, cắt điện thiết bị, rút ra luồng gió sạch, báo trực ban", "Lắc máy rồi đo lại", "Tắt chuông báo cho đỡ ồn", "Làm nhanh cho xong việc" }, 0, "Báo động khí là tín hiệu phải dừng ngay, không thử lại hay bỏ qua."));
            l.vd.Add(BIN("Khí nào tích tụ ở đâu?", new[] { "Gần nóc lò", "Gần nền lò" }, new[] { "Mê-tan CH₄", "Cacbonic CO₂", "Hyđrô H₂", "Hyđrô sunfua H₂S" }, new[] { 0, 1, 0, 1 }, "Khí nhẹ hơn không khí bốc lên nóc; khí nặng hơn đọng ở nền và chỗ trũng."));
            l.vd.Add(TF("Cảm biến mê-tan có thể đặt dưới nền lò cho dễ bảo dưỡng.", false, "Cảm biến mê-tan phải đặt sát nóc, đúng vị trí quy định, nơi khí tích tụ."));
            l.vd.Add(ORD("Xử lý khi máy đo báo khí vượt ngưỡng", new[] { "Dừng công việc", "Cắt điện thiết bị trong khu vực", "Rút người ra luồng gió sạch", "Báo cáo trực ban / chỉ huy" }, "Dừng, cắt điện (bỏ nguồn tia lửa), rút người, báo cáo."));
            L.Add(l);

            l = new Level { name = "Than tự cháy", sub = "Dấu hiệu sớm và xử lý",
                rule = "Dấu hiệu than tự cháy: nhiệt độ than và không khí tăng, hơi nước đọng hoặc sương mù, mùi dầu hỏa hay nhựa thông, khí CO tăng. Thấy dấu hiệu là báo cáo ngay." };
            l.kt.Add(BIN("Đâu là dấu hiệu than tự cháy?", new[] { "Dấu hiệu tự cháy", "Không phải" }, new[] { "Nhiệt độ vỉa than tăng", "Mùi nhựa thông, dầu hỏa", "Khí CO tăng", "Đèn lò sáng yếu", "Quạt cục bộ chạy đều" }, new[] { 0, 0, 0, 1, 1 }, "Nhiệt, mùi lạ và CO tăng là ba tín hiệu sớm của than tự cháy."));
            l.kt.Add(MC("Than cháy âm ỉ sinh ra nhiều khí độc nào?", new[] { "Cacbon monoxit CO", "Ôxy O₂", "Nitơ N₂", "Hơi nước" }, 0, "CO không màu, không mùi, rất độc. Bình tự cứu giúp thở an toàn khi thoát ra."));
            l.kt.Add(TF("Bình tự cứu giúp người lao động thở an toàn khi thoát khỏi khu vực có khí CO.", true, "Đó chính là công dụng của bình tự cứu: đủ thời gian rút ra nơi an toàn."));
            l.kt.Add(MC("Biện pháp nào giúp phòng than tự cháy?", new[] { "Thu dọn than rơi vãi, cách ly khu đã khai thác", "Để than vụn lại làm lớp đệm", "Thổi thêm gió qua đống than vụn", "Không cần làm gì" }, 0, "Than vụn tiếp xúc không khí sẽ ôxy hóa, nóng dần và tự cháy."));
            l.vd.Add(MC("Ở lò chợ bạn thấy nóng bất thường và ngửi thấy mùi nhựa thông.", new[] { "Báo trực ban, đo nhiệt độ và CO, sẵn sàng rút người", "Mở thêm quạt rồi làm tiếp", "Đợi hết ca mới báo", "Tự đào bới tìm chỗ cháy" }, 0, "Phát hiện sớm và báo cáo kịp thời giúp xử lý trước khi thành đám cháy."));
            l.vd.Add(ORD("Khi có cháy trong lò", new[] { "Đeo ngay bình tự cứu", "Báo động cho người xung quanh", "Rút theo lối thoát về phía gió sạch", "Tập trung, điểm danh tại nơi an toàn" }, "Bảo vệ đường thở trước, rồi báo động và thoát theo đúng lối."));
            l.vd.Add(TF("CO tăng nhẹ nhưng chưa thấy khói thì vẫn làm việc bình thường.", false, "CO tăng là dấu hiệu sớm. Phải báo cáo và làm theo chỉ huy, không chờ thấy khói."));
            l.vd.Add(MC("Khi rút khỏi khu vực cháy, đi theo hướng nào?", new[] { "Ngược chiều khói, về phía luồng gió sạch", "Theo chiều khói cho nhanh", "Đứng yên chờ cứu", "Hướng nào gần nhất" }, 0, "Khói đi theo luồng gió thải. Đi về phía gió sạch để tránh khí độc."));
            L.Add(l);

            l = new Level { name = "Bục nước", sub = "Nhận diện và rút người",
                rule = "Dấu hiệu bục nước: vách lò “đổ mồ hôi”, nước rỉ tăng dần, nước đục, có tiếng nước chảy trong vỉa, nhiệt độ giảm, có sương mù. Đào gần vùng nghi chứa nước phải khoan thăm dò trước." };
            l.kt.Add(BIN("Đâu là dấu hiệu bục nước?", new[] { "Dấu hiệu bục nước", "Không phải" }, new[] { "Vách lò “đổ mồ hôi”", "Nước rỉ ngày càng nhiều", "Tiếng nước chảy trong vỉa", "Mùi nhựa thông", "Nhiệt độ tăng cao" }, new[] { 0, 0, 0, 1, 1 }, "Mùi nhựa thông và nhiệt độ tăng là dấu hiệu than tự cháy, không phải bục nước."));
            l.kt.Add(MC("Đào lò gần khu lò cũ có thể chứa nước, phải làm gì?", new[] { "Khoan thăm dò trước gương lò", "Đào nhanh qua đoạn đó", "Chỉ cần bơm nước sẵn", "Đổi sang ca đêm" }, 0, "Khoan thăm dò giúp phát hiện và tháo nước trước khi gương lò tới gần."));
            l.kt.Add(TF("Nước rỉ ra từ vách mà trong vắt thì bình thường, không cần báo cáo.", false, "Mọi thay đổi về nước trong lò đều phải báo cáo để theo dõi."));
            l.kt.Add(MAT("Ghép dấu hiệu với nguy cơ", new[] { "Mùi nhựa thông", "Vách đổ mồ hôi, nước rỉ", "Máy đo báo CH₄ cao", "Hai cửa gió cùng mở" }, new[] { "Than tự cháy", "Bục nước", "Tích tụ khí", "Đoản mạch gió" }, "Nhận đúng nguy cơ thì mới chọn đúng cách xử lý."));
            l.vd.Add(MC("Gương lò cụt rỉ nước mạnh dần, có tiếng róc rách.", new[] { "Dừng đào, rút người khỏi vùng nguy hiểm, báo chỉ huy", "Đào nhanh cho qua", "Bịt tạm bằng giẻ", "Bơm nước rồi đào tiếp" }, 0, "Đây là dấu hiệu bục nước rõ ràng: dừng ngay và rút người."));
            l.vd.Add(ORD("Xử lý khi có dấu hiệu bục nước", new[] { "Dừng công việc", "Rút người lên vị trí cao, an toàn", "Báo cáo chỉ huy / trực ban", "Chỉ trở lại khi được kiểm tra, cho phép" }, "Rút trước, báo sau, không tự ý quay lại."));
            l.vd.Add(TF("Khi rút khỏi vùng có nguy cơ bục nước, nên đi lên các đường lò ở mức cao hơn.", true, "Nước chảy xuống chỗ thấp; lên mức cao hơn để tránh bị nước cuốn."));
            l.vd.Add(MC("Sau sự cố nước, ai cho phép quay lại khu vực?", new[] { "Chỉ huy / cán bộ kỹ thuật sau khi kiểm tra an toàn", "Tự quay lại khi thấy hết nước", "Tổ trưởng tổ bên cạnh", "Bất kỳ ai đến trước" }, 0, "Chỉ trở lại khi có kiểm tra và lệnh của người có thẩm quyền."));
            L.Add(l);

            for (int li = 0; li < L.Count; li++)
            {
                for (int i = 0; i < L[li].kt.Count; i++) { var q = L[li].kt[i]; q.lv = li; q.id = li + "kt" + i; QM[q.id] = q; }
                for (int i = 0; i < L[li].vd.Count; i++) { var q = L[li].vd[i]; q.lv = li; q.id = li + "vd" + i; QM[q.id] = q; }
            }
            return L;
        }

        public static readonly string[] Stations = { "Kiến thức", "Vận dụng", "Tổng hợp" };
        public static readonly string[] StationIcons = { "book", "helmet", "star" };
        public static readonly string[] Teams = { "Tổ 1 – PX Khai thác 1", "Tổ 3 – PX Khai thác 1", "Tổ 2 – PX Đào lò 2", "Tổ 5 – PX Thông gió", "Tổ 4 – PX Vận tải", "Tổ 6 – PX Cơ điện" };
        public static readonly int[] TeamBase = { 1840, 1610, 1490, 1320, 1185, 960 };
        public static readonly string[] Mates = { "Nguyễn Văn Hùng", "Trần Đức Long", "Lê Minh Tuấn", "Phạm Quang Huy", "Đỗ Thành Nam" };
        public static readonly int[] MateXp = { 420, 365, 290, 210, 140 };
        public static readonly string[] Praise = { "Chính xác!", "Rất tốt!", "Đúng quy định!", "Chuẩn an toàn!" };
    }
}
