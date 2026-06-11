# festival_tet_001 — Prompt tách/generate item dạng sticker

Tài liệu này dùng cho ảnh reference `festival_tet_001.png`.

Mục tiêu: dùng mockup làm visual reference để generate từng asset riêng, giữ đúng góc nhìn/góc xoay/tư thế trong ảnh, tách thành item hoàn chỉnh, có viền trắng dạng sticker và nền trong suốt.

> Không tạo background trong các prompt item này. Background sẽ xử lý ở file riêng sau.

---

## Global rule áp dụng cho tất cả item


Use the provided mockup image as the only visual reference. Generate one isolated 2D game asset only. Preserve the original item identity, viewing angle, orientation, pose, proportions, hand-painted Vietnamese Tet market style, warm sunlight, painterly texture, and natural color palette from the reference image. Do not redesign, restyle, rotate, mirror, simplify, or change the camera angle.

If any part of the item is hidden, cropped, partially occluded, covered by another object, or cut off by the image edge, reconstruct and complete the full object naturally based on the visible design. The completed parts must match the original shape, material, color, lighting, perspective, and style.

Add a thick clean white sticker-style border around the entire completed outer silhouette of the asset. The border must be smooth, closed, clearly visible, and consistent in thickness, placed outside the object without covering important details. The final result must look like a complete standalone sticker game asset, not a plain cutout or cropped fragment.

Transparent background outside the sticker border. Centered composition. High resolution. Clean edges. No text unless explicitly requested.


## Common negative prompt


background scene, street, sky, full market, extra people, extra vehicles, extra props, text, watermark, logo, UI, cropped object, incomplete object, missing parts, cut off edges, borderless cutout, missing sticker border, broken white border, border around cropped fragment, messy outline, rough mask, hard rectangular crop, wrong angle, mirrored direction, different pose, different costume, different scale, realistic photo, 3D render, low resolution, blurry, noisy edges, heavy cast shadow, black background, white background


---

## Item prompts

