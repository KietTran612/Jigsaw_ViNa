# Collapsible Editor Layout Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Modify `JigsawVinaGameDataEditor.cs` to implement a Tab-bar selection interface and collapsible foldouts for the details panel to optimize workspace width.

**Architecture:** Add a global `_detailTabSelected` integer to choose between "Thông tin & Key Items" and "Độ khó & Phần thưởng" tabs. Expand class `EditorTabState` with foldout booleans for each difficulty, and draw panels at full width based on selection.

**Tech Stack:** Unity Editor GUI (IMGUI), Unity 6000.3

---

### Task 1: Update EditorTabState and JigsawVinaGameDataEditor Fields

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)

**Step 1: Write code modifications to define foldout state variables**

Add the detail tab state variable to `JigsawVinaGameDataEditor` and the foldout boolean fields to `EditorTabState`.

In [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs#L13-L49):

```csharp
        private int _detailTabSelected = 0;
```

And in `EditorTabState`:
```csharp
            // Foldout states
            public bool easyExpanded = true;
            public bool normalExpanded = true;
            public bool hardExpanded = true;
```

**Step 2: Compile check**
Verify Unity compiles the field additions without errors.

---

### Task 2: Refactor DrawTabDetails to Support Tab-bar and Foldouts

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs#L276-L400)

**Step 1: Rewrite DrawTabDetails**

Replace the side-by-side two-column layout with tab selection and foldouts.

```csharp
        private void DrawTabDetails(EditorTabState state)
        {
            var prevAsset = state.folderAsset;
            state.folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Thư mục tranh", state.folderAsset, typeof(DefaultAsset), false);

            if (state.folderAsset != prevAsset && state.folderAsset != null)
            {
                AutoFillFromFolder(state);
            }

            if (state.folderAsset == null)
            {
                EditorGUILayout.HelpBox("Hãy kéo thả thư mục chứa tranh vào đây.", MessageType.Info);
                return;
            }

            var (mainTexture, itemTextures) = ScanFolder(state.folderAsset);
            SyncItemStates(state, itemTextures);

            EditorGUILayout.Space();

            // Tab selection toolbar
            string[] detailTabs = { "Thông tin & Key Items", "Độ khó & Phần thưởng" };
            _detailTabSelected = GUILayout.Toolbar(_detailTabSelected, detailTabs);
            EditorGUILayout.Space();

            if (_detailTabSelected == 0)
            {
                // TAB 1: Assets & Metadata (Full width)
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
                GUILayout.Label("Thông tin tranh chính & Key Items", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                // Picture Configuration
                GUILayout.Label("Tranh Chính", EditorStyles.boldLabel);
                if (mainTexture != null)
                {
                    var rect = GUILayoutUtility.GetRect(120, 90, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(rect, mainTexture, ScaleMode.ScaleToFit);
                }
                state.pictureId = EditorGUILayout.IntField("Picture ID", state.pictureId);
                state.idString = EditorGUILayout.TextField("ID String", state.idString);
                state.displayName = EditorGUILayout.TextField("Tên Tranh", state.displayName);

                EditorGUILayout.Space();
                GUILayout.Label("Danh Sách Key Items", EditorStyles.boldLabel);

                if (itemTextures.Count == 0)
                {
                    EditorGUILayout.HelpBox("Không tìm thấy key item nào.", MessageType.None);
                }

                for (int i = 0; i < itemTextures.Count; i++)
                {
                    var tex = itemTextures[i];
                    var itemState = state.itemStates.Find(it => it.filename == tex.name);
                    if (itemState == null) continue;

                    GUILayout.BeginVertical(GUI.skin.box);
                    
                    GUILayout.BeginHorizontal();
                    var r = GUILayoutUtility.GetRect(40, 40, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                    
                    GUILayout.BeginVertical();
                    GUILayout.Label($"File: {tex.name}", EditorStyles.miniBoldLabel);
                    itemState.displayName = EditorGUILayout.TextField("Tên hiển thị", itemState.displayName);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    itemState.description = EditorGUILayout.TextField("Mô tả", itemState.description);
                    
                    string[] rarities = { "common", "uncommon", "rare", "epic", "legendary" };
                    int rarityIdx = Mathf.Max(0, Array.IndexOf(rarities, itemState.rarity));
                    rarityIdx = EditorGUILayout.Popup("Độ hiếm", rarityIdx, rarities);
                    itemState.rarity = rarities[rarityIdx];

                    GUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                GUILayout.EndVertical();
            }
            else
            {
                // TAB 2: Difficulties & Rewards (Full width with foldouts)
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
                GUILayout.Label("Cấu hình độ khó & Phần thưởng", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                string[] itemNames = new string[itemTextures.Count + 1];
                itemNames[0] = "None";
                for (int i = 0; i < itemTextures.Count; i++)
                {
                    itemNames[i + 1] = itemTextures[i].name;
                }

                // EASY
                state.easyExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(state.easyExpanded, "DỄ (Easy)");
                if (state.easyExpanded)
                {
                    EditorGUI.indentLevel++;
                    state.easyCols = EditorGUILayout.IntField("Columns", state.easyCols);
                    state.easyRows = EditorGUILayout.IntField("Rows", state.easyRows);
                    state.easyCoins = EditorGUILayout.IntField("First Clear Coin", state.easyCoins);
                    state.easyReplayCoins = EditorGUILayout.IntField("Replay Coin", state.easyReplayCoins);
                    state.easyHints = EditorGUILayout.IntField("First Clear Hint", state.easyHints);
                    state.easyKeyRewardIndex = EditorGUILayout.Popup("Reward Key Item", state.easyKeyRewardIndex, itemNames);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();

                // NORMAL
                state.normalExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(state.normalExpanded, "TRUNG BÌNH (Normal)");
                if (state.normalExpanded)
                {
                    EditorGUI.indentLevel++;
                    state.normalCols = EditorGUILayout.IntField("Columns", state.normalCols);
                    state.normalRows = EditorGUILayout.IntField("Rows", state.normalRows);
                    state.normalCoins = EditorGUILayout.IntField("First Clear Coin", state.normalCoins);
                    state.normalReplayCoins = EditorGUILayout.IntField("Replay Coin", state.normalReplayCoins);
                    state.normalHints = EditorGUILayout.IntField("First Clear Hint", state.normalHints);
                    state.normalKeyRewardIndex = EditorGUILayout.Popup("Reward Key Item", state.normalKeyRewardIndex, itemNames);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();

                // HARD
                state.hardExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(state.hardExpanded, "KHÓ (Hard)");
                if (state.hardExpanded)
                {
                    EditorGUI.indentLevel++;
                    state.hardCols = EditorGUILayout.IntField("Columns", state.hardCols);
                    state.hardRows = EditorGUILayout.IntField("Rows", state.hardRows);
                    state.hardCoins = EditorGUILayout.IntField("First Clear Coin", state.hardCoins);
                    state.hardReplayCoins = EditorGUILayout.IntField("Replay Coin", state.hardReplayCoins);
                    state.hardHints = EditorGUILayout.IntField("First Clear Hint", state.hardHints);
                    state.hardKeyRewardIndex = EditorGUILayout.Popup("Reward Key Item", state.hardKeyRewardIndex, itemNames);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                GUILayout.EndVertical();
            }
        }
```

**Step 2: Compile Check**
Verify the code compiles without warnings/errors.
Check that Editor compiling and domain reloading complete successfully.

**Step 3: Verification**
Open the Editor window in Unity and check:
- Detail view tab selection works.
- Foldouts expand/collapse correctly.
- Layout adapts to full width.
- Save config outputs JSON correctly.
