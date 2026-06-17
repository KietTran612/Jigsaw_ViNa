using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameSettingsPopup : MonoBehaviour
    {
        public event Action<bool> OnMusicToggleChanged;
        public event Action<bool> OnSfxToggleChanged;
        public event Action<string> OnLanguageSelectionChanged;
        public event Action OnResumeClicked;
        public event Action OnQuitClicked;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _musicLabel;
        [SerializeField] private TMP_Text _sfxLabel;
        [SerializeField] private TMP_Text _languageLabel;
        
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _sfxToggle;
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _quitButton;

        private JigsawVina.Core.Services.ILocalizationService _localizationService;
        private CanvasGroup _canvasGroup;
        private bool _isInitialized;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_musicToggle != null)
                _musicToggle.onValueChanged.AddListener(HandleMusicValueChanged);
            if (_sfxToggle != null)
                _sfxToggle.onValueChanged.AddListener(HandleSfxValueChanged);
            if (_languageDropdown != null)
                _languageDropdown.onValueChanged.AddListener(HandleLanguageDropdownChanged);
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            if (_quitButton != null)
                _quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public void Setup(JigsawVina.Core.Services.ILocalizationService localizationService)
        {
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged -= TranslateTexts;
            }

            _localizationService = localizationService;
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged += TranslateTexts;
            }

            TranslateTexts();
        }

        private void OnDestroy()
        {
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged -= TranslateTexts;
            }
        }

        public void Show(bool musicOn, bool sfxOn, string langCode)
        {
            _isInitialized = false;
            EnsureCanvasGroup();

            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            if (_musicToggle != null)
                _musicToggle.isOn = musicOn;
            if (_sfxToggle != null)
                _sfxToggle.isOn = sfxOn;

            if (_languageDropdown != null)
            {
                // Index 0 = vi, Index 1 = en
                int langIndex = (langCode == "en") ? 1 : 0;
                _languageDropdown.value = langIndex;
            }

            _isInitialized = true;
            TranslateTexts();
        }

        public void Hide()
        {
            EnsureCanvasGroup();

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void HandleMusicValueChanged(bool isOn)
        {
            if (!_isInitialized) return;
            OnMusicToggleChanged?.Invoke(isOn);
        }

        private void HandleSfxValueChanged(bool isOn)
        {
            if (!_isInitialized) return;
            OnSfxToggleChanged?.Invoke(isOn);
        }

        private void HandleLanguageDropdownChanged(int index)
        {
            if (!_isInitialized) return;
            string langCode = (index == 1) ? "en" : "vi";
            OnLanguageSelectionChanged?.Invoke(langCode);
        }

        private void TranslateTexts()
        {
            if (_localizationService == null) return;

            if (_titleText != null)
                _titleText.text = _localizationService.Get(JigsawVina.Core.Services.LocalizationKeys.SettingsTitle);
            if (_musicLabel != null)
                _musicLabel.text = _localizationService.Get(JigsawVina.Core.Services.LocalizationKeys.SettingsMusic);
            if (_sfxLabel != null)
                _sfxLabel.text = _localizationService.Get(JigsawVina.Core.Services.LocalizationKeys.SettingsSfx);
            if (_languageLabel != null)
                _languageLabel.text = _localizationService.Get(JigsawVina.Core.Services.LocalizationKeys.SettingsLanguage);

            if (_resumeButton != null)
            {
                var text = _resumeButton.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = _localizationService.Get(JigsawVina.Core.Services.LocalizationKeys.SettingsResume);
            }

            if (_quitButton != null)
            {
                var text = _quitButton.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = _localizationService.Get(JigsawVina.Core.Services.LocalizationKeys.SettingsQuit);
            }
        }
    }
}
