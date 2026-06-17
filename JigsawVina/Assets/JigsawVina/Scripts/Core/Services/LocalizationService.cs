using System;
using System.Collections.Generic;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    [Serializable]
    public class LocalizationEntry
    {
        public string Key;
        public string Value;
    }

    [Serializable]
    public class LocalizationData
    {
        public List<LocalizationEntry> Entries = new();
    }

    public class LocalizationService : ILocalizationService
    {
        public event Action OnLanguageChanged;

        private readonly ISaveDataService _saveDataService;
        private readonly Dictionary<string, string> _translations = new();
        private string _currentLanguage;

        public string CurrentLanguage => _currentLanguage;

        public LocalizationService(ISaveDataService saveDataService)
        {
            _saveDataService = saveDataService;
            Initialize();
        }

        private void Initialize()
        {
            var save = _saveDataService.Load();
            _currentLanguage = save.Language;
            if (string.IsNullOrEmpty(_currentLanguage))
            {
                _currentLanguage = "vi";
            }
            LoadLanguageDictionary(_currentLanguage);
        }

        public void SetLanguage(string langCode)
        {
            if (langCode != "vi" && langCode != "en")
            {
                langCode = "vi";
            }

            if (_currentLanguage == langCode) return;

            _currentLanguage = langCode;
            var save = _saveDataService.Load();
            save.Language = langCode;
            _saveDataService.Save(save);

            LoadLanguageDictionary(langCode);

            OnLanguageChanged?.Invoke();
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            if (_translations.TryGetValue(key, out string translatedValue))
            {
                return translatedValue;
            }

            return key; // Fallback to key itself if not found
        }

        public string GetFormat(string key, params object[] args)
        {
            string format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Localization] Error formatting key '{key}': {ex.Message}");
                return format;
            }
        }

        private void LoadLanguageDictionary(string langCode)
        {
            _translations.Clear();

            string resourcePath = $"Localization/strings_{langCode}";
            var textAsset = Resources.Load<TextAsset>(resourcePath);

            if (textAsset == null)
            {
                Debug.LogError($"[Localization] Failed to load TextAsset at '{resourcePath}'");
                return;
            }

            try
            {
                var data = JsonUtility.FromJson<LocalizationData>(textAsset.text);
                if (data != null && data.Entries != null)
                {
                    foreach (var entry in data.Entries)
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.Key))
                        {
                            _translations[entry.Key] = entry.Value;
                        }
                    }
                }
                Debug.Log($"[Localization] Successfully loaded {stringsCount()} terms for language '{langCode}'");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Localization] Error parsing localization JSON for language '{langCode}': {ex.Message}");
            }
        }

        private int stringsCount()
        {
            return _translations.Count;
        }
    }
}
