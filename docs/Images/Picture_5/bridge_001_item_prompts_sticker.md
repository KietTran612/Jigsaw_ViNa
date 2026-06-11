# bridge_001 — Prompt tách / generate item riêng dạng sticker

Nguồn tham chiếu: `bridge_001.png`

Mục tiêu: dùng mockup làm reference để generate từng asset riêng, giữ đúng góc nhìn, hướng quay, phối cảnh, ánh sáng và phong cách vẽ tay của ảnh gốc. Không tạo background hoàn chỉnh trong file này. Background / scene layer sẽ xử lý sau.

---

## 0. Rule chung bắt buộc cho mọi item

Dùng các rule này cho toàn bộ prompt bên dưới:


Use the provided bridge_001 reference image as the only visual reference. Generate one isolated 2D game asset only. Preserve the original item identity, viewing angle, orientation, perspective, proportions, color palette, warm hand-painted Vietnamese riverside style, sunlight, and soft painterly detail as faithfully as possible.

If any part of the item is hidden, cropped, too small, or partially occluded by another object, reconstruct and complete the full object naturally based on the visible design. The completed parts must match the original shape, material, color, lighting, perspective, and painterly style.

Add a thick clean white sticker-style border around the complete outer silhouette of the asset. The border must be smooth, closed, clearly visible, and consistent in thickness. The final result must look like a standalone sticker game asset, not a plain cutout and not a cropped fragment.

Transparent background outside the sticker border. Centered composition. High resolution. Clean edges. No text. No watermark. No extra objects.


### Negative prompt chung


background scene, sky, clouds, river water, road surface, unrelated objects, extra vehicles, extra people, text, logo, watermark, cropped object, incomplete object, broken silhouette, missing parts, missing sticker border, borderless cutout, thin uneven border, broken white border, border around cropped fragment, realistic photo, 3D render, low resolution, blurry edges, messy alpha, harsh shadow, different camera angle, front-only view, top-down view


---

# A. Công trình / kiến trúc lớn

## 01. CableStayedBridge_Large_Perspective_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the large cable-stayed bridge. Preserve the exact sweeping perspective from the reference: the bridge starts large from the left foreground and recedes toward the right distance, with tall concrete pylons, many diagonal suspension cables, bridge deck, railing, and long elegant structure. Keep the warm sunlit hand-painted style, soft Vietnamese riverside atmosphere, pale concrete material, and original sense of scale.

Complete any cropped or hidden bridge parts naturally, especially the left foreground side and distant right side. Keep the full bridge as one complete landmark asset. Add a thick clean white sticker-style border around the entire completed bridge silhouette. Transparent background outside the sticker border. No sky, no river, no vehicles, no people, no landscape.


Negative prompt:


sky background, river, road traffic, people, motorcycles, boats, houses, clouds, cropped bridge, missing cables, broken cables, incomplete pylon, wrong bridge angle, flat front view, top-down view, no sticker border, thin border, messy outline


---

## 02. BridgePylon_Main_Center_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the main central bridge pylon. Preserve the tall concrete tower shape, H-like cross beam, vertical perspective, pale warm concrete color, sunlit highlights, subtle shadows, and the original viewing angle from the reference. Include the visible diagonal cable attachments if they are structurally connected to the pylon.

If cables or edges are obscured by the scene, reconstruct them cleanly and naturally. Add a thick clean white sticker-style border around the complete pylon silhouette. Transparent background outside the sticker border. No bridge deck, no road, no river, no sky, no vehicles.


Negative prompt:


full bridge scene, road deck, cars, motorcycles, sky, river, background landscape, cropped pylon, missing top, broken cables, different architecture, no sticker border, borderless cutout


---

## 03. BridgePylon_Back_Distant_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the distant bridge pylon on the right side of the bridge. Preserve the smaller far-away scale, pale concrete material, twin vertical tower shape, diagonal cable fan, and receding perspective from the reference image.

Reconstruct any missing or unclear cable details naturally while keeping the distant simplified painterly look. Add a thick clean white sticker-style border around the completed pylon and cable silhouette. Transparent background outside the sticker border. No sky, no river, no road, no other bridge parts.


