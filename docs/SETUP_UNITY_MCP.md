# Hướng dẫn Cấu hình và Sửa lỗi Unity MCP với Antigravity

Tài liệu này ghi lại chi tiết sự cố không nạp được công cụ **Unity MCP** trên agent Antigravity, nguyên nhân gốc rễ và cách khắc phục để duy trì kết nối trực tiếp ổn định.

---

## 1. Mô tả Sự cố (Problem)

Khi bắt đầu phiên làm việc:
* Agent Antigravity báo lỗi không tìm thấy các công cụ thuộc nhóm `UnityMCP/*` hoặc `mcp__unity_*` trong danh sách các tool khả dụng.
* Tiến trình Python MCP (`unity_mcp_server.py`) không được Chat App (IDE) tự động kích hoạt dưới nền.
* Mặc dù cấu hình đã được khai báo chính xác trong file `C:\Users\Hoang.H\.gemini\antigravity-ide\mcp_config.json`, hệ thống vẫn không nhận diện được.

---

## 2. Nguyên nhân Gốc rễ (Root Cause)

* **Nhầm lẫn thư mục Profile:** Agent Antigravity chạy độc lập trong môi trường CLI/Agent của nó và nạp tệp cấu hình từ thư mục profile chính của agent tại:
  `C:\Users\Hoang.H\.gemini\antigravity\`
  Thay vì thư mục profile của giao diện IDE tại:
  `C:\Users\Hoang.H\.gemini\antigravity-ide\`
* **Tệp cấu hình rỗng:** Tệp `C:\Users\Hoang.H\.gemini\antigravity\mcp_config.json` ở trạng thái **0 byte** (rỗng hoàn toàn), khiến agent không nạp bất kỳ MCP server nào (bao gồm cả `UnityMCP`).

---

## 3. Quy trình Khắc phục (Solution Steps)

Để đồng bộ cấu hình và kích hoạt Unity MCP cho agent, thực hiện các bước sau:

### Bước 1: Sao chép tệp cấu hình hợp lệ
Sao chép cấu hình từ thư mục của IDE sang thư mục profile của Agent bằng PowerShell:
```powershell
Copy-Item -Path "C:\Users\Hoang.H\.gemini\antigravity-ide\mcp_config.json" -Destination "C:\Users\Hoang.H\.gemini\antigravity\mcp_config.json" -Force
```

### Bước 2: Xác nhận nội dung tệp cấu hình mới
Nội dung của tệp `C:\Users\Hoang.H\.gemini\antigravity\mcp_config.json` sau khi copy phải chứa khai báo server `UnityMCP` với các đường dẫn tuyệt đối chính xác:
```json
{
  "mcpServers": {
    "claude-mem": {
      "command": "C:\\Program Files\\nodejs\\node.exe",
      "args": [
        "C:\\Users\\Hoang.H\\.claude\\plugins\\marketplaces\\thedotmack\\plugin\\scripts\\mcp-server.cjs"
      ]
    },
    "pencil": {
      "command": "C:\\Users\\Hoang.H\\.pencil\\mcp\\antigravity\\out\\mcp-server-windows-x64.exe",
      "args": [
        "--app",
        "antigravity"
      ],
      "env": {}
    },
    "UnityMCP": {
      "command": "D:\\soflware\\Unity\\Source\\Jigsaw_ViNa\\JigsawVina\\Assets\\StreamingAssets\\realvirtual-MCP\\python\\python.exe",
      "args": [
        "D:\\soflware\\Unity\\Source\\Jigsaw_ViNa\\JigsawVina\\Assets\\StreamingAssets\\realvirtual-MCP\\unity_mcp_server.py",
        "--mode",
        "stdio",
        "--project-path",
        "D:\\soflware\\Unity\\Source\\Jigsaw_ViNa\\JigsawVina\\Assets"
      ]
    }
  }
}
```

> [!IMPORTANT]
> **Giải thích chi tiết về các đường dẫn trong cấu hình (Tùy biến theo từng Project):**
> 
> Giả sử thư mục gốc (root) của dự án Unity của bạn nằm tại: `<ProjectRoot>` 
> (Ví dụ trong dự án này là: `D:\soflware\Unity\Source\Jigsaw_ViNa\JigsawVina`).
> 
> * **`command`**: Đường dẫn tuyệt đối đến tệp thực thi Python nhúng đi kèm trong gói MCP:
>   * Cấu trúc: `<ProjectRoot>\\Assets\\StreamingAssets\\realvirtual-MCP\\python\\python.exe`
>   * Lưu ý: Cần sử dụng ký tự gạch chéo ngược kép (`\\`) trên hệ điều hành Windows để tránh lỗi phân tích chuỗi (JSON escape characters).
> * **`args[0]`**: Đường dẫn tuyệt đối đến tệp script Python khởi tạo server MCP:
>   * Cấu trúc: `<ProjectRoot>\\Assets\\StreamingAssets\\realvirtual-MCP\\unity_mcp_server.py`
> * **`--project-path`**: Đường dẫn tuyệt đối đến thư mục `Assets` của dự án Unity mà bạn muốn kết nối:
>   * Cấu trúc: `<ProjectRoot>\\Assets`
>   * Tham số này rất quan trọng giúp server MCP định vị và tự động nhận diện cổng kết nối (WebSocket port) của đúng cửa sổ Unity Editor đang mở dự án đó (qua tệp trạng thái trong thư mục `~/.unity-mcp/`).

### Bước 3: Khởi động lại Chat App
Tắt hoàn toàn App Chat (IDE) và mở lại để hệ thống nạp lại tệp `mcp_config.json` mới cập nhật từ thư mục profile chính xác.

---

## 4. Xác minh Kết nối (Verification)

Sau khi khởi động lại, agent có thể sử dụng tool `call_mcp_tool` để kiểm tra trạng thái của server `UnityMCP`.

### Lệnh 1: Kiểm tra trạng thái chung (unity_status)
* **ServerName:** `UnityMCP`
* **ToolName:** `unity_status`
* **Arguments:** `{}` hoặc `{ "kwargs": {} }`

**Kết quả mong đợi:**
```json
{
  "connected": true,
  "ws_url": "ws://127.0.0.1:18711/mcp",
  "tools_count": 73,
  "state": "ready",
  "buffered_messages": 0,
  "circuit_breaker": "closed",
  "heartbeat": {
    "status": "ok",
    "tools_count": 73
  }
}
```
*(Trạng thái `connected: true` và `state: "ready"` xác nhận kết nối trực tiếp thành công).*

### Lệnh 2: Quét Scene hiện tại (scene_hierarchy)
* **ServerName:** `UnityMCP`
* **ToolName:** `scene_hierarchy`
* **Arguments:** `{ "kwargs": {} }`

**Kết quả mong đợi:**
Trả về cấu trúc GameObject trên Scene hiện tại của Unity (ví dụ: `Main Camera`, `Global Light 2D`).

---

## 5. Quy trình Thiết lập cho từng Dự án mới (Project-specific Setup)

Khi bạn chuyển sang làm việc với một dự án Unity mới hoàn toàn, bạn không cần chỉnh sửa các tệp cấu hình toàn cục một cách thủ công. Hãy thực hiện quy trình tự động hóa sau thông qua giao diện của Unity:

### Bước 1: Cài đặt gói Unity MCP Package
1. Trong cửa sổ Unity Editor của dự án mới, mở **Window -> Package Manager**.
2. Click vào biểu tượng dấu cộng (`+`) ở góc trên bên trái, chọn **Add package from git URL...**.
3. Nhập đường dẫn sau và click **Add**:
   `https://github.com/game4automation/io.realvirtual.mcp.git`

