# old_village_south_001 — Prompt tách item dạng sticker

File này tổng hợp prompt cho các item có thể tách/generate riêng từ ảnh `old_village_south_001`.

## Quy tắc đã chốt

- Không tạo ảnh ở bước này, chỉ dùng prompt để chuẩn bị tách asset.
- Chỉ tách các item độc lập, silhouette tương đối rõ.
- Không tách background trong file này.
- Không tách modular nhỏ như từng viên ngói, từng cột, từng cánh cửa, từng mảng tường.
- Item giống nhau nhưng khác góc/khác trạng thái phải tách thành asset riêng.
- Mọi item phải có **border trắng dạng sticker**.
- Với item bị che khuất/cắt mép: phải **hoàn thiện object đầy đủ trước**, sau đó mới thêm sticker border.

## Global prompt rule dùng chung


Use the provided mockup only as visual reference. Generate one isolated 2D game asset only. Preserve the exact original item identity, viewing angle, orientation, proportions, material, warm sunny lighting, and cozy hand-painted Vietnamese countryside style from the reference image. Do not redesign, restyle, rotate, flip, or change the camera angle. If the item is hidden, cropped, partially occluded, shadowed, or blended with the background, reconstruct and complete the full object naturally based on the visible design before adding the sticker border. Add a thick clean white sticker-style border around the entire completed outer silhouette. The border must be smooth, closed, clearly visible, and consistent in thickness. Transparent background outside the sticker border.


## Negative prompt dùng chung


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## Danh sách ưu tiên nên tách trước

1. `House_Vietnamese_OldVillage_South_LeftAngle_01`
2. `CoconutTree_Tall_Center_01`
3. `BambooCluster_Right_01`
4. `BananaTree_LeftHouse_01`
5. `WaterJar_Large_ForegroundLeft_01`
6. `WaterBasin_Round_ForegroundLeft_01`
7. `WoodenChair_Porch_LeftAngle_01`
8. `BirdCage_Hanging_Porch_01`
9. `PottedFlower_Yellow_PorchSteps_01`
10. `HangingGarlic_DriedCrop_Wall_01`
11. `RoundBambooTray_LeaningWall_01`
12. `HayHut_BackCenterRight_01`
13. `BambooFence_HayHut_Angled_01`
14. `WoodFence_WaterEdge_Horizontal_01`
15. `WoodFence_RightBank_Angled_01`
16. `WoodenBridge_Canal_RightAngle_01`
17. `WoodenBoat_Canal_Foreground_01`
18. `FishingBasket_Bamboo_OnBoat_01`
19. `FlowerBush_RightHouse_Pink_01`
20. `LotusLeaves_Cluster_ForegroundCenter_01`
21. `LotusCluster_RightPond_PinkFlowers_01`
22. `LotusFlower_Small_ForegroundLeft_01`
23. `LotusFlower_Small_BottomRight_01`

---

# Prompt chi tiết từng item

## 01. `House_Vietnamese_OldVillage_South_LeftAngle_01` — Nhà làng chính bên trái

### Prompt


Isolated 2D game sticker asset of the main old Vietnamese countryside house from the reference image. Preserve the original left-side viewing angle, slightly low/eye-level perspective, warm sunny lighting, aged yellow plaster walls, reddish-brown clay tile roof, wooden doors, porch columns, rustic front steps, and cozy rural Vietnamese village atmosphere. Generate the house as one complete standalone object, keeping the same orientation and proportions as the mockup. The house is partially hidden by foliage and cropped near the left edge, so reconstruct and complete any hidden or cropped parts naturally based on the visible design before adding the border. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border. No text, no people, no sky, no ground, no separate background scene.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 02. `SmallHouse_BackSide_LeftCenter_01` — Nhà phụ nhỏ phía sau nhà chính

### Prompt