Negative prompt:


main large pylon, full bridge, road, vehicles, river, clouds, excessive detail, front view, cropped tower, missing sticker border, broken white outline


---

## 04. RiversideHouse_BlueTinRoof_Foreground_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small blue riverside house in the lower middle foreground. Preserve the rustic blue wooden walls, red-brown corrugated tin roof, small riverside porch, warm sunlight, Vietnamese river village style, and the same angled view from the reference.

Some parts are partially covered by banana leaves and foreground plants. Reconstruct and complete the full house naturally based on the visible design. Keep it as one complete standalone house asset. Add a thick clean white sticker-style border around the entire completed house silhouette. Transparent background outside the sticker border. No river water, no bridge, no sky, no large plants in front unless structurally attached.


Negative prompt:


modern house, city building, full background scene, bridge, river, banana leaves covering the house, cropped roof, missing porch, no sticker border, broken border, realistic photo


---

## 05. RiversideHouses_RightBank_Cluster_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small cluster of riverside houses on the far right riverbank. Preserve the distant scale, small blue and red-roof houses, wooden riverside platforms, tropical riverbank feeling, and warm hand-painted style from the reference.

Reconstruct any unclear or partially hidden house edges naturally while keeping the cluster as a single background-prop asset. Add a thick clean white sticker-style border around the complete outer silhouette of the house cluster. Transparent background outside the sticker border. No river, no sky, no bridge, no palm trees unless directly attached to the cluster silhouette.


Negative prompt:


single large house, foreground house, full river scene, bridge, sky, water, cropped houses, too realistic, no sticker border, messy cutout


---

## 06. DistantVillage_Houses_UnderBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the distant village house cluster below the bridge. Preserve the very small far-distance scale, low rooftops, green riverbank context, warm Vietnamese riverside village feel, and soft painterly style.

Reconstruct the cluster as a clean simplified distant decor asset. Add a thick clean white sticker-style border around the complete cluster silhouette. Transparent background outside the sticker border. No bridge, no river water, no sky, no extra large trees.


Negative prompt:


large foreground buildings, modern city, bridge included, river included, sky included, cropped cluster, overly detailed foreground asset, no sticker border


---

# B. Phương tiện trên cầu

## 07. MotorbikeRider_Foreground_LeftBackView_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the closest motorbike rider on the left foreground of the bridge. Preserve the rear-view angle, white helmet, white shirt, dark pants, small motorcycle body, mirror, rear light, and the feeling of riding away along the bridge. Keep the same warm painterly style and lighting.

Complete any cropped or unclear parts of the motorcycle and rider naturally. Keep rider and motorbike together as one complete asset. Add a thick clean white sticker-style border around the full rider-and-motorbike silhouette. Transparent background outside the sticker border. No road, no bridge railing, no sky, no other vehicles.


Negative prompt:


front view motorbike, side view only, extra rider, road background, bridge railing, cropped wheels, missing helmet, realistic photo, no sticker border, broken outline


---

## 08. MotorbikeRider_YellowShirt_MidBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the mid-distance motorbike rider wearing a yellow shirt. Preserve the smaller scale, rear/three-quarter riding angle, yellow shirt, helmet, compact motorbike shape, and the original bridge traffic perspective.

Because the reference detail is small, reconstruct the rider and motorcycle clearly while keeping the same distance-like stylization. Add a thick clean white sticker-style border around the full completed silhouette. Transparent background outside the sticker border. No road, no bridge, no other vehicles.


Negative prompt:


large foreground rider, wrong shirt color, front view, extra passengers, road, bridge, cropped motorbike, blurry tiny object, missing sticker border


---

## 09. MotorbikeRider_Distant_MidBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small distant motorbike rider on the bridge. Preserve the tiny far-away scale, rear-facing bridge traffic angle, simplified motorbike shape, and warm hand-painted look.

Reconstruct the small rider and motorbike into a clean readable distant traffic asset. Add a thick clean white sticker-style border around the full silhouette. Transparent background outside the sticker border. No road, no bridge, no other vehicles.


Negative prompt:


