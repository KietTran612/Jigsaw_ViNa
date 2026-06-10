# Game Jigsaw Việt Nam - Tổng hợp luật chơi và hệ thống item

**Phiên bản:** Draft 1  
**Ngày:** 2026-06-09  
**Thể loại:** Jigsaw Puzzle / Collection / Unlock Progression  
**Chủ đề:** Hình ảnh, văn hóa, phong cảnh, món ăn và đời sống Việt Nam

---

## 1. Tóm tắt concept

Game là một trò chơi ghép hình nhiều mảnh, trong đó người chơi hoàn thành các bức tranh về Việt Nam. Mỗi bức tranh có thể chứa các item ẩn thụ động. Khi người chơi ghép xong tranh, hệ thống sẽ tự trao item theo setting của bức tranh và độ khó.

Các item này được dùng để mở khóa tranh mới, mở nội dung bonus, tham gia event, đổi hint, mở postcard, cosmetic hoặc các phần thưởng đặc biệt khác.

Core loop chính:

```text
Chọn tranh -> Chọn độ khó -> Ghép tranh -> Hoàn thành -> Nhận item/reward -> Mở khóa tranh mới -> Tiếp tục khám phá
```

---

## 2. Core gameplay

### 2.1. Gameplay chính

Người chơi chọn một bức tranh, sau đó tranh được cắt thành nhiều mảnh. Người chơi kéo thả các mảnh để ghép lại thành bức tranh hoàn chỉnh.

Khi hoàn thành tranh:

1. Game kiểm tra tranh và độ khó hiện tại.
2. Game trao reward lần đầu nếu người chơi chưa nhận.
3. Game trao reward chơi lại nếu người chơi đã hoàn thành trước đó.
4. Game kiểm tra item mới có đủ để mở khóa tranh khác không.
5. Game cập nhật album, inventory và trạng thái unlock.

---

## 3. Hệ thống tranh

### 3.1. Mỗi tranh có thể có nhiều độ khó

Một bức tranh có thể có nhiều độ khó, ví dụ:

| Độ khó | Grid MVP | Số mảnh MVP | Mục đích |
|---|---:|---:|---|
| Dễ | 6 x 4 | 24 mảnh | Người chơi casual, làm quen |
| Vừa | 8 x 6 | 48 mảnh | Trải nghiệm chính |
| Khó | 12 x 8 | 96 mảnh | Người chơi muốn thử thách, reward tốt hơn |

MVP dùng ảnh tỷ lệ 4:3, mảnh hình chữ nhật, không xoay mảnh. Runtime tự tạo mảnh từ ảnh gốc và cấu hình grid, không setup từng mảnh thủ công trong data.

### 3.2. Mở độ khó theo hybrid policy

MVP dùng cơ chế hybrid:

- 5 tranh đầu mở sẵn khi bắt đầu game.
- 5 tranh đầu mở độ khó theo chuỗi: Dễ -> Vừa -> Khó.
- Tranh sau bị khóa ban đầu.
- Khi người chơi đủ key item và bấm Unlock để mở tranh sau, cả 3 độ khó của tranh đó mở cùng lúc.

Ví dụ:

```text
Tranh onboarding: Làng Quê Việt Nam
Trạng thái: Mở sẵn

Ban đầu:
- Dễ mở
- Vừa khóa, yêu cầu hoàn thành Dễ
- Khó khóa, yêu cầu hoàn thành Vừa

Tranh sau onboarding: Chợ Quê Bắc Bộ
Trạng thái: Đủ key item, người chơi bấm Unlock

Sau khi unlock:
- Dễ mở
- Vừa mở
- Khó mở
```

### 3.3. Mỗi độ khó có setting reward riêng

Mỗi độ khó có thể có:

- key item riêng
- coin riêng
- star achievement riêng
- hint reward riêng
- rate item chance riêng
- reward lần đầu riêng
- reward chơi lại riêng

Ví dụ:

| Tranh | Độ khó | Key item lần đầu | Coin | Sao | Rate item chance |
|---|---|---|---:|---:|---:|
| Làng Quê Việt Nam | Dễ | Cây Chuối | 30 | 1 | 10% |
| Làng Quê Việt Nam | Vừa | Con Gà | 60 | 2 | 15% |
| Làng Quê Việt Nam | Khó | Con Trâu | 120 | 3 | 25% |

---

## 4. Item ẩn trong tranh

### 4.1. Cách hiểu item ẩn

Game chọn cơ chế item ẩn thụ động.

Người chơi không cần tự tìm item bằng mắt trong bức tranh. Chỉ cần hoàn thành tranh, hệ thống sẽ tự kiểm tra và trao item theo setting.

Ví dụ:

```text
Người chơi hoàn thành tranh "Đồng Quê Mùa Gặt" ở độ khó Khó.
Hệ thống kiểm tra reward setting.
Nếu người chơi chưa nhận key item ở độ khó này, hệ thống trao "Con Trâu".
```

### 4.2. Lý do chọn item ẩn thụ động

- Dễ hiểu với người chơi.
- Giữ trọng tâm gameplay là jigsaw puzzle.
- Dễ làm MVP.
- Không biến game thành hidden object game quá sớm.
- Dễ quản lý bằng data setting.

---

## 5. Phân loại item

Game có nhiều loại item, trong đó quan trọng nhất là:

1. Key Item / Item khóa chính
2. Rate Item / Item tỷ lệ
3. Consumable Item / Item tiêu hao
4. Coin / Hint resource

---

## 6. Key Item / Item khóa chính

### 6.1. Vai trò

Key item là item dùng để mở khóa các bức tranh chính trong progression.

Ví dụ key item:

- Cây Chuối
- Con Trâu
- Nón Lá
- Đèn Lồng
- Ghe Thuyền
- Bánh Chưng
- Hoa Sen
- Áo Dài
- Trống Đồng

### 6.2. Luật của key item

Key item có các luật sau:

- Rơi chắc chắn 100% theo setting.
- Chỉ rơi ở lần hoàn thành đầu tiên của tranh hoặc độ khó tương ứng.
- Là item vĩnh viễn.
- Không bị tiêu hao khi dùng để mở khóa tranh.
- Dùng như điều kiện sở hữu.

Ví dụ:

```text
Tranh "Đồng Quê Mùa Gặt" - Độ khó Khó
Reward lần đầu: Con Trâu

Người chơi hoàn thành lần đầu -> nhận Con Trâu 100%.
Con Trâu được lưu vĩnh viễn trong inventory/collection.
Khi mở tranh khác yêu cầu Con Trâu, game chỉ kiểm tra người chơi đã sở hữu Con Trâu hay chưa.
Con Trâu không bị trừ đi.
```

### 6.3. Key item có thể khác nhau theo độ khó

Cùng một tranh có thể rơi các key item khác nhau ở các độ khó khác nhau.

Ví dụ:

| Tranh | Độ khó | Key item |
|---|---|---|
| Làng Quê Việt Nam | Dễ | Cây Chuối |
| Làng Quê Việt Nam | Vừa | Con Gà |
| Làng Quê Việt Nam | Khó | Con Trâu |

Điều này giúp người chơi có lý do chơi các độ khó khác nhau của cùng một bức tranh.

---

## 7. Rate Item / Item tỷ lệ

### 7.1. Vai trò

Rate item là item rơi theo xác suất, dùng cho các nội dung phụ hoặc đặc biệt.

Có thể dùng cho:

- event
- tranh bonus
- tranh special
- pack đặc biệt
- postcard đặc biệt
- cosmetic
- frame
- sticker
- đổi hint

Ví dụ rate item:

- Vé Du Lịch
- Tem Bưu Thiếp
- Mảnh Bản Đồ
- Hoa Mai Hiếm
- Lì Xì Vàng
- Huy Hiệu Hội An
- Mảnh Tranh Đặc Biệt

