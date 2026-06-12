# Jigsaw Việt Nam — Data Design Notes v0.1

Tài liệu này tổng hợp các quyết định hiện tại về thiết kế dữ liệu cho game ghép hình chủ đề Việt Nam. Ở giai đoạn này, dữ liệu được hiểu là **local JSON trong app**, nhưng cấu trúc cần đủ sạch để sau này có thể dùng chung với **remote API/backend** nếu cần.

---

## 1. Mục tiêu của thiết kế dữ liệu

Thiết kế dữ liệu có vai trò như **master data/schema design** cho game, không nhất thiết là database SQL.

Hiện tại, “bảng” có thể được triển khai bằng:

- JSON file trong app
- CSV export từ editor
- ScriptableObject trong Unity
- Resource file trong Godot
- SQLite local nếu cần
- API response nếu sau này có backend

Mục tiêu chính:

- Tránh hard-code gameplay content.
- Dễ làm editor nội bộ.
- Dễ chỉnh reward, unlock, item, difficulty, drop rate.
- Dễ validate lỗi dữ liệu.
- Dễ mở rộng thêm tranh, event, item, pack.
- Dễ chuyển sang backend hoặc remote config sau này.
- Giữ save data ổn định khi game cập nhật.

---

## 2. Static Data và Dynamic Data

### 2.1 Static Data

Static data là dữ liệu do designer/dev tạo ra.

Hiện tại:

- Lưu trong app.
- Được tạo/sửa bằng editor nội bộ.
- Export ra JSON/config để app đọc.

Sau này nếu cần:

- Có thể sync/backend.
- Có thể import/export cho backend.
- Có thể dùng làm master data server-side.

Ví dụ static data:

- `items`
- `pictures`
- `categories`
- `difficulties`
- `drop_tables`
- `unlock_requirements`
- `picture_assets`
- `events`
- `localization`

### 2.2 Dynamic Data / Player Save Data

Dynamic data là dữ liệu theo từng người chơi.

Hiện tại:

- Lưu local trong app.

Sau này nếu cần:

- Sync backend.
- Backup cloud.
- Hỗ trợ multi-device.

Ví dụ dynamic data:

- Người chơi sở hữu item nào.
- Số lượng coin/hint.
- Tranh nào đã mở.
- Tranh nào đã hoàn thành.
- Reward lần đầu của tranh + độ khó nào đã nhận.
- Rate item hôm nay đã rơi mấy lần.
- Ngày reset gần nhất.
- Tiến trình event.

Không lưu trạng thái người chơi trực tiếp vào static data như `pictures.json` hoặc `items.json`.

---

## 3. Quy ước ID

Mỗi record trong các bảng chính sẽ có 2 loại ID:

### 3.1 `id` — integer/index

Đây là ID số, dùng làm liên kết chính giữa các bảng và trong runtime/save data.

Ví dụ:

```json
{
  "id": 101,
  "id_string": "banana_tree",
  "display_name": "Cây Chuối"
}
```

Khi bảng khác liên kết tới item, ưu tiên dùng `id`:

```json
{
  "required_item_ids": [101, 102]
}
```

### 3.2 `id_string` — readable string ID

Đây là ID chữ dễ đọc, dùng cho:

- Editor.
- Debug.
- Log.
- Export/import dễ hiểu.
- Đọc config thủ công.

Ví dụ:

```json
{
  "id": 101,
  "id_string": "banana_tree",
  "display_name": "Cây Chuối"
}
```

### 3.3 Quy tắc ổn định ID

Đã chốt:

- `id` cố định vĩnh viễn.
- `id_string` cố định vĩnh viễn.
- Không đổi `id` sau khi record đã được dùng/release.
- Không đổi `id_string` nếu không thật sự cần.
- Nếu record không dùng nữa, đổi `status = "deprecated"`, không xóa.
- Save data ưu tiên lưu bằng `id`, không lưu bằng `id_string`.

---