large close-up motorcycle, detailed modern racing bike, front view, road background, bridge railing, extra traffic, missing sticker border, messy alpha


---

## 10. BlueBus_MidBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the blue bus or blue truck on the bridge. Preserve the small mid-distance scale, blue vehicle body, boxy silhouette, bridge perspective angle, sunlit painterly style, and simplified traffic-asset look.

Reconstruct any unclear wheels and windows naturally while keeping the vehicle consistent with the reference. Add a thick clean white sticker-style border around the complete vehicle silhouette. Transparent background outside the sticker border. No road, no bridge, no sky, no other vehicles.


Negative prompt:


large modern bus, front-only view, side-only view, road scene, bridge scene, extra cars, cropped wheels, no sticker border, realistic photo


---

## 11. WhiteCar_MidBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small white car on the bridge. Preserve the tiny mid-distance scale, pale white body, simple compact silhouette, and the same receding road perspective from the reference.

Reconstruct the car cleanly so it reads as a small traffic asset. Add a thick clean white sticker-style border around the full car silhouette. Transparent background outside the sticker border. No road, no bridge, no other vehicles.


Negative prompt:


large car close-up, front view, sports car, road background, bridge railing, cropped car, unclear silhouette, missing sticker border


---

## 12. DistantTraffic_Bridge_Cluster_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of a tiny distant traffic cluster from the bridge. Preserve the small simplified silhouettes of multiple vehicles, the receding bridge perspective, and the warm painterly style.

Keep the traffic as one small grouped decor asset, not separate vehicles. Add a thick clean white sticker-style border around the entire cluster silhouette. Transparent background outside the sticker border. No road, no bridge, no sky.


Negative prompt:


large vehicles, detailed close-up traffic, road surface, bridge deck, too many vehicles, messy cluster, no sticker border, broken white border


---

# C. Đèn đường / vật thể trên cầu

## 13. StreetLamp_Tall_Left_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the tall street lamp on the left side of the bridge. Preserve the thin curved pole, small lamp head, tall vertical shape, slight perspective angle, dark metal color, and clean painterly style.

Complete the lamp pole and lamp head naturally if any part is unclear. Add a thick clean white sticker-style border around the full street lamp silhouette. Transparent background outside the sticker border. No sky, no bridge railing, no road.


Negative prompt:


street scene, bridge background, multiple lamps, modern decorative lamp, cropped pole, missing lamp head, no sticker border, uneven border


---

## 14. StreetLamp_MidBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the mid-distance street lamp on the bridge. Preserve the smaller scale, slim pole, curved lamp head, road-side perspective, and warm hand-painted style.

Reconstruct the lamp cleanly as a standalone object. Add a thick clean white sticker-style border around the complete silhouette. Transparent background outside the sticker border. No bridge, no road, no vehicles, no sky.


Negative prompt:


large foreground lamp, multiple lamps, road background, bridge railing, cropped pole, missing sticker border, messy cutout


---

## 15. StreetLamp_DistantBridge_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of a tiny distant bridge street lamp. Preserve the far-away simplified scale, slim pole, small lamp head, and receding perspective.

Keep it readable as a small repeated bridge lamp asset. Add a thick clean white sticker-style border around the full silhouette. Transparent background outside the sticker border. No road, no bridge, no sky.


Negative prompt:


large detailed lamp, foreground lamp, street scene, bridge scene, multiple poles, cropped object, no sticker border


---

# D. Thuyền / phương tiện dưới nước

## 16. WoodenBoat_ThatchedCanopy_RightForeground_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the large wooden river boat in the lower right foreground. Preserve the long dark wooden hull, pointed bow, woven thatched canopy, cargo baskets, Vietnamese riverboat style, warm sunlight, and the same right-facing angled view from the reference.

Keep the boat as one complete standalone asset. Reconstruct any hidden or unclear hull details naturally. Add a thick clean white sticker-style border around the entire boat silhouette. Transparent background outside the sticker border. No river water, no waves, no bridge, no background houses. If including the rower, keep the rower attached naturally to the boat; otherwise exclude the rower completely.


Negative prompt:


river background, water reflection, bridge, extra boats, modern motorboat, cropped bow, missing canopy, no sticker border, broken outline


