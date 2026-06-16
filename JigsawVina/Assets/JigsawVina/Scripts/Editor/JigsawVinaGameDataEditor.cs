#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("JigsawVina.Tests")]

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
        internal class EditorItemState
        {
            public string filename = "";
            public string displayName = "";
            public string description = "";
            public string rarity = "common";
            public string displayNameKey = "";
            public string descriptionKey = "";
        }

        [Serializable]
        internal class EditorTabState
        {
            public DefaultAsset folderAsset;
            public int pictureId;
            public string idString = "";
            public string displayName = "";
            public int categoryId = 1;
            public List<EditorItemState> itemStates = new();
            public string displayNameKey = "";
            public string descriptionKey = "";

            // New locking fields
            public bool isInitiallyUnlocked = true;
            public string difficultyUnlockPolicy = "sequential";
            public List<int> unlockRequirements = new();

            // Foldout states
            public bool easyExpanded = true;
            public bool normalExpanded = true;
            public bool hardExpanded = true;

            // Difficulty settings
            public int easyCols = 6, easyRows = 4;
            public int easyCoins = 30, easyReplayCoins = 10, easyHints = 0;
            public int easyKeyRewardIndex = 0;
            public int easyDropTableId = 0;

            public int normalCols = 8, normalRows = 6;
            public int normalCoins = 60, normalReplayCoins = 20, normalHints = 0;
            public int normalKeyRewardIndex = 0;
            public int normalDropTableId = 0;

            public int hardCols = 12, hardRows = 8;
            public int hardCoins = 120, hardReplayCoins = 40, hardHints = 0;
            public int hardKeyRewardIndex = 0;
            public int hardDropTableId = 0;
        }

        [SerializeField] internal List<EditorTabState> _tabs = new();

        [Serializable]
        internal class EditorCategoryState
        {
            public int id;
            public string idString = "";
            public string displayName = "";
            public string displayNameKey = "";
            public string descriptionKey = "";
        }
        [SerializeField] internal List<EditorCategoryState> _categories = new();
        private Vector2 _categoryScroll;
        private int _mainTabSelected = 0;
        [SerializeField] internal List<ItemDto> _globalItems = new();
        [SerializeField] internal List<DropTableDto> _dropTables = new();
        [SerializeField] internal List<DropTableItemDto> _dropTableItems = new();
        [SerializeField] internal List<DailyRewardDto> _dailyRewards = new();
        private Vector2 _itemsScroll;
        private Vector2 _dailyRewardsScroll;
        private Vector2 _keyItemsScroll;
        private PlayerSave _cachedSave = new();
        [NonSerialized] private bool _saveLoaded = false;
        [NonSerialized] private string _saveJsonText = "";

        [MenuItem("JigsawVina/Game Data Editor")]
        public static void ShowWindow()
        {
            GetWindow<JigsawVinaGameDataEditor>("Game Data Editor");
        }

        public static void DebugSave()
        {
            var window = GetWindow<JigsawVinaGameDataEditor>("Game Data Editor");
            window.SaveConfig();
            window.Close();
        }

        public static void DebugLoadPlayerSave()
        {
            var window = GetWindow<JigsawVinaGameDataEditor>("Game Data Editor");
            window.LoadPlayerSave();
            window.Repaint();
        }

        public static void DebugReloadFromDisk()
        {
            var window = GetWindow<JigsawVinaGameDataEditor>("Game Data Editor");
            window.LoadFromDisk();
            window.Repaint();
        }

        private void OnEnable()
        {
            LoadFromDisk();
        }

        internal void SetStateForTesting(List<EditorTabState> tabs, List<EditorCategoryState> categories, List<ItemDto> globalItems)
        {
            _tabs = tabs ?? new();
            _categories = categories ?? new();
            _globalItems = globalItems ?? new();
            
            _dailyRewards = new List<DailyRewardDto>();
            for (int d = 1; d <= 7; d++)
            {
                _dailyRewards.Add(new DailyRewardDto { day_index = d, item_id = 1, amount = 50 * d });
            }
        }

        private void LoadFromDisk()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    var dto = JsonUtility.FromJson<StaticDataDto>(json);
                    if (dto != null)
                    {
                        LoadStateFromDto(dto);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JigsawVina Editor] Could not parse config: {e.Message}");
                }
            }
            LoadStateFromDto(new StaticDataDto());
        }

        internal void LoadStateFromDto(StaticDataDto dto)
        {
            _tabs.Clear();
            _categories.Clear();
            _globalItems.Clear();
            _dropTables.Clear();
            _dropTableItems.Clear();

            if (dto.drop_tables != null)
            {
                _dropTables = new List<DropTableDto>(dto.drop_tables);
            }
            if (dto.drop_table_items != null)
            {
                _dropTableItems = new List<DropTableItemDto>(dto.drop_table_items);
            }

            _dailyRewards.Clear();
            if (dto.daily_rewards != null && dto.daily_rewards.Count == 7)
            {
                _dailyRewards = new List<DailyRewardDto>(dto.daily_rewards);
            }
            else
            {
                // Auto-seed/populate exactly 7 default rewards (using Item ID 1 for coins)
                for (int d = 1; d <= 7; d++)
                {
                    _dailyRewards.Add(new DailyRewardDto { day_index = d, item_id = 1, amount = 50 * d });
                }
            }

            // 1. Hydrate Categories first
            if (dto.categories != null)
            {
                foreach (var cat in dto.categories)
                {
                    _categories.Add(new EditorCategoryState
                    {
                        id = cat.id,
                        idString = cat.id_string,
                        displayName = cat.display_name,
                        displayNameKey = cat.display_name_key,
                        descriptionKey = cat.description_key
                    });
                }
            }

            // 2. Hydrate Picture tabs
            if (dto.pictures != null)
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

                foreach (var pic in dto.pictures)
                {
                    var state = new EditorTabState
                    {
                        pictureId = pic.id,
                        idString = pic.id_string,
                        displayName = pic.display_name,
                        categoryId = pic.category_id != 0 ? pic.category_id : 1,
                        displayNameKey = pic.display_name_key,
                        descriptionKey = pic.description_key,
                        isInitiallyUnlocked = pic.is_initially_unlocked,
                        difficultyUnlockPolicy = string.IsNullOrEmpty(pic.difficulty_unlock_policy) ? "sequential" : pic.difficulty_unlock_policy,
                        unlockRequirements = pic.unlock_requirements != null ? new List<int>(pic.unlock_requirements) : new List<int>()
                    };

                    // Reconstruct folder asset path
                    if (!string.IsNullOrEmpty(pic.asset_path))
                    {
                        string relativeDir = Path.GetDirectoryName(pic.asset_path).Replace("\\", "/");
                        string folderPath = $"Assets/Resources/{relativeDir}";
                        state.folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                    }

                    // Sync item states from DTO (Filter generated key items, safety 64-bit int logic to prevent overflow)
                    state.itemStates.Clear();
                    if (dto.items != null)
                    {
                        long picId = pic.id;
                        foreach (var item in dto.items)
                        {
                            long itemId = item.id;
                            if (itemId > picId * 100 && itemId < (picId + 1) * 100 && item.item_type == "key_item")
                            {
                                string filename = Path.GetFileNameWithoutExtension(item.asset_path);
                                state.itemStates.Add(new EditorItemState
                                {
                                    filename = filename,
                                    displayName = item.display_name,
                                    description = item.description,
                                    rarity = string.IsNullOrEmpty(item.rarity) ? "common" : item.rarity,
                                    displayNameKey = item.display_name_key,
                                    descriptionKey = item.description_key
                                });
                            }
                        }
                    }

                    // If folderAsset exists, do production sync to scan new folder items
                    if (state.folderAsset != null)
                    {
                        var (_, scannedItems) = ScanFolder(state.folderAsset);
                        SyncItemStates(state, scannedItems);
                    }

                    // 3. Hydrate Difficulty settings (Completely independent of folderAsset existence)
                    if (diffsByPic.TryGetValue(pic.id, out var picDiffs))
                    {
                        foreach (var d in picDiffs)
                        {
                            int rewardIdx = 0;
                            if (d.first_clear_reward_item_ids != null && d.first_clear_reward_item_ids.Count > 0)
                            {
                                int rewardId = d.first_clear_reward_item_ids[0];
                                int calculatedIndex = (rewardId - pic.id * 100) - 1;

                                if (state.folderAsset != null)
                                {
                                    // Production scan index matching
                                    var (_, scannedItems) = ScanFolder(state.folderAsset);
                                    if (calculatedIndex >= 0 && calculatedIndex < scannedItems.Count)
                                    {
                                        rewardIdx = calculatedIndex + 1;
                                    }
                                }
                                else
                                {
                                    // Fallback mock mapping using itemStates sorted alphabetically using Ordinal comparer
                                    state.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                                    if (calculatedIndex >= 0 && calculatedIndex < state.itemStates.Count)
                                    {
                                        rewardIdx = calculatedIndex + 1;
                                    }
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
                                state.easyDropTableId = d.drop_table_id;
                            }
                            else if (d.difficulty_id == 1)
                            {
                                state.normalCols = d.grid_columns;
                                state.normalRows = d.grid_rows;
                                state.normalCoins = d.first_clear_coin;
                                state.normalReplayCoins = d.replay_coin;
                                state.normalHints = d.first_clear_hint;
                                state.normalKeyRewardIndex = rewardIdx;
                                state.normalDropTableId = d.drop_table_id;
                            }
                            else if (d.difficulty_id == 2)
                            {
                                state.hardCols = d.grid_columns;
                                state.hardRows = d.grid_rows;
                                state.hardCoins = d.first_clear_coin;
                                state.hardReplayCoins = d.replay_coin;
                                state.hardHints = d.first_clear_hint;
                                state.hardKeyRewardIndex = rewardIdx;
                                state.hardDropTableId = d.drop_table_id;
                            }
                        }
                    }
                    _tabs.Add(state);
                }
            }

            // 4. Hydrate Global Items (Exclude key items via persisted metadata: item_type == "key_item")
            if (dto.items != null)
            {
                foreach (var item in dto.items)
                {
                    if (item.item_type != "key_item")
                    {
                        _globalItems.Add(item);
                    }
                }
            }

            // 5. Ensure Default Categories and Reserved Items exist (Deduplicated order)
            if (_categories.Count == 0)
            {
                _categories.Add(new EditorCategoryState
                {
                    id = 1,
                    idString = "vietnam_landscapes",
                    displayName = "Phong Cảnh Việt Nam"
                });
            }
            EnsureReservedItems();

            if (_tabs.Count == 0)
            {
                _tabs.Add(new EditorTabState { pictureId = 1, categoryId = _categories[0].id });
            }
            _tabs.Sort((a, b) => a.pictureId.CompareTo(b.pictureId));
        }

        internal void EnsureReservedItems()
        {
            var coin = _globalItems.Find(i => i.id == 1);
            if (coin == null)
            {
                _globalItems.Insert(0, new ItemDto
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
            }
            else
            {
                if (coin.id_string != "coin") coin.id_string = "coin";
                if (coin.item_type != "currency") coin.item_type = "currency";
            }

            var hint = _globalItems.Find(i => i.id == 2);
            if (hint == null)
            {
                int hintIdx = _globalItems.FindIndex(i => i.id > 2);
                if (hintIdx < 0) hintIdx = _globalItems.Count;
                _globalItems.Insert(hintIdx, new ItemDto
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
            }
            else
            {
                if (hint.id_string != "hint") hint.id_string = "hint";
                if (hint.item_type != "currency") hint.item_type = "currency";
            }
        }

        private void OnGUI()
        {
            // Top Toolbar Section
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (_mainTabSelected == 0)
            {
                if (GUILayout.Button("Thêm Tranh Mới", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    int nextId = 1;
                    foreach (var t in _tabs)
                    {
                        if (t.pictureId >= nextId) nextId = t.pictureId + 1;
                    }
                    var newTab = new EditorTabState { pictureId = nextId, categoryId = _categories.Count > 0 ? _categories[0].id : 1 };
                    _tabs.Add(newTab);
                    _tabs.Sort((a, b) => a.pictureId.CompareTo(b.pictureId));
                    _selectedIndex = _tabs.IndexOf(newTab);
                    GUI.FocusControl(null);
                }
            }
            else if (_mainTabSelected == 1)
            {
                if (GUILayout.Button("Thêm Danh Mục Mới", EditorStyles.toolbarButton, GUILayout.Width(150)))
                {
                    int nextId = 1;
                    foreach (var cat in _categories)
                    {
                        if (cat.id >= nextId) nextId = cat.id + 1;
                    }
                    _categories.Add(new EditorCategoryState
                    {
                        id = nextId,
                        idString = $"new_category_{nextId}",
                        displayName = $"Danh mục mới {nextId}"
                    });
                    GUI.FocusControl(null);
                }
            }
            else if (_mainTabSelected == 2)
            {
                if (GUILayout.Button("Thêm Vật Phẩm Mới", EditorStyles.toolbarButton, GUILayout.Width(150)))
                {
                    int newId = GetNextAvailableItemId();
                    _globalItems.Add(new ItemDto
                    {
                        id = newId,
                        id_string = $"new_item_{newId}",
                        display_name = $"Vật phẩm mới {newId}",
                        description = "",
                        display_name_key = $"item.new_item_{newId}.name",
                        description_key = $"item.new_item_{newId}.description",
                        item_type = "collectible",
                        rarity = "common",
                        is_consumable = false,
                        is_time_limited = false,
                        max_stack = 1,
                        status = "active",
                        sort_order = newId,
                        asset_path = ""
                    });
                    GUI.FocusControl(null);
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Save & Generate JSON", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                SaveConfig();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Main Tab Selection Toolbar
            string[] mainTabs = { "Cấu hình Tranh", "Quản lý Danh mục", "Quản lý Vật phẩm", "Trình sửa Save (Cheat)", "Cấu hình Daily Reward" };
            int prevMainTab = _mainTabSelected;
            _mainTabSelected = GUILayout.Toolbar(_mainTabSelected, mainTabs);
            if (_mainTabSelected != prevMainTab)
            {
                GUI.FocusControl(null);
            }
            EditorGUILayout.Space();

            switch (_mainTabSelected)
            {
                case 0:
                    DrawPicturesTab();
                    break;
                case 1:
                    DrawCategoriesTab();
                    break;
                case 2:
                    DrawGlobalItemsTab();
                    break;
                case 3:
                    DrawSaveTab();
                    break;
                case 4:
                    DrawDailyRewardsTab();
                    break;
            }
        }

        private void DrawPicturesTab()
        {
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
                    var targetTab = tab;
                    _tabs.Sort((a, b) => a.pictureId.CompareTo(b.pictureId));
                    _selectedIndex = _tabs.IndexOf(targetTab);
                    GUI.FocusControl(null);
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

        private void DrawCategoriesTab()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Quản lý Danh mục (Categories)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _categoryScroll = EditorGUILayout.BeginScrollView(_categoryScroll);

            for (int i = 0; i < _categories.Count; i++)
            {
                var cat = _categories[i];

                Color oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = (i % 2 == 0)
                    ? new Color(1.5f, 1.5f, 1.5f, 1.0f)
                    : new Color(0.6f, 0.6f, 0.6f, 1.0f);

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();

                GUILayout.Label($"Danh mục #{cat.id}", EditorStyles.boldLabel, GUILayout.Width(100));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Xóa", GUILayout.Width(60)))
                {
                    string reason;
                    if (CanDeleteCategory(cat.id, out reason))
                    {
                        if (EditorUtility.DisplayDialog("Xóa Danh Mục", $"Bạn có chắc chắn muốn xóa Danh mục '{cat.displayName}'?", "Có", "Không"))
                        {
                            _categories.RemoveAt(i);
                            i--;
                            GUI.FocusControl(null);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Lỗi Xóa Danh Mục", reason, "OK");
                    }
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                cat.id = EditorGUILayout.IntField("Category ID", cat.id);
                cat.idString = EditorGUILayout.TextField("ID String", cat.idString);
                cat.displayName = EditorGUILayout.TextField("Tên hiển thị", cat.displayName);
                cat.displayNameKey = EditorGUILayout.TextField("Khóa tên hiển thị", cat.displayNameKey);
                cat.descriptionKey = EditorGUILayout.TextField("Khóa mô tả", cat.descriptionKey);

                GUILayout.EndVertical();
                GUI.backgroundColor = oldBgColor;
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawGlobalItemsTab()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Quản lý Vật phẩm (Global Items)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _itemsScroll = EditorGUILayout.BeginScrollView(_itemsScroll);

            for (int i = 0; i < _globalItems.Count; i++)
            {
                var item = _globalItems[i];

                Color oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = (i % 2 == 0)
                    ? new Color(1.5f, 1.5f, 1.5f, 1.0f)
                    : new Color(0.6f, 0.6f, 0.6f, 1.0f);

                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Vật phẩm #{item.id}", EditorStyles.boldLabel, GUILayout.Width(150));
                GUILayout.FlexibleSpace();

                bool isReserved = (item.id == 1 || item.id == 2);
                EditorGUI.BeginDisabledGroup(isReserved);
                if (GUILayout.Button("Xóa", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Xóa Vật Phẩm", $"Bạn có chắc chắn muốn xóa Vật phẩm '{item.display_name}'?", "Có", "Không"))
                    {
                        _globalItems.RemoveAt(i);
                        i--;
                        GUI.FocusControl(null);
                        EditorGUI.EndDisabledGroup();
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        GUI.backgroundColor = oldBgColor;
                        continue;
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(isReserved);
                item.id = EditorGUILayout.IntField("Item ID", item.id);
                item.id_string = EditorGUILayout.TextField("ID String", item.id_string);
                EditorGUI.EndDisabledGroup();

                item.display_name = EditorGUILayout.TextField("Tên hiển thị", item.display_name);
                item.description = EditorGUILayout.TextField("Mô tả", item.description);
                item.display_name_key = EditorGUILayout.TextField("Khóa tên hiển thị", item.display_name_key);
                item.description_key = EditorGUILayout.TextField("Khóa mô tả", item.description_key);

                EditorGUI.BeginDisabledGroup(isReserved);
                string[] baseTypes = { "currency", "consumable", "collectible" };
                List<string> typeList = new List<string>(baseTypes);
                if (!typeList.Contains(item.item_type))
                {
                    typeList.Add(item.item_type);
                }
                string[] types = typeList.ToArray();
                int typeIdx = Array.IndexOf(types, item.item_type);
                if (typeIdx < 0) typeIdx = 0;
                typeIdx = EditorGUILayout.Popup("Loại vật phẩm", typeIdx, types);
                item.item_type = types[typeIdx];
                EditorGUI.EndDisabledGroup();

                string[] rarities = { "common", "uncommon", "rare", "epic", "legendary" };
                int rarityIdx = Array.IndexOf(rarities, item.rarity);
                if (rarityIdx < 0) rarityIdx = 0;
                rarityIdx = EditorGUILayout.Popup("Độ hiếm", rarityIdx, rarities);
                item.rarity = rarities[rarityIdx];

                item.is_consumable = EditorGUILayout.Toggle("Tiêu thụ được", item.is_consumable);
                item.is_time_limited = EditorGUILayout.Toggle("Giới hạn thời gian", item.is_time_limited);
                item.max_stack = EditorGUILayout.IntField("Stack tối đa", item.max_stack);

                string[] statuses = { "active", "inactive" };
                int statusIdx = Array.IndexOf(statuses, item.status);
                if (statusIdx < 0) statusIdx = 0;
                statusIdx = EditorGUILayout.Popup("Trạng thái", statusIdx, statuses);
                item.status = statuses[statusIdx];

                item.sort_order = EditorGUILayout.IntField("Thứ tự sắp xếp", item.sort_order);
                item.asset_path = EditorGUILayout.TextField("Đường dẫn Asset", item.asset_path);

                GUILayout.EndVertical();
                GUI.backgroundColor = oldBgColor;
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        internal List<int> GetActiveItemIds(bool scanFolders)
        {
            var ids = new List<int>();
            foreach (var item in _globalItems)
            {
                ids.Add(item.id);
            }

            foreach (var tab in _tabs)
            {
                if (scanFolders && tab.folderAsset != null)
                {
                    var (_, scannedItems) = ScanFolder(tab.folderAsset);
                    for (int itemIndex = 0; itemIndex < scannedItems.Count; itemIndex++)
                    {
                        ids.Add(tab.pictureId * 100 + (itemIndex + 1));
                    }
                }
                else
                {
                    tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                    for (int itemIndex = 0; itemIndex < tab.itemStates.Count; itemIndex++)
                    {
                        ids.Add(tab.pictureId * 100 + (itemIndex + 1));
                    }
                }
            }

            return ids;
        }

        internal int GetNextAvailableItemId()
        {
            var activeIds = new HashSet<int>(GetActiveItemIds(scanFolders: true));
            int nextId = 1;
            while (activeIds.Contains(nextId))
            {
                nextId++;
            }
            return nextId;
        }

        private void DrawSaveTab()
        {
            if (!_saveLoaded)
            {
                LoadPlayerSave();
            }

            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Trình sửa Save (Cheat & Player Save Editor)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Dùng để xem và sửa đổi dữ liệu lưu trữ trực tiếp của người chơi lưu trong PlayerPrefs.", MessageType.Info);
            EditorGUILayout.Space();

            // Load & Save Buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tải Save từ Disk / Prefs", GUILayout.Width(180)))
            {
                LoadPlayerSave();
                GUI.FocusControl(null);
            }
            if (GUILayout.Button("Lưu Save xuống Disk / Prefs", GUILayout.Width(180)))
            {
                SavePlayerSave();
                EditorUtility.DisplayDialog("Lưu Save Thành Công", "Cấu hình save game đã được lưu vào PlayerPrefs!", "OK");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Edit basic fields: Coins & Hints
            GUILayout.Label("Tài Nguyên Người Chơi", EditorStyles.boldLabel);
            
            var wordWrapMiniLabel = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };

            _cachedSave.Coins = EditorGUILayout.IntField("Số Xu (Coins)", _cachedSave.Coins);
            EditorGUILayout.LabelField("-> Coins (Số Xu): Đơn vị tiền tệ chính để mở khóa tranh hoặc tính năng trong tương lai.", wordWrapMiniLabel);
            
            _cachedSave.Hints = EditorGUILayout.IntField("Số Gợi Ý (Hints)", _cachedSave.Hints);
            EditorGUILayout.LabelField("-> Hints (Số Gợi Ý): Số lượng gợi ý hỗ trợ tự động tìm vị trí đúng của mảnh ghép.", wordWrapMiniLabel);

            EditorGUILayout.Space();

            // Owned Key Items section
            GUILayout.Label("Vật Phẩm Sở Hữu (OwnedItemIds)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("-> OwnedItemIds (Key Items): Các vật phẩm vĩnh viễn thu được khi vượt màn lần đầu (First Clear), dùng để đáp ứng điều kiện mở khóa tranh.", wordWrapMiniLabel);
            
            _cachedSave.OwnedItemIds ??= new List<int>();

            // Collect all Key Items from pictures/tabs
            var keyItems = new List<(int id, string displayName)>();
            foreach (var tab in _tabs)
            {
                tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                for (int i = 0; i < tab.itemStates.Count; i++)
                {
                    int itemId = tab.pictureId * 100 + (i + 1);
                    string name = string.IsNullOrEmpty(tab.itemStates[i].displayName) 
                        ? tab.itemStates[i].filename 
                        : tab.itemStates[i].displayName;
                    keyItems.Add((itemId, $"[{tab.displayName}] {name} (ID: {itemId})"));
                }
            }

            _keyItemsScroll = EditorGUILayout.BeginScrollView(_keyItemsScroll, GUILayout.Height(120));
            if (keyItems.Count == 0)
            {
                GUILayout.Label("Không có Key Item nào được cấu hình trong các tranh.", EditorStyles.miniLabel);
            }
            else
            {
                foreach (var item in keyItems)
                {
                    bool owned = _cachedSave.OwnedItemIds.Contains(item.id);
                    
                    GUILayout.BeginHorizontal();
                    bool newOwned = EditorGUILayout.Toggle(owned, GUILayout.Width(20));
                    GUILayout.Label(item.displayName);
                    GUILayout.EndHorizontal();

                    if (newOwned != owned)
                    {
                        if (newOwned)
                        {
                            if (!_cachedSave.OwnedItemIds.Contains(item.id))
                                _cachedSave.OwnedItemIds.Add(item.id);
                        }
                        else
                        {
                            _cachedSave.OwnedItemIds.Remove(item.id);
                        }
                        _saveJsonText = JsonUtility.ToJson(_cachedSave, true);
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sở hữu tất cả Key Items", GUILayout.Width(200)))
            {
                _cachedSave.OwnedItemIds ??= new List<int>();
                foreach (var item in keyItems)
                {
                    if (!_cachedSave.OwnedItemIds.Contains(item.id))
                    {
                        _cachedSave.OwnedItemIds.Add(item.id);
                    }
                }
                SavePlayerSave();
                EditorUtility.DisplayDialog("Thành Công", "Đã thêm tất cả Key Items vào danh sách sở hữu!", "OK");
            }
            if (GUILayout.Button("Xóa sạch Key Items", GUILayout.Width(200)))
            {
                _cachedSave.OwnedItemIds.Clear();
                SavePlayerSave();
                EditorUtility.DisplayDialog("Thành Công", "Đã xóa sạch toàn bộ Key Items sở hữu!", "OK");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Unlock All & Reset Save Buttons
            GUILayout.Label("Trình Cheat / Hỗ Trợ Test", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("-> CompletedPuzzles: Lưu trữ lịch sử vượt màn tốt nhất (sao, thời gian) để kiểm tra mở khóa độ khó tuần tự.", wordWrapMiniLabel);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Mở Khóa Toàn Bộ Tranh (Unlock All)", GUILayout.Width(240)))
            {
                ApplyUnlockAll(_cachedSave);
                SavePlayerSave();
                EditorUtility.DisplayDialog("Mở Khóa Hoàn Tất", "Đã mở khóa 3 sao cho tất cả độ khó của các bức tranh hiện tại!", "OK");
            }

            if (GUILayout.Button("Xóa / Reset Trạng Thái Save", GUILayout.Width(240)))
            {
                if (EditorUtility.DisplayDialog("Xác Nhận Reset Save", "Bạn có chắc chắn muốn xóa toàn bộ save game hiện tại? Thao tác này sẽ xóa key save và đưa bộ nhớ cache về trạng thái rỗng.", "Có", "Không"))
                {
                    ResetPlayerSave();
                    EditorUtility.DisplayDialog("Đã Reset Save", "Save game đã được reset hoàn toàn!", "OK");
                }
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Display raw JSON for debugging
            GUILayout.Label("Raw JSON Save Data (Read-only):", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(_saveJsonText, GUILayout.ExpandHeight(true));

            GUILayout.EndVertical();
        }

        internal class AvailableRewardItem
        {
            public int id;
            public string displayName;
            public string itemType;
            public string assetPath;
        }

        private List<AvailableRewardItem> GetAvailableRewardItems()
        {
            var list = new List<AvailableRewardItem>();
            foreach (var item in _globalItems)
            {
                if (item.status == "active")
                {
                    list.Add(new AvailableRewardItem
                    {
                        id = item.id,
                        displayName = $"{item.display_name} (ID: {item.id}, {item.item_type})",
                        itemType = item.item_type,
                        assetPath = item.asset_path
                    });
                }
            }

            foreach (var tab in _tabs)
            {
                tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                for (int i = 0; i < tab.itemStates.Count; i++)
                {
                    int itemId = tab.pictureId * 100 + (i + 1);
                    string name = string.IsNullOrEmpty(tab.itemStates[i].displayName)
                        ? tab.itemStates[i].filename
                        : tab.itemStates[i].displayName;
                    
                    list.Add(new AvailableRewardItem
                    {
                        id = itemId,
                        displayName = $"[{tab.displayName}] {name} (ID: {itemId}, key_item)",
                        itemType = "key_item",
                        assetPath = ""
                    });
                }
            }

            return list;
        }

        private void DrawDailyRewardsTab()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label("Cấu hình Daily Reward (Điểm danh 7 ngày)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Phần thưởng đăng nhập hàng ngày theo chu kỳ 7 ngày. Mỗi ngày index từ 1 đến 7 tương ứng với phần thưởng của ngày đó. Day Index là ngày cố định.", MessageType.Info);
            EditorGUILayout.Space();

            var availableItems = GetAvailableRewardItems();
            if (availableItems.Count == 0)
            {
                GUILayout.Label("Không có vật phẩm nào được cấu hình hoạt động.", EditorStyles.boldLabel);
                GUILayout.EndVertical();
                return;
            }

            string[] optionNames = availableItems.Select(ai => ai.displayName).ToArray();
            int[] itemIds = availableItems.Select(ai => ai.id).ToArray();

            _dailyRewardsScroll = EditorGUILayout.BeginScrollView(_dailyRewardsScroll);

            for (int i = 0; i < _dailyRewards.Count; i++)
            {
                var dr = _dailyRewards[i];
                Color oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = (i % 2 == 0)
                    ? new Color(1.5f, 1.5f, 1.5f, 1.0f)
                    : new Color(0.6f, 0.6f, 0.6f, 1.0f);

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();

                GUILayout.Label($"Ngày {dr.day_index}", EditorStyles.boldLabel, GUILayout.Width(80));

                int selectedIdx = Array.IndexOf(itemIds, dr.item_id);
                if (selectedIdx < 0) selectedIdx = 0;
                int newSelectedIdx = EditorGUILayout.Popup("Vật phẩm", selectedIdx, optionNames, GUILayout.Width(350));
                if (newSelectedIdx >= 0 && newSelectedIdx < itemIds.Length)
                {
                    dr.item_id = itemIds[newSelectedIdx];
                }

                GUILayout.Space(10);

                var selectedItem = availableItems[newSelectedIdx];
                Texture2D previewTex = null;
                if (selectedItem.itemType == "key_item")
                {
                    int picId = selectedItem.id / 100;
                    int localIdx = (selectedItem.id % 100) - 1;
                    var tab = _tabs.Find(t => t.pictureId == picId);
                    if (tab != null && tab.folderAsset != null)
                    {
                        var (_, itemTextures) = ScanFolder(tab.folderAsset);
                        if (localIdx >= 0 && localIdx < itemTextures.Count)
                        {
                            previewTex = itemTextures[localIdx];
                        }
                    }
                }

                if (previewTex != null)
                {
                    var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                    DrawTextureWithBorder(rect, previewTex, ScaleMode.ScaleToFit);
                }
                else
                {
                    var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                    EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1.0f));
                }

                GUILayout.Space(10);

                dr.amount = EditorGUILayout.IntField("Số lượng", dr.amount, GUILayout.Width(180));

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUI.backgroundColor = oldBgColor;
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        internal void LoadPlayerSave()
        {
            if (PlayerPrefs.HasKey(SaveDataService.SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveDataService.SaveKey);
                _cachedSave = JsonUtility.FromJson<PlayerSave>(json) ?? new PlayerSave();
                _saveJsonText = JsonUtility.ToJson(_cachedSave, true);
                Debug.Log($"[DebugSaveEditor] Loaded from prefs:\n{_saveJsonText}");
            }
            else
            {
                _cachedSave = new PlayerSave();
                _saveJsonText = JsonUtility.ToJson(_cachedSave, true);
                Debug.Log($"[DebugSaveEditor] New save:\n{_saveJsonText}");
            }
            _cachedSave.CompletedPuzzles ??= new List<CompletedPuzzleData>();
            _cachedSave.OwnedItemIds ??= new List<int>();
            _saveLoaded = true;
        }

        internal void SavePlayerSave()
        {
            if (_cachedSave == null) return;
            _cachedSave.CompletedPuzzles ??= new List<CompletedPuzzleData>();
            _cachedSave.OwnedItemIds ??= new List<int>();
            string json = JsonUtility.ToJson(_cachedSave);
            PlayerPrefs.SetString(SaveDataService.SaveKey, json);
            PlayerPrefs.Save();
            _saveJsonText = JsonUtility.ToJson(_cachedSave, true);
        }

        internal void ApplyUnlockAll(PlayerSave save)
        {
            if (save == null) return;
            save.CompletedPuzzles ??= new List<CompletedPuzzleData>();
            save.UnlockedPictureIds ??= new List<int>();

            var newCompletions = new List<CompletedPuzzleData>();
            var processedPicIds = new HashSet<int>();

            foreach (var tab in _tabs)
            {
                if (tab.pictureId <= 0 || !processedPicIds.Add(tab.pictureId))
                    continue;

                if (!tab.isInitiallyUnlocked && !save.UnlockedPictureIds.Contains(tab.pictureId))
                {
                    save.UnlockedPictureIds.Add(tab.pictureId);
                }

                for (int diffId = 0; diffId <= 2; diffId++)
                {
                    var existing = save.CompletedPuzzles.Find(cp => cp.PictureId == tab.pictureId && cp.DifficultyId == diffId);
                    if (existing != null)
                    {
                        existing.BestStar = 3;
                        existing.BestTimeSeconds = 45.0f;
                        newCompletions.Add(existing);
                    }
                    else
                    {
                        newCompletions.Add(new CompletedPuzzleData
                        {
                            PictureId = tab.pictureId,
                            DifficultyId = diffId,
                            BestStar = 3,
                            BestTimeSeconds = 45.0f
                        });
                    }
                }
            }

            save.CompletedPuzzles = newCompletions;
        }

        internal void ResetPlayerSave()
        {
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
            _cachedSave = new PlayerSave();
            _cachedSave.CompletedPuzzles ??= new List<CompletedPuzzleData>();
            _cachedSave.OwnedItemIds ??= new List<int>();
            _saveLoaded = true;
            _saveJsonText = JsonUtility.ToJson(_cachedSave, true);
            PlayerPrefs.Save();
        }

        internal bool CanDeleteCategory(int categoryId, out string reason)
        {
            if (_categories.Count <= 1)
            {
                reason = "Không thể xóa danh mục cuối cùng.";
                return false;
            }

            int count = 0;
            foreach (var tab in _tabs)
            {
                if (tab.categoryId == categoryId)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                reason = $"Không thể xóa danh mục này vì có {count} bức tranh đang thuộc danh mục này. Vui lòng thay đổi danh mục của các bức tranh đó trước.";
                return false;
            }

            reason = "";
            return true;
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
            int prevDetailTab = _detailTabSelected;
            _detailTabSelected = GUILayout.Toolbar(_detailTabSelected, detailTabs);
            if (_detailTabSelected != prevDetailTab)
            {
                GUI.FocusControl(null);
            }
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
                state.displayNameKey = EditorGUILayout.TextField("Khóa tên hiển thị", state.displayNameKey);
                state.descriptionKey = EditorGUILayout.TextField("Khóa mô tả", state.descriptionKey);

                state.isInitiallyUnlocked = EditorGUILayout.Toggle("Mở khóa mặc định", state.isInitiallyUnlocked);
                
                string[] policies = { "sequential", "all_unlocked" };
                int policyIdx = Array.IndexOf(policies, state.difficultyUnlockPolicy);
                if (policyIdx < 0) policyIdx = 0;
                policyIdx = EditorGUILayout.Popup("Cơ chế mở khóa độ khó", policyIdx, policies);
                state.difficultyUnlockPolicy = policies[policyIdx];

                state.unlockRequirements ??= new List<int>();
                EditorGUILayout.Space();
                GUILayout.Label("Yêu cầu Mở khóa (Key Item IDs)", EditorStyles.boldLabel);
                for (int u = 0; u < state.unlockRequirements.Count; u++)
                {
                    GUILayout.BeginHorizontal();
                    state.unlockRequirements[u] = EditorGUILayout.IntField($"Vật phẩm yêu cầu {u + 1}", state.unlockRequirements[u]);
                    if (GUILayout.Button("Xóa", GUILayout.Width(60)))
                    {
                        state.unlockRequirements.RemoveAt(u);
                        u--;
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Thêm Key Item ID yêu cầu", GUILayout.Width(200)))
                {
                    state.unlockRequirements.Add(0);
                }
                EditorGUILayout.Space();

                if (_categories != null && _categories.Count > 0)
                {
                    string[] catNames = new string[_categories.Count];
                    for (int c = 0; c < _categories.Count; c++) catNames[c] = _categories[c].displayName;
                    int activeCatIdx = _categories.FindIndex(cat => cat.id == state.categoryId);
                    activeCatIdx = Mathf.Max(0, activeCatIdx);
                    int newCatIdx = EditorGUILayout.Popup("Danh mục (Category)", activeCatIdx, catNames);
                    if (newCatIdx >= 0 && newCatIdx < _categories.Count)
                    {
                        state.categoryId = _categories[newCatIdx].id;
                    }
                }

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
                    itemState.displayNameKey = EditorGUILayout.TextField("Khóa tên hiển thị", itemState.displayNameKey);
                    itemState.descriptionKey = EditorGUILayout.TextField("Khóa mô tả", itemState.descriptionKey);

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

                GUIContent[] itemGUIContents = new GUIContent[itemTextures.Count + 1];
                itemGUIContents[0] = new GUIContent("None");
                for (int i = 0; i < itemTextures.Count; i++)
                {
                    itemGUIContents[i + 1] = new GUIContent(itemTextures[i].name, itemTextures[i]);
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
                    state.easyDropTableId = EditorGUILayout.IntField("Drop Table ID", state.easyDropTableId);

                    GUILayout.BeginHorizontal();
                    state.easyKeyRewardIndex = EditorGUILayout.Popup(new GUIContent("Reward Key Item"), state.easyKeyRewardIndex, itemGUIContents);
                    GUILayout.Space(5);
                    if (state.easyKeyRewardIndex > 0 && state.easyKeyRewardIndex <= itemTextures.Count)
                    {
                        var tex = itemTextures[state.easyKeyRewardIndex - 1];
                        var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                        DrawTextureWithBorder(rect, tex, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1.0f));
                    }
                    GUILayout.EndHorizontal();

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
                    state.normalDropTableId = EditorGUILayout.IntField("Drop Table ID", state.normalDropTableId);

                    GUILayout.BeginHorizontal();
                    state.normalKeyRewardIndex = EditorGUILayout.Popup(new GUIContent("Reward Key Item"), state.normalKeyRewardIndex, itemGUIContents);
                    GUILayout.Space(5);
                    if (state.normalKeyRewardIndex > 0 && state.normalKeyRewardIndex <= itemTextures.Count)
                    {
                        var tex = itemTextures[state.normalKeyRewardIndex - 1];
                        var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                        DrawTextureWithBorder(rect, tex, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1.0f));
                    }
                    GUILayout.EndHorizontal();

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
                    state.hardDropTableId = EditorGUILayout.IntField("Drop Table ID", state.hardDropTableId);

                    GUILayout.BeginHorizontal();
                    state.hardKeyRewardIndex = EditorGUILayout.Popup(new GUIContent("Reward Key Item"), state.hardKeyRewardIndex, itemGUIContents);
                    GUILayout.Space(5);
                    if (state.hardKeyRewardIndex > 0 && state.hardKeyRewardIndex <= itemTextures.Count)
                    {
                        var tex = itemTextures[state.hardKeyRewardIndex - 1];
                        var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                        DrawTextureWithBorder(rect, tex, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        var rect = GUILayoutUtility.GetRect(24, 24, GUILayout.Width(24), GUILayout.Height(24));
                        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1.0f));
                    }
                    GUILayout.EndHorizontal();

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
                        rarity = "common",
                        displayNameKey = $"item.{tex.name.ToLower()}.name",
                        descriptionKey = $"item.{tex.name.ToLower()}.description"
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
                state.displayNameKey = $"picture.{state.idString}.name";
                state.descriptionKey = $"picture.{state.idString}.description";
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

        internal bool TryBuildConfig(out StaticDataDto config, out string errorMessage, bool validateAssets = true)
        {
            config = new StaticDataDto
            {
                schema_version = 1,
                data_version = 1,
                drop_tables = new List<DropTableDto>(_dropTables ?? new()),
                drop_table_items = new List<DropTableItemDto>(_dropTableItems ?? new()),
                daily_rewards = new List<DailyRewardDto>()
            };
            errorMessage = "";

            // 1. Validate and Map Categories
            var categoryIds = new HashSet<int>();
            var categoryIdStrings = new HashSet<string>();

            foreach (var cat in _categories)
            {
                if (cat.id <= 0)
                {
                    errorMessage = $"ID Danh mục '{cat.displayName}' phải là số nguyên dương (> 0).";
                    return false;
                }
                if (string.IsNullOrEmpty(cat.idString))
                {
                    errorMessage = $"ID String của Danh mục ID {cat.id} không được để trống.";
                    return false;
                }
                if (!categoryIds.Add(cat.id))
                {
                    errorMessage = $"Trùng lặp ID Danh mục: {cat.id}.";
                    return false;
                }
                if (!categoryIdStrings.Add(cat.idString))
                {
                    errorMessage = $"Trùng lặp ID String Danh mục: '{cat.idString}'.";
                    return false;
                }

                config.categories.Add(new CategoryDto
                {
                    id = cat.id,
                    id_string = cat.idString,
                    display_name = cat.displayName,
                    display_name_key = string.IsNullOrEmpty(cat.displayNameKey) ? $"category.{cat.idString}.name" : cat.displayNameKey,
                    description_key = string.IsNullOrEmpty(cat.descriptionKey) ? $"category.{cat.idString}.description" : cat.descriptionKey
                });
            }

            // 2. Validate and Map Pictures and Items
            var pictureIds = new HashSet<int>();
            var pictureIdStrings = new HashSet<string>();
            var itemIds = new HashSet<int>();
            var itemIdStrings = new HashSet<string>();

            // Map Global Items first
            foreach (var item in _globalItems)
            {
                if (item.id <= 0)
                {
                    errorMessage = $"ID Vật phẩm '{item.display_name}' phải là số nguyên dương (> 0).";
                    return false;
                }
                if (string.IsNullOrEmpty(item.id_string))
                {
                    errorMessage = $"ID String của Vật phẩm ID {item.id} không được để trống.";
                    return false;
                }
                if (item.item_type == "key_item")
                {
                    errorMessage = $"Vật phẩm Global '{item.display_name}' không được phép có item_type là 'key_item'.";
                    return false;
                }
                if (!itemIds.Add(item.id))
                {
                    errorMessage = $"Trùng lặp ID Vật phẩm Global: {item.id}.";
                    return false;
                }
                if (!itemIdStrings.Add(item.id_string))
                {
                    errorMessage = $"Trùng lặp ID String Vật phẩm Global: '{item.id_string}'.";
                    return false;
                }

                config.items.Add(item);
            }

            // Validate Reserved Items
            var coinItem = _globalItems.Find(i => i.id == 1);
            if (coinItem == null || coinItem.id_string != "coin" || coinItem.item_type != "currency")
            {
                errorMessage = "Vật phẩm cốt lõi ID 1 (coin) phải tồn tại và có id_string là 'coin' với kiểu 'currency'.";
                return false;
            }
            var hintItem = _globalItems.Find(i => i.id == 2);
            if (hintItem == null || hintItem.id_string != "hint" || hintItem.item_type != "currency")
            {
                errorMessage = "Vật phẩm cốt lõi ID 2 (hint) phải tồn tại và có id_string là 'hint' với kiểu 'currency'.";
                return false;
            }

            // Map pictures, scanned items, and difficulties
            foreach (var tab in _tabs)
            {
                if (tab.pictureId <= 0)
                {
                    errorMessage = $"ID Tranh '{tab.displayName}' phải là số nguyên dương (> 0).";
                    return false;
                }
                if (tab.pictureId >= 20000000)
                {
                    errorMessage = $"ID Tranh '{tab.pictureId}' quá lớn (phải nhỏ hơn 20,000,000) để tránh tràn số.";
                    return false;
                }
                if (string.IsNullOrEmpty(tab.idString))
                {
                    errorMessage = $"ID String của Tranh ID {tab.pictureId} không được để trống.";
                    return false;
                }
                if (!pictureIds.Add(tab.pictureId))
                {
                    errorMessage = $"Trùng lặp ID Tranh: {tab.pictureId}.";
                    return false;
                }
                if (!pictureIdStrings.Add(tab.idString))
                {
                    errorMessage = $"Trùng lặp ID String Tranh: '{tab.idString}'.";
                    return false;
                }
                if (!categoryIds.Contains(tab.categoryId))
                {
                    errorMessage = $"Tranh '{tab.displayName}' tham chiếu Danh mục ID {tab.categoryId} không tồn tại.";
                    return false;
                }

                string mainPath = "";
                string resourceFolder = "";
                List<string> itemFilenames = new();

                if (validateAssets)
                {
                    if (tab.folderAsset == null)
                    {
                        errorMessage = $"Tranh ID {tab.pictureId} chưa gán Thư mục tranh.";
                        return false;
                    }
                    string folderPath = AssetDatabase.GetAssetPath(tab.folderAsset);
                    if (!folderPath.StartsWith("Assets/Resources/"))
                    {
                        errorMessage = $"Thư mục '{folderPath}' phải nằm bên trong 'Assets/Resources/'.";
                        return false;
                    }

                    var (main, scannedItems) = ScanFolder(tab.folderAsset);
                    if (main == null)
                    {
                        errorMessage = $"Không tìm thấy ảnh chính 'MAIN_' trong thư mục: {tab.folderAsset.name}";
                        return false;
                    }
                    if (scannedItems.Count > 99)
                    {
                        errorMessage = $"Thư mục tranh '{tab.folderAsset.name}' có quá 99 key items (hiện có {scannedItems.Count}). Giới hạn tối đa là 99.";
                        return false;
                    }

                    resourceFolder = folderPath.Substring("Assets/Resources/".Length);
                    mainPath = $"{resourceFolder}/{main.name}";

                    foreach (var itTex in scannedItems)
                    {
                        itemFilenames.Add(itTex.name);
                    }
                }
                else
                {
                    // Mock path
                    mainPath = $"Textures/MAIN_mock_{tab.idString}";
                    resourceFolder = "Textures";

                    // Sort mock itemStates alphabetically using Ordinal comparison
                    tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                    if (tab.itemStates.Count > 99)
                    {
                        errorMessage = $"Tranh ID {tab.pictureId} có quá 99 key items. Giới hạn tối đa là 99.";
                        return false;
                    }

                    foreach (var itState in tab.itemStates)
                    {
                        itemFilenames.Add(itState.filename);
                    }
                }

                // Map pictureDto
                config.pictures.Add(new PictureDto
                {
                    id = tab.pictureId,
                    id_string = tab.idString,
                    display_name = tab.displayName,
                    category_id = tab.categoryId,
                    asset_path = mainPath,
                    difficulty_unlock_policy = string.IsNullOrEmpty(tab.difficultyUnlockPolicy) ? "sequential" : tab.difficultyUnlockPolicy,
                    display_name_key = string.IsNullOrEmpty(tab.displayNameKey) ? $"picture.{tab.idString}.name" : tab.displayNameKey,
                    description_key = string.IsNullOrEmpty(tab.descriptionKey) ? $"picture.{tab.idString}.description" : tab.descriptionKey,
                    is_initially_unlocked = tab.isInitiallyUnlocked,
                    unlock_requirements = tab.unlockRequirements != null ? new List<int>(tab.unlockRequirements) : new List<int>()
                });

                // Map scanned items DTO
                var localItems = new Dictionary<string, int>();
                for (int itemIndex = 0; itemIndex < itemFilenames.Count; itemIndex++)
                {
                    string filename = itemFilenames[itemIndex];
                    string itemIdString = filename.ToLower();

                    if (!itemIdStrings.Add(itemIdString))
                    {
                        errorMessage = $"Trùng lặp ID String Vật phẩm: '{itemIdString}'.";
                        return false;
                    }

                    int itemId = tab.pictureId * 100 + (itemIndex + 1);
                    if (!itemIds.Add(itemId))
                    {
                        errorMessage = $"Trùng lặp ID Vật phẩm: {itemId}.";
                        return false;
                    }

                    var itemState = tab.itemStates.Find(it => it.filename == filename);
                    string dispName = itemState != null ? itemState.displayName : filename.Replace("_", " ");
                    string desc = itemState != null ? itemState.description : "";
                    string rarity = itemState != null ? itemState.rarity : "common";
                    string itPath = $"{resourceFolder}/{filename}";

                    config.items.Add(new ItemDto
                    {
                        id = itemId,
                        id_string = itemIdString,
                        display_name = dispName,
                        description = desc,
                        display_name_key = (itemState != null && !string.IsNullOrEmpty(itemState.displayNameKey)) ? itemState.displayNameKey : $"item.{itemIdString}.name",
                        description_key = (itemState != null && !string.IsNullOrEmpty(itemState.descriptionKey)) ? itemState.descriptionKey : $"item.{itemIdString}.description",
                        item_type = "key_item",
                        rarity = rarity,
                        is_consumable = false,
                        is_time_limited = false,
                        max_stack = 1,
                        status = "active",
                        sort_order = itemId,
                        asset_path = itPath
                    });
                    localItems[filename] = itemId;
                }

                // Map difficulties
                AddDifficulty(config, tab.pictureId, 0, "Dễ", tab.easyCols, tab.easyRows, tab.easyCoins, tab.easyReplayCoins, tab.easyHints, tab.easyKeyRewardIndex, itemFilenames, localItems, tab.easyDropTableId);
                AddDifficulty(config, tab.pictureId, 1, "Trung bình", tab.normalCols, tab.normalRows, tab.normalCoins, tab.normalReplayCoins, tab.normalHints, tab.normalKeyRewardIndex, itemFilenames, localItems, tab.normalDropTableId);
                AddDifficulty(config, tab.pictureId, 2, "Khó", tab.hardCols, tab.hardRows, tab.hardCoins, tab.hardReplayCoins, tab.hardHints, tab.hardKeyRewardIndex, itemFilenames, localItems, tab.hardDropTableId);
            }

            if (_dailyRewards == null || _dailyRewards.Count != 7)
            {
                errorMessage = "Daily Rewards cấu hình phải có đúng 7 ngày.";
                return false;
            }

            var activeIds = new HashSet<int>(GetActiveItemIds(scanFolders: false));
            for (int i = 0; i < 7; i++)
            {
                var dr = _dailyRewards[i];
                if (dr.day_index != i + 1)
                {
                    errorMessage = $"Daily Reward index không hợp lệ tại dòng {i + 1}. Phải là Ngày {i + 1}.";
                    return false;
                }
                if (!activeIds.Contains(dr.item_id))
                {
                    errorMessage = $"Daily Reward Ngày {dr.day_index} tham chiếu Item ID {dr.item_id} không tồn tại hoặc không hoạt động.";
                    return false;
                }
                if (dr.amount <= 0)
                {
                    errorMessage = $"Daily Reward Ngày {dr.day_index} có amount {dr.amount} phải là số nguyên dương (> 0).";
                    return false;
                }

                // If it is a Key Item, amount must be exactly 1
                var rewardItem = _globalItems.Find(item => item.id == dr.item_id);
                bool isKeyItem = false;
                if (rewardItem != null && rewardItem.item_type == "key_item")
                {
                    isKeyItem = true;
                }
                else if (rewardItem == null)
                {
                    foreach (var tab in _tabs)
                    {
                        for (int itIdx = 0; itIdx < tab.itemStates.Count; itIdx++)
                        {
                            int keyItemId = tab.pictureId * 100 + (itIdx + 1);
                            if (keyItemId == dr.item_id)
                            {
                                isKeyItem = true;
                                break;
                            }
                        }
                        if (isKeyItem) break;
                    }
                }

                if (isKeyItem && dr.amount != 1)
                {
                    errorMessage = $"Daily Reward Ngày {dr.day_index} là Key Item, số lượng (Amount) bắt buộc phải là 1.";
                    return false;
                }

                config.daily_rewards.Add(new DailyRewardDto
                {
                    day_index = dr.day_index,
                    item_id = dr.item_id,
                    amount = dr.amount
                });
            }

            // Sort DTOs for deterministic, clean JSON output
            config.categories.Sort((a, b) => a.id.CompareTo(b.id));
            config.pictures.Sort((a, b) => a.id.CompareTo(b.id));
            config.items.Sort((a, b) => a.id.CompareTo(b.id));
            config.picture_difficulties.Sort((a, b) =>
            {
                int comp = a.picture_id.CompareTo(b.picture_id);
                if (comp != 0) return comp;
                return a.difficulty_id.CompareTo(b.difficulty_id);
            });
            config.daily_rewards.Sort((a, b) => a.day_index.CompareTo(b.day_index));

            return true;
        }

        private void SaveConfig()
        {
            StaticDataDto config;
            string err;
            if (!TryBuildConfig(out config, out err, true))
            {
                EditorUtility.DisplayDialog("Lỗi Cấu Hình", err, "OK");
                return;
            }

            string json = JsonUtility.ToJson(config, true);
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[JigsawVina Editor] Static data written successfully to {SavePath}.");
            EditorUtility.DisplayDialog("Hoàn Thành", $"Đã lưu và cấu hình static data tại {SavePath}!", "OK");
        }

        private void AddDifficulty(StaticDataDto config, int pictureId, int diffId, string displayName, int cols, int rows, int firstClearCoins, int replayCoins, int firstClearHints, int rewardIndex, List<string> items, Dictionary<string, int> localItems, int dropTableId)
        {
            var listRewards = new List<int>();
            if (rewardIndex > 0 && rewardIndex <= items.Count)
            {
                var texName = items[rewardIndex - 1];
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
                first_clear_reward_item_ids = listRewards,
                drop_table_id = dropTableId
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
                _tabs.Sort((a, b) => a.pictureId.CompareTo(b.pictureId));
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