### 01. `TetMarket_ManBuyer_Left_01` — Người đàn ông bên trái đang mua hoa


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of an old Vietnamese man buyer on the left side, wearing a brown jacket, dark pants, sandals, and a green pith-style helmet, standing in a Tet flower market pose while receiving or holding a small flower pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. His hands and lower body are partly covered by flowers and the seller. Reconstruct the complete arms, hands, legs, and body naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 02. `TetMarket_WomanSeller_ConicalHat_Left_01` — Người phụ nữ bán hoa đội nón lá bên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a Vietnamese woman flower seller on the left side, wearing a purple áo bà ba style outfit and a conical hat, smiling while handing or holding a potted yellow blossom plant. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Her hands, lower torso, and legs are partly hidden by the pot, flowers, and foreground plants. Complete the full standing character naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 03. `TetMarket_Woman_ConicalHat_BlueAoBaBa_Center_01` — Người phụ nữ áo xanh đội nón lá ở giữa


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a Vietnamese woman in the center wearing a light blue áo bà ba style outfit, black pants, sandals, and a conical hat, standing in profile and holding a small kumquat pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Preserve the calm standing pose and side-facing angle. Complete any small hidden hand or pot details naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 04. `TetMarket_ManMotorbikeHelmet_CenterRight_01` — Người đàn ông đội mũ bảo hiểm đứng cạnh xe máy


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a Vietnamese man near the center-right wearing a blue motorcycle helmet, light beige jacket, dark pants, and sandals, standing beside a motorbike in a Tet market scene. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The body overlaps with the motorbike. Reconstruct the complete standing character as a standalone NPC, including legs, feet, arms, and jacket edges.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 05. `TetMarket_WomanPinkDress_RightBack_01` — Người phụ nữ áo hồng bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a Vietnamese woman on the right side, seen mostly from behind, wearing a soft pink outfit with long dark hair, standing and looking toward the flower stalls. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Her lower body is partly covered by peach blossom branches and pots. Complete the full body naturally while keeping the back-facing angle.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 06. `TetMarket_ManGreenHelmet_RightBack_01` — Người đàn ông đội mũ cối bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a Vietnamese man on the far right wearing a green pith helmet, light green jacket, pants, and sandals, standing from a rear three-quarter angle while looking at flowers. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. His lower body and side are partly hidden by flowers. Reconstruct the full standing figure naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 07. `TetMarket_BackgroundPerson_WhiteShirt_Center_01` — Người áo trắng nhỏ ở trung cảnh


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small background NPC in the center distance wearing a white shirt and dark pants, standing in a Vietnamese Tet market street. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Small distant character. Complete the full simple silhouette while keeping the smaller background scale.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 08. `TetMarket_BackgroundPerson_PurpleShirt_Center_01` — Người áo tím nhỏ ở trung cảnh


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small background NPC in the center distance wearing a purple top, standing or walking in the Tet market street. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Small distant character. Reconstruct a clean complete silhouette without adding extra people.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 09. `Motorbike_Green_Foreground_Left_01` — Xe máy xanh lớn tiền cảnh bên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a vintage green Vietnamese motorbike in the bottom-left foreground, angled diagonally into the scene, with white front shield, chrome rack, visible wheels, handlebars, seat, and detailed painted body. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The front wheel and lower parts are cut off by the image edge. Reconstruct the full motorbike naturally, including complete front wheel, tires, stand, pedals, and lower frame.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 10. `Motorbike_BlueGray_CenterRight_01` — Xe máy xanh/xám ở giữa phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a blue-gray motorbike at center-right, parked at a slight diagonal angle beside the helmeted man, with visible front wheel, handlebar, seat, body frame, and rear section. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Parts of the motorbike are hidden by the man. Reconstruct the complete vehicle naturally as a standalone asset.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 11. `YellowApricotBlossom_LargePot_Foreground_01` — Chậu mai vàng lớn tiền cảnh giữa dưới


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large potted yellow apricot blossom tree in the foreground, with many bright yellow flowers, green leaves, branching stems, and a large round ceramic pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Preserve the full lush rounded silhouette and foreground scale.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 12. `YellowApricotBlossom_LargePot_LeftStall_01` — Chậu mai vàng lớn bên trái sạp


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large potted yellow apricot blossom tree on the left market stall area, with tall branching stems, many yellow flowers, and a terracotta pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Some branches overlap with lantern strings and stall elements. Complete the flower tree naturally as a standalone asset.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 13. `YellowApricotBlossom_SmallPot_BottomLeft_01` — Chậu mai vàng nhỏ góc dưới trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small potted yellow apricot blossom plant at the bottom-left area near the motorbike, with yellow flowers and green leaves in a small pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Partly hidden by the motorbike and image edge. Complete the full pot and flower silhouette naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 14. `YellowApricotBlossom_Pots_LeftShelf_01` — Cụm chậu mai vàng trên kệ trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small cluster of potted yellow apricot blossom plants displayed on the left wooden market shelf, with multiple terracotta pots and bright yellow flowers. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Treat as one grouped decor asset. Complete any pots or branches hidden by the stall structure.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 15. `PinkPeachBlossom_Tree_CenterLeft_01` — Cây đào hồng trung tâm phía sau


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a potted pink peach blossom tree near the center-left background, with many soft pink flowers, dark branching twigs, and a market display pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The base and lower branches are partly hidden by kumquat trees and people. Reconstruct the complete potted tree naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 16. `PinkPeachBlossom_LargePot_Right_01` — Chậu đào hồng lớn bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large potted pink peach blossom tree on the right side, with tall thin branches, many pink flowers, and a visible basket or pot base. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Partly occluded by people and other flower pots. Complete the full tree and pot naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 17. `PinkPeachBlossom_Pot_ForegroundRight_01` — Chậu đào hồng tiền cảnh phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a foreground right potted pink peach blossom plant with many thin branches, bright pink flowers, and a visible gray bucket or planter base. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full planter and branching silhouette. Keep the same right-side angle.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 18. `KumquatTree_LargePot_Center_01` — Cây quất/cam lớn giữa ảnh


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large potted kumquat tree in the center, full of small orange fruits, glossy green leaves, rounded crown, and a terracotta pot. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Preserve the round fruit tree silhouette and pot. Do not include nearby people or other trees.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 19. `KumquatTree_Small_BottomRight_01` — Cây quất/cam nhỏ dưới phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small kumquat tree at the bottom-right edge, with orange fruits, green leaves, and a compact shrub shape. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The plant is cut off by the image edge. Reconstruct the full small kumquat tree and pot naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 20. `KumquatPot_Handheld_Center_01` — Chậu quất nhỏ người áo xanh đang cầm


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small handheld kumquat pot with green leaves and small orange fruits, like the pot held by the woman in blue at center. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The pot is partly covered by hands. Reconstruct the full small potted plant naturally without including the person.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 21. `TerracottaPot_LeftUnderLantern_01` — Chậu đất nung bên trái dưới đèn lồng lớn


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a standalone terracotta flower pot under the large red lantern on the left, warm brown-orange ceramic material, slightly weathered, with a simple rural market look. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the pot and remove any surrounding stall/background elements.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 22. `TerracottaPot_SellerHolding_Left_01` — Chậu cây đang trao giữa người bán và người mua


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a terracotta potted plant being exchanged between the seller and buyer on the left, with small green foliage and flowers. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Hands cover much of the pot. Reconstruct the full pot, plant, and visible branches naturally without including human hands.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 23. `SquarePlanter_White_Foreground_01` — Chậu vuông trắng dưới chậu mai lớn


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a square white-gray planter box in the foreground under the large yellow blossom plant, slightly worn ceramic or concrete material. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full planter shape. Do not include the yellow blossom tree unless it is explicitly attached.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 24. `GrayBucketPlanter_Right_01` — Chậu nhựa/xô xám bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a gray bucket-style planter on the right side holding pink peach blossom branches, simple metal or plastic bucket shape with rural market texture. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the bucket and branch base naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 25. `WovenBasketPlanter_Right_01` — Giỏ tre chứa cây bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a woven bamboo basket planter on the right side, holding tall pink peach blossom branches, with visible woven texture and warm brown material. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full basket and any hidden lower rim naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 26. `WoodenPlanterBox_Right_01` — Chậu gỗ vuông bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a square wooden planter box on the right side, warm brown planks, rustic Tet market flower display style. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the box shape and remove surrounding plants unless they are part of the planter asset.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 27. `SmallFlowerPots_CenterShelf_01` — Dãy chậu nhỏ trên kệ trung tâm


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a grouped set of small flower pots on a center market shelf, several tiny ceramic pots with green plants and blossoms, arranged as a single decor asset. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Treat as one grouped shelf display item. Complete any hidden pot edges naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 28. `RedLantern_Large_TopLeft_01` — Lồng đèn đỏ lớn trên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large red Vietnamese Tet lantern hanging near the top-left, round paper lantern shape, golden tassels, red hanging cord, warm sunlight, decorative festival style. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the hanging cord and tassel if partly hidden by leaves. Do not include tree branches or stall roof.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 29. `RedLantern_Medium_Left_01` — Lồng đèn đỏ vừa bên trái dưới


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a medium red Tet lantern hanging on the left market stall, round paper lantern with gold trim and tassel. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the cord and tassel naturally. Keep the same smaller scale and angle.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 30. `RedLantern_Small_BackCenter_01` — Lồng đèn đỏ nhỏ phía trung tâm xa


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small distant red Tet lantern in the back-center market area, simple round lantern shape with red color and small gold tassel. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Small background decor. Complete the full tiny lantern silhouette cleanly.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 31. `RedLantern_Large_Right_01` — Lồng đèn đỏ lớn bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large red Tet lantern hanging under the tree on the right, round paper lantern, red cord, gold tassels, warm highlights. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Parts may overlap with tree leaves and hanging ornaments. Complete the full lantern naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 32. `RedLantern_Small_RightEdge_01` — Lồng đèn đỏ nhỏ mép phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small red Tet lantern near the far right edge, hanging among market decorations, with simple round shape and tassel. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Partly hidden and small. Reconstruct a clean complete lantern silhouette.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 33. `TetHangingOrnament_RedVertical_TopLeft_01` — Dây liễn đỏ dọc dưới lồng đèn lớn trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a vertical red Tet hanging ornament string near the top-left lantern, made of multiple red diamond ornaments with gold decorative patterns and tassels. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full vertical ornament string. Avoid readable text; use decorative gold marks only.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 34. `TetHangingOrnament_RedVertical_LeftStall_01` — Dây liễn đỏ dọc bên trái sạp


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a red vertical Tet hanging charm on the far-left stall, with red rectangular/diamond shapes and gold decorative accents. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full hanging charm string. Use decorative marks, not readable text.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 35. `TetHangingOrnament_RedVertical_MidLeft_01` — Dây liễn đỏ dọc giữa trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a red vertical Tet hanging ornament string in the mid-left stall area, with several red diamond charms and small tassels. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the string naturally. Do not include the stall roof or flowers.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 36. `TetHangingOrnament_RedVertical_Right_01` — Dây liễn đỏ dọc bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a red vertical Tet hanging ornament string under the right lantern, with gold decorative marks, red diamonds, and tassel details. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the ornament string and keep the right-side hanging angle.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 37. `TetHangingOrnament_RedSmall_BackgroundSet_01` — Set dây trang trí đỏ nhỏ trong chợ


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small set of distant red Tet hanging ornaments from the market background, tiny red charms with gold decorative marks, grouped as one decor asset. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Use as a grouped background-decor sticker asset. Do not add readable text.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 38. `MarketUmbrella_Orange_CenterRight_01` — Dù cam lớn bên phải trung tâm


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large orange market umbrella at center-right, seen from a slightly low side angle, warm fabric folds, central pole, Vietnamese Tet market style. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the lower fabric and pole if hidden by people or flowers. Do not include stalls or background.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 39. `MarketUmbrella_Blue_BackCenter_01` — Dù xanh dương nhỏ trung tâm


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small blue market umbrella in the back-center distance, soft fabric canopy, simple pole, seen behind the main market crowd. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full canopy silhouette while keeping its distant smaller scale.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 40. `MarketUmbrella_Green_Right_01` — Dù xanh lá bên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a green market umbrella on the right side, low canopy angle, warm sunlight, partly behind people and flowers. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Reconstruct the hidden lower canopy and pole naturally. Do not include people or flower pots.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 41. `MarketAwning_BlueOrange_Left_01` — Mái bạt xanh/cam sạp bên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a blue-and-orange fabric awning from the left flower stall, rustic market canopy with wooden support feel, same angle as in the reference. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. This item is attached to the stall and partly hidden by lanterns. Complete the awning as a standalone market prop.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 42. `FlowerStall_Left_01` — Sạp bán hoa bên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a left-side Vietnamese Tet flower stall as one complete prop, with rustic wooden structure, flower display shelves, hanging red decorations, potted yellow blossoms, and warm market canopy. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Treat as one complete stall asset, not separate modular pieces. Reconstruct parts hidden by people, motorbike, and flowers naturally.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 43. `WoodenDisplayShelf_Left_01` — Kệ gỗ thấp bên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a rustic wooden display shelf from the left flower stall, low market shelf used for potted flowers, warm aged wood, hand-painted texture. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The shelf is covered by flowers and the motorbike. Reconstruct the full shelf naturally without including extra pots unless needed for context.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 44. `TreeBranch_Canopy_TopLeft_01` — Cành cây lớn phía trên trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large overhanging tree branch canopy from the top-left, with dark brown branches and many green-yellow leaves, used as a foreground decor layer. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The branch is cut by the image edges. Reconstruct a complete natural branch cluster with clean sticker silhouette.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 45. `TreeCanopy_TopRight_01` — Tán cây lớn phía trên phải


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a large leafy tree canopy from the top-right, green leaves and branches casting a cozy market shade, foreground/background decor layer. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. The canopy is cut by image edges and overlaps with buildings. Complete the leafy canopy naturally as a standalone decor asset.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 46. `PalmTree_BackCenter_01` — Cây cọ/dừa phía xa trung tâm


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a distant palm tree in the back-center of the Tet market street, tall slender trunk with green palm leaves, tropical Vietnamese village look. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Complete the full trunk and leaf crown without including distant houses.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 47. `TetRedSquareHangingCharm_Left_01` — Biển/liễn đỏ vuông nhỏ treo dưới mái trái


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small set of red square Tet hanging charms under the left stall roof, red paper ornaments with gold decorative marks and tiny tassels. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Use decorative gold marks only, no readable text. Complete each charm in the grouped set.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 48. `TetRedDecor_BackCenter_01` — Cụm dây trang trí đỏ trung cảnh


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small grouped set of red Tet decorations in the back-center market area, tiny hanging charms and red festive accents. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Grouped background-decor asset. Keep it simple, no readable text, no extra scene elements.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.