Isolated 2D game sticker asset of the small side/back house structure partially visible behind the main old village house. Preserve the same warm hand-painted Vietnamese countryside style, red/brown tiled roof, aged plaster wall material, and the original angled perspective from the reference. Reconstruct the hidden and occluded parts naturally so it becomes a complete small standalone background house prop. Add a thick clean white sticker-style border around the complete silhouette. Transparent background outside the sticker border. No main house, no trees, no path, no sky, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 03. `HayHut_BackCenterRight_01` — Nhà/chòi mái rơm giữa phải

### Prompt


Isolated 2D game sticker asset of a small Vietnamese countryside hay hut or straw-roof hut from the reference image. Keep the warm golden straw texture, rounded hay roof shape, rustic rural feeling, and original angle behind the bamboo fence area. Complete any parts hidden by plants, fence, or palm trunk naturally. Add a thick clean white sticker-style border around the full completed hut silhouette. Transparent background outside the sticker border. No trees, no fence, no water, no sky, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 04. `BackgroundHouse_RedTile_Right_01` — Nhà nhỏ mái ngói xa bên phải

### Prompt


Isolated 2D game sticker asset of the small distant red-tile-roof Vietnamese house on the right side of the reference. Preserve its small background-house proportions, red clay roof, light plaster wall, rural village style, and right-side distant viewing angle. Complete the parts hidden by bamboo, banana leaves, and background foliage. Add a thick clean white sticker-style border around the entire completed silhouette. Transparent background outside the border. No surrounding trees, no field, no sky, no water.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 05. `BackgroundVillageHouses_Center_01` — Cụm nhà làng xa giữa nền

### Prompt


Isolated 2D game sticker asset of a small cluster of distant Vietnamese village houses from the center background of the reference image. Keep the tiny rural houses, warm tiled roofs, soft atmospheric hand-painted style, and the same distant perspective. Reconstruct hidden parts naturally while keeping it as a compact background decor asset. Add a thick clean white sticker-style border around the full cluster silhouette. Transparent background outside the sticker border. No field, no river, no sky, no trees as separate background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 06. `CoconutTree_Tall_Center_01` — Cây dừa lớn giữa ảnh

### Prompt


Isolated 2D game sticker asset of the tall coconut palm tree in the center of the reference. Preserve the slightly leaning trunk, visible coconut cluster, long palm fronds, warm sunlight, tropical Vietnamese village style, and original scale feeling. Complete any fronds cropped by the top edge naturally. Add a thick clean white sticker-style border around the complete outer silhouette of the full tree. Transparent background outside the border. No sky, no house, no ground, no other trees.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 07. `BambooCluster_Right_01` — Cụm tre lớn bên phải

### Prompt


Isolated 2D game sticker asset of the large bamboo cluster on the right edge of the reference. Preserve the tall green bamboo stems, dense bamboo leaves, vertical cluster shape, sunny hand-painted style, and original right-side angle. The bamboo cluster is cropped by the right and top edges, so reconstruct and complete the missing stems and leaves naturally. Add a thick clean white sticker-style border around the full completed bamboo cluster silhouette. Transparent background outside the sticker border. No sky, no house, no river, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 08. `TreeCanopy_TopLeft_01` — Tán cây lớn góc trên trái

### Prompt


Isolated 2D game sticker asset of the large leafy tree canopy from the top-left foreground of the reference. Preserve the dense green leaves, branch shapes, warm sunlight, hand-painted countryside style, and foreground canopy feeling. Complete cropped branches and leaves naturally so it becomes a usable standalone canopy decor asset. Add a thick clean white sticker-style border around the full outer silhouette. Transparent background outside the border. No roof, no sky, no house, no background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 09. `BananaTree_LeftHouse_01` — Cây chuối lớn bên trái nhà

### Prompt


Isolated 2D game sticker asset of the banana tree beside the left side of the house. Preserve the broad green banana leaves, tropical rural style, warm sunlight, and original left-side angle from the reference. Complete any leaves cropped by the image edge or hidden by the house and foliage. Add a thick clean white sticker-style border around the complete banana tree silhouette. Transparent background outside the border. No house wall, no ground, no other plants.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 10. `BananaCluster_BackRight_01` — Cụm chuối/cây nhiệt đới phía sau bên phải

### Prompt