---

## 17. BoatRower_ConicalHat_RightForeground_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the boat rower wearing a Vietnamese conical hat. Preserve the seated pose, conical hat, small human scale, rowing posture, traditional riverside feeling, and the same angled view from the reference.

The lower body is partially connected to the boat, so reconstruct and complete the full seated figure naturally. Add a thick clean white sticker-style border around the complete character silhouette. Transparent background outside the sticker border. No boat, no river, no pole unless the pole is requested as attached.


Negative prompt:


standing person, front portrait, modern clothing, full boat, river background, cropped body, missing legs, no sticker border, messy silhouette


---

## 18. BoatPole_RightForeground_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the long rowing pole from the right foreground boat. Preserve the thin bamboo/wooden pole shape, slight diagonal angle, simple hand-painted texture, and original orientation.

Complete the full pole cleanly as a standalone accessory asset. Add a thick clean white sticker-style border around the entire pole silhouette. Transparent background outside the sticker border. No person, no boat, no river.


Negative prompt:


boat, rower, river, fishing rod, spear, cropped pole, broken pole, no sticker border, messy edge


---

## 19. SmallWoodenBoat_RedRoof_CenterRiver_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small wooden boat with a red-brown roof in the center of the river. Preserve the compact boat shape, small cabin, blue side panels, reddish roof, soft reflection-inspired lighting, and the same distant angled view from the reference.

Reconstruct the boat clearly while keeping its mid-distance scale and painterly style. Add a thick clean white sticker-style border around the complete boat silhouette. Transparent background outside the sticker border. No water, no bridge, no waves, no background.


Negative prompt:


large foreground boat, thatched canopy boat, river background, water reflection, extra boats, cropped cabin, missing roof, no sticker border


---

## 20. SmallBoat_Distant_LeftRiver_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small distant boat under the bridge on the left side of the river. Preserve the tiny far-away scale, simple wooden boat silhouette, low cabin shape, and soft painterly river-village style.

Reconstruct the boat into a clean readable distant decor asset. Add a thick clean white sticker-style border around the whole boat silhouette. Transparent background outside the sticker border. No water, no bridge, no background village.


Negative prompt:


large boat, foreground boat, detailed canopy boat, bridge background, river water, cropped shape, no sticker border


---

## 21. SmallBoat_RightBank_Distant_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small distant boat near the right riverbank houses. Preserve the small scale, rustic wooden form, riverside orientation, and warm hand-painted style from the reference.

Complete any unclear boat edges naturally. Add a thick clean white sticker-style border around the complete boat silhouette. Transparent background outside the sticker border. No river, no houses, no bridge, no trees.


Negative prompt:


large foreground boat, modern boat, full riverbank scene, houses, water, cropped boat, missing sticker border, blurry cutout


---

# E. Cây cối / thực vật lớn

## 22. BananaTreeCluster_Foreground_Left_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the large banana tree cluster in the lower left foreground. Preserve the broad green banana leaves, tropical riverside feeling, layered leaves, warm sunlight, painterly texture, and the same foreground angle from the reference.

Some leaves are cropped by the image edge or covered by other foliage. Reconstruct and complete the full banana cluster naturally based on the visible design. Add a thick clean white sticker-style border around the entire completed leaf cluster silhouette. Transparent background outside the sticker border. No bridge, no house, no river, no road.


Negative prompt:


single small banana leaf, palm tree, background scene, bridge, river, house, cropped leaves, missing leaves, no sticker border, broken white border


---

## 23. GreenBushCluster_UnderBridge_Left_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the dense green bush cluster under the bridge on the left side. Preserve the lush tropical foliage, varied leaf shapes, warm sunlight, and hand-painted texture from the reference.

Reconstruct the bush as a complete standalone foliage cluster. Add a thick clean white sticker-style border around the outer silhouette. Transparent background outside the sticker border. No bridge, no banana leaves, no river, no houses.


Negative prompt:


full landscape, bridge, river, banana tree cluster, houses, cropped bush, flat green blob, no sticker border, messy edge


---

