# Hướng Dẫn & Lưu Ý Dành Cho Agent (AGENTS.md)

Tài liệu này cung cấp toàn bộ bối cảnh kiến trúc, nguyên tắc kỹ thuật và các quy định nghiêm ngặt khi đọc, sửa đổi hoặc mở rộng project **gamemo** ("An toàn hầm lò - bản test Unity").

---

## 1. Tổng Quan Dự Án (Project Overview)
- **Tên dự án / Product:** An toàn hầm lò — bản test Unity (ý tưởng 5 & 6).
- **Mục tiêu:** Ứng dụng đào tạo, kiểm tra kiến thức và nhận diện nguy cơ an toàn lao động trong khai thác than hầm lò (tham chiếu theo Quy chuẩn Kỹ thuật Quốc gia **QCVN 01:2011/BCT**).
- **Hai chế độ chơi chính:**
  1. **Game 5 (Chinh phục đường lò):** Học theo trạm ngắn (60–90s) phong cách Duolingo, tính năng tim (hearts), streak chuỗi ngày, XP, ôn tập câu sai, bảng xếp hạng tổ.
  2. **Game 6 (Đường lò không sai sót):** Quan sát hình ảnh trực quan từng đoạn lò (phối cảnh 2D vector), phát hiện điểm bất thường/nguy cơ (bảo hộ, khí CH₄, thông gió, bục nước, than tự cháy) và báo cáo.
- **Unity Engine:** `Unity 6000.3.x` (cụ thể `6000.3.29f1`).
- **Nền tảng mục tiêu (Target Platforms):** Standalone Windows (.exe), Android (.apk), Web (WebGL).
- **Tỉ lệ hiển thị (Aspect Ratio):** Mặc định màn hình dọc (Portrait), canvas tham chiếu `420 x 860` (khuyên dùng Game view tỉ lệ `9:16` hoặc `1080x1920`). Cột nội dung tự co giãn tối đa `560px` và căn giữa màn hình.

---

## 2. Triết Lý Thiết Kế Cốt Lõi (Core Architecture — BẮT BUỘC TUÂN THỦ)

> [!IMPORTANT]
> **TOÀN BỘ GIAO DIỆN & HÌNH ẢNH DỰNG BẰNG CODE (PURE CODE uGUI)**
> - **KHÔNG dùng Prefab, KHÔNG kéo thả trên Inspector.**
> - Mọi thành phần UI, hình khối, bố cục, icon, và hiệu ứng tương tác đều được khởi tạo và cấu hình bằng C# code thông qua bộ thư viện nội bộ `UIK` và `VG`.
> - **KHÔNG yêu cầu file asset âm thanh ngoài (.mp3, .wav):** Toàn bộ âm thanh hiệu ứng (sfx) được tổng hợp bằng toán học trực tiếp trong code (`Sfx.cs`).
> - **Chỉ có asset duy nhất trong Resources:** Font chữ `MAT_Font.otf` / `MAT_Font_Bold.otf` (Inter OFL hỗ trợ tiếng Việt có dấu).

Khi viết tính năng mới hoặc sửa lỗi, **tuyệt đối KHÔNG tạo ra sự phụ thuộc vào Inspector hay Prefab kéo thả**. Mọi đối tượng phải tự khởi tạo (self-bootstrapping) độc lập.

---

## 3. Cấu Trúc Thư Mục & Vai Trò Codebase

```
Assets/MoAnToan/
├── Editor/
│   └── MATBuild.cs         # Menu "Mỏ An Toàn", tự tạo scene MoAnToan.unity khi mở project, build Windows/Android/Web.
├── Resources/
│   ├── MAT_Font.otf        # Font chữ chính (Inter, OFL) hỗ trợ tiếng Việt.
│   └── MAT_Font_Bold.otf   # Font chữ đậm.
├── Scenes/
│   └── MoAnToan.unity      # Scene duy nhất (tự động tạo bởi MATBuild.cs).
└── Scripts/
    ├── Core/               # Hạ tầng cốt lõi của ứng dụng
    │   ├── App.cs          # Entry point, khởi tạo Camera, EventSystem, Canvas responsive, menu chính.
    │   ├── UIK.cs          # Bộ dựng giao diện uGUI: Box, Btn, Txt, Card, Scroll, Modal, Toast, màu sắc, font, layout.
    │   ├── VG.cs           # Vẽ vector 2D mesh procedurally (Polygon, Tri, Quad, Ellipse, Line, Icon...).
    │   ├── Sfx.cs          # Tổng hợp âm thanh procedural (ok, bad, tick, tap, win, step) không cần file audio.
    │   └── Interactions.cs # Thành phần tương tác vuốt thẻ SwipeCard (Tinder-style) và kéo thả DropZone.
    ├── Game5/              # Game 5: Trạm học & trắc nghiệm an toàn
    │   ├── Data5.cs        # Cấu trúc câu hỏi (MC, TF, Bin, Order, Match) & bộ dữ liệu quy chuẩn an toàn.
    │   └── Game5.cs        # Vòng lặp chơi, tim, streak, XP, leaderboard, ôn tập câu sai.
    └── Game6/              # Game 6: Quan sát và tìm lỗi an toàn hầm lò
        ├── Scene6.cs       # Vẽ cảnh đường lò phối cảnh 2D bằng VG, tính toán hit-box chạm lỗi.
        └── Game6.cs        # Luồng chơi quan sát, danh sách checklist, kiểm tra bất thường, báo cáo.
```

---

## 4. Các Quy Chuẩn Lập Trình & API Quan Trọng

