#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using JigsawVina.Core.Data;
using UnityEditor;
using UnityEngine;

namespace JigsawVina.Editor
{
    public class JigsawVinaGameDataEditor : EditorWindow
    {
        private const string SavePath = "Assets/Resources/GameData/jigsaw_vina_game_data.json";
        private Vector2 _sidebarScroll;
        private Vector2 _detailScroll;
        private int _selectedIndex = 0;
        private int _detailTabSelected = 0;

        [Serializable]
        private class EditorItemState
        {
            public string filename = "";
            public string displayName = "";
            public string description = "";
            public string rarity = "common";
        }

        [Serializable]
        private class EditorTabState
        {
            public DefaultAsset folderAsset;
            public int pictureId;
            public string idString = "";
            public string displayName = "";
            public List<EditorItemState> itemStates = new();
            
            // Foldout states
            public bool easyExpanded = true;
            public bool normalExpanded = true;
            public bool hardExpanded = true;

            // Difficulty settings
            public int easyCols = 6, easyRows = 4;
            public int easyCoins = 30, easyReplayCoins = 10, easyHints = 0;
            public int easyKeyRewardIndex = 0;

            public int normalCols = 8, normalRows = 6;
            public int normalCoins = 60, normalReplayCoins = 20, normalHints = 0;
            public int normalKeyRewardIndex = 0;

            public int hardCols = 12, hardRows = 8;
            public int hardCoins = 120, hardReplayCoins = 40, hardHints = 0;
            public int hardKeyRewardIndex = 0;
        }

        [SerializeField] private List<EditorTabState> _tabs = new();

        [MenuItem("JigsawVina/Game Data Editor")]
        public static void ShowWindow()
        {
            GetWindow<JigsawVinaGameDataEditor>("Game Data Editor");
        }

        private void OnEnable()
        {
            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            _tabs.Clear();
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    var dto = JsonUtility.FromJson<StaticDataDto>(json);
                    if (dto != null && dto.pictures != null)
                    {
                        var diffsByPic = new Dictionary<int, List<PictureDifficultyDto>>();
                        if (dto.picture_difficulties != null)
                        {
                            foreach (var diff in dto.picture_difficulties)
                            {
                                if (!diffsByPic.ContainsKey(diff.picture_id))
                                    diffsByPic[diff.picture_id] = new List<PictureDifficultyDto>();
                                diffsByPic[diff.picture_id].Add(diff);
                            }
                        }

                        for (int i = 0; i < dto.pictures.Count; i++)
                        {
                            var pic = dto.pictures[i];
                            var state = new EditorTabState();
                            state.pictureId = pic.id;
                            state.idString = pic.id_string;
                            state.displayName = pic.display_name;

                            // Reconstruct folder asset path
                            if (!string.IsNullOrEmpty(pic.asset_path))
                            {
                                string relativeDir = Path.GetDirectoryName(pic.asset_path).Replace("\\", "/");
                                string folderPath = $"Assets/Resources/{relativeDir}";
                                state.folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                            }

                            // Load custom item states from global item list
                            state.itemStates.Clear();
                            if (dto.items != null)
                            {
                                foreach (var item in dto.items)
                                {
                                    if (item.id >= pic.id * 100 && item.id < (pic.id + 1) * 100)
                                    {
                                        string filename = Path.GetFileNameWithoutExtension(item.asset_path);
                                        state.itemStates.Add(new EditorItemState
                                        {
                                            filename = filename,
                                            displayName = item.display_name,
                                            description = item.description,
                                            rarity = string.IsNullOrEmpty(item.rarity) ? "common" : item.rarity
                                        });
                                    }
                                }
                            }

                            // Scan to match reward indices mathematically via stable ID formula
                            if (state.folderAsset != null)
                            {
                                var (_, scannedItems) = ScanFolder(state.folderAsset);
                                SyncItemStates(state, scannedItems);

                                if (diffsByPic.TryGetValue(pic.id, out var picDiffs))
                                {
                                    foreach (var d in picDiffs)
                                    {
                                        int rewardIdx = 0;
                                        if (d.first_clear_reward_item_ids != null && d.first_clear_reward_item_ids.Count > 0)
                                        {
                                            int rewardId = d.first_clear_reward_item_ids[0];
                                            int calculatedIndex = (rewardId - pic.id * 100) - 1;
                                            if (calculatedIndex >= 0 && calculatedIndex < scannedItems.Count)
                                            {
                                                rewardIdx = calculatedIndex + 1;
                                            }
                                        }

                                        if (d.difficulty_id == 0)
                                        {
                                            state.easyCols = d.grid_columns;
                                            state.easyRows = d.grid_rows;
                                            state.easyCoins = d.first_clear_coin;
                                            state.easyReplayCoins = d.replay_coin;
                                            state.easyHints = d.first_clear_hint;
                                            state.easyKeyRewardIndex = rewardIdx;
                                        }
                                        else if (d.difficulty_id == 1)
                                        {
                                            state.normalCols = d.grid_columns;
                                            state.normalRows = d.grid_rows;
                                            state.normalCoins = d.first_clear_coin;
                                            state.normalReplayCoins = d.replay_coin;
                                            state.normalHints = d.first_clear_hint;
                                            state.normalKeyRewardIndex = rewardIdx;
                                        }
                                        else if (d.difficulty_id == 2)
                                        {
                                            state.hardCols = d.grid_columns;
                                            state.hardRows = d.grid_rows;
                                            state.hardCoins = d.first_clear_coin;
                                            state.hardReplayCoins = d.replay_coin;
                                            state.hardHints = d.first_clear_hint;
                                            state.hardKeyRewardIndex = rewardIdx;
                                        }
                                    }
                                }
                            }
                            _tabs.Add(state);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JigsawVina Editor] Could not parse existing jigsaw_vina_game_data.json: {e.Message}");
                }
            }