Isolated 2D game sticker asset of the background banana and tropical plant cluster on the right side of the village scene. Preserve the distant scale, green banana leaves, warm Vietnamese countryside style, and original angle. Complete hidden parts naturally while keeping it as a compact decor prop. Add a thick clean white sticker-style border around the full cluster silhouette. Transparent background outside the border. No house, no water, no field, no sky.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 11. `WaterJar_Large_ForegroundLeft_01` — Chum nước lớn góc dưới trái

### Prompt


Isolated 2D game sticker asset of the large brown ceramic water jar in the foreground left of the reference image. Preserve the rounded jar body, glossy water surface at the opening, aged ceramic texture, warm sunlight highlights, and original foreground angle. The jar may be slightly cropped by the lower/left edge, so complete the full jar naturally. Add a thick clean white sticker-style border around the complete outer silhouette. Transparent background outside the sticker border. No plants, no basin, no ground, no background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 12. `WaterBasin_Round_ForegroundLeft_01` — Bồn/chậu nước tròn cạnh chum lớn

### Prompt


Isolated 2D game sticker asset of the low round water basin in the foreground left area, with still water and small floating leaves. Preserve the original shallow round shape, dark ceramic/metal basin material, water reflection, hand-painted village style, and same viewing angle. Complete any hidden or background-covered rim parts naturally. Add a thick clean white sticker-style border around the complete basin silhouette. Transparent background outside the border. No large jar, no ground, no plants outside the basin.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 13. `WoodenChair_Porch_LeftAngle_01` — Ghế gỗ/ghế mây trước hiên

### Prompt


Isolated 2D game sticker asset of the rustic wooden or bamboo chair on the porch. Preserve the original left-angle view, woven rural chair structure, warm brown material, soft shadows, and cozy old Vietnamese house style. Complete any legs or edges hidden by shadow or nearby plants naturally. Add a thick clean white sticker-style border around the full completed chair silhouette. Transparent background outside the border. No porch, no house, no floor, no other furniture.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 14. `PottedFlower_Yellow_PorchSteps_01` — Chậu hoa vàng trước bậc thềm

### Prompt


Isolated 2D game sticker asset of the potted yellow flower plant placed near the porch steps. Preserve the terracotta pot, bright yellow flowers, green leaves, sunny hand-painted style, and original front/angled view. Complete any partially hidden leaves or pot base naturally. Add a thick clean white sticker-style border around the full plant and pot silhouette. Transparent background outside the border. No porch steps, no house, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 15. `PottedPlant_LeftPorch_01` — Chậu cây nhỏ bên trái hiên

### Prompt


Isolated 2D game sticker asset of the small potted plant near the left porch area. Preserve the rustic pot, green foliage, warm village style, and original angle. Reconstruct any leaves or pot parts blended into surrounding plants. Add a thick clean white sticker-style border around the complete plant silhouette. Transparent background outside the border. No chair, no house, no floor, no background foliage.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 16. `BirdCage_Hanging_Porch_01` — Lồng chim treo trước hiên

### Prompt


Isolated 2D game sticker asset of the hanging bird cage in front of the porch window. Preserve the bamboo/wooden cage structure, rounded cage top, delicate bars, small hanging hook, warm hand-painted style, and original front-left angle. Complete any tiny hidden parts naturally. Add a thick clean white sticker-style border around the complete cage silhouette. Transparent background outside the sticker border. No wall, no window, no plants, no house background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 17. `HangingGarlic_DriedCrop_Wall_01` — Bó tỏi/nông sản khô treo trên tường

### Prompt


Isolated 2D game sticker asset of the hanging dried garlic or dried farm crop bundle on the wall. Preserve the warm beige dried bulbs, tied bundle shape, rustic village material, and original hanging orientation. Complete any small occluded parts naturally. Add a thick clean white sticker-style border around the full bundle silhouette. Transparent background outside the border. No wall, no house, no shadow background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 18. `RoundBambooTray_LeaningWall_01` — Nón/mẹt tre tròn tựa tường

### Prompt