### 4.1. Hệ thống UI (`UIK.cs`)
- **Màu sắc chuẩn (Palette):**
  - Nền & Thẻ: `UIK.Bg` (`#121417`), `UIK.P` (`#1c1f24`), `UIK.P2` (`#252930`), `UIK.Line` (`#323741`).
  - Chữ & Nội dung: `UIK.Ink` (`#f3f1ea`), `UIK.Mute` (`#9da3ad`).
  - Màu nhấn & Trạng thái: `UIK.Y` (Vàng an toàn `#ffc21a`), `UIK.G` (Xanh chuẩn `#35c76a`), `UIK.R` (Đỏ cảnh báo `#ff5b4f`), `UIK.Blue` (`#6fc3ff`), `UIK.Orange` (`#ff9b2f`).
- **Tạo thành phần:**
  - Nút bấm: `UIK.Btn(parent, "Tiêu đề", bgColor, fgColor, fontSize, onClick)` (đã tự gắn âm thanh `tap`).
  - Hộp bo góc: `UIK.Box(parent, color, sprite, name)` (mặc định bo góc tròn `Round`).
  - Thẻ card chứa layout dọc: `UIK.Card(parent, bgColor, pad, spacing)`.
  - Cuộn dọc: `UIK.Scroll(parent, spacing, pad, out ScrollRect sr)`.
  - Hộp thoại: `UIK.Modal(out Action close)`.
  - Thông báo nổi: `UIK.Toast("Nội dung thông báo")`.

### 4.2. Đồ họa Vector Procedural (`VG.cs`)
- Kế thừa `MaskableGraphic`, vẽ bằng `VertexHelper` trên hệ tọa độ thiết kế `W x H` (mặc định `360 x 440`, y hướng xuống), tự co giãn theo `RectTransform`.
- Hỗ trợ các hàm nguyên thủy: `Rect()`, `Outline()`, `Tri()`, `Quad()`, `Poly()`, `Ellipse()`, `Arc()`, `Line()`.
- Thư viện icon vector tích hợp sẵn: `VG.Icon(parent, name, color, size)` hỗ trợ các icon: `book`, `helmet`, `flame`, `water`, `wind`, `hazard`, `check`, `cross`, `gear`, `volume`, `refresh`...

### 4.3. Âm thanh Procedural (`Sfx.cs`)
- Sử dụng `Sfx.Play(soundName)`. Các âm thanh hiện có:
  - `"tap"`: Chạm nút bấm.
  - `"ok"`: Trả lời đúng, vượt ải thành công.
  - `"bad"`: Trả lời sai, phạm lỗi an toàn.
  - `"tick"`: Đếm nhịp thời gian.
  - `"win"`: Hoàn thành màn/thắng lợi.
  - `"step"`: Chuyển bước đi lò.
- Kiểm tra tắt/bật qua `Sfx.Muted` (lưu tại key `mat_mute` trong `PlayerPrefs`).

### 4.4. Lưu Trữ Trạng Thái (Persistence)
- Lưu trữ cục bộ qua `PlayerPrefs` với định dạng JSON (`JsonUtility`):
  - `mat5_v1`: Chuỗi JSON của `Save5` (xp, hearts, streak, daily progress, wrong questions, done levels).
  - `mat6_v1`: Chuỗi JSON của `Save6` (unlocked zones, completed zone records).
  - `mat_mute`: `0` hoặc `1`.

---

## 5. Quy Tắc Khi Agent Đọc & Sửa Code (Dos & Don'ts)

1. **Tuyệt đối KHÔNG xóa hoặc bỏ quên file `.meta`:**
   - Trong Unity, mỗi file trong thư mục `Assets/` đều có một file `.meta` tương ứng. Khi thêm file mới, Unity Editor sẽ tự sinh file meta; nhưng nếu chỉnh sửa/đổi tên file qua công cụ dòng lệnh, cần bảo đảm không làm mất đồng bộ `.meta`.
2. **Tuân thủ mã hóa UTF-8:**
   - Tất cả chuỗi văn bản trong code là **tiếng Việt có dấu**. Mọi thao tác ghi file phải dùng chuẩn UTF-8 (không BOM hoặc có BOM chuẩn) để tránh lỗi font chữ tiếng Việt trên giao diện.
3. **Giữ nguyên tính tự lập (Self-Contained):**
   - Scene chính `MoAnToan.unity` được tạo bằng code (`MATBuild.cs`). Khi khởi chạy `Play`, `App.cs` tự động gắn `App` GameObject thông qua `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`. Đừng chuyển luồng điều khiển sang các prefab rời rạc.
4. **Đảm bảo tính chính xác về an toàn lao động:**
   - Nội dung câu hỏi và quy tắc an toàn trong `Data5.cs` và `Scene6.cs` liên quan trực tiếp đến ngành mỏ than hầm lò (QCVN 01:2011/BCT: nồng độ CH₄ < 1%, cấm lửa trần/bật lửa/điện thoại thường, cửa gió đôi không được mở cùng lúc, cảm biến CH₄ treo sát nóc lò, bình tự cứu cá nhân...). Khi sửa đổi dữ liệu, hãy giữ vững tính chuyên môn ngành mỏ.
5. **Kiểm tra tương thích đa nền tảng:**
   - Code cần chạy được trên Windows, Android và WebGL. Tránh sử dụng các API chỉ hỗ trợ riêng cho 1 platform (như file system đồng bộ cục bộ trực tiếp ngoài `Application.persistentDataPath` hoặc `PlayerPrefs`).