## 4. Base Fields cho các bảng chính

Các bảng chính nên có các field cơ bản sau:

```json
{
  "id": 1,
  "id_string": "example_id",
  "display_name": "Tên hiển thị",
  "description": "Mô tả record.",
  "display_name_key": "example.name",
  "description_key": "example.description",
  "status": "active",
  "sort_order": 10,
  "created_at": "2026-06-10T00:00:00Z",
  "updated_at": "2026-06-10T00:00:00Z"
}
```

Với bảng kỹ thuật không cần hiển thị cho player, vẫn nên có:

```json
{
  "id": 1,
  "id_string": "example_id",
  "description": "Mô tả record.",
  "status": "active",
  "created_at": "2026-06-10T00:00:00Z",
  "updated_at": "2026-06-10T00:00:00Z"
}
```

### 4.1 Không dùng field `name`

Đã chốt:

- Không dùng `name`.
- Dùng `display_name` cho tên hiển thị/fallback.
- Dùng `id_string` cho readable code ID.
- Nếu sau này cần tên nội bộ thì có thể thêm `editor_name`, MVP chưa cần.

---

## 5. Status Record

Không xóa record đã tạo/release. Sử dụng field `status`.

Các status đề xuất:

| Status | Ý nghĩa |
|---|---|
| `draft` | Đang thiết kế |
| `active` | Đang dùng trong game |
| `hidden` | Có trong data nhưng chưa hiển thị |
| `deprecated` | Không dùng nữa nhưng giữ để không hỏng save |
| `event_only` | Chỉ dùng trong event |

Editor cần kiểm tra không dùng record `deprecated` cho content mới.

---

## 6. Sort Order

Các bảng cần hiển thị theo thứ tự nên có:

```json
{
  "sort_order": 10
}
```

Áp dụng cho:

- `categories`
- `pictures`
- `difficulties`
- `events`
- có thể áp dụng cho `items` nếu inventory/collection cần sắp xếp

---

## 7. Created At / Updated At

Mỗi record nên có:

```json
{
  "created_at": "2026-06-10T00:00:00Z",
  "updated_at": "2026-06-10T00:00:00Z"
}
```

Dùng cho:

- Editor.
- Audit.
- Debug.
- Theo dõi thay đổi data.

Runtime có thể đọc hoặc bỏ qua tùy implementation.

---

## 8. Localization

Có localization từ đầu.

Mỗi record vẫn có:

- `display_name`
- `description`

Hai field này dùng làm fallback.

Đồng thời thêm:

- `display_name_key`
- `description_key`

Ví dụ:

```json
{
  "id": 101,
  "id_string": "banana_tree",
  "display_name": "Cây Chuối",
  "description": "Một vật phẩm quen thuộc trong đời sống làng quê.",
  "display_name_key": "item.banana_tree.name",
  "description_key": "item.banana_tree.description"
}
```

Cách đọc text:

1. Nếu có localization theo key thì dùng localization.
2. Nếu không có localization thì fallback về `display_name` / `description`.

---

## 9. Quy ước naming

Đã chốt:

| Nơi dùng | Naming |
|---|---|
| JSON / config / API | `snake_case` |
| C# model | `PascalCase` |
| TypeScript model | `camelCase` |

Ví dụ JSON:

```json
{
  "id_string": "banana_tree",
  "display_name": "Cây Chuối",
  "created_at": "2026-06-10T00:00:00Z"
}
```

Ví dụ C#:

```csharp
public string IdString;
public string DisplayName;
public DateTime CreatedAt;
```

---

## 10. Enum trong JSON

Enum trong JSON lưu dạng string, không lưu dạng int.

Ví dụ:

```json
{
  "item_type": "key_item",
  "rarity": "common",
  "status": "active"
}
```

Khi parse vào model thì convert sang enum.

Ví dụ trong code:

```csharp
ItemType.KeyItem
ItemRarity.Common
DataStatus.Active
```

Lý do:

- JSON dễ đọc.
- Dễ debug.
- Editor dễ hiển thị.
- Không bị mù nghĩa như số `1`, `2`, `3`.

---

## 11. Default Value và Required Field

Cần có default value cho các field phụ, nhưng các field bắt buộc phải validate.

### 11.1 Field bắt buộc không nên default

Thiếu các field này thì báo lỗi:

- `id`
- `id_string`
- `display_name`
- `description`
- foreign key quan trọng như `category_id`, `item_id`, `difficulty_id`
- enum quan trọng như `item_type`, `status`
- reward/requirement field bắt buộc trong record tương ứng

### 11.2 Field phụ có thể default

Ví dụ:

```json
{
  "status": "draft",
  "sort_order": 0,
  "is_consumable": false,
  "is_time_limited": false,
  "display_name_key": "",
  "description_key": ""
}
```

Default value nên được quy định trong editor/mapper, không để mỗi nơi tự hiểu khác nhau.

---

## 12. Data Version và Schema Version

Nên có file hoặc bảng metadata:

```json
{
  "schema_version": 1,
  "data_version": 1,
  "description": "Initial MVP data version."
}
```

Ý nghĩa:

| Field | Dùng cho |
|---|---|
| `schema_version` | Khi cấu trúc bảng/field thay đổi |
| `data_version` | Khi nội dung data thay đổi |

Hiện tại chưa cần hash/checksum. Khi có backend hoặc remote data thì thêm sau.

---

## 13. Currency là Item

Đã chốt:

- Coin là item type `currency`.
- Hint cũng là item type `currency`.
- Star không phải currency item. Star là achievement/progress score, lưu dưới dạng `best_star` theo từng tranh + độ khó và tổng sao được tính từ save data.

Ví dụ coin:

```json
{
  "id": 1,
  "id_string": "coin",
  "display_name": "Xu",
  "description": "Đơn vị tiền cơ bản của game.",
  "display_name_key": "item.coin.name",
  "description_key": "item.coin.description",
  "item_type": "currency",
  "rarity": "common",
  "is_consumable": true,
  "is_time_limited": false,
  "status": "active"
}
```

Ví dụ hint:

```json
{
  "id": 2,
  "id_string": "hint",
  "display_name": "Gợi Ý",
  "description": "Vật phẩm dùng để hỗ trợ người chơi khi ghép tranh.",
  "display_name_key": "item.hint.name",
  "description_key": "item.hint.description",
  "item_type": "currency",
  "rarity": "common",
  "is_consumable": true,
  "is_time_limited": false,
  "status": "active"
}
```

Lợi ích:

- Reward system thống nhất.
- Drop table có thể rơi coin/hint.
- Inventory xử lý chung.
- Editor dễ setup phần thưởng.

Nếu sau này cần điều kiện mở khóa theo sao, dùng requirement kiểu `total_star_at_least` và không tiêu hao sao.

---

## 14. Các bảng/data chính đề xuất

Các data có thể tách bảng thì sẽ tách và liên kết bằng `id`.

| Bảng | Vai trò |
|---|---|
| `items` | Toàn bộ item/currency/key/event item |
| `categories` | Nhóm chủ đề tranh |
| `difficulties` | Cấu hình độ khó |
| `pictures` | Thông tin tranh |
| `picture_difficulties` | Setting/reward theo tranh + độ khó |
| `unlock_requirements` | Điều kiện mở khóa |
| `drop_tables` | Bảng rơi item tỷ lệ |
| `drop_table_items` | Item và rate cụ thể trong từng drop table |
| `picture_assets` | Ảnh, thumbnail, preview |
| `events` | Sự kiện có thời hạn, để sau cũng được |
| `localization` | Text đa ngôn ngữ |
| `reward_groups` | Nếu sau này muốn gom reward tái sử dụng |
| `tags` | Nếu tag cần quản lý bằng editor |

MVP nên có ngay:

- `items`
- `categories`
- `difficulties`
- `pictures`
- `picture_difficulties`
- `unlock_requirements`
- `drop_tables`
- `drop_table_items`
- `picture_assets`
- `player_save`

Các field quan trọng cần có cho puzzle MVP:

| Bảng | Field | Ý nghĩa |
|---|---|---|
| `picture_assets` | `aspect_ratio` | Tỷ lệ ảnh, MVP dùng `4:3` |
| `picture_difficulties` | `grid_columns` | Số cột mảnh ghép |
| `picture_difficulties` | `grid_rows` | Số hàng mảnh ghép |
| `picture_difficulties` | `piece_count` | Tổng số mảnh, phải bằng `grid_columns * grid_rows` |
| `picture_difficulties` | `piece_shape_type` | MVP dùng `rectangle` |
| `picture_difficulties` | `allow_rotation` | MVP dùng `false` |
| `pictures` | `difficulty_unlock_policy` | `sequential` hoặc `all_when_picture_unlocked` |

MVP difficulty grid:

| Difficulty | Grid | Piece Count |
|---|---:|---:|
| Easy | 6 x 4 | 24 |
| Normal | 8 x 6 | 48 |
| Hard | 12 x 8 | 96 |

Có thể để sau:

- `events`
- `reward_groups`
- `tags`
- backend sync
- A/B testing
- remote config

---

## 15. Root JSON tổng hợp

Tạm thời local JSON và remote API được xem là cùng format.

MVP có thể dùng một root JSON tổng hợp:

```json
{
  "schema_version": 1,
  "data_version": 1,
  "items": [],
  "categories": [],
  "difficulties": [],
  "pictures": [],
  "picture_difficulties": [],
  "unlock_requirements": [],
  "drop_tables": [],
  "drop_table_items": [],
  "picture_assets": []
}
```

Sau này editor có thể export:

- một file lớn: dễ load
- nhiều file nhỏ: dễ quản lý

Nhưng DTO root nên có dạng tổng hợp.

---

## 16. JSON → DTO → Validator → Mapper → Model → Repository

Đã chốt chọn pipeline:

```text
JSON source
   ↓
DTO / Raw Data
   ↓
Validate
   ↓
Convert to Runtime Model
   ↓
Build Index / Repository
   ↓
Gameplay Service sử dụng
```

Pipeline chính thức:

```text
1. Load JSON
2. Deserialize JSON → DTO
3. Apply default values cho field phụ
4. Validate required fields
5. Validate enum values
6. Validate cross-reference IDs
7. Validate unlock/deadlock
8. Convert DTO → Runtime Model
9. Build Repository Index
10. Gameplay Service dùng Repository + Player Save
```

Gameplay không dùng JSON trực tiếp.

---

## 17. DTO

DTO là lớp nhận dữ liệu thô từ JSON/API. DTO nên bám sát JSON nhất có thể.

Ví dụ `ItemDto` trong C#:

```csharp
public class ItemDto
{
    public int id;
    public string id_string;
    public string display_name;
    public string description;
    public string display_name_key;
    public string description_key;
    public string item_type;
    public string rarity;
    public bool is_consumable;
    public bool is_time_limited;
    public string status;
    public int sort_order;
    public string created_at;
    public string updated_at;
}
```

DTO không cần thông minh. Nó chỉ nhận raw data.

---

## 18. Runtime Model

Runtime model là object sạch để gameplay dùng. Model nên gần như read-only sau khi load xong static data.

Ví dụ `ItemData`:

```csharp
public class ItemData
{
    public int Id { get; }
    public string IdString { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string DisplayNameKey { get; }
    public string DescriptionKey { get; }
    public ItemType ItemType { get; }
    public ItemRarity Rarity { get; }
    public bool IsConsumable { get; }
    public bool IsTimeLimited { get; }
    public DataStatus Status { get; }
    public int SortOrder { get; }
}
```

Static model không nên lưu state của người chơi. Ví dụ không lưu `amount` trong `ItemData`.