Isolated 2D game sticker asset of the round woven bamboo tray leaning against the house wall. Preserve the circular woven texture, warm tan bamboo material, slight tilted angle, and original village prop style. Complete any parts hidden by foliage or wall shadow naturally. Add a thick clean white sticker-style border around the complete tray silhouette. Transparent background outside the sticker border. No wall, no plants, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 19. `PorchFurniture_DarkInterior_01` — Đồ nội thất tối trong hiên

### Prompt


Isolated 2D game sticker asset of the dark wooden interior porch furniture visible inside the shadowed porch. Preserve the rustic Vietnamese wooden furniture style, warm brown tones, dim indoor lighting, and original angle. Since the item is partially hidden in shadow, reconstruct and complete it naturally as a standalone furniture prop while matching the visible design. Add a thick clean white sticker-style border around the full completed silhouette. Transparent background outside the border. No house interior background, no wall, no floor.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 20. `BambooFence_HayHut_Angled_01` — Hàng rào tre quanh chòi rơm

### Prompt


Isolated 2D game sticker asset of the angled bamboo fence around the hay hut area. Preserve the vertical and horizontal bamboo slats, rustic tied construction, warm sunlit village style, and the original diagonal perspective. Complete any slats hidden by plants, shadows, or the hut naturally. Add a thick clean white sticker-style border around the entire completed fence silhouette. Transparent background outside the border. No hut, no palm tree, no ground, no background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 21. `WoodFence_WaterEdge_Horizontal_01` — Hàng rào gỗ thấp ven nước phía giữa

### Prompt


Isolated 2D game sticker asset of the low horizontal wooden fence along the water edge. Preserve the rough wooden posts and rails, simple rural construction, warm sunlight, and original horizontal viewing angle. Complete any partly hidden rails naturally. Add a thick clean white sticker-style border around the full fence silhouette. Transparent background outside the sticker border. No water, no path, no grass, no bridge.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 22. `WoodFence_RightBank_Angled_01` — Hàng rào gỗ nhỏ bên phải

### Prompt


Isolated 2D game sticker asset of the small angled wooden fence on the right bank near the lotus area. Preserve its shorter scale, rough wooden material, angled perspective, warm hand-painted style, and separation from the horizontal fence variant. Complete hidden sections naturally. Add a thick clean white sticker-style border around the complete fence silhouette. Transparent background outside the border. No water, no lotus, no plants, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 23. `WoodenBridge_Canal_RightAngle_01` — Cầu gỗ bắc qua kênh

### Prompt


Isolated 2D game sticker asset of the rustic wooden bridge crossing the canal on the right side of the reference. Preserve the original strong perspective, curved/angled plank walkway, wooden rail posts, support structure, warm sunlight, and rural Vietnamese village style. Generate the bridge as one complete standalone object, completing any parts hidden by water reflections or edge cropping. Add a thick clean white sticker-style border around the full completed bridge silhouette. Transparent background outside the sticker border. No water, no riverbank, no plants, no boat.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 24. `WoodenPost_Water_Foreground_01` — Cọc gỗ đứng dưới nước/gần thuyền

### Prompt


Isolated 2D game sticker asset of the single weathered wooden post standing in the water near the foreground boat. Preserve the vertical post shape, aged wood texture, water-worn base feeling, and original viewing angle. Complete the submerged/lower part naturally as a usable prop. Add a thick clean white sticker-style border around the complete post silhouette. Transparent background outside the border. No water, no boat, no bridge.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 25. `BridgeSupportPosts_Water_01` — Cọc/trụ cầu dưới nước

### Prompt


Isolated 2D game sticker asset of the wooden support posts used under the bridge. Preserve the rough vertical posts, dark water-worn wood, simple rural bridge construction, and original angle. Complete any hidden sections naturally while keeping it as a small support-post prop set. Add a thick clean white sticker-style border around the complete silhouette. Transparent background outside the border. No bridge deck, no water, no reflections, no plants.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 26. `WoodenBoat_Canal_Foreground_01` — Thuyền gỗ dưới kênh

### Prompt


