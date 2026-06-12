# Thiết kế: Hiển thị hình ảnh cho dòng chọn Reward Key Item trong Game Data Editor

Tài liệu này mô tả chi tiết thiết kế giao diện cho phép hiển thị hình ảnh của các Key Items trực tiếp trong dropdown chọn vật phẩm phần thưởng của cấu hình độ khó, kèm theo một ô xem trước ảnh thu nhỏ (preview thumbnail) bên cạnh.

## Mục tiêu
- Giúp người thiết kế game dễ dàng nhận biết và lựa chọn chính xác vật phẩm phần thưởng cho từng độ khó thay vì chỉ dựa vào tên file text.
- Đảm bảo tính nhất quán thẩm mỹ với phong cách giao diện hiện tại của Game Data Editor.

## Chi tiết Thiết kế

### 1. Chuẩn bị Dữ liệu GUIContent
Thay vì chỉ chuyển đổi danh sách texture thành mảng chuỗi `string[]` như trước:
```csharp
string[] itemNames = new string[itemTextures.Count + 1];
itemNames[0] = "None";
for (int i = 0; i < itemTextures.Count; i++)
{
    itemNames[i + 1] = itemTextures[i].name;
}
```

Chúng ta sẽ chuyển sang sử dụng mảng `GUIContent[]` để lưu trữ cả tên hiển thị và đối tượng `Texture2D` làm biểu tượng:
```csharp
GUIContent[] itemGUIContents = new GUIContent[itemTextures.Count + 1];
itemGUIContents[0] = new GUIContent("None");
for (int i = 0; i < itemTextures.Count; i++)
{
    itemGUIContents[i + 1] = new GUIContent(itemTextures[i].name, itemTextures[i]);
}
```

### 2. Layout Hiển thị trên IMGUI
Giao diện cấu hình phần thưởng cho mỗi độ khó sẽ được vẽ như sau:
```csharp
GUILayout.BeginHorizontal();

// Vẽ Dropdown hiển thị icon bên cạnh chữ
state.easyKeyRewardIndex = EditorGUILayout.Popup(
    new GUIContent("Reward Key Item"), 
    state.easyKeyRewardIndex, 
    itemGUIContents
);

GUILayout.Space(5);

// Vẽ ô xem trước (preview thumbnail) kích thước 24x24 bên phải dropdown
if (state.easyKeyRewardIndex > 0 && state.easyKeyRewardIndex <= itemTextures.Count)
{
    var tex = itemTextures[state.easyKeyRewardIndex - 1];
    var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
    DrawTextureWithBorder(rect, tex, ScaleMode.ScaleToFit);
}
else
{
    var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
    EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1.0f));
}

GUILayout.EndHorizontal();
```

## Kế hoạch Xác minh
- **Xác minh trực quan (Manual Verification)**:
  - Mở cửa sổ Game Data Editor từ Unity.
  - Chọn tab "Cấu hình Tranh" -> chuyển sang tab phụ "Độ khó & Phần thưởng".
  - Mở rộng các phần gập (foldouts) của các độ khó.
  - Nhấp vào dropdown "Reward Key Item" để kiểm tra xem mỗi hàng lựa chọn đã có icon ảnh nhỏ đi kèm hay chưa.
  - Chọn một vật phẩm bất kỳ và đảm bảo ô xem trước (preview) kích thước 24x24 hiển thị đúng hình ảnh phóng to của vật phẩm đã chọn.
- **Kiểm thử biên**:
  - Khi không chọn vật phẩm ("None"), ô xem trước phải là một hộp màu xám trống, không gây lỗi giao diện.
  - Kiểm tra xem việc thay đổi lựa chọn và bấm "Save & Generate JSON" có lưu chính xác ID của vật phẩm vào file JSON hay không.