### 7.2. Luật của rate item

Rate item có các luật sau:

- Rơi theo tỷ lệ.
- Có thể xuất hiện trong reward setting của từng tranh hoặc từng độ khó.
- Có thể là item tiêu hao hoặc item vĩnh viễn tùy setting.
- Không dùng để chặn progression chính quá sớm.
- Phù hợp cho event, bonus, special và resource phụ.

### 7.3. Không nên dùng rate item để khóa đường chính

Không nên thiết kế tranh chính yêu cầu item có tỷ lệ rơi thấp, ví dụ:

```text
Sai hướng:
Mở tranh chính số 8 cần Lì Xì Vàng, mà Lì Xì Vàng chỉ rơi 5%.
```

Lý do: người chơi có thể bị kẹt progression vì xui.

Thiết kế khuyên dùng:

```text
Tranh chính -> yêu cầu key item vĩnh viễn, rơi chắc chắn.
Tranh bonus/event/special -> có thể yêu cầu rate item hoặc consumable item.
```

---

## 8. Consumable Item / Item tiêu hao

### 8.1. Vai trò

Consumable item là item có thể bị trừ đi khi sử dụng.

Item tiêu hao không dùng để khóa progression chính.

### 8.2. Mục đích sử dụng

Item tiêu hao dùng cho:

- mở tranh bonus
- mở tranh event
- đổi hint
- mở postcard đặc biệt
- mở pack tranh đặc biệt
- đổi cosmetic
- đổi frame
- đổi sticker
- tham gia event giới hạn thời gian

Ví dụ:

```text
Mở tranh bonus "Hội An Đêm Trăng":
- tiêu hao 3 Vé Du Lịch
- tiêu hao 2 Tem Bưu Thiếp
```

### 8.3. Không dùng item tiêu hao để mở tranh chính

Tranh chính nên yêu cầu sở hữu key item vĩnh viễn, không trừ item.

Lý do:

- Tránh cảm giác mất công sức.
- Giữ progression chính thân thiện.
- Giảm cảm giác grind khó chịu.

---

## 9. Coin và Hint Resource

### 9.1. Coin

Coin là tài nguyên cơ bản, có thể dùng cho:

- mua hint
- mở một số nội dung phụ
- mua cosmetic đơn giản
- đổi vật phẩm nhỏ

Coin có thể farm không giới hạn khi chơi lại tranh.

### 9.2. Hint Resource

Hint dùng để hỗ trợ người chơi khi ghép hình.

Các dạng hint có thể có:

- tự đặt đúng một mảnh
- làm sáng vùng đúng của một mảnh
- hiện ảnh mẫu rõ hơn
- gom các mảnh cạnh viền
- highlight mảnh đang cần tìm

Hint hoặc hint shard có thể nhận khi chơi lại tranh.

Hint resource có thể farm không giới hạn, tùy balancing.

---

## 10. Reward lần đầu

### 10.1. Reward lần đầu theo tranh và độ khó

Mỗi tranh và mỗi độ khó có reward lần đầu riêng.

Reward lần đầu có thể bao gồm:

- key item 100%
- coin
- star achievement
- hint
- rate item chance

Ví dụ:

```text
Tranh: Làng Quê Việt Nam
Độ khó: Khó

Reward lần đầu:
- Con Trâu: 100%
- 120 coin
- 3 star achievement
- Mảnh Bản Đồ: 25%
```

### 10.2. Key item chỉ nhận một lần

Nếu người chơi đã nhận key item của tranh/độ khó đó, khi chơi lại sẽ không nhận lại key item.

---

## 11. Reward chơi lại tranh cũ

### 11.1. Luật chơi lại

Khi chơi lại tranh cũ:

- Không rơi lại key item đã nhận.
- Vẫn có thể nhận coin.
- Vẫn có thể nhận hint hoặc hint shard.
- Vẫn có thể nhận rate item nếu setting cho phép.
- Coin/hint không giới hạn.