Isolated 2D game sticker asset of the wooden boat floating in the canal in the foreground. Preserve the original boat angle, curved wooden hull, interior planks, warm brown hand-painted texture, and Vietnamese countryside canal style. Complete any parts hidden by water or overlapping objects naturally. Add a thick clean white sticker-style border around the full completed boat silhouette. Transparent background outside the sticker border. No water, no bridge, no riverbank, no background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 27. `FishingBasket_Bamboo_OnBoat_01` — Lồng cá/giỏ tre trong thuyền

### Prompt


Isolated 2D game sticker asset of the woven bamboo fishing basket placed inside the boat. Preserve the dome-like woven structure, warm tan bamboo material, rustic fishing village style, and original small angled view. Complete any parts hidden by the boat rim naturally. Add a thick clean white sticker-style border around the complete basket silhouette. Transparent background outside the border. No boat, no water, no pole.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 28. `BoatPole_Wooden_OnBoat_01` — Sào/thanh gỗ trong thuyền

### Prompt


Isolated 2D game sticker asset of the long wooden boat pole lying inside the boat. Preserve the slender wooden shape, rustic brown texture, simple village tool style, and original diagonal orientation. Complete any hidden ends naturally. Add a thick clean white sticker-style border around the complete pole silhouette. Transparent background outside the border. No boat, no water, no basket.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 29. `FlowerVine_Roof_Pink_01` — Dây hoa hồng phủ mái nhà

### Prompt


Isolated 2D game sticker asset of the pink flowering vine growing over the old tiled roof. Preserve the cascading vine shape, small pink flowers, dense green leaves, warm sunlight, and hand-painted village style. Since it is intertwined with the roof and tree canopy, reconstruct the vine as a complete standalone decorative plant cluster while matching the visible design. Add a thick clean white sticker-style border around the complete vine silhouette. Transparent background outside the border. No roof tiles, no house, no tree branches.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 30. `FlowerBush_RightHouse_Pink_01` — Bụi hoa bên phải nhà

### Prompt


Isolated 2D game sticker asset of the pink flower bush beside the right side of the house. Preserve the rounded bush shape, small pink flowers, green leaves, warm sunny colors, and original angle. Complete hidden parts behind fence, wall, or nearby plants naturally. Add a thick clean white sticker-style border around the full bush silhouette. Transparent background outside the border. No house, no fence, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 31. `GreenBush_BottomLeft_01` — Bụi cây xanh góc dưới trái

### Prompt


Isolated 2D game sticker asset of the green leafy bush in the bottom-left foreground. Preserve the broad leaves, rich green color, sunlit hand-painted style, and original foreground angle. The bush is cropped by the image edges, so complete the missing leaves and base naturally. Add a thick clean white sticker-style border around the complete bush silhouette. Transparent background outside the border. No jar, no ground, no other plants.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 32. `GreenBush_RightWaterBank_01` — Cụm cây bụi ven bờ nước bên phải

### Prompt


Isolated 2D game sticker asset of the green bush cluster along the right water bank. Preserve the low plant shape, dense leaves, warm rural canal style, and original angle. Complete any hidden or background-blended parts naturally. Add a thick clean white sticker-style border around the full cluster silhouette. Transparent background outside the border. No water, no fence, no lotus, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 33. `FlowerBush_HayHut_Small_01` — Bụi hoa nhỏ gần chòi rơm

### Prompt


Isolated 2D game sticker asset of the small flower bush near the hay hut and bamboo fence. Preserve the small rural flower cluster, green leaves, soft pink/orange blossoms, sunny hand-painted style, and original scale. Complete hidden parts naturally. Add a thick clean white sticker-style border around the complete bush silhouette. Transparent background outside the border. No hay hut, no fence, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 34. `LotusLeaves_Cluster_ForegroundCenter_01` — Cụm lá sen lớn phía dưới giữa

### Prompt