Số lượng item của người chơi nằm trong player save/inventory.

---

## 19. Repository / Index

Sau khi convert model xong, build dictionary để tìm nhanh.

Ví dụ:

```csharp
public class GameDataRepository
{
    private Dictionary<int, ItemData> itemsById;
    private Dictionary<int, PictureData> picturesById;
    private Dictionary<int, CategoryData> categoriesById;

    public ItemData GetItemById(int id)
    {
        return itemsById[id];
    }

    public PictureData GetPictureById(int id)
    {
        return picturesById[id];
    }
}
```

Gameplay chỉ gọi repository/service, không đọc JSON trực tiếp.

---

## 20. Data Source

Vì local JSON và remote API tạm thời cùng format, nên có interface chung:

```csharp
public interface IGameDataSource
{
    Task<GameStaticDataDto> LoadAsync();
}
```

Hiện tại:

```text
LocalJsonDataSource
```

Sau này nếu có backend:

```text
RemoteApiDataSource
```

Phần pipeline phía sau giữ nguyên:

```csharp
var dto = await dataSource.LoadAsync();
validator.Validate(dto);
var model = mapper.ToModel(dto);
repository.Build(model);
```

---

## 21. Local JSON và Remote API

Tạm thời xem local JSON và remote API là như nhau.

Hiện tại:

```text
Local JSON → DTO → Validator → Mapper → Model → Repository
```

Sau này có backend:

```text
Remote API JSON → DTO → Validator → Mapper → Model → Repository
```

Vì cùng schema, gameplay không cần biết data đến từ local hay remote.

---

## 22. Xử lý khi data lỗi

Đã chốt:

| Môi trường | Cách xử lý |
|---|---|
| Editor / Dev build | Fail fast, báo lỗi rõ, không cho export hoặc không cho chạy |
| Production | Fallback về data local ổn định gần nhất |
| Remote data lỗi sau này | Không apply remote data lỗi, giữ data hiện tại |
| Lỗi nhẹ | Log warning nếu không ảnh hưởng gameplay |
| Lỗi nặng | Chặn load data hoặc fallback |

Ví dụ lỗi nặng:

- `id` trùng.
- `id_string` trùng.
- Item reward tham chiếu item không tồn tại.
- Picture thiếu category.
- Unlock bị deadlock.
- Thiếu field bắt buộc.
- Enum string không hợp lệ.

---

## 23. Editor Validation

Editor/validator phải kiểm tra trước khi export data.

Validation tối thiểu:

- Không trùng `id`.
- Không trùng `id_string`.
- Không thiếu `display_name`.
- Không thiếu `description`.
- Enum string phải hợp lệ.
- Các ID liên kết phải tồn tại.
- Không dùng item `deprecated` trong content mới.
- Không dùng picture/category/difficulty `deprecated` trong content mới.
- Drop table không tham chiếu item sai.
- Unlock requirement không tham chiếu item sai.
- Unlock không bị deadlock.
- `picture_difficulties.piece_count` phải bằng `grid_columns * grid_rows`.
- Grid puzzle phải hợp lý với `picture_assets.aspect_ratio`.
- MVP chỉ dùng `piece_shape_type = "rectangle"` và `allow_rotation = false`.
- `difficulty_unlock_policy` phải là enum hợp lệ.
- Item event nếu `is_time_limited = true` thì phải có event/rule phù hợp khi dùng.
- Field bắt buộc thiếu thì báo lỗi.
- Field phụ thiếu thì apply default value.
- Không dùng item tiêu hao để khóa progression chính nếu rule không cho phép.
- Không lưu hoặc tiêu hao star như currency.

---

## 24. Save Data

Dynamic/player save data phải tách khỏi static data.

Save data nên lưu bằng `id`, không lưu bằng `id_string`.

Ví dụ inventory:

```json
{
  "item_id": 101,
  "amount": 1
}
```

Không nên lưu:

```json
{
  "item_id_string": "banana_tree",
  "amount": 1
}
```

### 24.1 First Clear theo picture + difficulty

Vì mỗi độ khó có thể có reward/key item riêng, save data cần lưu theo cặp:

```json
{
  "picture_id": 1,
  "difficulty_id": 1,
  "first_clear_claimed": true
}
```

Không chỉ lưu theo `picture_id`.

### 24.2 Drop decay lưu trong save data

Drop table static lưu rule:

```json
{
  "base_rate": 0.6,
  "decay_per_success": 0.1,
  "min_rate": 0.2,
  "reset_rule": "local_date_daily"
}
```

Số lần item đã rơi hôm nay phải lưu trong save data:

```json
{
  "item_id": 201,
  "success_count_today": 2,
  "last_reset_date": "2026-06-10"
}
```

MVP tính drop decay theo `item_id` trên toàn game trong ngày. Nếu item `201` rơi từ bất kỳ drop table nào, `success_count_today` của item `201` đều tăng chung. Không key save state bằng `drop_table_id + item_id`.

### 24.3 Save Version

Hiện tại chưa cần migration phức tạp, nhưng player save nên có tối thiểu:

```json
{
  "save_version": 1
}
```

### 24.4 Star / Achievement Progress

Star không nằm trong inventory. Save data lưu `best_star` theo cặp `picture_id + difficulty_id`.

Ví dụ:

```json
{
  "picture_id": 1,
  "difficulty_id": 3,
  "best_star": 3,
  "best_time_seconds": 420
}
```

Tổng star được tính bằng cách cộng `best_star` của toàn bộ progress record. Nếu unlock cần sao, dùng requirement kiểu `total_star_at_least`, không trừ sao.

### 24.5 Lưu ý: Các trường đã tạm lược bỏ khỏi Save Data mẫu để khớp Runtime MVP

Dưới đây là các trường đã có trong thiết kế save mẫu ban đầu (`jigsaw_vietnam_player_save_sample_v0_1.json`) nhưng đã được lược bỏ trong bản cập nhật MVP ngày 2026-06-12 do code runtime hiện tại (`PlayerSave.cs`) chưa hỗ trợ:

1. **Thông tin chung của bản Save**:
   - `save_version` (Bản lưu): Phiên bản cấu trúc save (Hiện tại được lưu gián tiếp hoặc bỏ qua ở runtime MVP).
   - `player_id`: ID người chơi cục bộ (Mặc định là người chơi cục bộ).
   - `created_at` / `updated_at`: Thời gian tạo/cập nhật bản save.

2. **Inventory (Kho đồ) nâng cao**:
   - Cấu trúc cũ dạng:
     ```json
     "inventory": [
       { "item_id": 1, "amount": 0 },
       { "item_id": 2, "amount": 0 }
     ]
     ```
   - *Lý do lược bỏ*: Code runtime hiện tại lưu trực tiếp số lượng Coin và Hint qua trường `Coins` và `Hints` dạng số nguyên thẳng trong `PlayerSave`. Các Key Item thu thập được lưu trong danh sách phẳng `OwnedItemIds` (không cần số lượng vì là Key Item duy nhất).

3. **Picture Progress (Tiến trình tranh chính)**:
   - Cấu trúc cũ:
     ```json
     "picture_progress": [
       { "picture_id": 1, "is_unlocked": true, "is_completed_any_difficulty": false, "first_unlocked_at": "...", "last_played_at": "..." }
     ]
     ```
   - *Lý do lược bỏ*: Trạng thái mở khóa tranh và hoàn thành của người chơi hiện tại được suy ra trực tiếp từ logic tuần tự của `CompletedPuzzles` (Tranh 1 xong thì Tranh 2 mở), runtime chưa cần lưu trạng thái khóa/mở hoặc thời gian cụ thể của từng bức tranh riêng biệt để tiết kiệm bộ nhớ.