## 24. PalmTreeCluster_RightBank_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the palm and tropical tree cluster on the right riverbank. Preserve the tall palm silhouettes, leafy crowns, distant riverbank scale, warm sunlight, and soft painterly Vietnamese riverside style.

Reconstruct unclear overlapping leaves naturally while keeping the cluster as one complete decor asset. Add a thick clean white sticker-style border around the full tree cluster silhouette. Transparent background outside the sticker border. No houses, no river, no bridge, no sky.


Negative prompt:


single foreground banana tree, full riverbank scene, houses, water, bridge, sky, cropped palms, no sticker border, broken outline


---

## 25. RiverbankTreeLine_Background_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the distant green tree line along the riverbank. Preserve the low continuous tropical foliage silhouette, far-distance scale, soft painterly edges, and warm river landscape feeling from the reference.

Keep it as one simplified horizontal background-decor foliage strip. Add a thick clean white sticker-style border around the overall tree-line silhouette. Transparent background outside the sticker border. No river, no sky, no bridge, no houses.


Negative prompt:


foreground bush, single tree, bridge, river, sky, houses, overly detailed leaves, cropped strip, no sticker border


---

## 26. GreenBush_RightForeground_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the green bush cluster near the lower right foreground by the river. Preserve the dense tropical leaves, warm hand-painted style, and right-foreground placement feeling from the reference.

Reconstruct any hidden lower foliage naturally. Add a thick clean white sticker-style border around the complete bush silhouette. Transparent background outside the sticker border. No boat, no river water, no houses, no bridge.


Negative prompt:


boat, river, full scene, palm trees, bridge, cropped bush, muddy edges, no sticker border, messy alpha


---

# F. Nhà nổi / bến nước / chi tiết ven sông

## 27. WoodenDock_ForegroundHouse_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small wooden dock in front of the blue riverside house. Preserve the rustic planks, simple wooden platform, thin posts, riverside construction style, warm sunlight, and the same perspective angle from the reference.

Reconstruct any hidden planks or posts naturally. Keep the dock as one complete standalone asset. Add a thick clean white sticker-style border around the entire dock silhouette. Transparent background outside the sticker border. No house, no river water, no plants, no bridge.


Negative prompt:


full house, river, bridge, modern pier, cropped dock, missing posts, water background, no sticker border, broken border


---

## 28. WoodenPoles_RiversideDock_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of a small cluster of wooden riverside dock poles. Preserve the thin vertical wooden stakes, rustic handmade look, uneven heights, warm brown color, and the same riverside perspective from the reference.

Keep the poles grouped as one small decor asset, not individual separate poles. Add a thick clean white sticker-style border around the whole pole cluster silhouette. Transparent background outside the sticker border. No dock platform, no house, no river, no bridge.


Negative prompt:


single pole only, full dock, house, water, bridge, metal poles, cropped stakes, no sticker border, messy outline


---

## 29. PottedPlants_RiversideHouse_Cluster_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the small potted plant cluster in front of the riverside house. Preserve the tiny clay pots, small green leaves, warm hand-painted style, and rustic riverside home decoration feeling.

Because the reference detail is small, reconstruct the pots and leaves cleanly while keeping them as a small grouped decor asset. Add a thick clean white sticker-style border around the complete cluster silhouette. Transparent background outside the sticker border. No house, no dock, no river, no bridge.


Negative prompt:


large flower pot, full house, river background, dock, cropped pots, unclear leaves, no sticker border, messy cutout


---

# G. Item cân nhắc / scene-scale asset

Các prompt dưới đây dùng khi thật sự muốn tách thành layer riêng. Nếu dùng làm background hoặc scene layer, có thể bỏ câu “sticker border”. Nếu vẫn theo pipeline item sticker thì giữ border như bên dưới.

## 30. BridgeRailing_LeftPerspective_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the left bridge railing segment in strong perspective. Preserve the pale concrete railing, repeated posts, long receding angle from the lower left foreground toward the bridge distance, warm sunlight, and painterly detail from the reference.

Complete any cropped railing parts naturally. Keep it as one long perspective railing asset. Add a thick clean white sticker-style border around the complete railing silhouette. Transparent background outside the sticker border. No road surface, no vehicles, no sky, no river, no full bridge.