### Bước 2: Tải Python Server tự động
1. Sau khi cài đặt xong gói, bạn sẽ thấy một biểu tượng **bánh răng** hoặc biểu tượng **MCP** xuất hiện trên thanh công cụ (Toolbar) của Unity.
2. Click vào biểu tượng đó để mở bảng điều khiển Unity MCP.
3. Click vào nút **Clone Python Server**. Unity sẽ tự động tải phiên bản Python nhúng cùng mã nguồn server MCP về thư mục:
   `Assets/StreamingAssets/realvirtual-MCP/`

### Bước 3: Tạo cấu hình tự động (Configure Claude)
1. Trên cùng bảng điều khiển Unity MCP đó, click vào nút **Configure Claude**.
2. Thao tác này sẽ tự động phát hiện mọi đường dẫn tuyệt đối của dự án và sinh ra tệp cấu hình cục bộ mang tên `.mcp.json` nằm tại thư mục gốc (root) của dự án Unity mới.
3. Tệp `.mcp.json` cục bộ này có cấu trúc giống hệt tệp cấu hình toàn cục nhưng các đường dẫn thư mục đã được điều chỉnh tự động để trỏ chính xác đến dự án mới của bạn.

### Bước 4: Mở dự án trong IDE
1. Tắt App Chat (IDE) hiện tại.
2. Mở thư mục gốc của dự án Unity mới bằng IDE.
3. IDE khi khởi động sẽ tự động phát hiện tệp `.mcp.json` cục bộ trong dự án và kích hoạt server `UnityMCP` với cấu hình của dự án đó.

