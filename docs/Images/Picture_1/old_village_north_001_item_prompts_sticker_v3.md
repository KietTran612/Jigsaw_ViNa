# old_village_north_001 — Prompt tách / generate item riêng

> Mục tiêu: dùng mockup `old_village_north_001.png` làm reference style và reference góc nhìn, sau đó generate từng item riêng thành asset độc lập có viền trắng kiểu sticker.
>
> Không tạo background trong bước này. Background sẽ xử lý riêng sau.

---

## Cập nhật v3 — Sticker border

- Toàn bộ item phải có **viền trắng dày kiểu sticker** bao quanh silhouette ngoài cùng.
- Với item bị che/cắt, phải **hoàn thiện item trước**, sau đó mới thêm viền sticker quanh object hoàn chỉnh.
- Không áp dụng sticker border cho background; background sẽ xử lý ở file riêng.

---

## 0. Rule chung cho tất cả item

Dùng các rule này cho mọi prompt bên dưới.


Use the provided mockup image as the only visual reference. Generate one isolated 2D game asset only. Preserve the original item's viewing angle, orientation, perspective, proportions, hand-painted Vietnamese old village style, warm sunlight, natural color palette, and painterly texture as faithfully as possible. Keep the item as a complete standalone object. Add a thick clean white sticker-style border around the entire outer silhouette of the completed asset. The border must be smooth, closed, clearly visible, and consistent in thickness, sitting outside the object without covering important details. Transparent background outside the sticker border, clean edges, centered composition, high resolution. No text, no UI, no extra objects, no full scene background.


### Rule sticker border bắt buộc cho tất cả item

Dùng cho toàn bộ item trong file này. Background sẽ xử lý riêng sau nên không áp dụng rule này cho background.


Add a thick clean white sticker-style border around the entire outer silhouette of the completed asset. The border must be smooth, closed, clearly visible, and consistent with the cozy hand-drawn / painterly game asset style. The border should sit outside the object and must not cover important object details. The final result must look like a standalone sticker asset, not a plain cutout. Use a transparent background outside the sticker border.


### Rule bổ sung cho item bị che khuất / bị cắt mép

Dùng thêm câu này cho các item bị che, bị cắt ở mép ảnh, bị bóng tối che, hoặc bị dính nền.


If any part of the item is hidden, cropped, occluded, cut off by the image edge, or covered by surrounding objects, reconstruct and complete the full object naturally based on the visible design. The completed parts must match the original shape, material, color, lighting, perspective, and painterly old Vietnamese village style. After the item is completed, add the thick clean white sticker-style border around the completed outer silhouette. Do not place the border around an incomplete cropped fragment.

### Negative prompt chung


full background, full scene, sky, brick courtyard, pond, path, extra props, extra plants, extra characters, UI, text, watermark, logo, realistic photo, 3D render, low resolution, blurry edges, messy cutout, incorrect perspective, front-only view, top-down view, different camera angle, black outline, missing sticker border, borderless cutout, broken white border, incomplete border, border around cropped fragment, cast shadow on ground


> Ghi chú: Tất cả item trong file này phải là **standalone sticker asset**: object hoàn chỉnh + viền trắng dày sạch + nền ngoài viền trong suốt. Background sẽ làm riêng, không dùng viền sticker.

---

# A. Công trình / kiến trúc lớn

## 01. House_Vietnamese_OldVillage_LeftAngle_01

**Mô tả:** Nhà làng chính bên trái, mái ngói đỏ nâu, tường vữa cũ, cửa gỗ, hiên nhà, góc nhìn nghiêng từ trái sang phải. Nhà bị cắt nhẹ ở mép trái nên cần hoàn thiện.


Generate an isolated 2D game asset of the main old Vietnamese countryside house from the reference image. Preserve the exact left-side viewing angle and orientation: a long rustic house with a reddish-brown clay tile roof, aged yellow plaster walls, wooden shutters, wooden front doors, a small shaded porch, stone steps, and old rural details. Keep the same warm sunlit painterly style and natural Vietnamese village atmosphere. Include the house as one complete standalone building asset, including its porch and visible structural parts. If any part of the house is cropped by the image edge or hidden by nearby objects, reconstruct and complete it naturally while keeping the same architecture, roof angle, material, lighting, and perspective. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No courtyard ground, no trees, no bicycle, no jars, no extra plants, no sky, no text, no UI.


