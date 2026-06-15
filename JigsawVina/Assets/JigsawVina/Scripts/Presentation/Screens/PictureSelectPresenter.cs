using System;
using JigsawVina.Core.Services;
using UnityEngine;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectPresenter : IDisposable
    {
        private readonly PictureSelectView _view;
        private readonly GameSessionService _sessionService;
        private readonly IStaticDataService _staticDataService;

        public PictureSelectPresenter(
            PictureSelectView view,
            GameSessionService sessionService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _staticDataService = staticDataService;

            _view.OnPictureSelected += HandlePictureSelected;

            Initialize();
        }

        private void Initialize()
        {
            var pictures = _staticDataService.GetAllPictures();
            if (pictures == null || pictures.Count == 0)
            {
                Debug.LogError("[JigsawVina] StaticData error: No pictures found in IStaticDataService.");
                return;
            }

            _view.Setup(pictures);
        }

        private void HandlePictureSelected(int pictureId)
        {
            _sessionService.SetSelectedPicture(pictureId);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnPictureSelected -= HandlePictureSelected;
            }
        }
    }
}