### 11.2. Mục đích của chơi lại

Chơi lại tranh cũ giúp:

- farm coin
- farm hint
- farm rate item event/special
- hoàn thành tranh ở độ khó khác
- lấy key item của độ khó khác nếu chưa nhận
- tăng sao hoặc hoàn thiện album

---

## 12. Rate item decay

### 12.1. Cơ chế giảm tỷ lệ rơi

Với item giới hạn hoặc item event, người chơi có thể chơi lại bao nhiêu cũng được. Tuy nhiên, mỗi lần item đó rơi thành công trong ngày, tỷ lệ rơi sẽ giảm dần.

Tỷ lệ sẽ không giảm thấp hơn mức sàn.

Ví dụ:

| Số lần item đã rơi trong ngày | Tỷ lệ rơi hiện tại |
|---:|---:|
| 0 lần | 60% |
| 1 lần | 45% |
| 2 lần | 35% |
| 3 lần | 25% |
| 4+ lần | 20% |

### 12.2. Công thức gợi ý

```text
drop_rate = max(min_rate, base_rate - drop_count * decay_rate)
```

Ví dụ:

```text
base_rate = 60%
decay_rate = 10%
min_rate = 20%
```

Nếu item đã rơi 3 lần trong ngày:

```text
drop_rate = max(20%, 60% - 3 * 10%)
drop_rate = 30%
```

### 12.3. Reset mỗi ngày

Rate item decay reset mỗi ngày theo local date trong MVP.

Sang ngày mới:

- drop_count của item trong ngày được reset
- tỷ lệ rơi quay lại base_rate

Drop count tính theo từng item trên toàn game trong ngày, không tính riêng theo từng drop table. Nếu `postcard_stamp` rơi ở bất kỳ tranh hoặc độ khó nào, count ngày của `postcard_stamp` đều tăng chung.

Ví dụ:

```text
Ngày 1:
Mảnh Bản Đồ đã rơi 4 lần -> tỷ lệ còn 20%.

Ngày 2:
Mảnh Bản Đồ reset -> tỷ lệ quay lại 60%.
```

---

## 13. Unlock tranh

### 13.1. Tranh chính

Tranh chính được mở khóa bằng key item vĩnh viễn.

Người chơi cần sở hữu đủ key item yêu cầu, sau đó bấm Unlock ở màn chọn tranh. Item không bị tiêu hao.

Ví dụ:

```text
Tranh: Chợ Quê Bắc Bộ
Yêu cầu mở khóa:
- Cây Chuối
- Con Trâu
- Nón Lá

Người chơi đã sở hữu đủ 3 item -> card tranh hiện Ready to Unlock.
Người chơi bấm Unlock -> tranh mở khóa.
Sau khi mở khóa, 3 item vẫn còn trong collection.
```

### 13.2. Tranh bonus / event / special

Tranh bonus, event hoặc special có thể yêu cầu:

- consumable item
- rate item
- coin
- tổng sao tối thiểu, không tiêu hao sao
- điều kiện hoàn thành tranh khác

Ví dụ:

```text
Tranh bonus: Hội An Đêm Trăng
Yêu cầu mở khóa:
- 3 Vé Du Lịch, tiêu hao
- 2 Tem Bưu Thiếp, tiêu hao
- đã hoàn thành tranh Phố Cổ Hội An
```

### 13.3. Nguyên tắc mở khóa

- Tranh chính: dùng key item vĩnh viễn, bấm Unlock, không tiêu hao.
- Tranh phụ/special/event: có thể dùng consumable item.
- Không để item tỷ lệ thấp khóa progression chính.

---

## 14. UI chọn tranh

### 14.1. UI theo grid tranh và difficulty cards

MVP dùng màn chọn tranh dạng grid. Mỗi card tranh hiển thị thumbnail, tên tranh, trạng thái locked/unlocked/ready-to-unlock và trạng thái completed.