### 49. `SmallPlantPot_HandExchange_Left_01` — Chậu/giỏ nhỏ người bán đang cầm


Using the provided mockup image as the only visual reference, generate a single isolated 2D sticker-style game asset of a small potted plant being exchanged between two people on the left, with a rustic pot, green leaves, and small yellow blossoms. Preserve the exact original viewing angle, orientation, pose, proportions, lighting, warm hand-painted Vietnamese Tet market style, and painterly details from the reference image. Hands cover much of the item. Reconstruct the full small pot and plant naturally without including hands or people.

Add a thick clean white sticker-style border around the entire completed outer silhouette. Transparent background outside the sticker border. Centered, high-resolution, clean edges. Include only this item, no background, no extra objects.


**Negative prompt:** use the Common negative prompt above.


---

## Ghi chú sử dụng

- Với item bị che/cắt mép: luôn giữ câu `reconstruct and complete the full object naturally`.
- Với mọi item dạng prop/character/vehicle/decor: luôn giữ câu `thick clean white sticker-style border`.
- Không dùng các prompt này cho background. Background nên có prompt riêng, không border sticker.
- Nếu AI tạo ra asset đúng item nhưng thiếu viền trắng, hãy thêm dòng nhấn mạnh: `The sticker border is mandatory and must be clearly visible around the full completed silhouette.`