---

## 02. BackgroundHouse_RedTile_RightAngle_01

**Mô tả:** Nhà nhỏ mái đỏ ở xa bên phải, một phần bị cây chuối che. Đây là item phụ/trang trí xa.


Generate an isolated 2D game asset of a small distant Vietnamese village house with a red clay tile roof, based on the small background house visible on the right side of the reference image. Preserve its small scale feeling, right-side village placement, warm painterly style, and partially angled roof orientation. Complete any parts hidden by banana leaves or background foliage so it becomes a clean standalone small house prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No banana tree, no path, no wall, no sky, no full background, no text.


---

## 03. Haystack_Hut_BackRight_01

**Mô tả:** Mái rơm / đống rơm lớn phía sau cổng gạch, bên phải bụi tre.


Generate an isolated 2D game asset of the large golden haystack / thatched straw hut visible behind the brick gate in the reference image. Preserve the same warm golden straw color, rounded mound shape, rural Vietnamese village style, and original viewing angle. Complete any hidden lower or side parts naturally if they are blocked by the gate, plants, or nearby wall. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No brick gate, no banana tree, no path, no house, no sky, no extra background.


---

# B. Cây lớn / cụm thực vật lớn

## 04. BambooCluster_Large_Center_01

**Mô tả:** Cụm tre lớn ở giữa ảnh, thân tre cao, lá xanh dày, silhouette rõ.


Generate an isolated 2D game asset of the large bamboo cluster from the center of the reference image. Preserve the tall dense green bamboo stalks, layered bamboo leaves, natural sunlight, painterly texture, and the same slightly upward village perspective. The cluster should feel full, vertical, lush, and complete as a standalone plant asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No house, no wall, no gate pillars, no path, no sky background, no extra props.


---

## 05. BananaTree_RightAngle_01

**Mô tả:** Cây chuối lớn bên phải, lá bị cắt ở mép phải nên cần hoàn thiện.


Generate an isolated 2D game asset of the large banana tree from the right side of the reference image. Preserve the original right-side orientation, broad green banana leaves, tropical Vietnamese garden feeling, warm sunlight, and painterly foliage texture. If any leaves or trunk parts are cut off by the image edge or hidden behind the wall, reconstruct and complete the full banana tree naturally while matching the same angle and lighting. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No brick wall, no path, no house, no bamboo, no sky, no extra plants.


---

## 06. TreeCanopy_TopLeft_01

**Mô tả:** Tán cây xanh góc trên trái, có thể dùng làm decor foreground/background.


Generate an isolated 2D game asset of the leafy green tree canopy from the top-left area of the reference image. Preserve the sunlit dense foliage, soft painterly leaves, warm rural garden style, and partial overhanging canopy feeling. Complete any cropped edges naturally so the canopy works as a standalone foliage decor asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, high resolution. No roof, no house, no sky, no bamboo, no full background.


---

## 07. TreeCluster_Background_Center_01

**Mô tả:** Cụm cây xanh xa phía sau bụi tre.


Generate an isolated 2D game asset of a distant green tree cluster based on the background foliage behind the bamboo in the reference image. Preserve the soft painterly Vietnamese countryside style, rounded leafy shapes, warm daylight, and background-depth feeling. Keep it as a standalone foliage cluster asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No bamboo, no house, no sky, no path, no extra scene.


---

# C. Đồ vật sân vườn / nông thôn

## 08. Bicycle_OldVillage_LeftFacing_01

**Mô tả:** Xe đạp cũ tựa trước hiên nhà, có giỏ phía trước, góc nhìn rõ.


Generate an isolated 2D game asset of the old bicycle leaning in front of the porch from the reference image. Preserve the original left-facing side orientation, thin metal frame, front basket, two wheels, handlebar, seat, rustic village feeling, warm lighting, and painterly style. Make it a complete standalone bicycle prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No house wall, no porch floor, no door, no plants, no extra props, no text.


---

## 09. WaterJar_Large_Foreground_01

**Mô tả:** Chum nước lớn tiền cảnh góc dưới trái, miệng chum có nước phản chiếu.


Generate an isolated 2D game asset of the large brown ceramic water jar from the foreground of the reference image. Preserve its large rounded shape, dark brown glazed ceramic material, water visible at the top, strong foreground scale, warm highlights, and painterly rural Vietnamese style. Keep the original front-left viewing angle. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No basin, no potted plant, no metal scoop, no courtyard, no extra objects.


