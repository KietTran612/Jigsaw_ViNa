using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class RewardSummaryView : MonoBehaviour
    {
        public event Action OnReturnClicked;

        [SerializeField] private TMP_Text _starsText;
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private Button _returnButton;

        private void Awake()
        {
            if (_returnButton != null)
            {
                _returnButton.onClick.AddListener(() => OnReturnClicked?.Invoke());
            }
        }

        public void DisplayReward(int stars, int coins)
        {
            if (_starsText != null)
            {
                _starsText.text = $"Stars: {stars}";
            }

            if (_coinsText != null)
            {
                _coinsText.text = $"Coins Earned: {coins}";
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
