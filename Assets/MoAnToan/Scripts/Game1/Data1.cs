using System;
using System.Collections.Generic;

namespace MoAnToan
{
    [Serializable]
    public class Item1
    {
        public string id;
        public string name;
        public string icon;
        public bool isRequired; // Phải mang vào ba lô
        public bool isBanned;   // Thuộc danh mục vật cấm
        public string desc;
    }

    [Serializable]
    public class Level1Data
    {
        public int id;
        public string title;
        public string subtitle;
        public string tip;
        public string speechNpc;
        public List<Item1> items;
    }

    public static class Data1
    {
        public static List<Level1Data> Levels = new List<Level1Data>
        {
            new Level1Data
            {
                id = 1,
                title = "Chuẩn bị an toàn",
                subtitle = "Kiểm tra tư trang & Bảo hộ trước ca",
                tip = "Hãy kéo các trang bị bảo hộ cần thiết vào ba lô và loại bỏ các vật cấm!",
                speechNpc = "Trang bị đầy đủ và không mang chất dễ cháy nổ sẽ giúp bạn làm việc an toàn!",
                items = new List<Item1>
                {
                    new Item1 { id = "helmet", name = "Mũ bảo hộ", icon = "helmet", isRequired = true, isBanned = false, desc = "Bảo vệ phần đầu, có gá kẹp đèn lò đạt chuẩn." },
                    new Item1 { id = "lamp", name = "Đèn lò", icon = "lamp", isRequired = true, isBanned = false, desc = "Chiếu sáng phòng nổ, bắt buộc khi xuống hầm lò." },
                    new Item1 { id = "vest", name = "Áo phản quang", icon = "vest", isRequired = true, isBanned = false, desc = "Giúp nhận diện thợ mỏ trong môi trường thiếu sáng." },
                    new Item1 { id = "gloves", name = "Găng tay", icon = "gloves", isRequired = true, isBanned = false, desc = "Bảo hộ tay khi xúc than, kéo goòng và chống giữ vách." },
                    new Item1 { id = "boots", name = "Ủng lò", icon = "boots", isRequired = true, isBanned = false, desc = "Ủng cao su lót mũi thép, chống trơn trượt và nước mỏ." },
                    new Item1 { id = "phone", name = "Điện thoại thường", icon = "phone", isRequired = false, isBanned = true, desc = "Điện thoại di động dân dụng không phòng nổ, cấm tuyệt đối!" },
                    new Item1 { id = "lighter", name = "Bật lửa / Diêm", icon = "lighter", isRequired = false, isBanned = true, desc = "Nguồn sinh lửa/nhiệt, nguy cơ gây nổ khí metan và bụi than!" }
                }
            },
            new Level1Data
            {
                id = 2,
                title = "Nhận trang bị nhà đèn",
                subtitle = "Kiểm định thiết bị cứu hộ cá nhân",
                tip = "Nhận bình tự cứu cá nhân và kiểm tra niêm phong, áp suất!",
                speechNpc = "Bình tự cứu phải còn niêm phong chì và đồng hồ chỉ vạch xanh mới được nhận!",
                items = new List<Item1>
                {
                    new Item1 { id = "rescuer", name = "Bình tự cứu", icon = "shield", isRequired = true, isBanned = false, desc = "Cung cấp dưỡng khí oxy cách ly khi có sự cố khí độc hoặc cháy." },
                    new Item1 { id = "detector", name = "Máy đo khí CH4", icon = "hazard", isRequired = true, isBanned = false, desc = "Kiểm tra nồng độ khí cháy nổ trước khi vào gương than." },
                    new Item1 { id = "tag", name = "Thẻ thợ mỏ", icon = "star", isRequired = true, isBanned = false, desc = "Thẻ định danh theo dõi nhân sự ra/vào lò." },
                    new Item1 { id = "lighter2", name = "Thuốc lá & Bao diêm", icon = "lighter", isRequired = false, isBanned = true, desc = "Cấm mang mọi vật liệu phát lửa vào khu vực mỏ hầm lò." }
                }
            }
        };
    }
}