            if (_tabs.Count == 0)
            {
                _tabs.Add(new EditorTabState { pictureId = 1 });
            }
        }

        private void OnGUI()
        {
            // Top Toolbar Section
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Thêm Tranh Mới", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                int nextId = 1;
                foreach (var t in _tabs)
                {
                    if (t.pictureId >= nextId) nextId = t.pictureId + 1;
                }
                _tabs.Add(new EditorTabState { pictureId = nextId });
                _selectedIndex = _tabs.Count - 1;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Save & Generate JSON", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                SaveConfig();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            // Left Pane: Sidebar list of pictures
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200), GUILayout.ExpandHeight(true));
            GUILayout.Label("Danh sách Tranh", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawDragAndDropArea();
            EditorGUILayout.Space();

            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                string nameDisplay = string.IsNullOrEmpty(tab.displayName) ? "Chưa đặt tên" : tab.displayName;
                string label = $"Tranh {tab.pictureId}: {nameDisplay}";
                if (label.Length > 22)
                {
                    label = label.Substring(0, 19) + "...";
                }

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.alignment = TextAnchor.MiddleLeft;
                style.clipping = TextClipping.Clip;
                if (i == _selectedIndex)
                {
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.cyan;
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(label, style, GUILayout.Width(150)))
                {
                    _selectedIndex = i;
                }
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Xóa Tranh", $"Bạn có chắc chắn muốn xóa Tranh {tab.pictureId}?", "Có", "Không"))
                    {
                        _tabs.RemoveAt(i);
                        if (_selectedIndex >= _tabs.Count)
                        {
                            _selectedIndex = _tabs.Count - 1;
                        }
                        i--;
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.EndVertical();

            // Right Pane: Detail view of selected picture
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (_selectedIndex >= 0 && _selectedIndex < _tabs.Count)
            {
                _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
                DrawTabDetails(_tabs[_selectedIndex]);
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Hãy chọn hoặc thêm mới một tranh từ danh sách bên trái.", EditorStyles.centeredGreyMiniLabel);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

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
                GUILayout.Label("Tranh Chính (Click vào hình để tìm file)", EditorStyles.boldLabel);
                if (mainTexture != null)
                {
                    GUILayout.BeginHorizontal();
                    var rect = GUILayoutUtility.GetRect(120, 90, GUILayout.ExpandWidth(false));
                    DrawTextureWithBorder(rect, mainTexture, ScaleMode.ScaleToFit);
                    if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        Selection.activeObject = mainTexture;
                        EditorGUIUtility.PingObject(mainTexture);
                        Event.current.Use();
                    }

                    GUILayout.Space(10);
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Sprite Editor", GUILayout.Width(100)))
                    {
                        Selection.activeObject = mainTexture;
                        EditorGUIUtility.PingObject(mainTexture);
                        try
                        {
                            EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor");
                        }
                        catch (Exception)
                        {
                            AssetDatabase.OpenAsset(mainTexture);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
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

                    Color oldBgColor = GUI.backgroundColor;
                    GUI.backgroundColor = (i % 2 == 0)
                        ? new Color(1.5f, 1.5f, 1.5f, 1.0f)
                        : new Color(0.6f, 0.6f, 0.6f, 1.0f);

                    GUILayout.BeginVertical(GUI.skin.box);
                    
                    GUILayout.BeginHorizontal();
                    var r = GUILayoutUtility.GetRect(40, 40, GUILayout.ExpandWidth(false));
                    DrawTextureWithBorder(r, tex, ScaleMode.ScaleToFit);
                    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    {
                        Selection.activeObject = tex;
                        EditorGUIUtility.PingObject(tex);
                        Event.current.Use();
                    }

                    GUILayout.Space(10);
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"File: {tex.name}", EditorStyles.miniBoldLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Sprite Editor", GUILayout.Width(100)))
                    {
                        Selection.activeObject = tex;
                        EditorGUIUtility.PingObject(tex);
                        try
                        {
                            EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor");
                        }
                        catch (Exception)
                        {
                            AssetDatabase.OpenAsset(tex);
                        }
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    itemState.displayName = EditorGUILayout.TextField("Tên hiển thị", itemState.displayName);
                    itemState.description = EditorGUILayout.TextField("Mô tả", itemState.description);
                    
                    string[] rarities = { "common", "uncommon", "rare", "epic", "legendary" };
                    int rarityIdx = Mathf.Max(0, Array.IndexOf(rarities, itemState.rarity));
                    rarityIdx = EditorGUILayout.Popup("Độ hiếm", rarityIdx, rarities);
                    itemState.rarity = rarities[rarityIdx];

                    GUILayout.EndVertical();
                    GUI.backgroundColor = oldBgColor;
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

        private void SyncItemStates(EditorTabState state, List<Texture2D> scannedItems)
        {
            var scannedNames = new HashSet<string>();
            foreach (var tex in scannedItems)
            {
                scannedNames.Add(tex.name);
            }
            state.itemStates.RemoveAll(it => !scannedNames.Contains(it.filename));

            foreach (var tex in scannedItems)
            {
                var existing = state.itemStates.Find(it => it.filename == tex.name);
                if (existing == null)
                {
                    state.itemStates.Add(new EditorItemState
                    {
                        filename = tex.name,
                        displayName = tex.name.Replace("_", " "),
                        description = "",
                        rarity = "common"
                    });
                }
            }
        }

        private void AutoFillFromFolder(EditorTabState state)
        {
            var (main, _) = ScanFolder(state.folderAsset);
            if (main != null)
            {
                state.idString = main.name.Replace("MAIN_", "").ToLower();
                state.displayName = main.name.Replace("MAIN_", "").Replace("_", " ");
            }
        }

        private (Texture2D main, List<Texture2D> items) ScanFolder(DefaultAsset folder)
        {
            var itemTexs = new List<Texture2D>();
            if (folder == null)
            {
                return (null, itemTexs);
            }

            string path = AssetDatabase.GetAssetPath(folder);
            Texture2D mainTex = null;

            // Get absolute folder path for robust IO scanning
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
            if (!Directory.Exists(fullPath))
            {
                return (null, itemTexs);
            }

            var filePaths = Directory.GetFiles(fullPath, "*.png");
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var assetRelativePath = $"{path}/{fileName}";
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetRelativePath);
                if (tex == null) continue;

                if (tex.name.StartsWith("MAIN_"))
                {
                    mainTex = tex;
                }
                else
                {
                    itemTexs.Add(tex);
                }
            }

            // Alphabetically sort the scanned items to ensure deterministic ID assignment
            itemTexs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            return (mainTex, itemTexs);
        }

        private void SaveConfig()
        {
            var config = new StaticDataDto();
            config.schema_version = 1;
            config.data_version = 1;

            config.categories.Add(new CategoryDto
            {
                id = 1,
                id_string = "vietnam_landscapes",
                display_name = "Phong Cảnh Việt Nam"
            });

            var validatedPicIds = new HashSet<int>();
            var validatedPicIdStrings = new HashSet<string>();
            var validatedItemIdStrings = new HashSet<string>();

            // Always add coin and hint currency defaults to static item database to satisfy validator requirements
            config.items.Add(new ItemDto
            {
                id = 1,
                id_string = "coin",
                display_name = "Xu",
                description = "Đơn vị tiền cơ bản trong game.",
                display_name_key = "item.coin.name",
                description_key = "item.coin.description",
                item_type = "currency",
                rarity = "common",
                is_consumable = true,
                is_time_limited = false,
                max_stack = 999999,
                status = "active",
                sort_order = 1,
                asset_path = ""
            });

            config.items.Add(new ItemDto
            {
                id = 2,
                id_string = "hint",
                display_name = "Gợi Ý",
                description = "Vật phẩm hỗ trợ người chơi ghép tranh.",
                display_name_key = "item.hint.name",
                description_key = "item.hint.description",
                item_type = "currency",
                rarity = "common",
                is_consumable = true,
                is_time_limited = false,
                max_stack = 9999,
                status = "active",
                sort_order = 2,
                asset_path = ""
            });

            foreach (var tab in _tabs)
            {
                if (tab.folderAsset == null) continue;

                string folderPath = AssetDatabase.GetAssetPath(tab.folderAsset);
                if (!folderPath.StartsWith("Assets/Resources/"))
                {
                    EditorUtility.DisplayDialog("Lỗi Thư Mục", $"Thư mục '{folderPath}' phải nằm bên trong thư mục 'Assets/Resources/'.", "OK");
                    return;
                }

                var (main, items) = ScanFolder(tab.folderAsset);
                if (main == null)
                {
                    EditorUtility.DisplayDialog("Thiếu Tranh Chính", $"Không tìm thấy ảnh chính có prefix 'MAIN_' trong thư mục: {tab.folderAsset.name}", "OK");
                    return;
                }

                if (!validatedPicIds.Add(tab.pictureId))
                {
                    EditorUtility.DisplayDialog("Trùng ID Tranh", $"ID Tranh '{tab.pictureId}' bị trùng giữa các bức tranh.", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(tab.idString) || !validatedPicIdStrings.Add(tab.idString))
                {
                    EditorUtility.DisplayDialog("Trùng ID String Tranh", $"ID String '{tab.idString}' bị trùng hoặc trống.", "OK");
                    return;
                }

                // Strip Assets/Resources/ safely via Substring to avoid global Replace side effects
                string resourceFolder = folderPath.Substring("Assets/Resources/".Length);
                string mainPath = $"{resourceFolder}/{main.name}";
                
                config.pictures.Add(new PictureDto
                {
                    id = tab.pictureId,
                    id_string = tab.idString,
                    display_name = tab.displayName,
                    category_id = 1,
                    asset_path = mainPath,
                    difficulty_unlock_policy = "sequential"
                });

                var localItems = new Dictionary<string, int>();
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    var itTex = items[itemIndex];
                    string itemIdString = itTex.name.ToLower();
                    
                    if (!validatedItemIdStrings.Add(itemIdString))
                    {
                        EditorUtility.DisplayDialog("Trùng ID String Vật Phẩm", $"Tên file vật phẩm '{itTex.name}' bị trùng lặp trong dự án. Vui lòng sử dụng tên file duy nhất.", "OK");
                        return;
                    }

                    var itemState = tab.itemStates.Find(it => it.filename == itTex.name);
                    string dispName = itemState != null ? itemState.displayName : itTex.name.Replace("_", " ");
                    string desc = itemState != null ? itemState.description : "";
                    string rarity = itemState != null ? itemState.rarity : "common";

                    int itemId = tab.pictureId * 100 + (itemIndex + 1); // Stable, tab-isolated ID formula
                    string itPath = $"{resourceFolder}/{itTex.name}";
                    config.items.Add(new ItemDto
                    {
                        id = itemId,
                        id_string = itemIdString,
                        display_name = dispName,
                        description = desc,
                        display_name_key = $"item.{itemIdString}.name",
                        description_key = $"item.{itemIdString}.description",
                        item_type = "key_item",
                        rarity = rarity,
                        is_consumable = false,
                        is_time_limited = false,
                        max_stack = 1,
                        status = "active",
                        sort_order = itemId,
                        asset_path = itPath
                    });
                    localItems[itTex.name] = itemId;
                }

                AddDifficulty(config, tab.pictureId, 0, "Dễ", tab.easyCols, tab.easyRows, tab.easyCoins, tab.easyReplayCoins, tab.easyHints, tab.easyKeyRewardIndex, items, localItems);
                AddDifficulty(config, tab.pictureId, 1, "Trung bình", tab.normalCols, tab.normalRows, tab.normalCoins, tab.normalReplayCoins, tab.normalHints, tab.normalKeyRewardIndex, items, localItems);
                AddDifficulty(config, tab.pictureId, 2, "Khó", tab.hardCols, tab.hardRows, tab.hardCoins, tab.hardReplayCoins, tab.hardHints, tab.hardKeyRewardIndex, items, localItems);
            }

            // Sort DTOs for deterministic, clean JSON output and clean git diffs
            config.pictures.Sort((a, b) => a.id.CompareTo(b.id));
            config.items.Sort((a, b) => a.id.CompareTo(b.id));
            config.picture_difficulties.Sort((a, b) =>
            {
                int comp = a.picture_id.CompareTo(b.picture_id);
                if (comp != 0) return comp;
                return a.difficulty_id.CompareTo(b.difficulty_id);
            });

            string json = JsonUtility.ToJson(config, true);
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[JigsawVina Editor] Static data written successfully to {SavePath}.");
            EditorUtility.DisplayDialog("Hoàn Thành", $"Đã lưu và cấu hình static data tại {SavePath}!", "OK");
        }

        private void AddDifficulty(StaticDataDto config, int pictureId, int diffId, string displayName, int cols, int rows, int firstClearCoins, int replayCoins, int firstClearHints, int rewardIndex, List<Texture2D> items, Dictionary<string, int> localItems)
        {
            var listRewards = new List<int>();
            if (rewardIndex > 0 && rewardIndex <= items.Count)
            {
                var texName = items[rewardIndex - 1].name;
                if (localItems.TryGetValue(texName, out int itemId))
                {
                    listRewards.Add(itemId);
                }
            }

            config.picture_difficulties.Add(new PictureDifficultyDto
            {
                picture_id = pictureId,
                difficulty_id = diffId,
                display_name = displayName,
                grid_columns = cols,
                grid_rows = rows,
                piece_count = cols * rows,
                star_reward = diffId + 1, // Easy = 1, Normal = 2, Hard = 3
                first_clear_coin = firstClearCoins,
                first_clear_hint = firstClearHints,
                replay_coin = replayCoins,
                first_clear_reward_item_ids = listRewards
            });
        }



        private void AddMultipleFolders(List<DefaultAsset> folderAssets)
        {
            if (folderAssets == null || folderAssets.Count == 0) return;

            int addedCount = 0;
            List<string> duplicateMainNames = new();
            List<string> missingMainNames = new();
            List<string> invalidPathNames = new();

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

            foreach (var folderAsset in folderAssets)
            {
                string folderPath = AssetDatabase.GetAssetPath(folderAsset);
                if (!folderPath.StartsWith("Assets/Resources/"))
                {
                    invalidPathNames.Add(folderAsset.name);
                    continue;
                }

                var (mainTex, itemTexs) = ScanFolder(folderAsset);
                if (mainTex == null)
                {
                    missingMainNames.Add(folderAsset.name);
                    continue;
                }

                if (existingMainNames.Contains(mainTex.name))
                {
                    duplicateMainNames.Add($"{folderAsset.name} (ảnh trùng: {mainTex.name})");
                    Debug.LogWarning($"[JigsawVina Editor] Trùng tên ảnh chính: Thư mục '{folderAsset.name}' có ảnh chính '{mainTex.name}' trùng với một tranh khác.");
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
            if (invalidPathNames.Count > 0)
            {
                msg += $"\n[CẢNH BÁO] Phát hiện {invalidPathNames.Count} thư mục nằm ngoài Assets/Resources/ (đã bỏ qua):\n - " + string.Join("\n - ", invalidPathNames) + "\n";
            }

            EditorUtility.DisplayDialog("Hoàn Thành Quét", msg, "OK");

            if (addedCount > 0)
            {
                _selectedIndex = _tabs.Count - 1;
            }
        }

        private void DrawDragAndDropArea()
        {
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 40.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Kéo thả nhiều Folder\ntranh vào đây", GUI.skin.box);
            
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        List<DefaultAsset> droppedFolders = new List<DefaultAsset>();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is DefaultAsset asset)
                            {
                                string path = AssetDatabase.GetAssetPath(asset);
                                if (AssetDatabase.IsValidFolder(path))
                                {
                                    droppedFolders.Add(asset);
                                }
                            }
                        }
                        
                        if (droppedFolders.Count > 0)
                        {
                            AddMultipleFolders(droppedFolders);
                        }
                    }
                    break;
            }
        }

        private void DrawTextureWithBorder(Rect rect, Texture2D tex, ScaleMode scaleMode = ScaleMode.ScaleToFit)
        {
            if (tex == null) return;
            
            // Draw outer border (light silver grey border)
            Color borderColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
            EditorGUI.DrawRect(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), borderColor);
            
            // Draw inner background (dark grey)
            Color bgColor = new Color(0.18f, 0.18f, 0.18f, 1.0f);
            EditorGUI.DrawRect(rect, bgColor);
            
            // Draw texture inside
            GUI.DrawTexture(rect, tex, scaleMode);
        }
    }
}
#endif