Khi người chơi click vào một tranh đã mở khóa, UI hiển thị panel với 3 difficulty cards.

Ví dụ:

```text
Tranh: Làng Quê Việt Nam
Cards: Dễ | Vừa | Khó
```

Mỗi difficulty card hiển thị:

- số mảnh
- trạng thái đã hoàn thành hay chưa
- trạng thái difficulty đã mở hay chưa
- key item lần đầu
- key item đã nhận hay chưa
- coin reward
- star achievement reward
- hint reward
- rate item có thể rơi
- tỷ lệ rơi hiện tại
- best time nếu đã hoàn thành

### 14.2. Ví dụ UI difficulty card

```text
Tranh: Làng Quê Việt Nam
Card: Khó - 96 mảnh

Thưởng lần đầu:
- Con Trâu - Key Item - 100% - Chưa nhận
- 120 coin
- 3 star achievement

Chơi lại:
- 40 coin
- 3 hint
- Mảnh Bản Đồ - 25% hiện tại
```

### 14.3. Hiển thị item có thể rơi

Game chọn kết hợp:

- Mức 1: hiển thị rõ item có thể rơi khi chọn tranh.
- Mức 2: hiển thị gợi ý khi người chơi thiếu item.

Khi click vào tranh, người chơi thấy rõ:

- item chắc chắn rơi lần đầu
- item tỷ lệ có thể rơi
- tỷ lệ rơi
- item đã nhận hay chưa
- độ khó nào rơi item nào

---

## 15. Gợi ý khi thiếu item

Khi người chơi cố mở một tranh nhưng thiếu item, game hiển thị rõ item còn thiếu và gợi ý nơi kiếm.

Ví dụ 1, gợi ý theo chủ đề:

```text
Bạn còn thiếu Con Trâu.
Gợi ý: hãy hoàn thành các tranh thuộc chủ đề Đồng Quê.
```

Ví dụ 2, gợi ý trực tiếp:

```text
Bạn còn thiếu Con Trâu.
Có thể tìm thấy trong tranh: Đồng Quê Mùa Gặt.
```

Gợi ý có thể chia theo giai đoạn:

- Giai đoạn đầu: nói rõ tranh cụ thể để người chơi dễ hiểu.
- Giai đoạn sau: gợi ý theo chủ đề để giữ cảm giác khám phá.

---

## 16. Data-driven design

Hệ thống nên được thiết kế theo data setting, tránh hard-code.

### 16.1. Picture data gợi ý

```json
{
  "id": 1,
  "id_string": "village_rice_field",
  "display_name": "Đồng Quê Mùa Gặt",
  "description": "Một bức tranh về mùa gặt ở làng quê Việt Nam.",
  "category_id": 1,
  "is_initially_unlocked": true,
  "difficulty_unlock_policy": "sequential",
  "item_binding_status": "placeholder",
  "status": "active",
  "sort_order": 10
}
```

Setting theo tranh + độ khó nên nằm ở bảng riêng như `picture_difficulties`:

```json
{
  "id": 10003,
  "id_string": "village_rice_field_hard",
  "display_name": "Đồng Quê Mùa Gặt - Khó",
  "picture_id": 1,
  "difficulty_id": 3,
  "grid_columns": 12,
  "grid_rows": 8,
  "piece_count": 96,
  "piece_shape_type": "rectangle",
  "allow_rotation": false,
  "first_clear_reward_item_ids": [103],
  "first_clear_coin": 120,
  "replay_coin": 40,
  "drop_table_id": 1003,
  "status": "active",
  "sort_order": 30
}
```

### 16.2. Item data gợi ý

```json
{
  "id": 103,
  "id_string": "water_buffalo",
  "display_name": "Con Trâu",
  "description": "Biểu tượng quen thuộc của làng quê Việt Nam.",
  "item_type": "key_item",
  "rarity": "common",
  "is_consumable": false,
  "is_time_limited": false,
  "status": "active"
}
```