Negative prompt:


full bridge, road, vehicles, river, sky, front view railing, straight flat fence, cropped railing, no sticker border, messy alpha


---

## 31. BridgeRoad_Perspective_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the bridge road surface in perspective. Preserve the wide gray asphalt road, lane markings, warm sunlight, and strong perspective receding from the left foreground into the distance.

Complete the road shape naturally as a clean scene-scale asset. Add a thick clean white sticker-style border around the full road silhouette if used as a sticker asset. Transparent background outside the sticker border. No vehicles, no railing, no sky, no river, no bridge pylons.


Negative prompt:


cars, motorcycles, bridge railing, full bridge, sky, river, city road, cropped road, no sticker border if sticker style is required, messy edges


---

## 32. BridgeCableLines_Cluster_01


Using the provided bridge_001 reference image as the only visual reference, generate a single isolated 2D game asset of the bridge cable line cluster. Preserve the thin diagonal suspension cables, fan-like arrangement, pale sunlit color, and original cable-stayed bridge perspective from the reference.

Keep the cables as one clean grouped asset, not separate individual strings. Reconstruct missing or faint cable segments naturally. Add a thick clean white sticker-style border around the overall cable cluster silhouette if used as a sticker asset. Transparent background outside the sticker border. No pylons, no bridge deck, no sky, no river.


Negative prompt:


full bridge, pylons, road, sky background, thick ropes, tangled cables, broken cable fan, no sticker border if required, messy alpha


---

# Danh sách ưu tiên generate trước

1. `CableStayedBridge_Large_Perspective_01`
2. `MotorbikeRider_Foreground_LeftBackView_01`
3. `MotorbikeRider_YellowShirt_MidBridge_01`
4. `BlueBus_MidBridge_01`
5. `StreetLamp_Tall_Left_01`
6. `StreetLamp_MidBridge_01`
7. `WoodenBoat_ThatchedCanopy_RightForeground_01`
8. `BoatRower_ConicalHat_RightForeground_01`
9. `SmallWoodenBoat_RedRoof_CenterRiver_01`
10. `SmallBoat_Distant_LeftRiver_01`
11. `SmallBoat_RightBank_Distant_01`
12. `BananaTreeCluster_Foreground_Left_01`
13. `RiversideHouse_BlueTinRoof_Foreground_01`
14. `WoodenDock_ForegroundHouse_01`
15. `PalmTreeCluster_RightBank_01`
16. `RiversideHouses_RightBank_Cluster_01`
17. `GreenBushCluster_UnderBridge_Left_01`

---

# Ghi chú cho các item bị che / bị cắt mép

Các item sau cần đặc biệt giữ rule “hoàn thiện object trước, rồi mới thêm sticker border”:

- `CableStayedBridge_Large_Perspective_01` — cầu bị cắt và trải dài ngoài khung hình.
- `RiversideHouse_BlueTinRoof_Foreground_01` — nhà bị lá chuối/cây che một phần.
- `MotorbikeRider_Foreground_LeftBackView_01` — sát mép trái và dính với mặt đường.
- `MotorbikeRider_YellowShirt_MidBridge_01` — nhỏ, chi tiết chưa rõ.
- `WoodenBoat_ThatchedCanopy_RightForeground_01` — dính với nước và người chèo.
- `BoatRower_ConicalHat_RightForeground_01` — thân dưới bị thuyền che.
- `BananaTreeCluster_Foreground_Left_01` — một số lá bị cắt mép ảnh.
- `GreenBushCluster_UnderBridge_Left_01` — dính với cầu, nhà và lá chuối.
- `WoodenDock_ForegroundHouse_01` — dính với nhà và nước.

---

# Không xử lý trong file này

Những phần này nên để sang file background riêng:

- Bầu trời
- Mây
- Mặt nước sông
- Phản chiếu trên mặt nước
- Ánh sáng tổng
- Dải cây xa liền nền
- Toàn cảnh đường cầu nếu dùng như background
- Các gợn sóng nhỏ quanh thuyền
- Không khí chiều sâu của toàn scene