4. **Picture Difficulty Progress (Tiến trình độ khó chi tiết)**:
   - Cấu trúc cũ:
     ```json
     "picture_difficulty_progress": [
       { "picture_id": 1, "difficulty_id": 1, "is_unlocked": true, "is_completed": false, "first_clear_claimed": false, "best_star": 0, "best_time_seconds": 0 }
     ]
     ```
   - *Lý do lược bỏ*: Runtime MVP gom các thông tin này vào struct gọn nhẹ `CompletedPuzzleData` trong danh sách `CompletedPuzzles` chỉ khi độ khó đó đã được chơi thắng (giảm thiểu lưu trữ những độ khó chưa chơi).

5. **Drop States (Trạng thái rơi vật phẩm hàng ngày)**:
   - Cấu trúc cũ:
     ```json
     "drop_states": [
       { "item_id": 201, "success_count_today": 0, "last_reset_date": "2026-06-10" }
     ]
     ```
   - *Lý do lược bỏ*: Tính năng giới hạn rơi vật phẩm hàng ngày chưa được tích hợp vào core gameplay và save system của MVP.

*Nếu sau này dự án mở rộng và cần tái lập các tính năng này, dev chỉ cần thêm các trường/struct tương ứng vào `PlayerSave.cs`, lập trình logic runtime, rồi cập nhật lại cấu trúc của file save.*

---

## 25. Item Binding Status

Key item và event item nằm trong Item Database riêng. Tranh/reward liên kết tới item bằng `id`.

Các key item hiện tại chỉ là placeholder để test progression, chưa bắt buộc khớp với hình ảnh thật.

Field `item_binding_status` dùng để biết trạng thái liên kết item với art.

Các status đề xuất:

| Status | Ý nghĩa |
|---|---|
| `placeholder` | Item gắn tạm để test progression |
| `art_review_needed` | Cần review lại item sau khi có tranh |
| `art_confirmed` | Item đã khớp với art và được duyệt |
| `needs_rebalance` | Item đúng art nhưng cần cân bằng lại progression |

Ví dụ:

```json
{
  "picture_id": 1,
  "item_binding_status": "placeholder"
}
```

Khi tạo/chọn hình thật, team sẽ chọn lại key item dựa trên vật thể nổi bật trong tranh nếu cần.

---

## 26. Backend sau này

Khi có backend, nguyên tắc vẫn tương tự:

```text
Backend/API JSON
   ↓
DTO
   ↓
Validator
   ↓
Mapper
   ↓
Runtime Model
   ↓
Repository
   ↓
Gameplay Service
```

Nếu backend dùng SQL/PostgreSQL/MySQL, app vẫn chỉ cần quan tâm API response đúng schema/version.

Remote data cần có:

- `schema_version`
- `data_version`
- sau này có thể thêm `content_hash`

Hiện tại chưa cần hash/checksum. Khi có backend/remote config thì bổ sung.

---

## 27. Kết luận hiện tại

Thiết kế dữ liệu hiện tại đã chốt các nguyên tắc chính:

- Static data và player save data tách riêng.
- Hiện tại dùng local JSON, sau này có thể dùng remote API cùng schema.
- Data có thể tách bảng thì tách bảng.
- Tất cả liên kết ưu tiên bằng `id` số.
- `id` và `id_string` cố định vĩnh viễn.
- Không xóa record đã release, dùng `status`.
- Có `display_name`, `description`, localization key và fallback text.
- JSON dùng `snake_case`.
- Enum trong JSON dùng string.
- Có default value cho field phụ.
- Field bắt buộc phải validate.
- Coin/hint là item type `currency`.
- Star là achievement/progress score, không phải item/currency.
- Editor chịu trách nhiệm tạo, setup, validate và export data.
- Pipeline parse chuẩn là JSON → DTO → Validator → Mapper → Runtime Model → Repository.
- Gameplay không dùng JSON trực tiếp.

Bước tiếp theo có thể bắt đầu thiết kế schema/model v0.1 cho từng bảng cụ thể.