---

## 10. WaterJar_Partial_LeftEdge_01

**Mô tả:** Chum/vại ở mép trái dưới, bị cắt nhiều nên cần hoàn thiện.


Generate an isolated 2D game asset of the partially visible brown water jar at the lower-left edge of the reference image. Preserve the same old ceramic material, rounded jar form, dark brown color, painterly highlights, and rural village style. Since the original jar is cropped by the image edge, reconstruct and complete the full jar naturally so it becomes a complete standalone prop with the same perspective and lighting. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No courtyard, no leaves, no large foreground jar, no basin, no extra props.


---

## 11. WaterBasin_Round_Foreground_01

**Mô tả:** Bồn/chậu tròn tiền cảnh chứa nước, có vật che phía trước. Nên tạo bồn hoàn chỉnh.


Generate an isolated 2D game asset of the round shallow water basin from the foreground of the reference image. Preserve the circular metal or ceramic basin shape, dark rim, reflective water surface, low front perspective, warm sunlight, and painterly village style. If the basin is partly covered by the potted plant or metal scoop, reconstruct the hidden parts naturally so the basin is complete. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No large jar, no potted plant, no scoop, no courtyard ground, no extra objects.


---

## 12. MetalScoop_Foreground_01

**Mô tả:** Ca/gáo kim loại nhỏ nằm ở bồn nước tiền cảnh.


Generate an isolated 2D game asset of the small metal water scoop from the foreground of the reference image. Preserve its small silver metal cup shape, short handle, angled resting position, warm reflected light, and painterly rural style. Complete any hidden contact edges naturally. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No water basin, no jar, no plant, no courtyard, no extra props.


---

## 13. PottedPlant_RoundLeaves_Foreground_01

**Mô tả:** Chậu cây nhỏ lá tròn ở tiền cảnh.


Generate an isolated 2D game asset of the small potted plant with round green leaves from the foreground of the reference image. Preserve the original small terracotta pot, round leafy plant shape, warm sunlight, painterly texture, and front foreground perspective. Complete the pot and leaves if any parts are hidden by the basin edge. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No basin, no jar, no scoop, no courtyard, no extra plants.


---

## 14. PottedPlant_Terracotta_Porch_01

**Mô tả:** Chậu cây đất nung trước hiên nhà, gần cột hiên.


Generate an isolated 2D game asset of the terracotta potted plant near the porch in the reference image. Preserve the warm clay pot, small green plant, sunlit painterly texture, and original porch-side viewing angle. Make it a complete standalone decor prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No house wall, no porch column, no courtyard, no large bamboo, no extra objects.


---

## 15. ClayJar_Tall_CenterRight_01

**Mô tả:** Bình gốm cao cạnh bụi tre, khác với chum nước lớn nên là asset riêng.


Generate an isolated 2D game asset of the tall brown clay jar near the bamboo cluster in the reference image. Preserve its slim vertical ceramic shape, warm brown color, subtle highlights, painterly old village texture, and original center-right viewing angle. Complete any hidden lower edge naturally if covered by flowers or foliage. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No bamboo, no flowers, no fence, no path, no extra props.


---

## 16. PottedPlant_IndoorPorch_01

**Mô tả:** Chậu cây nhỏ trong hiên tối, bị bóng và cửa che.


Generate an isolated 2D game asset of the small potted plant visible inside the shaded porch of the reference image. Preserve the dim indoor-porch lighting, small pot, green leaves, rustic wooden interior feeling, and painterly style. Because the item is partially hidden in shadow and by porch objects, reconstruct and complete the full potted plant naturally while keeping the same material, lighting, and perspective. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No table, no chair, no door, no wall, no porch background, no extra objects.


---

## 17. WoodenTable_IndoorPorch_01

**Mô tả:** Bàn/ghế gỗ trong hiên tối, không quá rõ nhưng có thể tạo lại làm prop.


Generate an isolated 2D game asset of the rustic wooden table or small wooden furniture visible inside the shaded porch of the reference image. Preserve the dark warm wood material, simple rural construction, dim interior lighting, painterly texture, and original perspective. Complete hidden or shadowed parts naturally so the furniture becomes a clean standalone prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No house wall, no doors, no plant, no porch background, no extra objects.


---

