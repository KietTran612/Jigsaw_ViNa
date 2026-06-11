# Bulk Add Pictures Editor Feature Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Add a "Thêm Nhiều Tranh Từ Thư Mục" button to `JigsawVinaGameDataEditor.cs` that opens a folder selection window to scan and import multiple picture folders simultaneously, checking for and logging duplicate main pictures.

**Architecture:** Implement a `BulkAddPictures` method in the editor class. It will open `EditorUtility.OpenFolderPanel`, scan subdirectories, load folder assets via `AssetDatabase.LoadAssetAtPath`, call existing `ScanFolder`/`AutoFillFromFolder`/`SyncItemStates` helpers, check for main name duplicates, log warnings to the Console, and display a summary dialog.

**Tech Stack:** Unity Editor GUI, Unity 6000.3

---

### Task 1: Add Bulk Add Button and Implement BulkAddPictures Method

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)

**Step 1: Write code modifications to draw the button**

In [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs#L190-L210), add the toolbar button right after "Thêm Tranh Mới":

```csharp
            if (GUILayout.Button("Thêm Nhiều Tranh Từ Thư Mục", EditorStyles.toolbarButton, GUILayout.Width(180)))
            {
                BulkAddPictures();
            }
```

**Step 2: Add BulkAddPictures method**

Add the bulk scanning logic at the end of the class before `DrawTextureWithBorder`:

```csharp
        private void BulkAddPictures()
        {
            string absolutePath = EditorUtility.OpenFolderPanel("Chọn thư mục cha chứa các thư mục tranh (ví dụ: Pictures)", "Assets", "");
            if (string.IsNullOrEmpty(absolutePath)) return;

            string normalizedPath = absolutePath.Replace("\\", "/");
            if (!normalizedPath.Contains("/Assets/Resources/"))
            {
                EditorUtility.DisplayDialog("Lỗi Thư Mục", "Thư mục được chọn phải nằm bên trong 'Assets/Resources/'.", "OK");
                return;
            }

            int assetsIndex = normalizedPath.IndexOf("/Assets/");
            if (assetsIndex == -1)
            {
                EditorUtility.DisplayDialog("Lỗi Thư Mục", "Không tìm thấy thư mục Assets trong đường dẫn.", "OK");
                return;
            }
            string parentRelativePath = normalizedPath.Substring(assetsIndex + 1);

            string[] subdirs = Directory.GetDirectories(absolutePath);
            if (subdirs.Length == 0)
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy thư mục con nào trong thư mục được chọn.", "OK");
                return;
            }

            int addedCount = 0;
            List<string> duplicateMainNames = new();
            List<string> missingMainNames = new();

            var existingMainNames = new HashSet<string>();
            foreach (var tab in _tabs)
            {
                if (tab.folderAsset != null)
                {
                    var (main, _) = ScanFolder(tab.folderAsset);
                    if (main != null)
                    {
                        existingMainNames.Add(main.name);
                    }
                }
            }

            foreach (string subdir in subdirs)
            {
                string subdirName = Path.GetFileName(subdir);
                string relativeSubdirPath = $"{parentRelativePath}/{subdirName}";

                DefaultAsset folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relativeSubdirPath);
                if (folderAsset == null) continue;

                var (mainTex, itemTexs) = ScanFolder(folderAsset);
                if (mainTex == null)
                {
                    missingMainNames.Add(subdirName);
                    continue;
                }

                if (existingMainNames.Contains(mainTex.name))
                {
                    duplicateMainNames.Add($"{subdirName} (ảnh trùng: {mainTex.name})");
                    Debug.LogWarning($"[JigsawVina Editor] Trùng tên ảnh chính: Thư mục '{subdirName}' có ảnh chính '{mainTex.name}' trùng với một tranh khác.");
                    continue;
                }

                int nextId = 1;
                foreach (var t in _tabs)
                {
                    if (t.pictureId >= nextId) nextId = t.pictureId + 1;
                }

                var state = new EditorTabState
                {
                    pictureId = nextId,
                    folderAsset = folderAsset
                };

                AutoFillFromFolder(state);
                SyncItemStates(state, itemTexs);

                _tabs.Add(state);
                existingMainNames.Add(mainTex.name);
                addedCount++;
            }

            AssetDatabase.Refresh();

            string msg = $"Đã thêm thành công {addedCount} tranh mới.\n";
            if (duplicateMainNames.Count > 0)
            {
                msg += $"\n[CẢNH BÁO] Phát hiện {duplicateMainNames.Count} thư mục trùng tên tranh chính (đã bỏ qua):\n - " + string.Join("\n - ", duplicateMainNames) + "\n";
            }
            if (missingMainNames.Count > 0)
            {
                msg += $"\n[CẢNH BÁO] Phát hiện {missingMainNames.Count} thư mục thiếu ảnh chính (đã bỏ qua):\n - " + string.Join("\n - ", missingMainNames) + "\n";
            }

            EditorUtility.DisplayDialog("Hoàn Thành Quét", msg, "OK");

            if (addedCount > 0)
            {
                _selectedIndex = _tabs.Count - 1;
            }
        }
```

**Step 3: Compile Check**
Verify the code compiles without warnings/errors.
Check that Editor compiling and domain reloading complete successfully.

**Step 4: Verification**
Open the Editor window in Unity and check:
- "Thêm Nhiều Tranh Từ Thư Mục" button is present in the toolbar.
- Clicking opens folder panel. Selecting a folder containing subdirectories successfully scans, logs warnings for duplicate main pictures, skips folders without MAIN_ images, and adds the valid folders dynamically.
