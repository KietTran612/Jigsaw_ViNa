using System;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class DifficultySelectView : MonoBehaviour
    {
        public event Action<int> OnDifficultySelected;

        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _normalButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _backButton;

        public Button BackButton => _backButton;

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
    }
}
