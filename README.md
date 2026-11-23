# An toàn hầm lò — bản test Unity (ý tưởng 5 và 6)

Unity 6000.3.20f1. Toàn bộ giao diện được dựng bằng code (uGUI), không cần prefab hay kéo thả.

## Mở và chạy
1. Unity Hub → **Add** → **Add project from disk** → chọn thư mục này.
2. Lần mở đầu tiên, project tự tạo `Assets/MoAnToan/Scenes/MoAnToan.unity` và đặt làm scene build.
3. Bấm **Play**. Nên để Game view ở tỉ lệ dọc (ví dụ 1080×1920 hoặc 9:16).

## Build
Menu **Mỏ An Toàn** trên thanh menu Unity:
- **Build Windows (.exe)** → `Builds/Windows/AnToanHamLo.exe`
- **Build Android (.apk)** → `Builds/Android/` (cần module Android Build Support)
- **Build Web (WebGL)** → `Builds/WebGL/` (cần module Web Build Support)

## Cấu trúc
- `Scripts/Core` — App (menu chính, canvas), UIK (bộ dựng UI), VG (vẽ vector), Sfx (âm thanh tổng hợp), kéo thả/vuốt
- `Scripts/Game5` — Chinh phục đường lò: dữ liệu câu hỏi (`Data5.cs`) và luồng chơi (`Game5.cs`)
- `Scripts/Game6` — Đường lò không sai sót: vẽ cảnh (`Scene6.cs`) và luồng chơi (`Game6.cs`)
- `Editor/MATBuild.cs` — tạo scene, menu build
- `Resources/MAT_Font*.otf` — font Inter (OFL), hỗ trợ tiếng Việt

Tiến độ lưu bằng PlayerPrefs (khóa `mat5_v1`, `mat6_v1`).

Nội dung câu hỏi/quy định là bản mẫu, cần cán bộ an toàn rà soát theo QCVN 01:2011/BCT và nội quy đơn vị.