Isolated 2D game sticker asset of the large lotus leaf cluster floating on the water in the foreground center. Preserve the round green leaves, varied leaf sizes, water-plant arrangement, warm hand-painted style, and original top/angled view. Complete any leaf edges hidden by water reflections naturally. Add a thick clean white sticker-style border around the complete lotus leaf cluster silhouette. Transparent background outside the border. No water, no reflections, no riverbank.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 35. `LotusCluster_RightPond_PinkFlowers_01` — Cụm sen và hoa bên phải

### Prompt


Isolated 2D game sticker asset of the large lotus cluster on the right side with pink lotus flowers and green round leaves. Preserve the original clustered arrangement, different flower heights, round leaves, warm sunlight, and Vietnamese pond style. Complete any hidden leaves or stems naturally. Add a thick clean white sticker-style border around the complete cluster silhouette. Transparent background outside the border. No water, no bank, no fence, no background plants.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 36. `LotusFlower_Small_ForegroundLeft_01` — Bông sen hồng nhỏ phía dưới trái giữa

### Prompt


Isolated 2D game sticker asset of the small pink lotus flower from the lower-left/foreground water area. Preserve its small scale, pink petals, green stem/nearby small leaves if visible, soft hand-painted style, and original angle. Complete any hidden petal or stem parts naturally. Add a thick clean white sticker-style border around the complete flower silhouette. Transparent background outside the border. No water, no large lotus cluster, no reflections.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 37. `LotusFlower_Small_BottomRight_01` — Bông sen hồng nhỏ phía dưới phải

### Prompt


Isolated 2D game sticker asset of the small pink lotus flower near the bottom-right area. Preserve the separate small lotus shape, pink petals, warm pond style, and original angle/scale. Complete any hidden petals or stem naturally. Add a thick clean white sticker-style border around the complete flower silhouette. Transparent background outside the border. No water, no leaves cluster unless directly attached, no background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 38. `HangingDriedCrop_UnderRoof_01` — Bó nông sản treo dưới mái hiên

### Prompt


Isolated 2D game sticker asset of the small hanging dried crop bundle under the porch roof. Preserve the warm beige dried produce, tied hanging shape, rustic old-house style, and original hanging orientation. Complete hidden tiny pieces naturally. Add a thick clean white sticker-style border around the full completed bundle silhouette. Transparent background outside the border. No roof, no wall, no house background.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 39. `SmallPottedPlant_PorchInterior_01` — Chậu/cây nhỏ trong góc hiên

### Prompt


Isolated 2D game sticker asset of a small potted plant from the porch interior area. Preserve the small pot, green leaves, dim porch lighting, warm hand-painted rural style, and original angle. Since the plant is partially hidden in shadow, reconstruct and complete it as a clear standalone prop while matching the visible design. Add a thick clean white sticker-style border around the complete plant silhouette. Transparent background outside the border. No furniture, no wall, no floor, no house interior.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


## 40. `BambooBasket_PorchArea_01` — Rổ/giỏ tre gần hiên

### Prompt


Isolated 2D game sticker asset of a woven bamboo basket from the porch area. Preserve the tan woven material, rustic handmade shape, warm old Vietnamese village style, and original perspective. Complete any hidden or shadowed parts naturally. Add a thick clean white sticker-style border around the complete basket silhouette. Transparent background outside the border. No house, no chair, no wall, no ground.


### Negative prompt


background scene, full environment, sky, clouds, ground, brick courtyard, water, canal, riverbank, shadows on ground, extra objects, unrelated props, people, animals not requested, text, logo, UI, cropped fragment, incomplete object, missing parts, missing sticker border, borderless cutout, broken white border, thin border, border around cropped fragment, realistic photo, 3D render, different camera angle, front-only view, top-down view, rotated asset, flipped asset, low resolution, blurry edges.


---

# Ghi chú cho bước background sau

Các phần sau không nằm trong file prompt item này và nên xử lý ở file background riêng: bầu trời, mây, ánh sáng nắng, bóng đổ trên sân, nền gạch sân, lối đi, ruộng nước phía xa, mặt nước kênh, bờ đá/kè đá ven nước, cụm cây rừng xa, reflection dưới nước và texture rêu/cỏ nhỏ trên nền.