## 18. HangingGarlic_DriedCrop_Porch_01

**Mô tả:** Cụm nông sản khô treo dưới mái hiên.


Generate an isolated 2D game asset of the hanging dried crop bundle under the porch roof from the reference image. Preserve the small clustered dried bulbs or garlic-like shapes, hanging string arrangement, warm sunlit porch lighting, rustic Vietnamese village style, and painterly texture. Make it a complete standalone hanging decor item. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No roof, no wall, no door, no house background, no extra props.


---

# D. Cổng, hàng rào, tường, vật chắn

## 19. BrickGatePillar_Left_01

**Mô tả:** Cột cổng gạch bên trái lối đi, sát bụi tre.


Generate an isolated 2D game asset of the left brick gate pillar from the reference image. Preserve the red-orange brick material, stacked cap stones, aged mortar, warm sunlight, painterly texture, and original viewing angle. If parts are covered by bamboo leaves, flowers, or wall edges, reconstruct and complete the pillar naturally as a standalone prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No bamboo, no wall, no path, no flowers, no gate surroundings.


---

## 20. BrickGatePillar_Right_01

**Mô tả:** Cột cổng gạch bên phải lối đi, khác góc và bị cây/tường ảnh hưởng.


Generate an isolated 2D game asset of the right brick gate pillar from the reference image. Preserve the red-orange brick material, stacked cap stones, aged rural look, warm sunlight, painterly texture, and original right-side viewing angle. Complete hidden or cropped parts naturally if covered by banana leaves, wall, or foliage. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No banana tree, no path, no wall, no bamboo, no full background.


---

## 21. BambooFence_Low_Center_01

**Mô tả:** Hàng rào tre thấp nằm ngang trước bụi tre.


Generate an isolated 2D game asset of the low bamboo fence in front of the bamboo cluster from the reference image. Preserve the short horizontal bamboo rails, small vertical posts, tied rustic construction, warm sunlight, painterly texture, and original center viewing angle. Make it a complete standalone fence prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No bamboo cluster behind it, no flowers, no wall, no ground, no extra props.


---

## 22. BrickWall_BackPorch_01

**Mô tả:** Mảng tường gạch đỏ phía sau hiên nhà, nếu cần làm prop môi trường.


Generate an isolated 2D game asset of the short red brick wall segment behind the porch area in the reference image. Preserve the aged red brick material, uneven rural wall texture, warm painterly lighting, and original angled perspective. Complete any hidden ends naturally so it becomes a standalone wall segment. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No house, no bamboo, no gate pillar, no path, no sky, no extra objects.


---

## 23. StoneWall_Right_01

**Mô tả:** Tường đá thấp bên phải. Có thể để background sau, nhưng nếu cần item riêng thì dùng prompt này.


Generate an isolated 2D game asset of the low stone wall on the right side of the reference image. Preserve the irregular gray stone blocks, mossy rural texture, warm sunlight, painterly style, and original perspective running into the scene. Complete hidden or cropped sections naturally so it works as a standalone wall prop. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No banana tree, no path, no pond, no plants, no full background.


---

# E. Hoa, bụi cây, cụm trang trí

## 24. FlowerBush_Pink_Center_01

**Mô tả:** Bụi hoa hồng nhỏ trước bụi tre.


Generate an isolated 2D game asset of the small pink flower bush near the bamboo cluster in the reference image. Preserve the soft green leaves, tiny pink flowers, warm sunlight, painterly rural garden style, and original low ground perspective. Complete any hidden leaf clusters naturally if overlapped by the fence or jar. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No bamboo, no fence, no clay jar, no ground, no extra plants.


---

## 25. GreenBush_Porch_Center_01

**Mô tả:** Bụi cây xanh cạnh chậu đất nung trước hiên.


Generate an isolated 2D game asset of the green bush near the porch and terracotta pot in the reference image. Preserve the dense small leaves, warm sunlit highlights, painterly Vietnamese garden style, and original porch-side viewing angle. Make it a complete standalone bush decor asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No house wall, no pot, no porch column, no courtyard, no extra objects.


---

## 26. GreenBush_RightPath_01

**Mô tả:** Cụm cây bụi bên phải lối đi, cạnh tường đá.


Generate an isolated 2D game asset of the green bush beside the right-side path from the reference image. Preserve the leafy rural garden look, warm sunlight, painterly foliage texture, and original right-path perspective. Complete hidden edges naturally if mixed with wall plants or ground foliage. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No stone wall, no path, no pond, no banana tree, no full background.


