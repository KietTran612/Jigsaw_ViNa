using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Tests
{
    public class LocalizationServiceTests
    {
        [SetUp]
        public void Setup()
        {
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void LocalizationService_LoadsDefaultVietnamese()
        {
            var saveService = new SaveDataService();
            var locService = new LocalizationService(saveService);

            Assert.AreEqual("vi", locService.CurrentLanguage);
            string backText = locService.Get(LocalizationKeys.GameplayBack);
            Assert.AreEqual("Quay lại", backText);
        }

        [Test]
        public void SetLanguage_ChangesLanguageAndFiresEvent()
        {
            var saveService = new SaveDataService();
            var locService = new LocalizationService(saveService);

            bool eventFired = false;
            locService.OnLanguageChanged += () => eventFired = true;

            locService.SetLanguage("en");

            Assert.AreEqual("en", locService.CurrentLanguage);
            Assert.IsTrue(eventFired);
            string backText = locService.Get(LocalizationKeys.GameplayBack);
            Assert.AreEqual("Back", backText);
        }

        [Test]
        public void Get_UnknownKey_ReturnsKeyItself()
        {
            var saveService = new SaveDataService();
            var locService = new LocalizationService(saveService);

            string unknown = locService.Get("some.unknown.key.123");
            Assert.AreEqual("some.unknown.key.123", unknown);
        }

        [Test]
        public void LocalizationKeyIntegrity_VerifyAllKeysExistInBothLanguages()
        {
            var stringsViAsset = Resources.Load<TextAsset>("Localization/strings_vi");
            var stringsEnAsset = Resources.Load<TextAsset>("Localization/strings_en");

            Assert.IsNotNull(stringsViAsset, "strings_vi.json must exist in Resources/Localization");
            Assert.IsNotNull(stringsEnAsset, "strings_en.json must exist in Resources/Localization");

            var viData = JsonUtility.FromJson<JigsawVina.Core.Services.LocalizationData>(stringsViAsset.text);
            var enData = JsonUtility.FromJson<JigsawVina.Core.Services.LocalizationData>(stringsEnAsset.text);

            Assert.IsNotNull(viData, "strings_vi could not be deserialized");
            Assert.IsNotNull(enData, "strings_en could not be deserialized");

            var viKeys = new HashSet<string>();
            foreach (var entry in viData.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Key), "Found empty key in strings_vi");
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value), $"Key '{entry.Key}' has empty value in strings_vi");
                viKeys.Add(entry.Key);
            }

            var enKeys = new HashSet<string>();
            foreach (var entry in enData.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Key), "Found empty key in strings_en");
                Assert.IsFalse(string.IsNullOrEmpty(entry.Value), $"Key '{entry.Key}' has empty value in strings_en");
                enKeys.Add(entry.Key);
            }

            // 1. Check all constants defined in LocalizationKeys class
            var keysType = typeof(LocalizationKeys);
            var fields = keysType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    string keyVal = (string)field.GetRawConstantValue();
                    Assert.IsTrue(viKeys.Contains(keyVal), $"LocalizationKeys key '{keyVal}' is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(keyVal), $"LocalizationKeys key '{keyVal}' is missing from strings_en.json");
                }
            }

            // 2. Check all display_name_key and description_key in jigsaw_vina_game_data.json
            var staticDataAsset = Resources.Load<TextAsset>("GameData/jigsaw_vina_game_data");
            Assert.IsNotNull(staticDataAsset, "jigsaw_vina_game_data.json must exist in Resources/GameData");
            var staticData = JsonUtility.FromJson<StaticDataDto>(staticDataAsset.text);
            Assert.IsNotNull(staticData, "jigsaw_vina_game_data could not be deserialized");

            foreach (var pic in staticData.pictures)
            {
                if (!string.IsNullOrEmpty(pic.display_name_key))
                {
                    Assert.IsTrue(viKeys.Contains(pic.display_name_key), $"Picture display_name_key '{pic.display_name_key}' (Pic ID: {pic.id}) is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(pic.display_name_key), $"Picture display_name_key '{pic.display_name_key}' (Pic ID: {pic.id}) is missing from strings_en.json");
                }
                if (!string.IsNullOrEmpty(pic.description_key))
                {
                    Assert.IsTrue(viKeys.Contains(pic.description_key), $"Picture description_key '{pic.description_key}' (Pic ID: {pic.id}) is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(pic.description_key), $"Picture description_key '{pic.description_key}' (Pic ID: {pic.id}) is missing from strings_en.json");
                }
            }

            foreach (var cat in staticData.categories)
            {
                if (!string.IsNullOrEmpty(cat.display_name_key))
                {
                    Assert.IsTrue(viKeys.Contains(cat.display_name_key), $"Category display_name_key '{cat.display_name_key}' (Cat ID: {cat.id}) is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(cat.display_name_key), $"Category display_name_key '{cat.display_name_key}' (Cat ID: {cat.id}) is missing from strings_en.json");
                }
                if (!string.IsNullOrEmpty(cat.description_key))
                {
                    Assert.IsTrue(viKeys.Contains(cat.description_key), $"Category description_key '{cat.description_key}' (Cat ID: {cat.id}) is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(cat.description_key), $"Category description_key '{cat.description_key}' (Cat ID: {cat.id}) is missing from strings_en.json");
                }
            }

            foreach (var item in staticData.items)
            {
                if (!string.IsNullOrEmpty(item.display_name_key))
                {
                    Assert.IsTrue(viKeys.Contains(item.display_name_key), $"Item display_name_key '{item.display_name_key}' (Item ID: {item.id}) is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(item.display_name_key), $"Item display_name_key '{item.display_name_key}' (Item ID: {item.id}) is missing from strings_en.json");
                }
                if (!string.IsNullOrEmpty(item.description_key))
                {
                    Assert.IsTrue(viKeys.Contains(item.description_key), $"Item description_key '{item.description_key}' (Item ID: {item.id}) is missing from strings_vi.json");
                    Assert.IsTrue(enKeys.Contains(item.description_key), $"Item description_key '{item.description_key}' (Item ID: {item.id}) is missing from strings_en.json");
                }
            }
        }
    }
}