Ví dụ item tiêu hao:

```json
{
  "id": 201,
  "id_string": "travel_ticket",
  "display_name": "Vé Du Lịch",
  "description": "Dùng để mở các bức tranh du lịch đặc biệt.",
  "item_type": "consumable",
  "rarity": "rare",
  "is_consumable": true,
  "is_time_limited": false,
  "status": "active"
}
```

---

## 17. Trạng thái cần lưu của người chơi

Game cần lưu các dữ liệu sau:

### 17.1. Trạng thái tranh

Theo từng tranh và từng độ khó:

- đã mở khóa chưa
- đã hoàn thành chưa
- đã nhận first clear reward chưa
- key item của độ khó đó đã nhận chưa
- số sao cao nhất
- thời gian hoàn thành tốt nhất, nếu có

Ví dụ:

```json
{
  "picture_id": 1,
  "difficulty_id": 3,
  "is_completed": true,
  "first_clear_claimed": true,
  "best_star": 3,
  "best_time_seconds": 420
}
```

### 17.2. Inventory

Cần lưu:

- item vĩnh viễn đã sở hữu
- item tiêu hao và số lượng
- coin
- hint

Sao không nằm trong inventory. `best_star` lưu theo từng tranh + độ khó, tổng sao được tính từ các `best_star` đã lưu.

Ví dụ:

```json
{
  "inventory": [
    {
      "item_id": 1,
      "amount": 1250
    },
    {
      "item_id": 2,
      "amount": 18
    },
    {
      "item_id": 101,
      "amount": 1
    },
    {
      "item_id": 103,
      "amount": 1
    },
    {
      "item_id": 201,
      "amount": 3
    }
  ]
}
```

### 17.3. Rate item daily state

Cần lưu số lần item giới hạn đã rơi trong ngày để tính decay.

Ví dụ:

```json
{
  "date": "2026-06-09",
  "drop_counts": {
    "201": 3,
    "202": 1
  }
}
```

Sang ngày mới, hệ thống reset drop count.

---

## 18. Balancing guideline

### 18.1. Progression chính

- Không để progression chính phụ thuộc vào RNG thấp.
- Key item mở tranh chính phải rơi chắc chắn.
- 5 tranh đầu nên dễ mở để người chơi hiểu hệ thống.
- Giai đoạn đầu chỉ nên yêu cầu 1-2 key item.
- Giai đoạn sau có thể yêu cầu 3-5 key item.

### 18.2. Rate item

- Dùng cho nội dung phụ, event, bonus, cosmetic.
- Có thể giảm tỷ lệ sau mỗi lần rơi.
- Reset decay theo local date trong MVP.
- Cần hiển thị rõ tỷ lệ hiện tại nếu muốn minh bạch.

### 18.3. Độ khó

- Độ khó cao nên có reward tốt hơn.
- Độ khó cao có thể rơi key item riêng.
- Độ khó cao có thể tăng coin, star achievement và rate item chance.
- 5 tranh đầu dùng Dễ -> Vừa -> Khó để onboarding.
- Tranh sau mở cả 3 độ khó sau khi người chơi đủ key item và bấm Unlock.

---

## 19. MVP đề xuất

Cho bản MVP đầu tiên, nên có:

- 20 bức tranh
- 3 độ khó: Dễ, Vừa, Khó
- 5 bức tranh mở sẵn ban đầu
- 15 bức tranh mở bằng key item
- 5 tranh đầu mở độ khó theo chuỗi Dễ -> Vừa -> Khó
- Tranh sau mở cả 3 độ khó sau khi người chơi bấm Unlock
- 4:3 landscape-first
- 24 / 48 / 96 mảnh cho Dễ / Vừa / Khó
- key item rơi chắc chắn lần đầu theo tranh/độ khó
- item vĩnh viễn cho progression chính
- item tiêu hao cho bonus/event
- coin/hint reward khi chơi lại
- rate item với decay reset theo local date
- UI chọn tranh dạng grid và difficulty cards
- UI hiển thị item có thể rơi
- gợi ý khi thiếu item

