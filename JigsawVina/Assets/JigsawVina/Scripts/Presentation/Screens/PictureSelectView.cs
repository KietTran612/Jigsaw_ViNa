using System;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectView : MonoBehaviour
    {
        public event Action<int> OnPictureSelected;

        [SerializeField] private Button _pic1Button;
        [SerializeField] private Button _pic2Button;

        private void Awake()
        {
            if (_pic1Button != null)
            {
                _pic1Button.onClick.AddListener(() => OnPictureSelected?.Invoke(1));
            }

            if (_pic2Button != null)
            {
                _pic2Button.onClick.AddListener(() => OnPictureSelected?.Invoke(2));
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