---

## 27. GreenLeaves_BottomLeft_01

**Mô tả:** Cụm lá xanh góc dưới trái, bị cắt mép.


Generate an isolated 2D game asset of the green leafy plant cluster from the bottom-left foreground of the reference image. Preserve the broad fresh green leaves, shaded foreground lighting, painterly texture, and original close-up perspective. Since the plant is partly cropped by the image edge, reconstruct and complete the full plant cluster naturally while matching the same style and lighting. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No water jar, no courtyard, no basin, no extra background.


---

# F. Ao sen / vật thể trên mặt nước

## 28. LotusFlower_Pink_RightPond_01

**Mô tả:** Bông sen hồng lớn ở ao bên phải.


Generate an isolated 2D game asset of the large pink lotus flower from the pond on the right side of the reference image. Preserve the layered pink petals, yellow center, floating aquatic feeling, warm sunlight, painterly texture, and original pond-side perspective. Make it a clean standalone lotus flower asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No pond water, no lotus leaves, no stone border, no extra flowers, no background.


---

## 29. LotusFlower_Pink_Small_BottomRight_01

**Mô tả:** Bông sen hồng nhỏ phía dưới phải, khác kích thước/góc với bông lớn.


Generate an isolated 2D game asset of the smaller pink lotus flower from the lower-right pond area in the reference image. Preserve its smaller scale, layered pink petals, aquatic lotus shape, warm painterly style, and original viewing angle. Make it a complete standalone flower asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No pond water, no lotus leaves, no large lotus flower, no stone border, no background.


---

## 30. LotusLeaves_Cluster_RightPond_01

**Mô tả:** Cụm lá sen lớn giữa ao.


Generate an isolated 2D game asset of the large cluster of round lotus leaves from the right-side pond in the reference image. Preserve the circular green lily pad shapes, subtle veins, floating arrangement, sunlight highlights, painterly texture, and original pond perspective. Keep the leaves as one complete standalone cluster asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No pond water background, no lotus flowers, no stone border, no extra objects.


---

## 31. LotusLeaves_SmallCluster_UpperPond_01

**Mô tả:** Cụm lá sen nhỏ phía trên ao, khác scale/vị trí với cụm lớn.


Generate an isolated 2D game asset of the smaller lotus leaf cluster from the upper area of the right-side pond in the reference image. Preserve the small round green lily pads, aquatic floating look, warm painterly lighting, and original perspective. Make it a complete standalone leaf cluster asset. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border, clean edges, centered, high resolution. No pond water, no lotus flowers, no stone border, no background.


---

# G. Danh sách ưu tiên nên generate trước

Nếu cần làm theo thứ tự, nên làm trước các item có silhouette rõ và hữu dụng nhất:

1. `House_Vietnamese_OldVillage_LeftAngle_01`
2. `BambooCluster_Large_Center_01`
3. `Bicycle_OldVillage_LeftFacing_01`
4. `BananaTree_RightAngle_01`
5. `WaterJar_Large_Foreground_01`
6. `ClayJar_Tall_CenterRight_01`
7. `PottedPlant_RoundLeaves_Foreground_01`
8. `PottedPlant_Terracotta_Porch_01`
9. `BambooFence_Low_Center_01`
10. `BrickGatePillar_Left_01`
11. `BrickGatePillar_Right_01`
12. `Haystack_Hut_BackRight_01`
13. `LotusFlower_Pink_RightPond_01`
14. `LotusFlower_Pink_Small_BottomRight_01`
15. `LotusLeaves_Cluster_RightPond_01`
16. `FlowerBush_Pink_Center_01`
17. `GreenBush_Porch_Center_01`
18. `MetalScoop_Foreground_01`
19. `HangingGarlic_DriedCrop_Porch_01`

---

# H. Các phần để xử lý sau ở nhóm background

Các phần sau không nên tách trong file item này, nên xử lý ở file background riêng:

- Bầu trời
- Mây
- Ánh nắng tổng thể
- Sân gạch đỏ
- Lối đi bên phải
- Ao nước
- Viền đá quanh ao nếu làm theo background layer
- Nền cây xa phía sau
- Bóng đổ trên sân
- Các mảng cỏ nhỏ dính nền
- Texture tường / nền / sân
