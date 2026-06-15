using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JigsawVina.Presentation.Screens
{
    public class DifficultySelectView : MonoBehaviour
    {
        public event Action<int> OnDifficultySelected;

        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _normalButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _backButton;

        [SerializeField] private GameObject[] _lockIcons;
        [SerializeField] private TMP_Text[] _achievementTexts;

        public Button BackButton => _backButton;
        public GameObject[] LockIcons => _lockIcons;
        public TMP_Text[] AchievementTexts => _achievementTexts;

        private void Awake()
        {
            if (_easyButton != null)
            {
                _easyButton.onClick.AddListener(() => OnDifficultySelected?.Invoke(0));
            }

            if (_normalButton != null)
            {
                _normalButton.onClick.AddListener(() => OnDifficultySelected?.Invoke(1));
            }

            if (_hardButton != null)
            {
                _hardButton.onClick.AddListener(() => OnDifficultySelected?.Invoke(2));
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetDifficultyState(int difficultyId, bool isUnlocked, string achievementText)
        {
            Button targetButton = null;
            switch (difficultyId)
            {
                case 0: targetButton = _easyButton; break;
                case 1: targetButton = _normalButton; break;
                case 2: targetButton = _hardButton; break;
            }

            if (targetButton != null)
            {
                targetButton.interactable = isUnlocked;
            }

            if (_lockIcons != null && difficultyId >= 0 && difficultyId < _lockIcons.Length)
            {
                var lockIcon = _lockIcons[difficultyId];
                if (lockIcon != null)
                {
                    lockIcon.SetActive(!isUnlocked);
                }
            }

            if (_achievementTexts != null && difficultyId >= 0 && difficultyId < _achievementTexts.Length)
            {
                var textElement = _achievementTexts[difficultyId];
                if (textElement != null)
                {
                    textElement.text = achievementText;
                }
            }
        }
    }
}
