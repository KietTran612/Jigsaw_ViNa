# Thiết kế Giao diện Editor Tab-bar và Thu gọn (Collapsible Editor Layout)

Tài liệu này mô tả thiết kế tối ưu hóa không gian hiển thị của `JigsawVinaGameDataEditor.cs` bằng cách chuyển từ bố cục hai cột song song (Trái/Phải) sang dạng Tab-bar chuyển đổi cho vùng chi tiết cấu hình bức tranh, kết hợp với các nhóm foldout thu gọn cho từng độ khó.

## Mục tiêu
- Khắc phục tình trạng chật chội chiều ngang khi hiển thị đồng thời cả phần Metadata/Key Items (Bên trái) và phần Độ khó/Phần thưởng (Bên phải).
- Cung cấp tính năng thu gọn/mở rộng (collapse/expand) các cấu hình độ khó (Easy, Normal, Hard) để tập trung chỉnh sửa dễ dàng hơn.

## Thiết kế Chi tiết

### 1. Tab-bar chuyển đổi vùng chi tiết
Thêm một trường riêng để lưu trạng thái Tab được chọn hiện tại:
```csharp
private int _detailTabSelected = 0; // 0: Thông tin & Key Items, 1: Độ khó & Phần thưởng
```

Giao diện chi tiết của Bức tranh được chọn sẽ được phân tách thành hai Tab:
- **Tab 1: Thông tin & Key Items**
  - Hiển thị Thư mục tranh (`folderAsset`), các thông tin cơ bản của Tranh (ID, ID String, Tên tranh).
  - Hiển thị danh sách Key Items (file, tên hiển thị, mô tả, độ hiếm) với kích thước rộng tối đa theo chiều ngang của cửa sổ Editor.
- **Tab 2: Độ khó & Phần thưởng**
  - Hiển thị cấu hình cho 3 độ khó Easy, Normal, Hard.

### 2. Collapsible Foldouts cho Độ khó
Thêm các biến trạng thái foldout vào class `EditorTabState` để trạng thái ẩn/hiện của từng độ khó được lưu trữ độc lập cho mỗi bức tranh:
```csharp
public bool easyExpanded = true;
public bool normalExpanded = true;
public bool hardExpanded = true;
```

Trong Tab "Độ khó & Phần thưởng", sử dụng `EditorGUILayout.BeginFoldoutHeaderGroup` để nhóm các trường cấu hình:
- **DỄ (Easy)**: Gồm Columns, Rows, First Clear Coin, Replay Coin, First Clear Hint, Reward Key Item.
- **TRUNG BÌNH (Normal)**: Gồm các trường tương tự.
- **KHÓ (Hard)**: Gồm các trường tương tự.

## Kế hoạch kiểm thử & Xác minh
- Chạy Unity Editor compile không lỗi.
- Mở cửa sổ `JigsawVina/Game Data Editor` và kiểm tra chuyển đổi qua lại giữa 2 tab.
- Đóng/mở foldout của các độ khó, chuyển bức tranh ở sidebar rồi quay lại để xác nhận trạng thái foldout của tranh đó được bảo toàn.
- Bấm Save & Generate JSON để kiểm tra tính năng ghi đè lưu trữ dữ liệu sang JSON hoạt động đúng đắn.
