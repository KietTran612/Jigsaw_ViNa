using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePlayingView : MonoBehaviour
    {
        public event Action OnCheatWinClicked;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _cheatWinButton;

        private void Awake()
        {
            if (_cheatWinButton != null)
            {
                _cheatWinButton.onClick.AddListener(() => OnCheatWinClicked?.Invoke());
            }
        }

        public void Setup(string pictureName, string difficultyName)
        {
            if (_titleText != null)
            {
                _titleText.text = $"Playing: {pictureName} ({difficultyName})";
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
