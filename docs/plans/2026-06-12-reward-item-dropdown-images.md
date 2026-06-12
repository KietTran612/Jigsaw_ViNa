# Reward Item Dropdown Images Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Display key item images/icons inside the difficulty configurations dropdown for "Reward Key Item" and draw a preview thumbnail next to the dropdown in JigsawVinaGameDataEditor.

**Architecture:** 
- Convert the text-only dropdown options to `GUIContent` objects containing loaded `Texture2D` assets.
- Draw a bordered thumbnail preview of the selected key item next to the dropdown in each difficulty section (Easy, Normal, Hard) using `DrawTextureWithBorder`.

**Tech Stack:** Unity Editor GUI (IMGUI), Unity C# scripting.

---

### Task 1: Update Dropdown and Add Selected Thumbnail Preview in Game Data Editor

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs#L1065-L1120)

**Step 1: Replace itemNames array with itemGUIContents**
Construct an array of `GUIContent` where each option includes both the key item texture name and the texture itself:
```csharp
                GUIContent[] itemGUIContents = new GUIContent[itemTextures.Count + 1];
                itemGUIContents[0] = new GUIContent("None");
                for (int i = 0; i < itemTextures.Count; i++)
                {
                    itemGUIContents[i + 1] = new GUIContent(itemTextures[i].name, itemTextures[i]);
                }
```

**Step 2: Update Easy, Normal, and Hard dropdown calls**
Wrap the popup fields in a horizontal layout and add a 24x24 pixel preview box. If an item is selected, call `DrawTextureWithBorder` to render it. Otherwise, draw a clean dark placeholder box.

For **EASY** difficulty:
```csharp
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
```

Apply the equivalent block for **NORMAL** and **HARD** difficulties.

**Step 3: Run Unity compile check**
Wait for Unity compilation and verify there are no compiler warnings or errors.

**Step 4: Manual Visual Verification**
- Open `JigsawVina -> Game Data Editor` menu in Unity.
- Select "Cấu hình Tranh" -> "Độ khó & Phần thưởng".
- Open Easy/Normal/Hard foldouts, click "Reward Key Item" and check that dropdown items display their thumbnails.
- Confirm that selecting an item updates the preview thumbnail on the right.