Chưa cần trong MVP:

- server
- multiplayer
- leaderboard phức tạp
- event quá nhiều lớp
- daily quest phức tạp
- hệ thống shop lớn

---

## 20. Rule summary ngắn gọn

```text
Game là jigsaw puzzle về Việt Nam.
Mỗi tranh có nhiều độ khó.
MVP dùng ảnh 4:3, landscape-first, mảnh chữ nhật không xoay.
Độ khó MVP là 24 / 48 / 96 mảnh cho Dễ / Vừa / Khó.
5 tranh đầu mở sẵn và mở độ khó theo chuỗi Dễ -> Vừa -> Khó.
Tranh sau mở bằng key item; sau khi người chơi bấm Unlock thì mở cả 3 độ khó.
Hoàn thành tranh sẽ nhận reward theo setting.
Key item rơi chắc chắn lần đầu, là item vĩnh viễn, dùng mở tranh chính và không tiêu hao.
Mỗi độ khó có thể có key item riêng.
Rate item rơi theo tỷ lệ, dùng cho event, bonus, special, pack, postcard, cosmetic hoặc hint.
Rate item giới hạn có tỷ lệ giảm dần theo số lần item đó rơi trong ngày trên toàn game, reset theo local date.
Item tiêu hao chỉ dùng cho nội dung phụ, không khóa progression chính.
Chơi lại tranh không rơi lại key item đã nhận, nhưng vẫn nhận coin, hint và rate item nếu setting cho phép.
UI chọn tranh dùng grid tranh và difficulty cards, cho thấy rõ item có thể rơi.
Nếu thiếu item mở khóa, game hiển thị item còn thiếu và gợi ý nơi kiếm.
```

---

## 21. Các quyết định đã chốt

| Chủ đề | Quyết định |
|---|---|
| Loại game | Jigsaw puzzle nhiều mảnh |
| Chủ đề | Việt Nam |
| Item ẩn | Thụ động, rơi sau khi hoàn thành tranh |
| Key item | Vĩnh viễn, rơi chắc chắn lần đầu |
| Key item dùng để | Mở khóa tranh chính |
| Key item có tiêu hao không | Không |
| Rate item | Rơi theo tỷ lệ, tùy setting |
| Rate item dùng cho | Event, special, bonus, pack, postcard, cosmetic, hint |
| Item tiêu hao | Có, nhưng chỉ dùng cho nội dung phụ |
| Chơi lại tranh cũ | Nhận coin/hint/rate item, không nhận lại key item |
| Rate item giới hạn | Rơi xong thì tỷ lệ thấp dần |
| Tỷ lệ thấp nhất | Có min rate, ví dụ 20% |
| Reset decay | Reset theo local date |
| Phạm vi decay | Theo từng item trên toàn game trong ngày |
| Độ khó | Hybrid: 5 tranh đầu Dễ -> Vừa -> Khó, tranh sau mở cả 3 độ khó sau khi Unlock |
| Item theo độ khó | Mỗi độ khó có thể rơi key item khác nhau |
| UI item | Hiển thị theo difficulty cards |
| Hiển thị item rơi | Cho người chơi xem rõ khi chọn tranh |
| Gợi ý thiếu item | Có, hiển thị item còn thiếu và nơi kiếm |

---

## 22. Việc nên làm tiếp theo

Bước tiếp theo nên thiết kế bảng nội dung đầu tiên:

1. Danh sách 20 bức tranh đầu tiên.
2. Chủ đề của từng tranh.
3. Tranh nào mở sẵn ban đầu.
4. Điều kiện mở khóa từng tranh.
5. Key item rơi theo từng độ khó.
6. Coin/star achievement/hint reward theo từng độ khó.
7. Rate item có thể rơi.
8. Gợi ý hiển thị khi thiếu item.
